using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    private AudioSource audioSource;

    void Awake()
    {
               // AudioSource を取得
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f; // 2Dサウンド
    }

    /// <summary>
    /// 汎用的なサウンド再生メソッド
    /// </summary>
    public void PlaySound(AudioClip clip)
    {
        if (clip == null)
        {
            Debug.LogError("❌ 再生する音声が設定されていません！");
            return;
        }

        audioSource.PlayOneShot(clip);
    }
}
