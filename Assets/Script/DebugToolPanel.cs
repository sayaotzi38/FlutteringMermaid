using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 共通デバッグツールパネル
/// 満腹度・水質の操作や、死亡ボタンなどをUIから直接実行
/// </summary>
public class DebugToolPanel : MonoBehaviour
{
    [Header("対象スクリプト")]
    public MermaidStatus mermaidStatus;
    public WaterManager waterManager;


    [Header("まんぷく度0%")]
    public Button killButton;

    void Awake()
    {
#if !UNITY_EDITOR && !DEVELOPMENT_BUILD
        gameObject.SetActive(false); // 本番ビルドでは非表示
#endif
    }

    IEnumerator Start()
    {
        yield return null; // セーブデータ読み込み後に実行

        if (killButton != null)
        {
            killButton.onClick.AddListener(ForceDie);
            Debug.Log("☠ 『満腹度0で死亡』ボタンを初期化しました");
        }

        
    }

    /// <summary>
    /// 水質を指定パーセンテージに設定（例：100%）
    /// </summary>
    public void SetWaterDirt(float targetPercent)
    {
        if (waterManager != null)
        {
            float alphaValue = waterManager.MaxDirtAlpha * (targetPercent / 100f);
            waterManager.SetDirtAlpha(alphaValue);
           
        }
    }

    /// <summary>
    /// 満腹度を0%にし、死亡処理を実行（確認付き）
    /// </summary>
    private void ForceDie()
    {
        if (mermaidStatus != null)
        {
            Debug.Log("☠ ボタン押下：満腹度を0にして死亡処理を実行します");

            // 死亡処理が通るように isAlive = true を明示的に設定
            mermaidStatus.isAlive = true;
            mermaidStatus.SetHunger(0f);
            mermaidStatus.Die();
        }
        else
        {
            Debug.LogWarning("⚠ MermaidStatus が未設定のため、死亡処理を実行できませんでした");
        }
    }


}
