using UnityEngine;
using System;

/// <summary>
/// セーブデータの管理クラス
/// </summary>
public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }

    public static bool isDebugSpeed = false;
    public static float debugTimeScale = 8400f;

    public SaveData SaveDataInstance; // ✅ Awakeで初期化しない

    private void Awake()
    {
        
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // ✅ アプリ終了時にも生き残る
            Debug.Log("💾 SaveManager が初期化されました");

            Load(); // ✅ 起動時に明示的にセーブデータをロード
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// アプリ終了時のセーブ処理
    /// </summary>
    // SaveManager.cs にこのまま書くのがベスト！
    private void OnApplicationQuit()
    {
        Debug.Log("📴 SaveManager: OnApplicationQuit() 実行");

        if (SaveDataInstance != null)
        {
            Save();
        }
        else
        {
            Debug.LogWarning("⚠ SaveDataInstance が null のため保存をスキップします");
        }
    }


    /// <summary>
    /// データの保存
    /// </summary>
    public void Save()
    {
        if (SaveDataInstance == null)
        {
            Debug.LogError("💥 SaveDataInstance が null のため保存を中止します");
            return;
        }

        SaveDataInstance.lastSaveTime = DateTime.Now;

        PlayerPrefs.SetInt("growthLevel", SaveDataInstance.mermaidGrowthLevel);
        PlayerPrefs.SetFloat("hunger", SaveDataInstance.hungerTimeRemaining);
        PlayerPrefs.SetFloat("pollution", SaveDataInstance.waterPollutionLevel);
        PlayerPrefs.SetInt("days", SaveDataInstance.daysPassed);
        PlayerPrefs.SetString("lastSaveTime", SaveDataInstance.lastSaveTime.ToString());
        PlayerPrefs.SetInt("isWeak", SaveDataInstance.isWeak ? 1 : 0);
        PlayerPrefs.SetString("gameStartTime", SaveDataInstance.gameStartTime.ToString());

        PlayerPrefs.SetString("LastPollutionLog", $"Saved {SaveDataInstance.waterPollutionLevel:F2}% at {DateTime.Now}");


        PlayerPrefs.Save();
        Debug.Log($"💾 セーブ完了！時刻: {SaveDataInstance.lastSaveTime}");
    }

    /// <summary>
    /// データの読み込み
    /// </summary>
    public void Load()
    {
        Debug.Log("📦 SaveManager: ロード処理開始");

        SaveDataInstance = new SaveData();

        SaveDataInstance.mermaidGrowthLevel = PlayerPrefs.GetInt("growthLevel", 1);
        SaveDataInstance.hungerTimeRemaining = PlayerPrefs.GetFloat("hunger", 345600f);
        SaveDataInstance.waterPollutionLevel = PlayerPrefs.GetFloat("pollution", 0f);
        SaveDataInstance.daysPassed = PlayerPrefs.GetInt("days", 0);

        string savedTime = PlayerPrefs.GetString("lastSaveTime", "");
        if (!string.IsNullOrEmpty(savedTime) && DateTime.TryParse(savedTime, out var parsed))
        {
            SaveDataInstance.lastSaveTime = parsed;
        }
        else
        {
            Debug.LogWarning($"⚠ lastSaveTime の読み込みに失敗 → 現在時刻を使用します。savedTime='{savedTime}'");
            SaveDataInstance.lastSaveTime = DateTime.Now;
        }


        string startTime = PlayerPrefs.GetString("gameStartTime", null);
        SaveDataInstance.gameStartTime = DateTime.TryParse(startTime, out var parsedStart) ? parsedStart : DateTime.Now;
        Debug.Log($"🕰️ gameStartTime: {SaveDataInstance.gameStartTime}");

        SaveDataInstance.isWeak = PlayerPrefs.GetInt("isWeak", 0) == 1;

        Debug.Log($"📦 セーブデータ読み込み完了: 経過日数 = {SaveDataInstance.daysPassed}, Hunger = {SaveDataInstance.hungerTimeRemaining:F2}, Pollution = {SaveDataInstance.waterPollutionLevel:F2}");
    }

    /// <summary>
    /// ゲームデータをすべて初期化
    /// </summary>
    public void ResetAllGameState()
    {
        Debug.Log("🔄 SaveManager: ゲームデータの初期化を実行");

        SaveDataInstance = new SaveData();

        Save();
        Debug.Log("✅ ResetAllGameState(): データ初期化完了");
    }
}
