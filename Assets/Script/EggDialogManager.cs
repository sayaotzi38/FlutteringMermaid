using System.Collections;
using TMPro;
using UnityEngine;

public class EggDialogManager : MonoBehaviour
{
    [Header("ダイアログの CanvasGroup")]
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("ダイアログのメッセージ")]
    [SerializeField] private TextMeshProUGUI dialogText; // ✅ メッセージ用の TextMeshProUGUI を定義


    [Header("ダイアログ表示時間")]
    [SerializeField] private float displayTime = 2f;

    private Coroutine fadeCoroutine;

   

    private void Awake()
    {
       

        if (canvasGroup == null)
        {
            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                Debug.LogError("❌ `CanvasGroup` が見つかりません！");
                return;
            }
        }

        if (dialogText == null)
        {
            dialogText = GetComponentInChildren<TextMeshProUGUI>(); // ✅ 子オブジェクトから探す
            if (dialogText == null)
            {
                Debug.LogError("❌ `dialogText` が見つかりません！");
                return;
            }
        }

        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;
    }


    /// <summary>
    /// たまごダイアログの種類を表す列挙型
    /// </summary>
    public enum EggDialogType
    {
        CannotEat,   // ごはんを食べられない
        JustBorn,    // 生まれたばかり
        Sleepy       // 眠そう（必要に応じて追加）
    }


    /// <summary>
    /// ダイアログの種類に応じてメッセージを表示する
    /// </summary>
    public void ShowEggDialog(EggDialogType type)
    {
        Debug.Log("分岐型のメッセージを表示します");
        string message = "";

        switch (type)
        {
            case EggDialogType.CannotEat:
                message = "まだごはんはたべられないようだ…";
                break;
            case EggDialogType.JustBorn:
                message = "うまれたばかりでまだねむそうだ…";
                break;
            case EggDialogType.Sleepy:
                message = "たまごはゆっくりねむっている…";
                break;
            default:
                Debug.LogWarning("⚠ 未対応のダイアログタイプが指定されました");
                break;
        }

        // メッセージを表示用Textに反映：Inspectorの初期テキスト上書き
        if (dialogText != null)
        {
            dialogText.text = message;
        }

        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }

        fadeCoroutine = StartCoroutine(FadeInAndAutoFadeOut());
    }



   

    private IEnumerator FadeInAndAutoFadeOut()
    {
        yield return StartCoroutine(FadeIn());

        yield return new WaitForSeconds(displayTime);

        yield return StartCoroutine(FadeOut());
    }

    private IEnumerator FadeIn()
    {
        if (canvasGroup == null) yield break;

        canvasGroup.gameObject.SetActive(true);
        canvasGroup.blocksRaycasts = true;

        for (float t = 0; t <= 1; t += Time.deltaTime / 0.5f)
        {
            canvasGroup.alpha = t;
            yield return null;
        }
        canvasGroup.alpha = 1f;
    }

    private IEnumerator FadeOut()
    {
        if (canvasGroup == null) yield break;

        for (float t = 1; t >= 0; t -= Time.deltaTime / 0.5f)
        {
            canvasGroup.alpha = t;
            yield return null;
        }
        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;
    }
}
