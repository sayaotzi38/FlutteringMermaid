using System;
using UnityEngine;
using System.Collections; // ← これを追加

/// <summary>
/// ゲーム全体を管理するシングルトン。セーブ・ロードや通知処理を行う。
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public SaveManager SaveManagerInstance { get; private set; }

    public bool IsResetting { get; private set; } = false;

    private static bool isAdmobInitialized = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("🚀 GameManager が初期化されました");

         
           InitializeAdmobOnce();
            LocalPushNotification.RegisterChannel();
            Debug.Log("📲 通知チャンネルを登録しました");
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }
    private IEnumerator Start()
    {
        Debug.Log("🎮 GameManager の Start が呼ばれました");

        SaveManagerInstance = SaveManager.Instance;

        // セーブデータをここで必ずロードする（これがないと null のまま）
        SaveManagerInstance.Load();

        // SaveManager と SaveDataInstance の準備完了を待つ
        yield return new WaitUntil(() =>
            SaveManagerInstance != null &&
            SaveManagerInstance.SaveDataInstance != null);

        Debug.Log("✅ SaveDataInstance を確認しました → SimulateTimePassed 実行");

        SimulateTimePassed();  // ← 水質の加算処理
    }











    private void InitializeAdmobOnce()
    {
        if (!isAdmobInitialized)
        {
            AdmobLibrary.FirstSetting();
            isAdmobInitialized = true;
            Debug.Log("📡 AdMob の初期化を実行しました");
        }
    }

    private void OnApplicationFocus(bool focus)
    {
        Debug.Log($"== Focus Changed: {focus} ==");

        if (!focus)
        {
            Debug.Log(">> Saving & checking notification");

            if (SaveManagerInstance != null)
            {
                SaveManagerInstance.Save();
                SetupNotifications();
            }
            else
            {
                Debug.LogWarning("⚠ SaveManagerInstance が null のため、セーブスキップ");
            }
        }
        else
        {
            Debug.Log(">> Clear notifications");
            LocalPushNotification.AllClear();
        }
    }

    void OnApplicationPause(bool pause)
    {
        if (pause)
        {
            if (SaveManager.Instance != null)
            {
                SaveManager.Instance.Save();
                Debug.Log("[GameManager] アプリ中断 → セーブ実行");
            }
            else
            {
                Debug.LogWarning("⚠ SaveManager.Instance が null のため、セーブをスキップします");
            }
        }
    }


    /// <summary>
    /// アプリを閉じていた時間の間に経過したゲーム内処理を再現する
    /// </summary>
    private void SimulateTimePassed()
    {
        Debug.Log("🧪 SimulateTimePassed() 実行開始");

        if (SaveManager.Instance == null || SaveManager.Instance.SaveDataInstance == null)
        {
            Debug.LogWarning("⚠ SimulateTimePassed(): セーブデータが null → スキップ");
            return;
        }

        var saveData = SaveManager.Instance.SaveDataInstance;
        DateTime now = DateTime.Now;

        // 🎯 成長用：累計日数
        TimeSpan totalElapsed = now - saveData.gameStartTime;
        int totalDays = (int)totalElapsed.TotalDays;
        int newDays = totalDays - saveData.daysPassed;

        Debug.Log($"⏳ 累計プレイ時間: {totalElapsed.TotalHours:F1} 時間 → {totalDays} 日");

        if (newDays > 0)
        {
            saveData.daysPassed += newDays;
            Debug.Log($"🧜 成長日数を {newDays} 日加算 → 合計: {saveData.daysPassed} 日");

            var growthManager = FindFirstObjectByType<MermaidGrowthManager>();
            if (growthManager != null)
            {
                growthManager.ForceRefreshGrowth();
                Debug.Log("🧜‍♀️ 成長状態を SimulateTimePassed() で強制更新しました");
            }
        }


        Debug.Log($"🛠 [DEBUG] lastSaveTime: {saveData.lastSaveTime}, now: {now}GMcs");
        // 🌊 汚れ加算用：最後のセーブ時からの経過時間
        TimeSpan elapsedSinceLastSave = now - saveData.lastSaveTime;
        // ✅ この直後にログを追加
        Debug.Log($"🕓 アプリを閉じていた時間: {elapsedSinceLastSave.TotalMinutes:F1} 分（{elapsedSinceLastSave.TotalSeconds:F0} 秒）");

        float secondsElapsed = (float)elapsedSinceLastSave.TotalSeconds;
        float pollutionIncrease = secondsElapsed * 0.0002314815f; // 約5日で100%加算されるレート

        saveData.waterPollutionLevel = Mathf.Clamp(
            saveData.waterPollutionLevel + pollutionIncrease,
            0f, 100f);

        Debug.Log($"🌊 閉じていた時間による水質加算: +{pollutionIncrease:F2} → 現在: {saveData.waterPollutionLevel:F2}%");


        // 💡 空腹度減少処理をここに追加
        float hungerLoss = (float)(now - saveData.lastSaveTime).TotalSeconds * (100f / 345600f); // 4日でゼロ

        saveData.hungerTimeRemaining = Mathf.Clamp(
            saveData.hungerTimeRemaining - hungerLoss,
            0f, 100f);

        Debug.Log($"🍽 閉じていた間の空腹度減少: -{hungerLoss:F2} → 現在: {saveData.hungerTimeRemaining:F2}%");


        saveData.lastSaveTime = now;
        SaveManager.Instance.Save();


        var waterManager = FindFirstObjectByType<WaterManager>();
        if (waterManager != null)
        {
            waterManager.LoadDirtFromSaveData();
            Debug.Log("✅ WaterManager に LoadDirtFromSaveData() を指示しました");
        }
        else
        {
            Debug.LogWarning("⚠ WaterManager が見つかりませんでした → 水質反映スキップ");
        }

    }


    private void SetupNotifications()
    {
        var data = SaveManagerInstance.SaveDataInstance;

        Debug.Log($"📊 通知チェック開始：hunger={data.hungerTimeRemaining}, pollution={data.waterPollutionLevel}");

        if (data.hungerTimeRemaining <= 10f && data.hungerTimeRemaining > 0f)
        {
            Debug.Log("🔔 満腹度が10%以下 → 通知をスケジュール");
            LocalPushNotification.AddSchedule("警告", "人魚がおなかをすかせているよ！", 0);
        }

        float maxPollutionTime = 604800f;
        float currentPollutionTime = data.waterPollutionLevel / 100f * maxPollutionTime;

        if (data.waterPollutionLevel >= 90f)
        {
            Debug.Log("🔔 水質が90%以上汚れている → 通知をスケジュールします");
            LocalPushNotification.AddSchedule("警告", "水が汚れています！", 0);
        }
        else
        {
            Debug.Log($"✅ 水質通知スキップ：pollution={data.waterPollutionLevel}");
        }
    }

    public void SaveGame()
    {
        SaveManagerInstance.Save();
        Debug.Log("✅ SaveGame を呼び出して手動でセーブしました");
    }

    public void ResetEverything()
    {
        Debug.Log("🧹 ResetEverything(): ゲーム全体を初期化します");

        MermaidStatus.SkipLoadFromSaveData = true;

        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.ResetAllGameState();
        }
        else
        {
            Debug.LogWarning("⚠ SaveManager.Instance が null のため、ResetAllGameState をスキップします");
        }

        var status = FindFirstObjectByType<MermaidStatus>();
        if (status != null)
        {
            status.isAlive = true;
            status.ResetHunger(100.0f);
            status.ResetMermaidStatus();
            Debug.Log($"🐟 Reset後の isAlive: {status.isAlive}, hunger: {status.GetCurrentHunger()}");
        }
        else
        {
            Debug.LogWarning("⚠ MermaidStatus がシーンに存在しないため、ステータスリセットをスキップします");
        }

        var growth = FindFirstObjectByType<MermaidGrowthManager>();
        if (growth != null)
        {
            growth.ResetGrowth();
        }
        else
        {
            Debug.LogWarning("⚠ MermaidGrowthManager が見つかりませんでした");
        }

        var water = FindFirstObjectByType<WaterManager>();
        if (water != null)
        {
            water.ResetPollution();
        }
        else
        {
            Debug.LogWarning("⚠ WaterManager が見つかりませんでした");
        }

        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.Save();
            Debug.Log("✅ ResetEverything(): 状態リセット完了（セーブ済）");
        }
        else
        {
            Debug.LogWarning("⚠ SaveManager.Instance が null のため、セーブをスキップします");
        }
    }
}
