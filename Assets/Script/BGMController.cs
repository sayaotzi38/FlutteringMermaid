using UnityEngine;
using TMPro;

public class BGMController : MonoBehaviour
{
    [Header("BGM コントロール")]
    [SerializeField] private TMP_Dropdown bgmDropdown;  // ドロップダウン
    private AudioSource audioSource;   // 音を鳴らす AudioSource
    [SerializeField] private AudioClip bgm1;            // BGM1 (デフォルト)
    [SerializeField] private AudioClip bgm2;            // BGM2
    [SerializeField] private AudioClip bgm3;            // BGM3
    [SerializeField] private AudioClip bgm4;            // BGM4
    [SerializeField] private AudioClip bgm5;            // BGM5
    [SerializeField] private AudioClip bgm6;            // BGM6

    void Start()
    {
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>(); // ✅ `AudioSource` を取得
        }

        if (bgmDropdown != null)
        {
            bgmDropdown.onValueChanged.AddListener(ChangeBGM);
            UpdateDropdownLabel(bgmDropdown.value); // 🔄 `Start()` で `Dropdown` の初期ラベルを設定
        }
        // ✅ デフォルトのBGMを再生（例えば `bgm1`）
        if (audioSource.clip == null)
        {
            PlayBGM(bgm1); // 🎵 `bgm1` をデフォルトで再生
        }
    }

    /// <summary>
    /// 選択された BGM を変更
    /// </summary>
    private void ChangeBGM(int index)
    {
        switch (index)
        {

            case 0: PlayBGM(bgm1); break;
            case 1: PlayBGM(bgm2); break;
            case 2: PlayBGM(bgm3); break;
            case 3: PlayBGM(bgm4); break;
            case 4: PlayBGM(bgm5); break;
            case 5: PlayBGM(bgm6); break;
            case 6: audioSource.Stop(); audioSource.clip = null; break;
        }

        // 🎛 `Dropdown` のラベルを更新
        UpdateDropdownLabel(index);
        Debug.Log($"🎵 BGM が変更されました: {bgmDropdown.options[index].text}");
    }

    /// <summary>
    /// 指定した BGM を再生
    /// </summary>
    private void PlayBGM(AudioClip clip)
    {
        if (audioSource != null)
        {
            audioSource.Stop();         // 🔇 先に現在の BGM を停止
            audioSource.clip = clip;

            if (clip != null)
            {
                audioSource.loop = true; // 🔄 ループ再生
                audioSource.Play();
            }
        }
    }


    /// <summary>
    /// `Dropdown` のラベルを更新
    /// </summary>
    private void UpdateDropdownLabel(int index)
    {
        if (bgmDropdown != null && bgmDropdown.options.Count > index)
        {
            if (bgmDropdown.captionText != null)
            {
                bgmDropdown.captionText.text = bgmDropdown.options[index].text; // 🎛 `Dropdown` のタイトル部分を更新
            }
            else
            {
                Debug.LogError("❌ `Caption Text` が設定されていません！");
            }
        }
    }
}
