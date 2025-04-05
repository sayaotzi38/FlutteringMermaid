using UnityEditor;
using UnityEngine;

/// <summary>
/// **デバッグ用の基底エディタークラス**
/// </summary>
public abstract class BaseDebugEditor<T> : Editor where T : MonoBehaviour
{
    public override void OnInspectorGUI()
    {
        // 対象のコンポーネントを取得
        T targetComponent = (T)target;

        // デフォルトのInspector描画
        DrawDefaultInspector();

        // デバッグボタンを描画
        DrawDebugButtons(targetComponent);
    }

    /// <summary>
    /// **各エディターでカスタムデバッグボタンを実装**
    /// </summary>
    protected abstract void DrawDebugButtons(T targetComponent);
}
