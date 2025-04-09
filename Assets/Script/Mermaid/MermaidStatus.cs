using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Linq;

public class MermaidStatus : MermaidBase
{
    [Header("空腹度設定")]
    [SerializeField] private float maxHunger = 100.0f;

    [SerializeField] private float minHunger = 0f;

    // 4日（345600秒）でゼロになる計算
    [SerializeField] private float hungerDecreaseRate = 100f / 345600f; // ≒ 0.0002893519

    // 現在の満腹度（内部的に操作する）
    [SerializeField] private float hunger = 100.0f;


    [Header("満腹度表示テキスト")]
    public TextMeshProUGUI stomachText;

    [Header("デバッグ表示用：満腹度数値")]
    public TextMeshProUGUI hungerDebugText;

    [Header("水槽の汚れ管理")]
    public WaterManager waterManager;

    private Animator animator;
    private BlendExpression blendExpression;

    [Header("衰弱状態の閾値")]
    public float weakHungerThreshold = 10f;
    public float weakWaterDirtThreshold = 90f;

    [Header("ゲームオーバーUI")]
    public GameObject gameOverPanel;
    public Button returnButton;

    [Header("たまごの状態")]
    public bool isEgg { get; private set; }
    public bool isWeakState = false;

    private Coroutine hungerRoutine; // 空腹コルーチンのハンドル

    private MermaidGrowthManager growthManager;
    private AudioSource audioSource;

    // 🛑 セーブからの読み込みを一時スキップする（ResetEverything後用）
    public static bool SkipLoadFromSaveData = false;

    void Awake()
    {
        blendExpression = GetComponentInChildren<BlendExpression>();
    }

    void Start()
    {
        if (!SkipLoadFromSaveData)
        {
            LoadHungerFromSaveData();
        }
        else
        {
            Debug.Log("🛑 セーブ復元スキップ: ResetEverything からの再生成です");
            SkipLoadFromSaveData = false;
        }

        animator = GetComponentInChildren<Animator>();
        blendExpression = GetComponentInChildren<BlendExpression>();
        waterManager = FindAnyObjectByType<WaterManager>();
        growthManager = FindFirstObjectByType<MermaidGrowthManager>();

        if (growthManager != null)
        {
            UpdateGrowthStage(growthManager.GetCurrentStage());
        }

        UpdateHungerUI(); // 満腹度UI更新（内部で CheckWeakState 呼ぶ）

        if (isWeakState)
        {
            SetWeakState();
            Debug.Log("🔁 起動時にセーブ復元で SetWeakState を再適用しました");
        }

        CheckWeakState(); // 起動直後の再評価

        // ⚠ セーブデータとの整合性を確認し、必要なら復活処理
        if (!isAlive && hunger <= minHunger + 0.01f)
        {
            if (SaveManager.Instance?.SaveDataInstance != null && SaveManager.Instance.SaveDataInstance.isAlive)
            {
                Debug.Log("✅ セーブデータでは生存状態 → 復活処理スキップします");
            }
            else
            {
                Debug.Log("⚠ MermaidStatus: 死亡しており、満腹度がゼロ → 復活処理を実行");

                isAlive = true;
                ResetHunger(100f);        // 満腹度リセット & コルーチン開始
                ResetMermaidStatus();     // 表情など初期化
            }
        }
        else
        {
            Debug.Log("✅ MermaidStatus: 復活処理は不要です（状態維持）");
        }

        // 戻るボタンの初期化
        if (returnButton != null)
        {
            returnButton.onClick.AddListener(ReturnToTitle);
        }

        // 満腹度表示
        if (hungerDebugText != null)
        {
            hungerDebugText.text = $"Hunger: {hunger:F2}%";
        }

        // 🔽 空腹コルーチン開始
        hungerRoutine = StartCoroutine(DecreaseHungerOverTime());
    }






    // ------------------------
    // UIボタン系（デバッグ操作）呼ばれる用
    // ------------------------

    /// <summary>
    /// 満腹度を増減するデバッグ用メソッド
    /// </summary>
    /// <param name="amount">増加量（マイナスも可）</param>
    public void AddHungerDebug(float amount)
    {
        hunger = Mathf.Clamp(hunger + amount, minHunger, maxHunger);
        Debug.Log($"🍚 満腹度を {amount:+0.0;-0.0} 増加（現在: {hunger}％）");

        UpdateHungerUI();

        if (hunger <= minHunger)
        {
            Die();
        }
    }

   




    /// <summary>
    /// 満腹度と水質から衰弱状態を判定し、表情を変更する。
    /// ただし、たまご（Egg）状態では絶対に衰弱状態にはならない。
    /// </summary>
    public void CheckWeakState()
    {
        if (isEgg)
        {
            if (isWeakState)
            {
                isWeakState = false;
                ResetWeakState();
                Debug.Log("🥚 たまごなので衰弱表情をリセットします");
            }
            return;
        }

        if (waterManager == null) return;

        bool isWeak = hunger <= weakHungerThreshold || waterManager.DirtPercentage >= weakWaterDirtThreshold;

        if (isWeak && !isWeakState)
        {
            isWeakState = true;
            SetWeakState();
            Debug.Log("🧪 判定結果: 衰弱状態に入りました（満腹度 or 水質）");
        }
        else if (!isWeak && isWeakState)
        {
            isWeakState = false;
            ResetWeakState();
            Debug.Log("🧪 判定結果: 衰弱状態から回復しました");
        }
        else
        {
            
        }
    }


