using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace RamjetAnvil.Volo.Ui {
    public class SettingsView : MonoBehaviour {

        [SerializeField] private GameObject _titles;
        [SerializeField] private GameObject _content;

        public void AddWidget(GameObject title, GameObject content) {
            title.transform.SetParent(_titles.transform);
            title.transform.localRotation = Quaternion.identity;
            title.transform.localScale = Vector3.one;
            var titlePosition = title.transform.localPosition;
            titlePosition.z = 0;
            title.transform.localPosition = titlePosition;

            content.transform.SetParent(_content.transform);
            content.transform.localRotation = Quaternion.identity;
            content.transform.localScale = Vector3.one;
            var contentPosition = content.transform.localPosition;
            contentPosition.z = 0;
            content.transform.localPosition = contentPosition;
        }
    }
}
