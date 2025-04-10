using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 水槽の汚れ管理クラス（3D モバイル UI 対応版）
/// </summary>
public class WaterManager : MonoBehaviour

{
    [Header("人魚の状態スクリプト")]
    [SerializeField] private MermaidStatus mermaidStatus;

    [Header("汚れのUI設定")]
    [SerializeField] private Image dirtOverlay;

    [Header("汚れ増加設定")]
    private float maxDirtAlpha = 100.0f;
     private float dirtIncreaseRate = 0.0002314815f;//5日

    [Header("UI設定")]
    [SerializeField] private Button cleanWaterButton;
    [SerializeField] private TextMeshProUGUI dirtStatusText;

    [Header("掃除エフェクト設定")]
    [SerializeField] private GameObject waterSplashPrefab;
    [SerializeField] private Transform effectSpawnPoint;
    [SerializeField] private AudioClip[] cleaningSounds;
    private AudioSource audioSource;

    [Header("泡の設定")]
    [SerializeField] private float bubbleDuration = 3.0f;
    [SerializeField] private float fadeOutDuration = 1.0f;

    [Header("フラッシュ設定")]
    [SerializeField] private Image flashImage;
    [SerializeField] private float flashDuration = 0.5f;



    public float DirtPercentage => (SaveManager.Instance.SaveDataInstance.waterPollutionLevel / maxDirtAlpha) * 100f;

