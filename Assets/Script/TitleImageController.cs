using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class TitleImageController : MonoBehaviour
{
    [Header("タイトル画像")]
    [SerializeField] private Image titleImage;

    void Start()
    {
        if (titleImage == null)
        {
            Debug.LogError("タイトル画像が設定されていません！");
            return;
        }

        // **タイトル画像のフェードインを実行**
        StartCoroutine(FadeInTitle());
    }

    private IEnumerator FadeInTitle()
    {
        if (titleImage == null) yield break;

        // アルファ値を 0 にして、透明状態からスタート
        Color color = titleImage.color;
        color.a = 0;
        titleImage.color = color;

        float duration = 1.5f; // フェードイン時間
        float elapsedTime = 0;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            color.a = Mathf.Clamp01(elapsedTime / duration);
            titleImage.color = color;
            yield return null;
        }

        Debug.Log("✅ タイトル画像のフェードイン完了");
    }
}
