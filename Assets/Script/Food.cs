using System.Collections; // ✅ `IEnumerator` を使うために追加
using UnityEngine;

/// <summary>
/// **食べ物の管理**
/// - 一定時間後に消える
/// - 食べられたら即削除
/// - 食べられた時に音を鳴らす
/// </summary>
public class Food : MonoBehaviour
{
    private bool isEaten = false;

    [Header("かじる音（ごはんを食べたときの音）")]
    [SerializeField] public AudioClip biteSound; // ✅ かじる音
    private AudioSource audioSource; // ✅ 音を鳴らすためのコンポーネント




    private void Start()
    {
        Debug.Log($"🍙 `Food` が生成されました！ (Instance ID: {gameObject.GetInstanceID()})");

        // ✅ `SpriteRenderer` が無効になっていたら有効にする
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = true;
        }
        else
        {
            Debug.LogError("❌ `SpriteRenderer` が見つかりません！`Food` に追加してください！");
        }

       

        // ✅ もし `Destroy()` されるなら、それをログで確認
        Invoke(nameof(DestroyFoodDebug), 2.0f);


        // 🛠 修正: レイヤーを範囲内に設定する
        if (gameObject.layer < 0 || gameObject.layer > 31)
        {
            Debug.LogWarning("⚠ `Food` のレイヤーが無効です！デフォルトレイヤー (0) に変更します");
            gameObject.layer = 0; // ✅ `Default` レイヤーに修正
        }

        Invoke(nameof(DestroyFood), 40f); // ⏳ 40秒後に削除

        // ✅ `AudioSource` を取得 or 追加
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            Debug.LogWarning("⚠ `AudioSource` が見つかりません！ `Food` に追加します。");
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // 🎯 `Spatial Blend` を 2D に強制設定
        audioSource.spatialBlend = 0f;

        // ✅ `biteSound` の設定を確認（この順番を修正）
        if (biteSound == null)
        {
            Debug.LogError("❌ `biteSound` が設定されていません！ Inspector で設定してください！");
        }
        else
        {
            Debug.Log($"✅ `biteSound` は正しく設定されています: {biteSound.name}");
        }
    }

    private void DestroyFoodDebug()
    {
        Debug.Log($"🗑 `Food` が削除されます: (Instance ID: {gameObject.GetInstanceID()})");
    }

    /// <summary>
    /// **時間経過でごはんを削除**
    /// </summary>
    private void DestroyFood()
    {
        if (!isEaten)
        {
            Debug.Log("⏳ `Food` が時間切れで削除されます");
            Destroy(gameObject);
        }
    }


    private IEnumerator PlayAndDestroy()
    {
        PlayBiteSound();
        yield return new WaitForSeconds(0.1f); // 少し遅らせる
        Destroy(gameObject);
    }

    public void Eat(GameObject mermaid)
    {
        if (!isEaten)
        {
            isEaten = true;
            Debug.Log("🍽 `Food` が食べられました！Food.Cs");

            StartCoroutine(PlayAndDestroy());

            // 満腹処理
            MermaidStatus status = mermaid.GetComponent<MermaidStatus>();
            if (status != null)
            {
                status.EatFood(this.gameObject);
            }

            // FeedManager に通知（広告用）
            FeedManager feedManager = FindAnyObjectByType<FeedManager>();
            if (feedManager != null)
            {
                feedManager.OnFoodConsumed();
            }
            else
            {
                Debug.LogWarning("⚠ FeedManager が見つかりませんでした！");
            }
        }
    }




/// <summary>
/// **マーメイドが `Food` に衝突したとき**
/// </summary>
private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"🐟 `OnTriggerEnter2D()` 呼ばれた (Instance ID: {gameObject.GetInstanceID()}) at {Time.time}");

        if (isEaten) return;

        if (other.CompareTag("Mermaid"))
        {
            Debug.Log("🧜‍♀️ マーメイドが `Food` を食べました！Food.Cs");
            Eat(other.gameObject);
        }
    }




    /// <summary>
    /// **かじる音を再生**
    /// </summary>
    private void PlayBiteSound()
    {
        if (biteSound == null)
        {
            Debug.LogError("❌ `biteSound` が `null` です！ Inspector で設定してください！");
            return;
        }

        if (audioSource == null)
        {
            Debug.LogError("❌ `AudioSource` が `null` です！ コンポーネントがあるか確認してください！");
            return;
        }

        Debug.Log($"🎵 `かじる音を再生`: {biteSound.name}");
        audioSource.PlayOneShot(biteSound);
    }




}
