using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    [Header("UI パネル (CanvasGroup で管理)")]
    [SerializeField] private CanvasGroup menuCanvasGroup;
    [SerializeField] private CanvasGroup gameOverCanvasGroup;
    [SerializeField] private CanvasGroup nameChangeCanvasGroup;
    [SerializeField] private CanvasGroup resetDialogCanvasGroup;
    [SerializeField] private CanvasGroup startMessageCanvasGroup; // 🔹 追加: スタート時のダイアログ用

    [Header("スタート時のメッセージ表示")]
    [SerializeField] private TMP_Text startMessageText;
    [SerializeField] private float messageDisplayDuration = 5.0f;

    [Header("ボタン")]
    [SerializeField] private Button menuButton;
    [SerializeField] private Button closeButton;
    [SerializeField] private Button changeNameButton;
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button cancelButton;
    [SerializeField] private Button resetDataButton;
    [SerializeField] private Button confirmResetButton;
    [SerializeField] private Button cancelResetButton;
    [SerializeField] private Button returnButton; // 🔹 タイトルへ戻るボタン

       [Header("名前変更入力欄")]
    [SerializeField] private TMP_InputField nameInputField;

    [Header("BGM 設定")]
    [SerializeField] private TMP_Dropdown bgmDropdown;
   

    [Header("人魚の名前を管理するスクリプト")]
    [SerializeField] private MermaidNameManager mermaidNameManager;

    private const string MermaidNameKey = "MermaidName";
    private const string BGMKey = "BGMSetting";
    private const string GameInitializedKey = "GameInitialized"; // ← この1行を追加

    private const string ShowStartMessageKey = "ShowStartMessage";



    void Start()
    {
        // ボタンに機能をセット
        menuButton?.onClick.AddListener(() => TogglePanel(menuCanvasGroup, true));
        closeButton?.onClick.AddListener(() => TogglePanel(menuCanvasGroup, false));

        changeNameButton?.onClick.AddListener(() => SwitchPanel(menuCanvasGroup, nameChangeCanvasGroup));
        confirmButton?.onClick.AddListener(ConfirmNameChange);
        cancelButton?.onClick.AddListener(() => SwitchPanel(nameChangeCanvasGroup, menuCanvasGroup));

        resetDataButton?.onClick.AddListener(() => SwitchPanel(menuCanvasGroup, resetDialogCanvasGroup));
        confirmResetButton?.onClick.AddListener(ConfirmResetGame); // ✅ 修正
        cancelResetButton?.onClick.AddListener(() => SwitchPanel(resetDialogCanvasGroup, menuCanvasGroup));

        returnButton?.onClick.AddListener(ReturnToTitle);

        // パネルを最初は非表示に
        SetInitialPanelState(menuCanvasGroup, false);
        SetInitialPanelState(nameChangeCanvasGroup, false);
        SetInitialPanelState(resetDialogCanvasGroup, false);
        SetInitialPanelState(gameOverCanvasGroup, false);
        SetInitialPanelState(startMessageCanvasGroup, false); // 🔹 スタートメッセージ非表示

        // 🔹 ここが重要！スタート時メッセージを表示
        if (PlayerPrefs.GetInt(ShowStartMessageKey, 0) == 1)
        {
            ShowStartMessage();
            PlayerPrefs.DeleteKey(ShowStartMessageKey); // 一度表示したら削除
        }


    }

    /// <summary>
    /// ゲーム開始時にメッセージを一時的に表示する
    /// </summary>
    private void ShowStartMessage()
    {
        if (startMessageText != null)
        {
            startMessageText.text = "たまごをみつけた。\nうまれたらごはんをあげて、\n水がよごれたらそうじをしよう";
            Debug.Log("📢 スタートメッセージを表示します");
            StartCoroutine(DisplayMessageCoroutine());
        }
    }

    /// <summary>
    /// 指定時間だけメッセージを表示するコルーチン
    /// </summary>
    private IEnumerator DisplayMessageCoroutine()
    {
        TogglePanel(startMessageCanvasGroup, true);
        yield return new WaitForSeconds(messageDisplayDuration);
        TogglePanel(startMessageCanvasGroup, false);
    }

    public void ResetGameState()
    {
        Debug.Log("🧹 ResetGameState(): メニューから全体初期化を実行");

        // ✅ 名前の初期化（ResetEverything() ではカバーしていない）
        PlayerPrefs.SetString(MermaidNameKey, "人魚");
        PlayerPrefs.Save();

        if (mermaidNameManager != null)
        {
            mermaidNameManager.UpdateMermaidName("人魚");
        }

        // ✅ セーブ・満腹・水質・成長などは GameManager 経由で一括初期化！
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ResetEverything();
        }
        else
        {
            Debug.LogWarning("⚠ GameManager.Instance が null です（ResetGameState中）");
        }



        // ✅ スタートメッセージ等の再表示フラグ
        PlayerPrefs.SetInt(GameInitializedKey, 1);
        PlayerPrefs.SetInt(ShowStartMessageKey, 1);
        PlayerPrefs.Save();
    }



    /// <summary>
    /// **ゲームオーバーパネルを表示**
    /// </summary>
    public void TriggerGameOver()
    {
        Debug.Log("💀 ゲームオーバー発生！MCcs");

        // 即時に操作可能に
        gameOverCanvasGroup.alpha = 1;
        gameOverCanvasGroup.interactable = true;
        gameOverCanvasGroup.blocksRaycasts = true;
    }


    public void OnConfirmGameOver()
    {
        StartCoroutine(DelayedResetCall());
    }

    private IEnumerator DelayedResetCall()
    {
        yield return new WaitForSeconds(0.1f); // GameManagerが確実に生成された後に
        GameManager.Instance?.ResetEverything();
    }

    //タイトルへ戻るボタンの処理(初期化)
    public void OnReturnToTitleClicked()
    {
        Debug.Log("🏁 タイトルに戻るボタンが押されました → 初期化処理を実行");

        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.ResetAllGameState();
        }
        else
        {
            Debug.LogWarning("⚠ SaveManager.Instance が null です");
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.ResetEverything();
        }
        else
        {
            Debug.LogWarning("⚠ GameManager.Instance が null です（OnReturnToTitleClicked）");
        }

        SceneManager.LoadScene("Title");
    }




    /// <summary>
    /// **タイトル画面に戻る**
    /// </summary>
    public void ReturnToTitle()
    {
        ResetGameState();
        SceneManager.LoadScene("Title");
    }





    /// <summary>
    /// **OKボタンを押したとき：名前を保存し、メニューに戻る**
    /// </summary>
    public void ConfirmNameChange()
    {
        if (nameInputField != null)
        {
            string newName = nameInputField.text;
            PlayerPrefs.SetString(MermaidNameKey, newName);
            PlayerPrefs.Save();

            if (mermaidNameManager != null)
            {
                mermaidNameManager.UpdateMermaidName(newName);
            }

            Debug.Log("🎉 人魚の名前が変更されました: " + newName);
        }

        SwitchPanel(nameChangeCanvasGroup, menuCanvasGroup);
    }

    /// <summary>
    /// **リセット前の確認ダイアログを開く**
    /// </summary>
    public void ShowResetDialog()
    {
        SwitchPanel(menuCanvasGroup, resetDialogCanvasGroup);
    }

    /// <summary>
    /// **リセットをキャンセルしてメニューに戻る**
    /// </summary>
    public void CancelReset()
    {
        SwitchPanel(resetDialogCanvasGroup, menuCanvasGroup);
    }

    /// <summary>
    /// **ゲームのデータをリセット**
    /// </summary>
    public void ConfirmResetGame()
    {
        ResetGameState();
        SwitchPanel(resetDialogCanvasGroup, menuCanvasGroup);

        // 🔹 メニューも閉じて
        TogglePanel(menuCanvasGroup, false);

        // 🔹 その場でスタートメッセージを表示
        ShowStartMessage();
    }



    /// <summary>
    /// **パネルをトグル（表示・非表示）**
    /// </summary>
    private void TogglePanel(CanvasGroup panel, bool isVisible)
    {
        if (panel == null) return;
        StartCoroutine(FadePanel(panel, isVisible));
    }

    /// <summary>
    /// **1つのパネルを閉じて、別のパネルを開く**
    /// </summary>
    private void SwitchPanel(CanvasGroup closePanel, CanvasGroup openPanel)
    {
        TogglePanel(closePanel, false);
        TogglePanel(openPanel, true);
    }

    /// <summary>
    /// **フェードアニメーション**
    /// </summary>
    private IEnumerator FadePanel(CanvasGroup panel, bool fadeIn)
    {
        float duration = 0.2f;
        float elapsedTime = 0f;
        float startAlpha = panel.alpha;
        float targetAlpha = fadeIn ? 1f : 0f;

        panel.interactable = false;
        panel.blocksRaycasts = false;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            panel.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsedTime / duration);
            yield return null;
        }

        panel.alpha = targetAlpha;
        panel.interactable = fadeIn;
        panel.blocksRaycasts = fadeIn;
    }

    /// <summary>
    /// **パネルの初期状態を設定**
    /// </summary>
    private void SetInitialPanelState(CanvasGroup panel, bool isVisible)
    {
        if (panel == null) return;
        panel.alpha = isVisible ? 1 : 0;
        panel.interactable = isVisible;
        panel.blocksRaycasts = isVisible;
    }
}
