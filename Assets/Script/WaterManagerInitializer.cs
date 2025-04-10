using UnityEngine;
using System.Collections; // ← IEnumerator を使うために必要

/// <summary>
/// GameManager の初期化が完了してから WaterManager を安全に初期化する補助クラス
/// </summary>
public class WaterManagerInitializer : MonoBehaviour
{
    private IEnumerator Start()
    {
        Debug.Log("🕒 WaterManagerInitializer: GameManager の準備完了を待機中...");

        yield return new WaitUntil(() =>
            GameManager.Instance != null &&
            GameManager.Instance.SaveManagerInstance != null &&
            GameManager.Instance.SaveManagerInstance.SaveDataInstance != null);

        var waterManager = FindFirstObjectByType<WaterManager>();
        if (waterManager != null)
        {
            Debug.Log("🚰 WaterManagerInitializer：初期化開始");
            waterManager.StopAllCoroutines();
            waterManager.StartCoroutine("MyStart");
        }
        else
        {
            Debug.LogWarning("⚠ WaterManagerInitializer：WaterManager が見つかりませんでした");
        }
    }
}
