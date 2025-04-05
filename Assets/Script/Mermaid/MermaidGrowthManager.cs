using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;


/// <summary>
/// **人魚の成長 & サイズ管理**
/// - ゲーム内経過日数によって人魚の成長を制御
/// - SaveData.daysPassed と連携し、セーブ・ロード対応
/// </summary>
public class MermaidGrowthManager : MonoBehaviour
{
    [Header("デバッグモード (本番では false)")]
    public bool DebugMode = false;

    [Header("成長データ")]
    [SerializeField, Range(0, 1500)] private int currentDays = 0;

    [SerializeField] private int babyFishDays = 1;
    [SerializeField] private int childDays = 3;
    [SerializeField] private int youngDays = 6;
    [SerializeField] private int teenDays = 11;
    [SerializeField] private int adultDays = 16;
    [SerializeField] private int perfectDays = 21;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI growthStatusText;
    [SerializeField] private TextMeshProUGUI sizeText;
    [SerializeField] private TextMeshProUGUI growthDayText;

    [Header("モデルのスケール設定")]
    [SerializeField] private Transform mermaidModel;
    private Vector3 initialScale = new Vector3(1, 1, 1);
    private Vector3 maxScale = new Vector3(7, 7, 1); // 🔧 サイズ調整

    private MermaidAppearance appearance;
    private bool shouldUpdateAppearance = false;

    void Awake()
    {
        Debug.Log("🚀 `MermaidGrowthManager` の `Awake()` 実行");
    }
    private void Start()
    {
        Debug.Log("🐣 MermaidGrowthManager Start 呼び出し！");

        // ✅ SaveDataから復元
        if (SaveManager.Instance != null && SaveManager.Instance.SaveDataInstance != null)
        {
            currentDays = SaveManager.Instance.SaveDataInstance.daysPassed;
            Debug.Log($"📦 Start(): SaveData.daysPassed を currentDays に反映 → {currentDays} 日");
        }
        else
        {
            Debug.LogWarning("⚠ SaveManager.Instance または SaveDataInstance が null です");
        }

        appearance = GetComponent<MermaidAppearance>();
        if (appearance == null)
        {
            Debug.LogError("❌ MermaidAppearance が見つかりません！");
        }

        UpdateMermaidAppearance(); // ← これを SaveData復元後すぐに呼ぶ
        UpdateMermaidScale();      // ← スケールも更新

        StartCoroutine(UpdateDaysPassedOverTime()); // ← タイマーは最後に開始
        PrintGrowthStageRealTime();
    }




    private void Update()
    {
        if (shouldUpdateAppearance)
        {
            shouldUpdateAppearance = false;
            UpdateMermaidAppearance();
            UpdateMermaidScale();
        }

        
    }

    /// <summary>
    /// プレイ中に日数を進めるコルーチン（デバッグ用）
    /// </summary>
    private IEnumerator UpdateDaysPassedOverTime()
    {
        while (true)
        {
            float waitTime = SaveManager.isDebugSpeed ? 86400f / SaveManager.debugTimeScale : 86400f;

            Debug.Log($"⏱ 成長タイマー起動：{waitTime}秒待機 → 1日加算予定");

            yield return new WaitForSeconds(waitTime);

            SaveManager.Instance.SaveDataInstance.daysPassed += 1;
            CurrentDays = SaveManager.Instance.SaveDataInstance.daysPassed;

            Debug.Log($"📅 日数が進みました → {CurrentDays} 日目");
            UpdateMermaidAppearance();
            UpdateMermaidScale();
        }
    }

    private void ForceRefreshGrowth()
    {
        currentDays = SaveManager.Instance.SaveDataInstance.daysPassed;
        Debug.Log($"🔁 ForceRefreshGrowth(): daysPassed = {currentDays} を反映");

        UpdateMermaidAppearance();
        UpdateMermaidScale();
    }

    private void OnEnable()
    {
        if (SaveManager.Instance == null)
        {
            Debug.LogWarning("⚠ OnEnable(): SaveManager.Instance が null です → 処理スキップ");
            return;
        }

        int savedDays = SaveManager.Instance.SaveDataInstance.daysPassed;
        currentDays = savedDays;
        Debug.Log($"📦 セーブから成長日数を読み込みました: {currentDays} 日");

        // 🔽 セーブにも反映（明示的にセーブ）
        SaveManager.Instance.SaveDataInstance.daysPassed = currentDays;
        SaveManager.Instance.Save();
        Debug.Log("💾 currentDays を SaveData に保存しました");

        shouldUpdateAppearance = true; // ← 即時更新に変更
    }









    /// <summary>
    /// 日数を加算し、成長状態を更新する
    /// デバッグボタンから呼び出される
    /// </summary>
    /// <param name="daysToAdd">加算する日数</param>
    public void AddDays(int daysToAdd)
    {
        if (!DebugMode) return;

        int newDays = Mathf.Clamp(currentDays + daysToAdd, 0, 1500);
        Debug.Log($"⏩ {daysToAdd}日 加算: {currentDays} → {newDays}");

        currentDays = newDays;

        // ✅ セーブデータに反映
        SaveManager.Instance.SaveDataInstance.daysPassed = currentDays;
        SaveManager.Instance.Save();
        Debug.Log("💾 AddDays(): currentDays を SaveData に保存しました");

        UpdateMermaidAppearance();
        UpdateMermaidScale();
    }






