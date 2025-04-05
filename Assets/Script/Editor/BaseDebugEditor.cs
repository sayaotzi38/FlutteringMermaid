using UnityEditor;
using UnityEngine;

/// <summary>
/// **�f�o�b�O�p�̊��G�f�B�^�[�N���X**
/// </summary>
public abstract class BaseDebugEditor<T> : Editor where T : MonoBehaviour
{
    public override void OnInspectorGUI()
    {
        // �Ώۂ̃R���|�[�l���g���擾
        T targetComponent = (T)target;

        // �f�t�H���g��Inspector�`��
        DrawDefaultInspector();

        // �f�o�b�O�{�^����`��
        DrawDebugButtons(targetComponent);
    }

    /// <summary>
    /// **�e�G�f�B�^�[�ŃJ�X�^���f�o�b�O�{�^��������**
    /// </summary>
    protected abstract void DrawDebugButtons(T targetComponent);
}
