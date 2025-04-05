using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StartButtonController : MonoBehaviour
{
    [Header("スタートボタン")]
    [SerializeField] private Button startButton;

    [Header("フェード用コントローラー")]
    [SerializeField] private FadeController fadeController;

    [Header("次のシーン名")]
    [SerializeField] private string nextSceneName = "Main"; // 遷移するシーン名

    void Start()
    {
        if (startButton != null)
        {
            startButton.onClick.AddListener(OnStartButtonClicked);
        }
        else
        {
            Debug.LogError("❌ `startButton` が設定されていません！");
        }
    }

    public void OnStartButtonClicked()
    {
        Debug.Log("🎬 シーン遷移開始（フェードアウト）");

        if (fadeController != null)
        {
            fadeController.StartFadeOut(nextSceneName);
        }
        else
        {
            Debug.LogWarning("⚠ `fadeController` が設定されていません！直接シーン遷移を実行します");
            SceneManager.LoadScene(nextSceneName);
        }
    }
}
