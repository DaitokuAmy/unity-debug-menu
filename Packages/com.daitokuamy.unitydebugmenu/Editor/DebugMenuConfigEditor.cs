using UnityEditor;
using UnityEngine;

namespace UnityDebugMenu.Editor {
    /// <summary>
    /// DebugMenuConfigのインスペクタ拡張
    /// </summary>
    [CustomEditor(typeof(DebugMenuConfig))]
    public class DebugMenuConfigEditor : UnityEditor.Editor {
        /// <summary>
        /// インスペクタ拡張
        /// </summary>
        public override void OnInspectorGUI() {
            base.OnInspectorGUI();

            var config = target as DebugMenuConfig;

            if (config == null) {
                return;
            }

            // EditorGUIScale
            config.EditorGUIScale =
                EditorGUILayout.Slider("Editor GUI Scale", config.EditorGUIScale, 0.1f, 10.0f);
            
            EditorGUILayout.Space();

            // 選択中BuildTargetGroupの状態表示
            var selectedBuildTargetGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
            EditorGUILayout.LabelField("Define Symbol Status", EditorStyles.boldLabel);
            EditorGUILayout.LabelField(selectedBuildTargetGroup.ToString(), DebugMenuConfig.CheckDefineSymbol(selectedBuildTargetGroup).ToString());
            
            // DefineSymbolの設定
            using (new EditorGUILayout.HorizontalScope()) {
                if (GUILayout.Button("Set Define Symbol", GUILayout.Width(Screen.width * 0.5f - 13))) {
                    DebugMenuConfig.SetDefineSymbol(true);
                }
                if (GUILayout.Button("Reset Define Symbol", GUILayout.Width(Screen.width * 0.5f - 13))) {
                    DebugMenuConfig.SetDefineSymbol(false);
                }
            }
        }
    }
}