    private IEnumerator LateStart()
    {
        yield return new WaitForEndOfFrame();
        blendExpression = GetComponentInChildren<BlendExpression>();
    }
    /// <summary>
    /// 満腹度を一定量ずつ減少させる（加速に非対応、汚れと同方式）
    /// </summary>
    private IEnumerator DecreaseHungerOverTime()
    {
        Debug.Log("🍽 空腹コルーチン開始！");

        while (isAlive && hunger > minHunger)
        {
            // 毎秒一定量だけ減少（加速の影響を受けない）
            yield return new WaitForSeconds(1f);

            hunger = Mathf.Clamp(hunger - hungerDecreaseRate, minHunger, maxHunger);

           

            UpdateHungerUI();

            if (hunger <= minHunger + 0.01f)
            {
                Die();
            }
        }

        Debug.Log("🍽 空腹コルーチン終了！");
    }






    private void Update()
    {
        CheckWeakState();

        if (growthManager != null)
        {
            string currentStage = growthManager.GetCurrentStage();
            isEgg = (currentStage == "Egg");
        }
    }

    /// <summary>
    /// 人魚の状態を初期化（タイトルに戻った直後や復活時に呼ぶ）
    /// </summary>
  

    public void ResetMermaidStatus()
    {
        isAlive = true;
        isWeakState = false;

        UpdateAnimator();      // 🔄 Animatorと表情制御の再取得
        ResetWeakState();      // 🧼 弱り表情リセット
        Debug.Log("🔁 ResetMermaidStatus(): 人魚の状態を初期化しました");
    }





    /// <summary>
    /// 満腹度テキストを更新する（0%以下を特別に表示）
    /// </summary>
    public void UpdateHungerUI()
    {
       

        if (stomachText != null)
        {
            if (hunger <= 0.5f)
            {
                stomachText.text = "まんぷくど：0%";
            }
            else if (hunger < 1f)
            {
                stomachText.text = "まんぷくど：1%";
            }
            else
            {
                stomachText.text = $"まんぷくど：{hunger:F2}%";
            }
        }
        

        if (hungerDebugText != null)
        {
            hungerDebugText.text = $"Hunger: {hunger:F2}%";
        }

    }



    /// <summary>
    /// ごはんを食べたときの処理
    /// 衰弱中も表情を維持し、食後に状態再判定を行います
    /// </summary>
    public void EatFood(GameObject food)
    {
        if (isEgg)
        {
            Debug.Log("🥚 `Egg` 状態なので、ごはんを食べられません！");
            return;
        }

        Debug.Log("🍽 ごはんを食べました！");

        MermaidMovement movement = GetComponent<MermaidMovement>();
        if (movement != null)
        {
            movement.PlayEatAnimation(); // 🍴 食事アニメーション再生
        }

        UpdateHunger(10f); // 🍚 満腹度を増加
        UpdateHungerUI();  // UI反映

        // ✅ 衰弱中ならアニメ中でも衰弱顔を即適用
        if (isWeakState && blendExpression != null)
        {
            blendExpression.SetWeakExpression();
            
        }

        CheckWeakState(); // 💡 満腹で回復する可能性もあるので再評価

        StartCoroutine(EnsureWeakExpressionAfterAnimation()); // 💬 食後も状態に応じた表情に戻す
    }


    /// <summary>
    /// 食事アニメーション終了後に状態を再判定し、
    /// 衰弱中であれば表情を再適用します
    /// </summary>
    private IEnumerator EnsureWeakExpressionAfterAnimation()
    {
        yield return new WaitForSeconds(1.5f); // 🍴 アニメーションが終わるのを待つ

        CheckWeakState(); // ✅ 表情適用前に再チェック（状態変化に対応）

        if (isWeakState && blendExpression != null)
        {
            blendExpression.SetWeakExpression();
            
        }
    }

    public override void UpdateHunger(float amount)
    {
        hunger = Mathf.Clamp(hunger + amount, minHunger, maxHunger);
        UpdateHungerUI(); // UIに反映も忘れず！
    }



    public override void Die()
    {
        Debug.Log("🐟 Die() に入りました");

        if (!isAlive)
        {
            Debug.Log("⚠ Die(): すでに死亡済みのため処理スキップ");
            return;
        }

        isAlive = false;
        Debug.Log("❌ 人魚が死亡しました！Status");
        // 👇 人魚を非表示にする
        gameObject.SetActive(false);

        MenuController menuController = FindFirstObjectByType<MenuController>();
        if (menuController != null)
        {
            Debug.Log("🧩 MenuController を発見 → TriggerGameOver を呼びます");
            menuController.TriggerGameOver(); // ここで UI 表示のみ
        }
        else
        {
            Debug.LogError("❌ MenuController が null → ゲームオーバー表示できず");
        }

       
    }



