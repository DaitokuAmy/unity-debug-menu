using System;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Build;
#endif

namespace UnityDebugMenu {
    /// <summary>
    /// DebugMenu用の設定ファイル
    /// </summary>
    [CreateAssetMenu(fileName = "UnityDebugMenuConfig.asset", menuName = "Unity Debug Menu/Config")]
    public class DebugMenuConfig : ScriptableObject {
        /// <summary>
        /// DebugMenuの開き方
        /// </summary>
        [Serializable]
        public struct EditorMenuOpen {
            public bool shift;
            public bool alt;
            public bool controlOrCommand;
#if ENABLE_INPUT_SYSTEM
            public Key keycode;
#else
            public KeyCode keycode;
#endif
        }
        
        [Tooltip("最小のウィンドウサイズ(幅)")]
        public float minWindowWidth = 200.0f;
        [Tooltip("最小のウィンドウサイズ(高さ)")]
        public float minWindowHeight = 200.0f;
        [Tooltip("GUI表示の基準解像度(端末の大きい方の長さを比較に使用)")]
        public int baseResolution = 720;
        [Tooltip("DebugMenu表示条件(同時タッチ数)")]
        public int menuToggleTouchCount = 2;
        [Tooltip("DebugMenu表示条件(タッチ時間)")]
        public float menuToggleTouchTime = 3.0f;
        [Tooltip("DebugMenu表示条件(高速同時タッチ数)")]
        public int menuToggleFastTouchCount = 3;
        [Tooltip("DebugMenu表示条件(高速タッチ時間)")]
        public float menuToggleFastTouchTime = 1.0f;
        [Tooltip("Editor上でのDebugMenuの開き方")]
        public EditorMenuOpen editorMenuOpen = new() {
            shift = true,
            controlOrCommand = false,
            alt = false,
#if ENABLE_INPUT_SYSTEM
            keycode = Key.D,
#else
            keycode = KeyCode.D,
#endif
        };

        // GUI表示のEditor用スケール
        private float? _editorGuiScale = null;

        /// <summary>エディタ用のGUIScale</summary>
        public float EditorGUIScale {
            get {
#if UNITY_EDITOR
                if (!_editorGuiScale.HasValue) {
                    var key = string.Format($"{nameof(DebugMenuConfig)}://{nameof(EditorGUIScale)}");
                    _editorGuiScale = EditorPrefs.GetFloat(key, 1.0f);
                }

                return _editorGuiScale.Value;
#else
				return 1.0f;
#endif
            }
            set {
#if UNITY_EDITOR
                var prevValue = 1.0f;

                if (_editorGuiScale.HasValue) {
                    prevValue = _editorGuiScale.Value;
                }

                if ((value - prevValue) * (value - prevValue) <= float.Epsilon) {
                    return;
                }

                _editorGuiScale = value;

                var key = string.Format($"{nameof(DebugMenuConfig)}://{nameof(EditorGUIScale)}");
                EditorPrefs.SetFloat(key, _editorGuiScale.Value);
#endif
            }
        }

#if UNITY_EDITOR
        /// <summary>
        /// DebugMenuを有効化するDefineSymbolが設定されているかチェック
        /// </summary>
        /// <param name="buildTargetGroup">確認するTargetGroup</param>
        public static bool CheckDefineSymbol(BuildTargetGroup buildTargetGroup) {
            NamedBuildTarget namedBuildTarget;
            try {
                namedBuildTarget = NamedBuildTarget.FromBuildTargetGroup(buildTargetGroup);
            }
            catch {
                return false;
            }

            PlayerSettings.GetScriptingDefineSymbols(namedBuildTarget, out var defines);
            var defineList = defines.ToList();
            return defineList.Contains(DebugMenu.UseDefineSymbol);
        }

        /// <summary>
        /// DebugMenuを有効化するDefineSymbolを設定
        /// </summary>
        public static void SetDefineSymbol(bool use) {
            // DefineSymbolの変更
            var defineSymbol = DebugMenu.UseDefineSymbol;
            var targetGroups = ((BuildTarget[])Enum.GetValues(typeof(BuildTarget)))
                .Where(x => BuildPipeline.IsBuildTargetSupported(BuildPipeline.GetBuildTargetGroup(x), x))
                .Select(BuildPipeline.GetBuildTargetGroup)
                .ToArray();

            foreach (var buildTargetGroup in targetGroups) {
                NamedBuildTarget namedBuildTarget;
                try {
                    namedBuildTarget = NamedBuildTarget.FromBuildTargetGroup(buildTargetGroup);
                }
                catch {
                    continue;
                }

                PlayerSettings.GetScriptingDefineSymbols(namedBuildTarget, out var defines);
                var defineList = defines.ToList();

                if (use) {
                    if (!defineList.Contains(defineSymbol)) {
                        defineList.Add(defineSymbol);
                        PlayerSettings.SetScriptingDefineSymbols(namedBuildTarget, defineList.ToArray());
                    }
                }
                else {
                    if (defineList.Contains(defineSymbol)) {
                        defineList.Remove(defineSymbol);
                        PlayerSettings.SetScriptingDefineSymbols(namedBuildTarget, defineList.ToArray());
                    }
                }
            }

            AssetDatabase.SaveAssets();
        }
#endif
    }
}