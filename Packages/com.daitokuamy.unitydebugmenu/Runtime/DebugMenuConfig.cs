using System;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using Directory = UnityEngine.Windows.Directory;

#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.Build;
#endif

namespace UnityDebugMenu {
    /// <summary>
    /// DebugMenu用の設定ファイル
    /// </summary>
    public class DebugMenuConfig : ScriptableObject {
        /// <summary>
        /// DebugMenuの開き方
        /// </summary>
        [Serializable]
        public struct EditorMenuOpen {
            public bool active;
            public bool shift;
            public bool alt;
            public bool controlOrCommand;
#if ENABLE_INPUT_SYSTEM
            public Key keycode;
#else
            public KeyCode keycode;
#endif
        }

        private static DebugMenuConfig s_instance;

        [Tooltip("表示に利用するSkin")]
        public GUISkin skin;
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
            active = true,
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

        /// <summary>シングルトンインスタンス</summary>
        public static DebugMenuConfig Instance {
            get {
                if (s_instance != null) {
                    return s_instance;
                }

#if UNITY_EDITOR
                var config = Resources.FindObjectsOfTypeAll<DebugMenuConfig>().FirstOrDefault();
                if (config != null) {
                    var preloadedAssets = PlayerSettings.GetPreloadedAssets().ToList();
                    preloadedAssets.Add(config);
                    PlayerSettings.SetPreloadedAssets(preloadedAssets.ToArray());
                    AssetDatabase.SaveAssets();
                }

                s_instance = config;
#endif

                return s_instance;
            }
        }

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

        /// <summary>
        /// アクティブ時処理
        /// </summary>
        private void OnEnable() {
            if (s_instance == null) {
                s_instance = this;
            }
        }

#if UNITY_EDITOR
        /// <summary>
        /// コンフィグファイルの生成処理
        /// </summary>
        /// <exception cref="InvalidOperationException"></exception>
        [MenuItem("Assets/Create/Unity Debug Menu/Config Data")]
        private static void CreateConfigData() {
            // 既に存在していたらエラー
            var config = PlayerSettings.GetPreloadedAssets().OfType<DebugMenuConfig>().FirstOrDefault();
            if (config != null) {
                throw new InvalidOperationException($"{nameof(DebugMenuConfig)} already exists in preloaded assets.");
            }

            var assetPath = EditorUtility.SaveFilePanelInProject($"Save {nameof(DebugMenuConfig)}", nameof(DebugMenuConfig), "asset", "", "Assets");
            if (string.IsNullOrEmpty(assetPath)) {
                return;
            }

            // フォルダがなかったら作る
            var folderPath = Path.GetDirectoryName(assetPath);
            if (!string.IsNullOrEmpty(folderPath) && !Directory.Exists(folderPath)) {
                Directory.CreateDirectory(folderPath);
            }

            // アセットを作成してPreloadedAssetsに設定
            var instance = CreateInstance<DebugMenuConfig>();
            instance.skin = AssetDatabase.LoadAssetAtPath<GUISkin>("Packages/com.daitokuamy.unitydebugmenu/Runtime/Skin/UnityDebugMenuSkin.guiskin");
            AssetDatabase.CreateAsset(instance, assetPath);
            var preloadedAssets = PlayerSettings.GetPreloadedAssets().ToList();
            preloadedAssets.Add(instance);
            PlayerSettings.SetPreloadedAssets(preloadedAssets.ToArray());
            AssetDatabase.SaveAssets();
        }

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