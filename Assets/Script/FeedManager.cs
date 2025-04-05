using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using static EggDialogManager;

public class FeedManager : MonoBehaviour
{
    [Header("成長管理")]
    [SerializeField] private MermaidGrowthManager growthManager;

    [Header("ごはんのプレハブ")]
    [SerializeField] private GameObject foodPrefab;

    [Header("ごはんスポーン位置")]
    [SerializeField] private Transform foodSpawnPoint;

    [Header("ごはんボタン")]
    [SerializeField] private Button feedButton;

    [Header("たまごのダイアログ管理")]
    [SerializeField] private EggDialogManager eggDialogManager;

    private bool previousIsEgg;
    private bool isGameOver = false;

    private const string FoodCountKey = "FoodCount";


    void Awake()


    {

        Debug.Log($"🧐 Awake() 実行時の foodPrefab: {foodPrefab}");

        if (feedButton == null)
        {
            feedButton = GameObject.Find("feedButton")?.GetComponent<Button>();
        }

        if (feedButton != null)
        {
            feedButton.onClick.RemoveAllListeners();
            feedButton.onClick.AddListener(HandleFeedButton);
        }

        StartCoroutine(WaitForGrowthManager());
    }

    void OnEnable()
    {
        UpdateButtonAction();
    }

    void Start()
    {
        foodConsumedCount = PlayerPrefs.GetInt(FoodCountKey, 0); // デフォルトは0

        if (foodPrefab == null)
        {
            Debug.LogError("❌ `foodPrefab` が `null` です！Inspector で設定してください！");
        }
    }



    private IEnumerator WaitForGrowthManager()
    {
        while (growthManager == null)
        {
            growthManager = FindFirstObjectByType<MermaidGrowthManager>();
            yield return null;
        }

        UpdateButtonAction();
    }

    void Update()
    {
        if (isGameOver) return;

        if (growthManager == null)
        {
            isGameOver = true;
            return;
        }

        bool currentIsEgg = growthManager.IsEgg();
        if (currentIsEgg != previousIsEgg)
        {
            UpdateButtonAction();
            previousIsEgg = currentIsEgg;
        }
    }

    public void UpdateButtonAction()
    {
        if (growthManager == null) return;

        bool isEgg = growthManager.IsEgg();

        if (feedButton != null)
        {
            feedButton.onClick.RemoveAllListeners();
            feedButton.onClick.AddListener(HandleFeedButton);
        }
    }

    public void HandleFeedButton()
    {
        Debug.Log("🍽 ボタンが押されました！"); // ← ここが出るか確認！

        // ✅ 音を鳴らす（ButtonSoundManagerを探して再生）
        ButtonSoundManager soundManager = FindFirstObjectByType<ButtonSoundManager>();
        if (soundManager != null)
        {
            AudioSource audio = soundManager.GetComponent<AudioSource>();
            if (audio != null && soundManager.clickSound != null)
            {
                audio.PlayOneShot(soundManager.clickSound);
            }
        }

        if (growthManager == null)
        {
            Debug.LogError("❌ `growthManager` が `null` です！");
            return;
        }

        if (growthManager.IsEgg()) // まだたまご
        {
            if (eggDialogManager != null)
            {
                eggDialogManager.ShowEggDialog(EggDialogType.CannotEat); // ← enum型で渡す ✔

            }
        }

        else
        {
            SpawnFood(); // 🥣 ごはんをスポーン（ここで止まっている可能性あり）
        }
    }

private void SpawnFood()
{
    Debug.Log("🍚 SpawnFood() が呼ばれました");

    if (isGameOver || foodPrefab == null)
    {
        Debug.LogWarning("⚠ ごはんがスポーンされません。isGameOver または foodPrefab が無効です");
        return;
    }

    Debug.Log("✅ ごはんのスポーン処理を開始します");

    GameObject newFood = Instantiate(foodPrefab, foodSpawnPoint.position, Quaternion.identity);
    
    if (newFood == null)
    {
        Debug.LogError("❌ Instantiate に失敗しました！foodPrefab のインスタンスが作られませんでした。");
        return;
    }

    newFood.SetActive(true);
    Debug.Log($"✅ ごはんがスポーンされました！ 位置: {foodSpawnPoint.position}");
}

    private int foodConsumedCount = 0;

    public void OnFoodConsumed()
    {
        foodConsumedCount++;
        PlayerPrefs.SetInt("FoodCount", foodConsumedCount); // ここでセーブ！
        PlayerPrefs.Save(); // 忘れずに保存

        Debug.Log($"🍽 ごはんが {foodConsumedCount} 回食べられました");

        if (foodConsumedCount == 3)
        {
            Debug.Log("📺 インタースティシャル広告表示の条件を満たしました");

            AdmobLibrary.RequestInterstitial(); // 読み込みだけ先にやっておく

            // ✅ 5秒後に安全に広告を表示（AdmobLibrary のコルーチンを利用）
            StartCoroutine(AdmobLibrary.PlayInterstitialDelayed(5f)); // ← 5秒待ってから表示！

            foodConsumedCount = 0;
            PlayerPrefs.SetInt("FoodCount", foodConsumedCount); // カウントリセットも保存
            PlayerPrefs.Save();
        }
    }




}
