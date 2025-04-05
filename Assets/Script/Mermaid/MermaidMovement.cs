using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

/// <summary>
/// **人魚の動きを管理**
/// </summary>
public class MermaidMovement : MermaidBase
{
    private bool isMovingToTap = false; // ✅ タップ移動中かどうかのフラグ
    private Vector2 targetPosition; // ✅ 移動ターゲットの座標
    private float tapMoveEndTime = 0f; // ✅ タップ移動の終了時間

    [Header("移動速度")]
    public float swimSpeed = 1f;

    [Header("アニメーション管理")]
    private Animator animator; // ✅ 追加！Animator 変数を定義
    private bool isFacingRight = true; // ✅ 右を向いているかどうか

    [Header("成長管理")]
    [SerializeField] public MermaidGrowthManager growthManager; // ✅ public にする（Inspector で設定も可）


    [Header("ターゲットの到達判定距離")]
    public float targetThreshold = 0.5f;

    private GameObject foodTargetObject; // 🎯 現在狙っているごはん


    private void HandleTap()
    {
        if (Input.GetMouseButtonDown(0)) // ✅ タップしたら
        {
            Vector3 tapPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            tapPosition.z = 0;

          

            targetPosition = tapPosition;
            isMovingToTap = true;
        }
    }

    void Start()
    {
        

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>(); // ✅ Animator を取得！

        if (growthManager != null && growthManager.IsEgg()) // ✅ `卵` のとき
        {
            
            if (rb != null)
            {
                rb.bodyType = RigidbodyType2D.Static; // ✅ 物理演算を完全に停止（動かないようにする）
            }
        }
        else
        {
            if (rb != null) rb.bodyType = RigidbodyType2D.Dynamic; // ✅ 成長したら動けるようにする
        }

        animator = GetComponentInChildren<Animator>(); // ✅ `Animator` を取得（子オブジェクト含む）
        if (animator == null)
        {
            Debug.LogError("❌ `Animator` が見つかりません！ 正しいオブジェクトにアタッチされていますか？");
        }
    }

    void Update()
    {
        if (growthManager != null && growthManager.IsEgg())
        {
            

            if (animator != null)
            {
                animator.SetBool("IsIdle", true); // ✅ `Idle` アニメーションを強制適用
            }
            return; // 🚫 以降の移動処理をスキップ（アニメーションだけ動く）
        }

        HandleTap();

        if (foodTargetObject == null && GameObject.FindGameObjectWithTag("Food") != null)
        {
            FindFoodTarget();
        }

        Vector2 moveTarget = Vector2.zero;
        bool shouldMove = false;

        if (foodTargetObject != null)
        {
            moveTarget = foodTargetObject.transform.position;
            shouldMove = true;
            isMovingToTap = false;
        }
        else if (isMovingToTap)
        {
            moveTarget = targetPosition;
            shouldMove = true;
            if (Time.time >= tapMoveEndTime || Vector2.Distance(transform.position, targetPosition) < 0.5f)
            {
                isMovingToTap = false;
            }
        }
        else
        {
            if (Vector2.Distance(transform.position, targetPosition) < targetThreshold)
            {
                SetRandomTarget();
            }
            moveTarget = targetPosition;
            shouldMove = true;
        }

        if (shouldMove) MoveTowards(moveTarget);
        else
        {
            animator.SetBool("IsSwimming", false); // ✅ 動いていない時は `Idle`
        }
    }



    /// <summary>
    /// **現在の状態が卵 (`0日目`) かどうかを判定**
    /// </summary>
   public bool IsEgg()
{
    return growthManager.CurrentDays == 0;
    }

    /// <summary>
    /// **新しい `Animator` を適用**
    /// </summary>
    public void SetAnimator(Animator newAnimator)
    {
        animator = newAnimator;
        Debug.Log("🎬 `MermaidMovement` の `Animator` を更新しました！");
    }




    /// <summary>
    /// **ランダムな位置をターゲットに設定**
    /// </summary>
    private void SetRandomTarget()
    {
        targetPosition = GetRandomScreenPosition(); // 画面内のランダムな座標を取得
    }

    /// <summary>
    /// **画面内のランダムな位置を取得**
    /// </summary>
    private Vector2 GetRandomScreenPosition()
    {
        float minX = -5f, maxX = 5f; // 画面のX範囲
        float minY = -3f, maxY = 3f; // 画面のY範囲
        float x = Random.Range(minX, maxX);
        float y = Random.Range(minY, maxY);
        return new Vector2(x, y);
    }

    /// <summary>
    /// **最も近い "Food" をターゲットに設定**
    /// </summary>
    private void FindFoodTarget()
    {
       

        GameObject[] foodObjects = GameObject.FindGameObjectsWithTag("Food");

        if (foodObjects.Length == 0)
        {
            foodTargetObject = null;
            return;
        }

        float closestDistance = Mathf.Infinity;
        GameObject closestFood = null;

        foreach (GameObject food in foodObjects)
        {
            if (food == null || !food.activeInHierarchy) continue;

            float distance = Vector2.Distance(transform.position, food.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestFood = food;
            }
        }

        if (closestFood != null)
        {
            foodTargetObject = closestFood;
            Debug.Log($"🍙 最も近い `Food` に向かう: {foodTargetObject.name}");
        }
    }

    /// <summary>
    /// **ターゲットの位置へ向かう**
    /// </summary>
    private void MoveTowards(Vector2 target)
    {
        Vector2 newPosition = Vector2.MoveTowards(transform.position, target, swimSpeed * Time.deltaTime);
        transform.position = newPosition;
        FlipMermaid(target.x);
    }

    /// <summary>
    /// **移動方向によって人魚の向きを反転**
    /// </summary>
    private void FlipMermaid(float targetX)
    {
        bool shouldFaceRight = targetX > transform.position.x;

        if (shouldFaceRight != isFacingRight)
        {
            isFacingRight = shouldFaceRight;
            Vector3 newScale = transform.localScale;
            newScale.x = isFacingRight ? -Mathf.Abs(newScale.x) : Mathf.Abs(newScale.x);
            transform.localScale = newScale;
        }
    }

   /// <summary>
/// **ごはんを食べたらターゲットをリセット**
/// </summary>
private void OnTriggerEnter2D(Collider2D other)
{
    if (other.CompareTag("Food"))
    {
        Debug.Log($"✅ {other.gameObject.name} を食べました！ターゲットリセット");

        if (foodTargetObject == other.gameObject)
        {
            foodTargetObject = null; // ✅ 現在のターゲットをリセット
        }

       

        FindFoodTarget(); // ✅ 新しい `Food` を探す
    }
}


    /// <summary>
    /// **ごはんを食べたときの処理（アニメーションのみ）**
    /// </summary>
    public void PlayEatAnimation()

    { // ✅ ここで `Animator` を更新してから発火
        animator = GetComponentInChildren<Animator>();

        Debug.Log("🎬 `MermaidMovement.PlayEatAnimation()` が呼ばれました！");

        if (animator != null)
        {
            Debug.Log("🎬 `EatTrigger` をリセットして発火します！");
            animator.ResetTrigger("EatTrigger");
            animator.SetTrigger("EatTrigger");
        }
        else
        {
            Debug.LogError("❌ `Animator` が `null` です！モデルが正しく設定されていますか？");
        }
    }



}
