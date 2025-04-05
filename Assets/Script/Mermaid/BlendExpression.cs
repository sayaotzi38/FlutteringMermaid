using UnityEngine;
using Live2D.Cubism.Core; // CubismModel, CubismParameter に必要

/// <summary>
/// Animator と Live2D の両方を使って表情制御を行うクラス。
/// - Animator の BlendTree により表情ブレンド
/// - Live2D のパラメータ（例：Param_WeakState）も直接制御
/// </summary>
public class BlendExpression : MonoBehaviour
{
    private Animator _blendTree;
    private int _expressionIndex;

    [Header("表情のブレンド（0:通常, 1:衰弱）")]
    [SerializeField, Range(0f, 1f)]
    public float Blending = 0f;

    [Header("Expressionレイヤーの適用度（0:適用なし, 1:適用）")]
    [SerializeField, Range(0f, 1f)]
    public float ExpressionWeight = 1f;

    private CubismModel cubismModel;

    void Start()
    {
        _blendTree = GetComponent<Animator>();
        _expressionIndex = _blendTree.GetLayerIndex("Expression");

        cubismModel = GetComponentInChildren<CubismModel>();
        if (cubismModel == null)
        {
            Debug.LogWarning("⚠ Live2DのCubismModelが取得できませんでした");
        }
    }

    void Update()
    {
        if (_blendTree == null) return;

        _blendTree.SetFloat("Blend", Blending);

        if (_expressionIndex != -1)
            _blendTree.SetLayerWeight(_expressionIndex, ExpressionWeight);
    }

    /// <summary>
    /// 衰弱状態を適用（Blend=1）
    /// </summary>
    public void SetWeakExpression()
    {
        Blending = 1f;
        ExpressionWeight = 1f;
        ApplyExpression();

        // Live2Dの衰弱パラメータも適用
        SetLive2DWeakParameter(1f);
    }

    /// <summary>
    /// 通常表情に戻す（Blend=0）
    /// </summary>
    public void ResetExpression()
    {
        Blending = 0f;
        ExpressionWeight = 1f;
        ApplyExpression();

        // Live2Dの衰弱パラメータをリセット
        SetLive2DWeakParameter(0f);
    }

    /// <summary>
    /// 現在のブレンド値をAnimatorに即時反映
    /// </summary>
    private void ApplyExpression()
    {
        if (_blendTree == null) return;

        _blendTree.SetFloat("Blend", Blending);

        if (_expressionIndex != -1)
            _blendTree.SetLayerWeight(_expressionIndex, ExpressionWeight);
    }

    /// <summary>
    /// モデルが差し替わったときにAnimatorを再取得
    /// </summary>
    public void UpdateAnimator(Animator newAnimator)
    {
        _blendTree = newAnimator;
        _expressionIndex = _blendTree.GetLayerIndex("Expression");
        ApplyExpression();
    }

    /// <summary>
    /// Live2Dの「Param_WeakState」パラメータを変更（存在すれば）
    /// </summary>
    private void SetLive2DWeakParameter(float value)
    {
        if (cubismModel == null) return;

        var weakParam = System.Array.Find(cubismModel.Parameters, p => p.Id == "Param_WeakState");
        if (weakParam != null)
        {
            weakParam.Value = value;
            Debug.Log($"🔁 Param_WeakState を {value} に設定しました");
        }
    }

}
