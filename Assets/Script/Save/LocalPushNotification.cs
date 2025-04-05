using UnityEngine;
using Unity.Notifications.Android;

/// <summary>
/// ローカルプッシュ通知の管理
/// </summary>
public static class LocalPushNotification
{
    private const string ChannelId = "game_channel";

    /// <summary>
    /// 通知チャンネルを登録
    /// </summary>
    public static void RegisterChannel()
    {
        var channel = new AndroidNotificationChannel()
        {
            Id = ChannelId,
            Name = "ゲーム通知",
            Importance = Importance.Default,
            Description = "ゲームの重要な通知"
        };
        AndroidNotificationCenter.RegisterNotificationChannel(channel);
    }

    /// <summary>
    /// 通知をスケジュール設定
    /// </summary>
    public static void AddSchedule(string title, string text, int delaySeconds)
    {
        var notification = new AndroidNotification()
        {
            Title = title,
            Text = text,
            FireTime = System.DateTime.Now.AddSeconds(delaySeconds)
        };
        AndroidNotificationCenter.SendNotification(notification, ChannelId);
        Debug.Log($"通知設定: {title} - {delaySeconds}秒後");
    }

    /// <summary>
    /// すべての通知をクリア
    /// </summary>
    public static void AllClear()
    {
        AndroidNotificationCenter.CancelAllNotifications();
        Debug.Log("すべての通知をクリアしました");
    }

    /// <summary>
    /// テスト用の通知を3秒後に表示する
    /// </summary>
    public static void TestNotification()
    {
        AddSchedule("通知テスト", "これはテスト通知です", 3);
    }

}
