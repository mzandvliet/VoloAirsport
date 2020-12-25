using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using RamjetAnvil.DependencyInjection;
using RamjetAnvil.Gui;
using UnityEngine;

public class NotificationList : MonoBehaviour {

    public event Action<int, string> NotificationAdded;
    public event Action<int> NotificationRemoved;

    [Dependency, SerializeField] private SimpleScheduler _scheduler;

    private Queue<int> _notificationIds;

    void Awake() {
        _notificationIds = new Queue<int>(16);
        for (int i = 0; i < 16; i++) {
            _notificationIds.Enqueue(i);
        }
    }

    public int AddNotification(string message) {
        var notificationId = _notificationIds.Dequeue();
        if (NotificationAdded != null) {
            NotificationAdded(notificationId, message);
        }
        return notificationId;
    }

    public int AddTimedNotification(string message, TimeSpan timeout) {
        var notificationId = AddNotification(message);
        _scheduler.AddTask(() => RemoveNotification(notificationId), timeout);
        return notificationId;
    }

    public void RemoveNotification(int id) {
        _notificationIds.Enqueue(id);
        if (NotificationRemoved != null) {
            NotificationRemoved(id);
        }
    }

}
