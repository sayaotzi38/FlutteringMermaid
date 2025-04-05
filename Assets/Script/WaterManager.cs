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
    [SerializeField] private float maxDirtAlpha = 100.0f;
    [SerializeField] private float dirtIncreaseRate = 0.0002314815f;//5日
    [SerializeField] private float currentDirtAlpha = 0f;
    [Header("デバッグ表示用：水質数値")]
    public TextMeshProUGUI dirtDebugText;


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

    public float DirtPercentage => (currentDirtAlpha / maxDirtAlpha) * 100f;
    public float MaxDirtAlpha => maxDirtAlpha;

    private void Start()
    {


        LoadDirtFromSaveData(); // ✅ セーブデータから汚れ度を復元

        if (dirtOverlay != null)
        {
            dirtOverlay.color = new Color(1f, 1f, 1f, 0f);
        }

        if (cleanWaterButton != null)
        {
            cleanWaterButton.onClick.AddListener(CleanWater);
        }

        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;

        if (mermaidStatus == null)
        {
            mermaidStatus = FindAnyObjectByType<MermaidStatus>();
            if (mermaidStatus != null)
            {
                Debug.Log("✅ MermaidStatus を自動取得しました");
            }
            else
            {
                Debug.LogWarning("⚠ MermaidStatus が見つかりません！インスペクターにも設定されていません！");
            }
        }

        

        StartCoroutine(IncreaseDirtOverTime());
    }

    /// <summary>
    /// デバッグ用：水質（汚れ度）を指定した割合だけ増減する
    /// </summary>
    /// <param name="percent">増減する割合（マイナスも可）</param>
    public void AddDirtPercentage(float percent)
    {
        float addAmount = maxDirtAlpha * (percent / 100f);

        // 🔧 修正: 100%以上のときは強制的に最大値にする
        currentDirtAlpha = Mathf.Clamp(currentDirtAlpha + addAmount, 0f, maxDirtAlpha);

        // ここ追加！強制的に100%にするオプション
        if (percent >= 100f)
        {
            currentDirtAlpha = maxDirtAlpha;
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

    private void CleanWater()
    {
        Debug.Log("🧼 水替えを実行！ 汚れをリセットします。");

        currentDirtAlpha = 0f;
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
        Debug.Log($"🖌 汚れの透明度を {newAlpha} に設定_WMcs");

        currentDirtAlpha = Mathf.Clamp(newAlpha, 0f, maxDirtAlpha);
        UpdateDirtAlpha();

        // 💀 追加：汚れが最大になったら死亡チェックを実行
        CheckAndKillMermaidIfNeeded("SetDirtAlpha() により最大汚れに到達 → 死亡処理を実行");
    }



    private IEnumerator IncreaseDirtOverTime()
    {
        while (currentDirtAlpha < maxDirtAlpha)
        {
            float waitTime = SaveManager.isDebugSpeed ? 1f / SaveManager.debugTimeScale : 1f;
            yield return new WaitForSeconds(waitTime);

            currentDirtAlpha = Mathf.Clamp(currentDirtAlpha + dirtIncreaseRate, 0f, maxDirtAlpha);
            UpdateDirtAlpha();
            CheckAndKillMermaidIfNeeded("⚠ 汚れが 100% になりました！人魚は死んでしまいます...");
        }
    }


    /// <summary>
    /// 汚れが最大値に達したかを確認し、人魚を殺す処理を一元化
    /// </summary>
    public void CheckAndKillMermaidIfNeeded(string logMessage)
    {
        if (currentDirtAlpha >= maxDirtAlpha - 0.01f && !isMermaidKilled)
        {
            Debug.Log("⚠ 汚れが 100% になりました → 死亡トリガー発動");
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
        if (dirtOverlay != null)
        {
            float normalizedAlpha = Mathf.Clamp01(currentDirtAlpha / maxDirtAlpha);
            dirtOverlay.color = new Color(1f, 1f, 1f, normalizedAlpha);
        }

        if (dirtStatusText != null)
        {
            dirtStatusText.text = $"よごれ: {DirtPercentage:F2}%";
        }

        if (dirtDebugText != null)
        {
            dirtDebugText.text = $"Dirt: {DirtPercentage:F2}%";
        }
    }




    /// <summary>
    /// 水質（汚れ度）を初期状態にリセット
    /// </summary>
    public void ResetWaterQuality()
    {
        Debug.Log("🧼 ResetWaterQuality(): 水質を初期状態にリセットします");
        currentDirtAlpha = 0f;
        UpdateDirtAlpha();
        SaveDirtToSaveData();
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
        if (SaveManager.Instance == null)
        {
            Debug.LogWarning("⚠ SaveManager.Instance が null です。水質の保存をスキップします。");
            return;
        }

        SaveManager.Instance.SaveDataInstance.waterPollutionLevel = currentDirtAlpha;
        SaveManager.Instance.Save();

        Debug.Log($"💾 水質をセーブしました: {currentDirtAlpha}");
    }

    private void LoadDirtFromSaveData()
    {
        if (GameManager.Instance == null || GameManager.Instance.SaveManagerInstance == null)
        {
            Debug.LogWarning("⚠ GameManagerが未初期化のため、水質ロードをスキップします");
            return;
        }

        currentDirtAlpha = Mathf.Clamp(
            GameManager.Instance.SaveManagerInstance.SaveDataInstance.waterPollutionLevel,
            0f, maxDirtAlpha);

        UpdateDirtAlpha();
        Debug.Log($"📦 水質をロードしました: {currentDirtAlpha}");
        SaveManager.Instance.SaveDataInstance.waterPollutionLevel = currentDirtAlpha;

        if (mermaidStatus != null)
        {
            mermaidStatus.CheckWeakState();
            Debug.Log("🔁 水質読み込み後に CheckWeakState() を呼び出しました");
        }
    }

    private void SyncPollutionFromSave()
    {
        currentDirtAlpha = Mathf.Clamp(
            SaveManager.Instance.SaveDataInstance.waterPollutionLevel,
            0f, maxDirtAlpha);

        UpdateDirtAlpha();
        Debug.Log($"🔁 SaveData から currentDirtAlpha を同期しました: {currentDirtAlpha}");
    }


    /// <summary>
    /// 水質を初期化し、汚れ増加コルーチンを再開
    /// </summary>
    public void ResetPollution()
    {
        currentDirtAlpha = 0f;
        isMermaidKilled = false;

        UpdateDirtAlpha();
        SaveDirtToSaveData();

        StopAllCoroutines(); // 念のため、既存のコルーチンを止める
        StartCoroutine(IncreaseDirtOverTime()); // 再スタート

        Debug.Log("🧼 ResetPollution(): 水質をリセットし、コルーチンを再開しました");
    }


}
