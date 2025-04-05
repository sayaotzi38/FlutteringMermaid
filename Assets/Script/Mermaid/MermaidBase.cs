using UnityEngine;
using UnityEngine.SceneManagement;

public class MermaidBase : MonoBehaviour
{
    [Header("人魚の基本データ")]
    public bool isAlive = true;  // 生存判定
                                 //protected float hunger = 100f; // ✅ `protected` にする
                                 //protected float minHunger = 0f;
                                 //protected float maxHunger = 100f;



    // 👇 こうする！
    public virtual void UpdateHunger(float amount)
    {
        // 中身は空でもOK（子で上書きするので）
    }


    public virtual void Die()
    {
        if (!isAlive) return;
        isAlive = false;
        Debug.Log("❌ 人魚が死亡しました！Base");
        Destroy(gameObject); // 🗑 人魚オブジェクトを削除
    }



}


