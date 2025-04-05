using UnityEngine;
using TMPro;

public class MermaidNameManager : MonoBehaviour
{
    [Header("人魚の名前を表示するテキスト")]
    [SerializeField] private TMP_Text mermaidNameText;

    private const string MermaidNameKey = "MermaidName";  // 名前の保存キー

    void Start()
    {
        // **保存されている人魚の名前をロード**
        if (PlayerPrefs.HasKey(MermaidNameKey))
        {
            string savedName = PlayerPrefs.GetString(MermaidNameKey);
            mermaidNameText.text = savedName;
        }
        else
        {
            mermaidNameText.text = "人魚"; // デフォルト名
        }
    }

    /// <summary>
    /// 人魚の名前を更新する
    /// </summary>
    public void UpdateMermaidName(string newName)
    {
        if (mermaidNameText != null)
        {
            mermaidNameText.text = newName;
            Debug.Log($"🎉 人魚の名前を {newName} に更新しました！");
        }
        else
        {
            Debug.LogError("❌ `mermaidNameText` が `null` です！");
        }
    }

}
