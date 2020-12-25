using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RamjetAnvil.Coroutine;
using RamjetAnvil.InputModule;
using RamjetAnvil.Unity.Utility;
using RamjetAnvil.Volo.Ui;
using RamjetAnvil.Volo.Util;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace RamjetAnvil.Volo.Ui {

    public class NewsFeedView : MonoBehaviour, IUiContext {

        [SerializeField] private int _maxNewsItems;
        [SerializeField] private Button _continueButton;
        [SerializeField] private GameObject _loadingPlaceholder;
        [SerializeField] private int _maxItemLength = 200;
        [SerializeField] private NewsFeedModel _model;
        [SerializeField] private Transform _newsItemsParent;
        [SerializeField] private GameObject _newsItemPrefab;

        public UnityEvent OnContinue {
            get { return _continueButton.onClick; }
        }

        public GameObject FirstObject {
            get {
                return _continueButton.gameObject;
            }
        }

        void Awake() {
            _model.NewsItems.Subscribe(Render);
        }

        private void Render(Maybe<IList<NewsFeedModel.Item>> newsItems) {
            _loadingPlaceholder.SetActive(false);

            if (newsItems.IsJust) {
                for (var i = 0; i < newsItems.Value.Count; i++) {
                    if (i >= _maxNewsItems) {
                        break;
                    }
                    var newsItem = newsItems.Value[i];
                    newsItem = new NewsFeedModel.Item(
                        title: newsItem.Title,
                        description: newsItem.Description.Limit(_maxItemLength, trailingChars: "..."),
                        link: newsItem.Link,
                        publicationDate: newsItem.PublicationDate);
                    var itemView = Instantiate(_newsItemPrefab).GetComponent<NewsFeedItemView>();
                    itemView.Render(newsItem);
                    itemView.transform.SetParent(_newsItemsParent);
                    itemView.transform.localPosition = Vector3.zero;
                    itemView.transform.localRotation = Quaternion.identity;
                    itemView.transform.localScale = Vector3.one;
                }
            } else {
                Debug.LogError("Failed to fetch news");
            }
        }
    }
}
