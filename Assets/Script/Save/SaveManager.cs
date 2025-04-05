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
            DontDestroyOnLoad(gameObject);
            Debug.Log("💾 SaveManager が初期化されました");

            Load(); // ✅ 起動時に明示的にセーブデータをロード
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// データの保存
    /// </summary>
    public void Save()
    {
        if (SaveDataInstance == null)
        {
            Debug.LogWarning("💾 Save() 呼び出し時に SaveDataInstance が null → 初期化します");
            SaveDataInstance = new SaveData();
        }

        SaveDataInstance.lastSaveTime = DateTime.Now;

        PlayerPrefs.SetInt("growthLevel", SaveDataInstance.mermaidGrowthLevel);
        PlayerPrefs.SetFloat("hunger", SaveDataInstance.hungerTimeRemaining);
        PlayerPrefs.SetFloat("pollution", SaveDataInstance.waterPollutionLevel);
        PlayerPrefs.SetInt("days", SaveDataInstance.daysPassed);
        PlayerPrefs.SetString("lastSaveTime", SaveDataInstance.lastSaveTime.ToString());
        PlayerPrefs.SetInt("isWeak", SaveDataInstance.isWeak ? 1 : 0);

        PlayerPrefs.Save();
        Debug.Log($"💾 セーブ完了！時刻: {SaveDataInstance.lastSaveTime}");
    }

    /// <summary>
    /// データの読み込み
    /// </summary>
    public void Load()
    {
        Debug.Log("📦 SaveManager: ロード処理開始");

        SaveDataInstance = new SaveData(); // ✅ このタイミングで新規インスタンス化

        SaveDataInstance.mermaidGrowthLevel = PlayerPrefs.GetInt("growthLevel", 1);
        SaveDataInstance.hungerTimeRemaining = PlayerPrefs.GetFloat("hunger", 345600f);
        SaveDataInstance.waterPollutionLevel = PlayerPrefs.GetFloat("pollution", 0f);
        SaveDataInstance.daysPassed = PlayerPrefs.GetInt("days", 0);

        string savedTime = PlayerPrefs.GetString("lastSaveTime", null);
        SaveDataInstance.lastSaveTime = DateTime.TryParse(savedTime, out var parsed) ? parsed : DateTime.Now;

        SaveDataInstance.isWeak = PlayerPrefs.GetInt("isWeak", 0) == 1;

        Debug.Log($"📦 セーブデータ読み込み完了: 経過日数 = {SaveDataInstance.daysPassed}, Hunger = {SaveDataInstance.hungerTimeRemaining:F2}, Pollution = {SaveDataInstance.waterPollutionLevel:F2}");
    }

    /// <summary>
    /// ゲームデータをすべて初期化
    /// </summary>
    public void ResetAllGameState()
    {
        Debug.Log("🔄 SaveManager: ゲームデータの初期化を実行");

        SaveDataInstance = new SaveData(); // ✅ ここでは明示的に新規作成

        Save();
        Debug.Log("✅ ResetAllGameState(): データ初期化完了");
    }
}
