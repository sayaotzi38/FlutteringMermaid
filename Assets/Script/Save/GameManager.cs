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

            // ✅ SaveManager は直接シングルトンから取得
            SaveManagerInstance = SaveManager.Instance;

            LocalPushNotification.RegisterChannel();
            Debug.Log("📲 通知チャンネルを登録しました");
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }



    void Start()
    {
        Debug.Log("🎮 GameManager の Start が呼ばれました");

        StartCoroutine(InitializeAfterDelay());
    }

    private IEnumerator InitializeAfterDelay()
    {
        yield return null; // ✅ 他のAwakeが終わるのを待つ

        Debug.Log("🚀 GameManager 起動！（1フレーム遅延）");

        // ✅ SaveManager を明示的に取得・ロード
        SaveManagerInstance = SaveManager.Instance;

        if (SaveManagerInstance != null)
        {
            SaveManagerInstance.Load();
            Debug.Log("📦 GameManager で SaveManager.Load() を呼びました");
        }
        else
        {
            Debug.LogError("❌ GameManager: SaveManager.Instance が null です！（遅延後でも）");
            yield break; // これ以上進めない
        }

        if (SaveManagerInstance.SaveDataInstance != null)
        {
            Debug.Log($"🕒 最後のセーブ時刻: {SaveManagerInstance.SaveDataInstance.lastSaveTime}");
        }
        else
        {
            Debug.LogWarning("⚠ SaveDataInstance が null です（セーブ時刻は表示できません）");
            yield break; // データが無ければシミュレーションもしない
        }

        // ✅ 経過時間の再現処理を呼び出し
        SimulateTimePassed();
    }


    /// <summary>
    /// アプリ起動時に一度だけ AdMob を初期化する
    /// </summary>
    private void InitializeAdmobOnce()
    {
        if (!isAdmobInitialized)
        {
            AdmobLibrary.FirstSetting();
            isAdmobInitialized = true;
            Debug.Log("📡 AdMob の初期化を実行しました");
        }
    }




    /// <summary>
    /// アプリがフォーカスを失ったときの処理（バックグラウンド）
    /// </summary>
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
            SaveManager.Instance.Save();
            Debug.Log("[GameManager] アプリ中断 → セーブ実行");
        }
    }
    /// <summary>
    /// アプリ終了時のセーブ処理
    /// </summary>
    private void OnApplicationQuit()
    {
        if (SaveManagerInstance != null)
        {
            SaveManagerInstance.Save();
            Debug.Log("💾 アプリ終了時にデータを保存しました");
        }
        else
        {
            Debug.LogWarning("⚠ OnApplicationQuit(): SaveManagerInstance が null のためセーブできませんでした");
        }
    }

    private void SimulateTimePassed()
    {
        if (SaveManager.Instance == null)
        {
            Debug.LogWarning("⚠ SimulateTimePassed(): SaveManager.Instance が null → スキップ");
            return;
        }

        if (SaveManager.Instance.SaveDataInstance == null)
        {
            Debug.LogWarning("⚠ SimulateTimePassed(): SaveDataInstance が null → スキップ");
            return;
        }

        var saveData = SaveManager.Instance.SaveDataInstance;
        DateTime now = DateTime.Now;
        DateTime last = saveData.lastSaveTime;

        TimeSpan elapsed = now - last;

        Debug.Log($"⏳ 経過時間シミュレーション: {elapsed.TotalMinutes:F1} 分経過");

        // 🍽 満腹度：4日（345600秒）で0になる
        float hungerDecreaseRate = 100f / 345600f;
        float hungerDecrease = (float)(elapsed.TotalSeconds * hungerDecreaseRate);
        saveData.hungerTimeRemaining = Mathf.Max(0f, saveData.hungerTimeRemaining - hungerDecrease);
        Debug.Log($"🍽 経過時間で空腹度を {hungerDecrease:F2} 減少 → 現在: {saveData.hungerTimeRemaining:F2}%");

        // 💧 水質：5日（432000秒）で100%になる
        float pollutionIncreaseRate = 100f / 432000f;
        float pollutionIncrease = (float)(elapsed.TotalSeconds * pollutionIncreaseRate);
        saveData.waterPollutionLevel = Mathf.Min(100f, saveData.waterPollutionLevel + pollutionIncrease);
        Debug.Log($"💧 経過時間で汚れを {pollutionIncrease:F4} 増加 → 現在: {saveData.waterPollutionLevel:F2}%");

        // 🧜 成長：1日ごとに1成長
        int daysPassed = (int)elapsed.TotalDays;
        if (daysPassed > 0)
        {
            saveData.daysPassed += daysPassed;
            Debug.Log($"🧜 成長日数を {daysPassed} 日加算 → 合計: {saveData.daysPassed} 日");
        }

        SaveManager.Instance.Save(); // 上書き保存
    }




    /// <summary>
    /// 条件に応じた通知スケジュールを設定（バックグラウンド時のみ）
    /// </summary>
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



    /// <summary>
    /// ゲームデータを手動でセーブする関数
    /// </summary>
    public void SaveGame()
    {
        SaveManagerInstance.Save();
        Debug.Log("✅ SaveGame を呼び出して手動でセーブしました");
    }


    /// <summary>
    /// ゲーム全体の状態とUI・アニメ・セーブを完全初期化する関数（ゲームオーバーやタイトル復帰時に使用）
    /// </summary>
    public void ResetEverything()
    {
        Debug.Log("🧹 ResetEverything(): ゲーム全体を初期化します");

        MermaidStatus.SkipLoadFromSaveData = true;

        // ✅ セーブデータ初期化
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.ResetAllGameState();
        }
        else
        {
            Debug.LogWarning("⚠ SaveManager.Instance が null のため、ResetAllGameState をスキップします");
        }

        // ✅ 人魚のステータスを初期化
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

        // ✅ 成長データのリセット
        var growth = FindFirstObjectByType<MermaidGrowthManager>();
        if (growth != null)
        {
            growth.ResetGrowth();
        }
        else
        {
            Debug.LogWarning("⚠ MermaidGrowthManager が見つかりませんでした");
        }

        // ✅ 水質のリセット
        var water = FindFirstObjectByType<WaterManager>();
        if (water != null)
        {
            water.ResetPollution();
        }
        else
        {
            Debug.LogWarning("⚠ WaterManager が見つかりませんでした");
        }

        // ✅ 最後にセーブ
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
