using UnityEngine;

/// <summary>
/// タップした位置に波紋エフェクトを生成する処理
/// </summary>
public class TapEffectSpawner : MonoBehaviour
{
    [Header("タップ時に表示する波紋エフェクトのプレハブ")]
    public GameObject rippleEffectPrefab;

    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;
    }

    void Update()
    {
        // マウス or タップの入力チェック
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 screenPosition = Input.mousePosition;
            Vector3 worldPosition = mainCamera.ScreenToWorldPoint(screenPosition);
            worldPosition.z = 0f; // Z方向を0にして2D空間に調整

            Debug.Log($"📍 タップされた位置: {worldPosition}");

            if (rippleEffectPrefab != null)
            {
                // エフェクトを生成
                GameObject ripple = Instantiate(rippleEffectPrefab, worldPosition, Quaternion.identity);

                // ParticleSystem を取得して再生（PlayOnAwakeがOFFでも動作）
                ParticleSystem ps = ripple.GetComponent<ParticleSystem>();
                if (ps != null)
                {
                    ps.Play();
                    Debug.Log("🎞 ParticleSystem を再生しました");
                }
                else
                {
                    Debug.LogWarning("⚠ ParticleSystem が見つかりません！");
                }

                // 一定時間後に削除（1.5秒後など）
                Destroy(ripple, 1.5f);
                Debug.Log("🌊 波紋エフェクトを生成し、1.5秒後に破棄します");
            }
            else
            {
                Debug.LogWarning("⚠ rippleEffectPrefab が設定されていません！");
            }
        }
    }
}
