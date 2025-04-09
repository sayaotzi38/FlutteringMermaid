using System;
using UnityEngine;

/// <summary>
/// セーブデータの構造を定義
/// </summary>
[Serializable]
public class SaveData
{
    public bool isAlive = true;
    public float hungerTimeRemaining; // 満腹度の残り時間（秒）
    public float waterPollutionLevel; // 水の汚れレベル（0〜100）
    public DateTime lastSaveTime;     // 最後に保存した時間
    public int mermaidGrowthLevel;    // 人魚の成長度（レベル）
    public int daysPassed;            // ゲーム内経過日数
    public bool isWeak;               // 人魚が衰弱状態かどうか
    public DateTime gameStartTime;    // 🌟 ゲーム開始時刻（累計時間成長の基準）

    /// <summary>
    /// 初期データを設定
    /// </summary>
    public SaveData()
    {
        hungerTimeRemaining = 345600f; // 初期4日分
        waterPollutionLevel = 0f;
        mermaidGrowthLevel = 1;
        daysPassed = 0;
        isWeak = false;
        lastSaveTime = DateTime.Now;
        gameStartTime = DateTime.Now; // 🎯 初回に保存される
    }
}
