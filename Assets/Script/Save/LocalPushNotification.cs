using UnityEngine;
using Unity.Notifications.Android;

/// <summary>
/// ���[�J���v�b�V���ʒm�̊Ǘ�
/// </summary>
public static class LocalPushNotification
{
    private const string ChannelId = "game_channel";

    /// <summary>
    /// �ʒm�`�����l����o�^
    /// </summary>
    public static void RegisterChannel()
    {
        var channel = new AndroidNotificationChannel()
        {
            Id = ChannelId,
            Name = "�Q�[���ʒm",
            Importance = Importance.Default,
            Description = "�Q�[���̏d�v�Ȓʒm"
        };
        AndroidNotificationCenter.RegisterNotificationChannel(channel);
    }

    /// <summary>
    /// �ʒm���X�P�W���[���ݒ�
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
        Debug.Log($"�ʒm�ݒ�: {title} - {delaySeconds}�b��");
    }

    /// <summary>
    /// ���ׂĂ̒ʒm���N���A
    /// </summary>
    public static void AllClear()
    {
        AndroidNotificationCenter.CancelAllNotifications();
        Debug.Log("���ׂĂ̒ʒm���N���A���܂���");
    }

    /// <summary>
    /// �e�X�g�p�̒ʒm��3�b��ɕ\������
    /// </summary>
    public static void TestNotification()
    {
        AddSchedule("�ʒm�e�X�g", "����̓e�X�g�ʒm�ł�", 3);
    }

}
