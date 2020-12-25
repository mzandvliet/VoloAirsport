using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using RamjetAnvil.Volo.Ui;
using UnityEngine;
using UnityEngine.UI;

namespace RamjetAnvil.Volo.Ui {
    public class NewsFeedItemView : MonoBehaviour {

        public static readonly CultureInfo enUsCulture = CultureInfo.CreateSpecificCulture("en-US");

        [SerializeField] private Text _title;
        [SerializeField] private Text _date;
        [SerializeField] private Text _description;
        [SerializeField] private ClickableUrl _link;

        public void Render(NewsFeedModel.Item item) {
            _title.text = item.Title;
            if (item.PublicationDate.HasValue) {
                _date.enabled = true;
                _date.text = item.PublicationDate.Value.ToString("dddd, d MMMM", enUsCulture);
            } else {
                _date.enabled = false;
            }
            
            _description.text = item.Description;
            _link.SetUrl("Read more...", item.Link);
        }
    }
}
