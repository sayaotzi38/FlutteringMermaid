using UnityEngine;
using UnityEngine.UI;

public class ButtonSoundManager : MonoBehaviour
{
    [Header("ボタンのクリック音")]
    public AudioClip clickSound; // ✅ クリック音
    private AudioSource audioSource;

    private void Awake()
    {
        // 🎵 `AudioSource` を取得（なければ追加）
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // 🔄 音量や設定を適用
        audioSource.playOnAwake = false;
        audioSource.loop = false;
    }

    private void OnEnable()
    {
        ApplyButtonSounds(); // 🔄 シーン遷移後も確実に適用する
    }

    /// <summary>
    /// **シーン内のすべてのボタンにクリック音を適用**
    /// </summary>
    private void ApplyButtonSounds()
    {
        Button[] buttons = FindObjectsByType<Button>(FindObjectsSortMode.None);

        foreach (Button button in buttons)
        {
            button.onClick.RemoveListener(PlayClickSound); // 重複防止
            button.onClick.AddListener(PlayClickSound);
        }

        Debug.Log($"✅ {buttons.Length} 個のボタンにクリック音を適用しました！");
    }

    /// <summary>
    /// **ボタン音を再生**
    /// </summary>
    private void PlayClickSound()
    {
        if (clickSound != null)
        {
            audioSource.PlayOneShot(clickSound);
        }
        else
        {
            Debug.LogWarning("⚠ `clickSound` が設定されていません！");
        }
    }
}
