using UnityEditor;
using UnityEngine;

/// <summary>
/// `WaterManager` のカスタムエディター（デバッグボタン付き）
/// </summary>
[CustomEditor(typeof(WaterManager))]
public class WaterManagerEditor : BaseDebugEditor<WaterManager>
{
    protected override void DrawDebugButtons(WaterManager manager)
    {
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("デバッグ用: 汚れの透明度", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("0%"))
        {
            Undo.RecordObject(manager, "Set Dirt Alpha 0%");
            manager.SetDirtAlpha(0f);
            EditorUtility.SetDirty(manager);
        }
        if (GUILayout.Button("50%"))
        {
            Undo.RecordObject(manager, "Set Dirt Alpha 50%");
            manager.SetDirtAlpha(manager.MaxDirtAlpha * 0.5f);
            EditorUtility.SetDirty(manager);
        }
        if (GUILayout.Button("95%"))
        {
            Undo.RecordObject(manager, "Set Dirt Alpha 95%");
            manager.SetDirtAlpha(manager.MaxDirtAlpha * 0.95f);
            EditorUtility.SetDirty(manager);
        }
        if (GUILayout.Button("100%"))
        {
            Undo.RecordObject(manager, "Set Dirt Alpha 100%");
            manager.SetDirtAlpha(manager.MaxDirtAlpha);
            EditorUtility.SetDirty(manager);
        }
        EditorGUILayout.EndHorizontal();
    }
}