    public void SetDays(float days)
    {
        currentDays = Mathf.Clamp((int)days, 0, 1500);
        Debug.Log($"📅 成長日数変更: {currentDays} 日目");

        // ✅ セーブデータに反映
        SaveManager.Instance.SaveDataInstance.daysPassed = currentDays;
        SaveManager.Instance.Save();
        Debug.Log("💾 SetDays(): currentDays を SaveData に保存しました");

        UpdateMermaidAppearance();
        UpdateMermaidScale();
    }


    public string GetCurrentStage()
    {
        if (currentDays < babyFishDays) return "Egg";
        if (currentDays < childDays) return "BabyFish";
        if (currentDays < youngDays) return "Child";
        if (currentDays < teenDays) return "Young";
        if (currentDays < adultDays) return "Teen";
        if (currentDays < perfectDays) return "Adult";
        return "Perfect";
    }

    // ------------------------
    // 成長ロジック系
    // ------------------------
    private void UpdateMermaidAppearance()
    {
        if (appearance != null)
        {
            string newStage = GetCurrentStage();
            Debug.Log($"🎯 GM.CS`UpdateMermaidAppearance()` 実行！ 成長段階: {newStage}");

            appearance.ChangeAppearance(newStage);
        }
        else
        {
            Debug.LogError("❌ `MermaidAppearance` が `null` です！");
        }

        if (growthStatusText != null)
        {
            growthStatusText.text = $"じょうたい : {GetCurrentStageJapanese()}";
        }

        if (growthDayText != null)
        {
            growthDayText.text = $"{currentDays} にちめ";
        }
    }

    private void UpdateMermaidScale()
    {
        if (mermaidModel == null) return;

        float growthFactor = Mathf.InverseLerp(0, 1500, currentDays);
        mermaidModel.localScale = Vector3.Lerp(initialScale, maxScale, growthFactor);

        if (sizeText != null)
        {
            float displaySizeCm = mermaidModel.localScale.x * 3f;
            sizeText.text = $"サイズ: {displaySizeCm:F2}cm";
        }
    }
    /// <summary>
    /// 成長段階の日本語表記を返す
    /// </summary>
    public string GetCurrentStageJapanese()
    {
        switch (GetCurrentStage())
        {
            case "Egg": return "たまご";
            case "BabyFish": return "ちぎょ";
            case "Child": return "ようぎょ";
            case "Young": return "ようたい";
            case "Teen": return "わかうお";
            case "Adult": return "あおうお";
            case "Perfect": return "いちにんまえ";
            default: return "ふめい";
        }
    }
    // ------------------------
    // その他ユーティリティ
    // ------------------------
    public int CurrentDays
    {
        get { return currentDays; }
        set
        {
            currentDays = Mathf.Clamp(value, 0, 1500);
            shouldUpdateAppearance = true;
        }
    }

    public bool IsEgg()
    {
        return GetCurrentStage() == "Egg";
    }


    /// <summary>
    /// 現在の時間スケールで、各成長段階にかかるリアル時間（秒・分）を出力
    /// </summary>
    private void PrintGrowthStageRealTime()
    {
        float daySeconds = 86400f / SaveManager.debugTimeScale;

        Debug.Log("📊 成長ステージ到達までのリアル時間:");
        Debug.Log($"🥚 BabyFish（{babyFishDays}日）→ {babyFishDays * daySeconds:F1}秒（{(babyFishDays * daySeconds) / 60f:F1}分）");
        Debug.Log($"🐟 Child（{childDays}日）→ {childDays * daySeconds:F1}秒（{(childDays * daySeconds) / 60f:F1}分）");
        Debug.Log($"🐠 Young（{youngDays}日）→ {youngDays * daySeconds:F1}秒（{(youngDays * daySeconds) / 60f:F1}分）");
        Debug.Log($"🧜 Teen（{teenDays}日）→ {teenDays * daySeconds:F1}秒（{(teenDays * daySeconds) / 60f:F1}分）");
        Debug.Log($"🧜‍♀️ Adult（{adultDays}日）→ {adultDays * daySeconds:F1}秒（{(adultDays * daySeconds) / 60f:F1}分）");
        Debug.Log($"🌟 Perfect（{perfectDays}日）→ {perfectDays * daySeconds:F1}秒（{(perfectDays * daySeconds) / 60f:F1}分）");
    }


    public void ResetGrowth()
    {
        StopAllCoroutines(); // 前のタイマー停止
        currentDays = 0;
        SaveManager.Instance.SaveDataInstance.daysPassed = 0;
        SaveManager.Instance.Save();

        Debug.Log($"🔁 ResetGrowth(): 成長日数 = {currentDays}（セーブにも反映）Growth");

        UpdateMermaidAppearance();
        UpdateMermaidScale();

        StartCoroutine(UpdateDaysPassedOverTime()); // タイマー再開
        Debug.Log("🔁 ResetGrowth(): 成長状態を初期化＆コルーチン再スタートしました");
    }


}