    private IEnumerator DelayedReset()
    {
        yield return new WaitForSeconds(2f); // UIを表示する時間を確保

        if (GameManager.Instance != null)
        {
            GameManager.Instance.ResetEverything();
        }

        Destroy(gameObject);
    }



    private void OnDisable()
{
    // GameManagerが初期化中（タイトル復帰中）ならセーブしない
    if (!isAlive || GameManager.Instance == null || GameManager.Instance.IsResetting)
    {
        Debug.Log("🛑 OnDisable(): 死亡 or 初期化中のためセーブスキップ");
        return;
    }

    SaveHungerToSaveData();
}





    public void ReturnToTitle()
    {
        SceneManager.LoadScene("Title");
    }

    public void UpdateGrowthStage(string newStage)
    {
        isEgg = newStage == "Egg";
        Debug.Log($"🐣 `isEgg` の値を更新: {isEgg}");

        // 一度フラグをリセットしてから再チェック（強制的に表情再判定させる）
        isWeakState = false;
        ResetWeakState();

        CheckWeakState();
    }


    public void OnGrowthStageChanged(string newStage)
    {
        bool wasEgg = isEgg;
        isEgg = (newStage == "Egg");

        if (wasEgg && !isEgg)
        {
            Debug.Log("🐣 `Egg` から成長しました！");
        }
        else if (!wasEgg && isEgg)
        {
            Debug.Log("🥚 `Egg` に戻りました！");
        }

        UpdateHungerUI();
    }

    public void UpdateAnimator()
    {
        animator = GetComponentInChildren<Animator>();
        blendExpression = GetComponentInChildren<BlendExpression>();
        Debug.Log("🔄 `Animator` と `BlendExpression` を更新しました！");
    }

    public void SetWeakState()
    {
        animator = GetComponentInChildren<Animator>();
        blendExpression = GetComponentInChildren<BlendExpression>();

        if (animator != null)
        {
            Debug.Log("⚠ `WeakTrigger` を発火！");
            animator.SetTrigger("WeakTrigger");
        }

        if (blendExpression != null)
        {
            blendExpression.SetWeakExpression();
        }
    }

    public float GetCurrentHunger()
    {
        return hunger;
    }

    public void SetHunger(float value)
    {
        hunger = Mathf.Clamp(value, minHunger, maxHunger);
        UpdateHungerUI();
    }



    public void ResetWeakState()

    {
        animator = GetComponentInChildren<Animator>();
        blendExpression = GetComponentInChildren<BlendExpression>();

        if (animator != null)
        {
            if (animator.parameters.Any(p => p.name == "WeakTrigger"))
            {
                animator.ResetTrigger("WeakTrigger");
            }
            animator.Play("Idle");
        }

        if (blendExpression != null)
        {
            blendExpression.ResetExpression();
        }
    }

    public void ResetHunger(float value)
    {
        hunger = Mathf.Clamp(value, minHunger, maxHunger);
        isAlive = true;

        if (hungerRoutine != null)
        {
            StopCoroutine(hungerRoutine);
        }
       
        hungerRoutine = StartCoroutine(DecreaseHungerOverTime());

        
        CheckWeakState();
        UpdateHungerUI();
        Debug.Log($"🟢 ResetHunger(): 満腹度を {hunger}% に初期化しました");
    }







    private void OnApplicationFocus(bool focus)
    {
        if (!focus)
        {
            SaveHungerToSaveData();
        }
    }

    private void OnApplicationQuit()
    {
        SaveHungerToSaveData();
    }

    private void SaveHungerToSaveData()
    {
        if (SaveManager.Instance == null || SaveManager.Instance.SaveDataInstance == null)
        {
            Debug.LogWarning("⚠ SaveManager または SaveDataInstance が null のため、セーブをスキップします");
            return;
        }

        SaveManager.Instance.SaveDataInstance.isAlive = isAlive;
        SaveManager.Instance.SaveDataInstance.hungerTimeRemaining = hunger;
        SaveManager.Instance.Save();
        Debug.Log($"💾 空腹度をセーブデータに保存しました: {hunger}%");
    }


    private void LoadHungerFromSaveData()
    {
        if (SaveManager.Instance == null || SaveManager.Instance.SaveDataInstance == null)
        {
            Debug.LogWarning("⚠ SaveManagerが未初期化のため、空腹度ロードをスキップします");
            return;
        }

        isAlive = SaveManager.Instance.SaveDataInstance.isAlive;
        hunger = Mathf.Clamp(SaveManager.Instance.SaveDataInstance.hungerTimeRemaining, minHunger, maxHunger);
        isWeakState = SaveManager.Instance.SaveDataInstance.isWeak;

        Debug.Log($"📦 mermaidSt.csセーブデータから空腹度を読み込みました: {hunger}%");
        Debug.Log($"📦 mermaidSt.csセーブデータから衰弱状態を復元しました: {isWeakState}");
    }




}
