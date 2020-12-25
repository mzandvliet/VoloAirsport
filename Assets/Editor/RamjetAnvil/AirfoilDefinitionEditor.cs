using RamjetAnvil.Volo;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(AirfoilDefinition)), CanEditMultipleObjects]
public class AirfoilDefinitionEditor : Editor {
    [SerializeField] private bool[] _filters;

    private void OnEnable() {
        if (_filters == null) {
            _filters = new bool[4];
            for (int i = 0; i < 4; i++) {
                _filters[i] = true;
            }
        }
    }

    public override void OnInspectorGUI() {
        /* 
         * Todo: Find curve range dynamically
         */

        const float barH = 50f;
        const float swatchH = 200f;
        Color background = new Color(0f, 0f, 0f, 0f);

        if (targets.Length == 1) {
            var def = target as AirfoilDefinition;

            Rect curveRange = new Rect(-10f, 0f, 110f, 1.5f);

            Rect liftRect = new Rect(0f, barH, Screen.width, swatchH);
            GUI.Box(liftRect, "", GUI.skin.box);
            for (int i = 0; i < def.Profiles.Length; i++) {
                if (!_filters[i]) {
                    continue;
                }

                Color c = Color.HSVToRGB(0.33f + i * 0.05f, 1f, 1f);
                EditorGUIUtility.DrawCurveSwatch(
                    liftRect,
                    def.Profiles[i].Lift, null,
                    c, background,
                    curveRange);
            }

            Rect dragRect = new Rect(0f, barH + swatchH, Screen.width, swatchH);
            GUI.Box(dragRect, "", GUI.skin.box);
            for (int i = 0; i < def.Profiles.Length; i++) {
                if (!_filters[i]) {
                    continue;
                }

                Color c = Color.HSVToRGB(0.00f + i * 0.05f, 1f, 1f);
                EditorGUIUtility.DrawCurveSwatch(
                    dragRect,
                    def.Profiles[i].Drag, null,
                    c, background,
                    curveRange);
            }

            GUILayout.BeginVertical();
            GUILayout.Space(swatchH); // Skip Lift view
            GUILayout.Space(swatchH); // Skip Drag view

            GUILayout.BeginHorizontal();
            for (int i = 0; i < 4; i++) {
                _filters[i] = GUILayout.Toggle(_filters[i], def.Profiles[i].Name);
            }
            GUILayout.EndHorizontal();

            DrawDefaultInspector();
            GUILayout.EndVertical();
        }
    }
}
