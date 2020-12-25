#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace RTEditor
{
    /// <summary>
    /// Helper class which allows the client code to retrieve editor GUI styles
    /// that can be used in different kinds of situations.
    /// </summary>
    public static class EditorGUIStyles
    {
        #region Public Static Functions
        /// <summary>
        /// Returns a GUI style that can be used to draw labels which are used
        /// to supply information to the user.
        /// </summary>
        public static GUIStyle GetInformativeLabelStyle()
        {
            var informativeLabelStyle = new GUIStyle();
            informativeLabelStyle.wordWrap = true;
            informativeLabelStyle.alignment = TextAnchor.UpperLeft;
            informativeLabelStyle.normal.textColor = EditorGUIUtility.isProSkin ? Color.white : Color.black;

            return informativeLabelStyle;
        }
        #endregion
    }
}
#endif
