using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class FadeController : MonoBehaviour
{
    [Header("フェード用の画像")]
    [SerializeField] private Image fadeImage;

    [Header("フェード時間")]
    [SerializeField] private float fadeDuration = 1.5f;

    void Start()
    {
        // **タイトル画面では「深い青」からフェードイン**
        if (SceneManager.GetActiveScene().name == "Title")
        {
            fadeImage.color = new Color(0.0f, 0.1f, 0.3f, 1); // 深い青（#001A4D）
            StartCoroutine(FadeIn());
        }
        else
        {
            // メインゲームではフェードなし（透明）
            fadeImage.color = new Color(1, 1, 1, 0);
        }
    }

    /// <summary>
    /// 画面をフェードイン（徐々に透明にする）
    /// </summary>
    private IEnumerator FadeIn()
    {
        float elapsedTime = 0;
        Color color = fadeImage.color;
        color.a = 1; // 最初は完全に見える

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            color.a = 1 - (elapsedTime / fadeDuration);
            fadeImage.color = color;
            yield return null;
        }

        fadeImage.color = new Color(color.r, color.g, color.b, 0); // 完全に透明にする
    }

    /// <summary>
    /// シーン遷移時のフェードアウト（白）
    /// </summary>
    public void StartFadeOut(string nextScene)
    {
        StartCoroutine(FadeOut(nextScene));
    }

    /// <summary>
    /// 画面をフェードアウト（白にフェードアウト）
    /// </summary>
    private IEnumerator FadeOut(string nextScene)
    {
        float elapsedTime = 0;
        fadeImage.color = new Color(1, 1, 1, 0); // 最初は透明

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            fadeImage.color = new Color(1, 1, 1, elapsedTime / fadeDuration); // 徐々に白くする
            yield return null;
        }

        SceneManager.LoadScene(nextScene);
    }
}
