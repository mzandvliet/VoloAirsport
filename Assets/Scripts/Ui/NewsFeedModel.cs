using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Xml;
using RamjetAnvil.Coroutine;
using RamjetAnvil.DependencyInjection;
using RamjetAnvil.Unity.Utility;
using RestSharp.Contrib;
using UnityEngine;

namespace RamjetAnvil.Volo.Ui {

    public class NewsFeedModel : MonoBehaviour {

        [SerializeField] private string _newsFeedUrl;
        [Dependency, SerializeField] private UnityCoroutineScheduler _coroutineScheduler;

        private ISubject<Maybe<IList<Item>>> _newsItems;

        void Awake() {
            _newsItems = _newsItems ?? new ReplaySubject<Maybe<IList<Item>>>(1);
            _coroutineScheduler.Run(LoadNews());
        }

        private IEnumerator<WaitCommand> LoadNews() {
            var request = new WWW(_newsFeedUrl);
            while (!request.isDone) {
                yield return WaitCommand.WaitForNextFrame;
            }
            try {
                var rssDocument = new XmlDocument();
                rssDocument.LoadXml(request.text);
                var newsItems = new List<Item>();
                foreach (XmlNode node in rssDocument.SelectNodes("rss/channel/item")) {
                    DateTime? publicationDate = null;
                    if (node.SelectSingleNode("pubDate") != null) {
                        try {
                            //Tue, 11 Oct 2016 07:39:25 +0000
                            publicationDate = DateTime.ParseExact(node.SelectSingleNode("pubDate").InnerText,
                                "ddd, dd MMM yyyy HH:mm:ss zzz", CultureInfo.InvariantCulture);
                        } catch (Exception) {
                            Debug.LogWarning("Failed to parse publication date " +
                                             node.SelectSingleNode("pubDate").InnerText);
                        }
                    }
                    var newsItem = new Item(
                        title:
                        node.SelectSingleNode("title") != null
                            ? HttpUtility.HtmlDecode(node.SelectSingleNode("title").InnerText)
                            : "",
                        description:
                        node.SelectSingleNode("description") != null
                            ? HttpUtility.HtmlDecode(node.SelectSingleNode("description").InnerText)
                            : "",
                        link: node.SelectSingleNode("link") != null ? node.SelectSingleNode("link").InnerText : "",
                        publicationDate: publicationDate);
                    newsItems.Add(newsItem);
                }

                _newsItems.OnNext(Maybe.Just<IList<Item>>(newsItems));
            } catch (XmlException) {
                Debug.LogError("Could not read XML news feed from: " + _newsFeedUrl);
                _newsItems.OnNext(Maybe.Nothing<IList<Item>>());
            } catch (Exception e) {
                Debug.LogError("Failure trying to process news feed: " + e.Message);
            }
        }

        public IObservable<Maybe<IList<Item>>> NewsItems {
            get {
                _newsItems = _newsItems ?? new ReplaySubject<Maybe<IList<Item>>>(1);
                return _newsItems;
            }
        }

        public class Item {
            public readonly string Title;
            public readonly DateTime? PublicationDate;
            public readonly string Description;
            public readonly string Link;

            public Item(string title, DateTime? publicationDate, string description, string link) {
                Title = title;
                Description = description;
                Link = link;
                PublicationDate = publicationDate;
            }
        }
    }
}
