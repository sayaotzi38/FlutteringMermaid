using UnityEngine;

/// <summary>
/// **人魚の見た目を管理するクラス**
/// - `MermaidGrowthManager` から成長データを取得
/// - モデルの `SetActive` で切り替える
/// </summary>
public class MermaidAppearance : MonoBehaviour
{
    [Header("成長段階ごとのモデル")]
    [SerializeField] private GameObject eggStage;
    [SerializeField] private GameObject babyFishStage;
    [SerializeField] private GameObject childStage;
    [SerializeField] private GameObject youngStage;
    [SerializeField] private GameObject teenStage;
    [SerializeField] private GameObject adultStage;
    [SerializeField] private GameObject perfectStage;

    private GameObject currentModel;
    private MermaidGrowthManager growthManager;

    private void Start()
    {
        growthManager = GetComponent<MermaidGrowthManager>();
        if (growthManager == null)
        {
            Debug.LogError("❌ `MermaidGrowthManager` が見つかりません！");
            return;
        }

        UpdateMermaidAppearance(); // ゲーム開始時に見た目を更新
    }

    /// <summary>
    /// **現在の成長段階を取得し、見た目を変更**
    /// </summary>
    private void UpdateMermaidAppearance()
    {
        if (growthManager != null)
        {
            string newStage = growthManager.GetCurrentStage();
            Debug.Log($"🎯AP.CS `UpdateMermaidAppearance()` 実行！ 成長段階: {newStage}");
            ChangeAppearance(newStage);
        }
    }

    /// <summary>
    /// **見た目を更新（`SetActive` を使用）**
    /// </summary>
    public void ChangeAppearance(string growthStage)
    {
        Debug.Log($"🔄 成長段階変更: {growthStage}");

        // MermaidStatus を子オブジェクトから取得
        MermaidStatus status = GetComponentInChildren<MermaidStatus>();
        float savedHunger = 100f;
        bool savedIsWeak = false;

        if (status != null)
        {
            savedHunger = status.GetCurrentHunger();
            savedIsWeak = status.isWeakState;
            Debug.Log($"📥 状態保存: Hunger={savedHunger}, Weak={savedIsWeak}");
        }
        else
        {
            Debug.LogWarning("⚠ MermaidStatus が取得できませんでした（ChangeAppearance）");
        }

        SetActiveAllStages(false);
        GameObject newModel = GetModelForStage(growthStage);
        if (newModel != null)
        {
            newModel.SetActive(true);
            currentModel = newModel;

            if (status != null)
            {
                status.UpdateAnimator();
                status.SetHunger(savedHunger);

                if (savedIsWeak)
                {
                    status.SetWeakState();
                    Debug.Log("🔁 Animator更新後に再度 SetWeakState() を呼び出しました");
                }
                else
                {
                    status.ResetWeakState();
                }

                Debug.Log("✅ 状態を復元しました");
            }
        }
        else
        {
            Debug.LogError($"❌ モデルが見つかりません: {growthStage}");
        }
    }

    private void SetActiveAllStages(bool state)
    {
        if (eggStage) eggStage.SetActive(state);
        if (babyFishStage) babyFishStage.SetActive(state);
        if (childStage) childStage.SetActive(state);
        if (youngStage) youngStage.SetActive(state);
        if (teenStage) teenStage.SetActive(state);
        if (adultStage) adultStage.SetActive(state);
        if (perfectStage) perfectStage.SetActive(state);
    }

    private GameObject GetModelForStage(string stage)
    {
        return stage switch
        {
            "Egg" => eggStage,
            "BabyFish" => babyFishStage,
            "Child" => childStage,
            "Young" => youngStage,
            "Teen" => teenStage,
            "Adult" => adultStage,
            "Perfect" => perfectStage,
            _ => null
        };
    }
}