    public float MaxDirtAlpha => maxDirtAlpha;
    void Awake()
    {
        Debug.Log("🐣 WaterManager.Awake() 呼び出されました");

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                Debug.LogWarning("⚠ AudioSource がこの GameObject に存在しません。インスペクターで追加してください");
            }
        }
    }



    void OnEnable()
    {
        Debug.Log("📌 WaterManager.OnEnable() 呼び出されました");
    }

    private IEnumerator MyStart()
    {
        Debug.Log("🔍 MyStart() 開始：GameManager, SaveManagerInstance, SaveDataInstance の準備を確認");

        float timeout = 5f; // 5秒以内に準備が完了しない場合は中断
        float elapsed = 0f;

        while (elapsed < timeout)
        {
            bool ready = GameManager.Instance != null &&
                         GameManager.Instance.SaveManagerInstance != null &&
                         GameManager.Instance.SaveManagerInstance.SaveDataInstance != null;

            Debug.Log($"🧪 条件確認: GM={GameManager.Instance != null}, SM={GameManager.Instance?.SaveManagerInstance != null}, SD={GameManager.Instance?.SaveManagerInstance?.SaveDataInstance != null}");

            if (ready)
            {
                Debug.Log("🌊 条件クリア → MyStart() 続行");
                break;
            }

            yield return new WaitForSeconds(0.5f);
            elapsed += 0.5f;
        }

        if (elapsed >= timeout)
        {
            Debug.LogError("❌ MyStart(): 初期化タイムアウト → 初期化を中止します");
            yield break;
        }

        yield return new WaitForSeconds(0.1f);
        LoadDirtFromSaveData();
        StartCoroutine(IncreaseDirtOverTime());
    }







    /// <summary>
    /// デバッグ用：水質（汚れ度）を指定した割合だけ増減する
    /// </summary>
    /// <param name="percent">増減する割合（マイナスも可）</param>
    public void AddDirtPercentage(float percent)
    {
        var saveData = SaveManager.Instance.SaveDataInstance;

        if (saveData == null)
        {
            Debug.LogWarning("⚠ SaveDataInstance が null のため、AddDirtPercentage をスキップします");
            return;
        }

        float addAmount = maxDirtAlpha * (percent / 100f);

        // 加算してClamp
        saveData.waterPollutionLevel = Mathf.Clamp(
            saveData.waterPollutionLevel + addAmount,
            0f,
            maxDirtAlpha);

        // 100%以上なら強制的に最大に
        if (percent >= 100f)
        {
            saveData.waterPollutionLevel = maxDirtAlpha;
        }

        Debug.Log($"🧪 汚れを {percent:+0.0;-0.0}% 増減 → 現在: {DirtPercentage:F1}%");

        UpdateDirtAlpha();
        CheckAndKillMermaidIfNeeded("⚠ デバッグ操作により汚れが 100% に到達しました");
    }




    /// <summary>
    /// 指定秒数後にインタースティシャル広告を表示
    /// </summary>
    private IEnumerator ShowInterstitialAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        Debug.Log("⏰ 2秒経過 → 広告表示します");
        AdmobLibrary.PlayInterstitial();
    }

    public void CleanWater()
    {
        Debug.Log("🧼 水替えを実行！ 汚れをリセットします。");

        var saveData = SaveManager.Instance != null ? SaveManager.Instance.SaveDataInstance : null;
        if (saveData == null)
        {
            Debug.LogWarning("⚠ SaveDataInstance が null のため、水質リセットをスキップします");
            return;
        }

        saveData.waterPollutionLevel = 0f;
        UpdateDirtAlpha();

        // フラグ初期化（毎回掃除のたびにリセット）
        adShownAfterCleaning = false;

        if (waterSplashPrefab != null && effectSpawnPoint != null)
        {
            Debug.Log($"🌊 波紋エフェクトを生成！場所: {effectSpawnPoint.position}");
            GameObject splash = Instantiate(waterSplashPrefab, effectSpawnPoint.position, Quaternion.identity);

            ParticleSystem ps = splash.GetComponent<ParticleSystem>();
            if (ps != null) ps.Play();
            else Debug.LogError("⚠️ `WaterSplashEffect` に `ParticleSystem` がアタッチされていません！");

            StartCoroutine(FadeOutBubble(splash));
        }

        if (cleaningSounds.Length > 0)
        {
            int randomIndex = Random.Range(0, cleaningSounds.Length);
            audioSource.PlayOneShot(cleaningSounds[randomIndex]);
        }

        StartCoroutine(FlashEffect());

        StopCoroutine(IncreaseDirtOverTime());
        StartCoroutine(IncreaseDirtOverTime());
    }




    private IEnumerator FadeOutBubble(GameObject bubble)
    {
        if (this == null || gameObject == null)
        {
            Debug.LogWarning("⚠ WaterManager は既に破棄されています。バブル演出を中止します");
            yield break;
        }

        ParticleSystem bubbleParticle = bubble.GetComponent<ParticleSystem>();
        if (bubbleParticle == null)
        {
            Debug.LogError("⚠️ `WaterSplashEffect` に `ParticleSystem` がアタッチされていません！");
            yield break;
        }

        var mainModule = bubbleParticle.main;
        mainModule.loop = false;

        yield return new WaitForSeconds(bubbleDuration); // 演出時間待ち

        bubbleParticle.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        Destroy(bubble, fadeOutDuration);

        if (!adShownAfterCleaning)
        {
            Debug.Log("✨ 泡の演出終了 → 広告を読み込み、完了後に表示します");

            AdmobLibrary.OnLoadedInterstitial = () =>
            {
                Debug.Log("📦 インタースティシャル広告が読み込まれました → 表示します");

                if (this != null && gameObject != null)
                {
                    StartCoroutine(AdmobLibrary.PlayInterstitialDelayed(1f));
                    adShownAfterCleaning = true; // ✅ ここでフラグを立てる
                }
                else
                {
                    Debug.LogWarning("⚠ WaterManager が既に破棄されていたため、広告表示をスキップしました");
                }
            };

            AdmobLibrary.RequestInterstitial();
        }
    }


    private IEnumerator FlashEffect()
    {
        if (flashImage == null)
        {
            Debug.LogError("⚠️ `FlashImage` が設定されていません！");
            yield break;
        }

        float elapsedTime = 0f;
        Color flashColor = flashImage.color;

        while (elapsedTime < flashDuration / 2)
        {
            float alpha = Mathf.Lerp(0, 1, elapsedTime / (flashDuration / 2));
            flashImage.color = new Color(flashColor.r, flashColor.g, flashColor.b, alpha);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        flashImage.color = new Color(flashColor.r, flashColor.g, flashColor.b, 1f);

        elapsedTime = 0f;
        while (elapsedTime < flashDuration / 2)
        {
            float alpha = Mathf.Lerp(1, 0, elapsedTime / (flashDuration / 2));
            flashImage.color = new Color(flashColor.r, flashColor.g, flashColor.b, alpha);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        flashImage.color = new Color(flashColor.r, flashColor.g, flashColor.b, 0f);
    }

    private bool isMermaidKilled = false;

    // インタースティシャル広告を掃除後に一度だけ表示するためのフラグ
    private bool adShownAfterCleaning = false;



    /// <summary>
    /// 外部から直接汚れ度を指定値に設定するためのメソッド
    /// </summary>
    /// <param name="newAlpha">設定する汚れ度（0～100の範囲）</param>
    public void SetDirtAlpha(float newAlpha)
    {
        var saveData = SaveManager.Instance != null ? SaveManager.Instance.SaveDataInstance : null;
        if (saveData == null)
        {
            Debug.LogWarning("⚠ SaveDataInstance が null のため、SetDirtAlpha をスキップします");
            return;
        }

        saveData.waterPollutionLevel = Mathf.Clamp(newAlpha, 0f, 100f);
        Debug.Log($"🖌 汚れを {saveData.waterPollutionLevel:F3}% に設定しました");

        UpdateDirtAlpha();

        // 💀 汚れが最大になったら死亡チェックを実行
        CheckAndKillMermaidIfNeeded("SetDirtAlpha() により最大汚れに到達 → 死亡処理を実行");
    }




    private IEnumerator IncreaseDirtOverTime()
    {
        Debug.Log("📈 IncreaseDirtOverTime() コルーチン開始");

        yield return new WaitForSeconds(1.0f); // 最初の1秒は変更なし

        var saveData = SaveManager.Instance != null ? SaveManager.Instance.SaveDataInstance : null;
        if (saveData == null)
        {
            Debug.LogWarning("⚠ IncreaseDirtOverTime(): SaveDataInstance が null のため中断します");
            yield break;
        }

        while (saveData.waterPollutionLevel < 100f)
        {
            float waitTime = SaveManager.isDebugSpeed ? 1f / SaveManager.debugTimeScale : 1f;
            yield return new WaitForSeconds(waitTime);

            saveData.waterPollutionLevel = Mathf.Clamp(
                saveData.waterPollutionLevel + dirtIncreaseRate,
                0f, 100f);

            Debug.Log($"🔄 汚れ増加中: {saveData.waterPollutionLevel:F4}");

            UpdateDirtAlpha();
            CheckAndKillMermaidIfNeeded("⚠ 汚れが 100% になりました！人魚は死んでしまいます...");
        }

        Debug.Log("✅ 汚れが 100% に達したためコルーチン終了");
    }




    /// <summary>
    /// 汚れが最大値に達したかを確認し、人魚を殺す処理を一元化
    /// </summary>
    public void CheckAndKillMermaidIfNeeded(string logMessage)
    {
        var saveData = SaveManager.Instance != null ? SaveManager.Instance.SaveDataInstance : null;
        if (saveData == null)
        {
            Debug.LogWarning("⚠ CheckAndKillMermaidIfNeeded(): セーブデータが null のため中断");
            return;
        }

        if (saveData.waterPollutionLevel >= 100f - 0.01f && !isMermaidKilled)
        {
            Debug.Log(logMessage);
            KillMermaid();
        }
    }


    public void KillMermaid()
    {
        if (mermaidStatus == null)
        {
            mermaidStatus = FindFirstObjectByType<MermaidStatus>();
            Debug.LogWarning("🔍 KillMermaid(): MermaidStatus を再取得しました");
        }

        isMermaidKilled = true;

        if (mermaidStatus != null)
        {
            Debug.Log("🧩 KillMermaid(): mermaidStatus は null ではありません → Die() 呼びます");
            mermaidStatus.Die(); // ★ ここが動くかチェック！
            Debug.Log("✅ KillMermaid(): Die() 呼び出し完了");
        }
        else
        {
            Debug.LogError("❌ `MermaidStatus` が見つかりません！");
        }
    }




    private void UpdateDirtAlpha()
    {
        float current = SaveManager.Instance.SaveDataInstance.waterPollutionLevel;
        Debug.Log($"🌊 UpdateDirtAlpha() 実行: current = {current}, DirtPercentage = {DirtPercentage:F3}%");

        if (dirtOverlay != null)
        {
            float normalizedAlpha = Mathf.Clamp01(current / maxDirtAlpha);
            dirtOverlay.color = new Color(1f, 1f, 1f, normalizedAlpha);
        }

        if (dirtStatusText != null)
        {
            dirtStatusText.text = $"よごれ: {DirtPercentage:F3}%";
        }
    }





    /// <summary>
    /// 水質（汚れ度）を初期状態にリセット
    /// </summary>
    public void ResetWaterQuality()
    {
        Debug.Log("🧼 ResetWaterQuality(): 水質を初期状態にリセットします");

        var saveData = SaveManager.Instance != null ? SaveManager.Instance.SaveDataInstance : null;
        if (saveData == null)
        {
            Debug.LogWarning("⚠ セーブデータが null のため、水質リセットをスキップします");
            return;
        }

        saveData.waterPollutionLevel = 0f;
        UpdateDirtAlpha();
        SaveManager.Instance.Save();
    }



    // 🔽 セーブ・ロード処理

    private void OnApplicationQuit()
    {
        SaveDirtToSaveData();
    }

    private void OnApplicationFocus(bool focus)
    {
        if (!focus)
        {
            SaveDirtToSaveData();
        }
    }

    private void SaveDirtToSaveData()
    {
        if (SaveManager.Instance == null || SaveManager.Instance.SaveDataInstance == null)
        {
            Debug.LogWarning("⚠ SaveManager.Instance または SaveDataInstance が null です。水質の保存をスキップします。");
            return;
        }

        SaveManager.Instance.Save(); // すでに waterPollutionLevel を使用中なのでそのままセーブ
        Debug.Log($"💾 水質をセーブしました: {SaveManager.Instance.SaveDataInstance.waterPollutionLevel}");
    }


    public void LoadDirtFromSaveData()
    {
        if (GameManager.Instance == null || GameManager.Instance.SaveManagerInstance == null)
        {
            Debug.LogWarning("⚠ LoadDirtFromSaveData(): GameManagerが未初期化のためスキップ");
            return;
        }

        var saveData = GameManager.Instance.SaveManagerInstance.SaveDataInstance;
        if (saveData == null)
        {
            Debug.LogWarning("⚠ LoadDirtFromSaveData(): SaveDataInstance が null のためスキップ");
            return;
        }

        float savedPollution = Mathf.Clamp(saveData.waterPollutionLevel, 0f, maxDirtAlpha);
        saveData.waterPollutionLevel = savedPollution;

        Debug.Log($"📦 SaveData から読み込んだ水質: {savedPollution}");

        UpdateDirtAlpha();

        Debug.Log($"🧪 現在の水質: {saveData.waterPollutionLevel}, DirtPercentage = {DirtPercentage}%");
    }





    /// <summary>
    /// 水質を初期化し、汚れ増加コルーチンを再開
    /// </summary>
    public void ResetPollution()
    {
        if (SaveManager.Instance?.SaveDataInstance == null)
        {
            Debug.LogWarning("⚠ ResetPollution(): セーブデータが未初期化のためスキップします");
            return;
        }

        SaveManager.Instance.SaveDataInstance.waterPollutionLevel = 0f;
        isMermaidKilled = false;

        UpdateDirtAlpha();
        SaveManager.Instance.Save();

        StopAllCoroutines(); // 念のため、既存のコルーチンを止める
        StartCoroutine(IncreaseDirtOverTime()); // 再スタート

        Debug.Log("🧼 ResetPollution(): 水質をリセットし、コルーチンを再開しました");
    }



}
