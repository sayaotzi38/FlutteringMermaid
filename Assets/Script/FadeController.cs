using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class FadeController : MonoBehaviour
{
    [Header("�t�F�[�h�p�̉摜")]
    [SerializeField] private Image fadeImage;

    [Header("�t�F�[�h����")]
    [SerializeField] private float fadeDuration = 1.5f;

    void Start()
    {
        // **�^�C�g����ʂł́u�[���v����t�F�[�h�C��**
        if (SceneManager.GetActiveScene().name == "Title")
        {
            fadeImage.color = new Color(0.0f, 0.1f, 0.3f, 1); // �[���i#001A4D�j
            StartCoroutine(FadeIn());
        }
        else
        {
            // ���C���Q�[���ł̓t�F�[�h�Ȃ��i�����j
            fadeImage.color = new Color(1, 1, 1, 0);
        }
    }

    /// <summary>
    /// ��ʂ��t�F�[�h�C���i���X�ɓ����ɂ���j
    /// </summary>
    private IEnumerator FadeIn()
    {
        float elapsedTime = 0;
        Color color = fadeImage.color;
        color.a = 1; // �ŏ��͊��S�Ɍ�����

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            color.a = 1 - (elapsedTime / fadeDuration);
            fadeImage.color = color;
            yield return null;
        }

        fadeImage.color = new Color(color.r, color.g, color.b, 0); // ���S�ɓ����ɂ���
    }

    /// <summary>
    /// �V�[���J�ڎ��̃t�F�[�h�A�E�g�i���j
    /// </summary>
    public void StartFadeOut(string nextScene)
    {
        StartCoroutine(FadeOut(nextScene));
    }

    /// <summary>
    /// ��ʂ��t�F�[�h�A�E�g�i���Ƀt�F�[�h�A�E�g�j
    /// </summary>
    private IEnumerator FadeOut(string nextScene)
    {
        float elapsedTime = 0;
        fadeImage.color = new Color(1, 1, 1, 0); // �ŏ��͓���

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            fadeImage.color = new Color(1, 1, 1, elapsedTime / fadeDuration); // ���X�ɔ�������
            yield return null;
        }

        SceneManager.LoadScene(nextScene);
    }
}
