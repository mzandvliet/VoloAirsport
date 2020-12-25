using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace RamjetAnvil.Volo.Ui {
    
    public class NotificationListView : MonoBehaviour {

        [SerializeField] private NotificationList _notificationList;
        [SerializeField] private GameObject _notificationPrefab;

        private GameObject _noficationPool;
        private Queue<NotificationView> _pool;
        private IDictionary<int, NotificationView> _shownNotifcations;

        void Awake() {
            _noficationPool = GameObject.Find("_NotificationPool") ?? new GameObject("_NotificationPool");
            _noficationPool.SetActive(false);
            const int poolSize = 16;
            _pool = new Queue<NotificationView>(poolSize);
            for (int i = 0; i < poolSize; i++) {
                var notification = Instantiate(_notificationPrefab).GetComponent<NotificationView>();
                notification.transform.SetParent(_noficationPool.transform, worldPositionStays: false);
                _pool.Enqueue(notification);
            }

            // TODO Use array dictionary instead
            _shownNotifcations = new Dictionary<int, NotificationView>();

            _notificationList.NotificationAdded += AddNotification;
            _notificationList.NotificationRemoved += RemoveNotification;
        }

        void OnDestroy() {
            _notificationList.NotificationAdded -= AddNotification;
            _notificationList.NotificationRemoved -= RemoveNotification;
        }

        public void AddNotification(int id, string message) {
            var notification = _pool.Dequeue();
            notification.gameObject.transform.SetParent(transform, worldPositionStays: false);
            notification.gameObject.transform.SetAsFirstSibling();
            notification.SetText(message);
            _shownNotifcations[id] = notification;
        }

        public void RemoveNotification(int id) {
            var notification = _shownNotifcations[id];
            notification.transform.SetParent(_noficationPool.transform, worldPositionStays: false);
            _pool.Enqueue(notification);
        }
    }
}
