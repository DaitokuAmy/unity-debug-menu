using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UnityDebugMenu {
    /// <summary>
    /// DebugMenu
    /// </summary>
    public partial class DebugMenu : MonoBehaviour {
        // DebugMenuを有効にするdefine symbol
        internal const string UseDefineSymbol = "USE_UNITY_DEBUG_MENU";

        /// <summary>
        /// 表示モード
        /// </summary>
        public enum Mode {
            All,
            ContentsOnly,
        }

        /// <summary>
        /// DebugMenu登録時のItem解放用ハンドル
        /// </summary>
        public struct ItemHandle : IDisposable {
            /// <summary>空のHandle</summary>
            public static readonly ItemHandle Empty = new ItemHandle();

            private readonly string _registeredPath;

            /// <summary>
            /// コンストラクタ
            /// </summary>
            /// <param name="path"></param>
            internal ItemHandle(string path) {
                _registeredPath = path;
            }

            /// <summary>
            /// 解放処理
            /// </summary>
            public void Dispose() {
                if (!string.IsNullOrEmpty(_registeredPath)) {
                    RemoveItem(_registeredPath);
                }
            }
        }

        // Singletonインスタンス
        private static DebugMenu s_instance;

        // 背面の操作ON/OFF切り替え処理
        private Action<bool> _onChangeActiveBackground;
        // デバッグメニューにデフォルト表示を行うアクション
        private Action _defaultViewAction = null;

        // Menuの表示状態
        private bool _visible;
        // Menuの表示切り替えインターフェース
        private IDebugMenuHandler _handler;

        // 項目一覧
        private Item _rootItem = null;
        // 開いている項目
        private Item _openItem = null;
        // 登録されたウィンドウ
        private List<Window> _debugWindows = null;
        // デバッグ画面下へのタッチ有効化状態
        private bool _backgroundActive = false;

        /// <summary>表示状態</summary>
        public static bool IsVisible => Instance != null && Instance._visible;
        /// <summary>シングルトンインスタンス</summary>
        private static DebugMenu Instance {
            get {
                if (s_instance != null) {
                    return s_instance;
                }

                var singletonObject = new GameObject(nameof(DebugMenu), typeof(DebugMenu));
                DontDestroyOnLoad(singletonObject);
                s_instance = singletonObject.GetComponent<DebugMenu>();
                return s_instance;
            }
        }

        /// <summary>表示モード</summary>
        internal Mode ViewMode { get; set; }
        /// <summary>GUI描画用スキン</summary>
        internal GUISkin DebugGuiSkin => Config?.skin;
        /// <summary>設定ファイル</summary>
        internal DebugMenuConfig Config { get; set; }
        /// <summary>表示に使っているScreen領域</summary>
        internal Rect ScreenRect { get; set; }
        /// <summary>ウィンドウ内のボタンサイズ</summary>
        internal float ButtonSize => 24.0f;
        /// <summary>GUI表示の基準解像度(高)</summary>
        internal int BaseResolution => Config.baseResolution;
        /// <summary>GUI表示のエディタ用スケール</summary>
        internal float EditorGuiScale => Config.EditorGUIScale;
        /// <summary>DebugMenu表示条件(同時タッチ数)</summary>
        internal int MenuToggleTouchCount => Config.menuToggleTouchCount;
        /// <summary>DebugMenu表示条件(タッチ時間)</summary>
        internal float MenuToggleTouchTime => Config.menuToggleTouchTime;
        /// <summary>DebugMenu表示条件(高速同時タッチ数)</summary>
        internal int MenuToggleFastTouchCount => Config.menuToggleFastTouchCount;
        /// <summary>DebugMenu表示条件(高速タッチ時間)</summary>
        internal float MenuToggleFastTouchTime => Config.menuToggleFastTouchTime;

        // 表示スケール
        internal float DrawScale {
            get {
#if UNITY_EDITOR
                var scale = EditorGuiScale;
#else
                var scale = 1.0f;
#endif
                
                // 高さを基準にする
                var targetResolution = Mathf.Max(Screen.height, Screen.width);
                scale *= targetResolution / (float)BaseResolution;

                return scale;
            }
        }

        /// <summary>
        /// メニューに常時表示する内容のアクションを登録
        /// </summary>
        [System.Diagnostics.Conditional(UseDefineSymbol)]
        public static void SetDefaultView(Action action) {
            Instance.SetDefaultViewInternal(action);
        }

        /// <summary>
        /// 背景タッチの有効無効切り替えボタンの拡張
        /// </summary>
        [System.Diagnostics.Conditional(UseDefineSymbol)]
        public static void SetChangeActiveBackgroundFunction(Action<bool> action) {
            Instance._onChangeActiveBackground = action;
        }

        /// <summary>
        /// 解像度変更時のリセット機能
        /// </summary>
        [System.Diagnostics.Conditional(UseDefineSymbol)]
        public static void ResetResolution() {
            Instance.CalcScreenRect();
        }

        /// <summary>
        /// コンテンツの追加
        /// </summary>
        public static ItemHandle AddItem(string path, ExecuteHandler onExecute, DisposeHandler onDispose = null) {
            if (Instance == null || !Application.isPlaying) {
                return ItemHandle.Empty;
            }

#if USE_UNITY_DEBUG_MENU
            Instance.AddItemInternal(path, onExecute, onDispose);
#endif
            return new ItemHandle(path);
        }

        /// <summary>
        /// ウィンドウの追加
        /// </summary>
        /// <param name="path">追加するパス</param>
        /// <param name="onDraw">描画処理</param>
        /// <param name="windowRect">Windowの位置/サイズ</param>
        /// <param name="enableScrollBar">スクロールバーを表示するか</param>
        public static ItemHandle AddWindowItem(string path, DrawHandler onDraw, Rect windowRect, bool enableScrollBar = true) {
            if (Instance == null || !Application.isPlaying) {
                return ItemHandle.Empty;
            }
            
#if USE_UNITY_DEBUG_MENU
            var splitPaths = path.Split('/');
            var window = new Window(splitPaths[splitPaths.Length - 1], onDraw, windowRect,
                enableScrollBar);
            return AddItem(path, _ => window.Open(path), _ => window.Close());
#else
            return ItemHandle.Empty;
#endif
        }

        /// <summary>
        /// ウィンドウの追加
        /// </summary>
        /// <param name="path">追加するパス</param>
        /// <param name="onDraw">描画処理</param>
        public static ItemHandle AddWindowItem(string path, DrawHandler onDraw) {
            return AddWindowItem(path, onDraw, Window.DefaultRect);
        }

        /// <summary>
        /// ウィンドウの追加
        /// </summary>
        /// <param name="path">追加するパス</param>
        /// <param name="onDraw">描画処理</param>
        /// <param name="windowScale">Windowのサイズに対するスケール値</param>
        public static ItemHandle AddWindowItem(string path, DrawHandler onDraw, Vector2 windowScale) {
            var defaultRect = Window.DefaultRect;
            return AddWindowItem(path, onDraw, new Rect(defaultRect.min, new Vector2(defaultRect.width * windowScale.x, defaultRect.height * windowScale.y)));
        }

        /// <summary>
        /// ウィンドウの追加
        /// </summary>
        /// <param name="path">追加するパス</param>
        /// <param name="onDrawHeader">ヘッダー部分のGUI描画用コール</param>
        /// <param name="onDraw">GUI描画用コールバック</param>
        /// <param name="onDrawFooter">フッター部分のGUI描画用コール</param>
        /// <param name="windowRect">Windowのサイズを表すRect</param>
        /// <param name="enableScrollBar">Scrollbarを使うか</param>
        public static ItemHandle AddWindowItem(string path, DrawHeaderHandler onDrawHeader, DrawHandler onDraw, DrawFooterHandler onDrawFooter, Rect windowRect, bool enableScrollBar = true) {
            if (Instance == null || !Application.isPlaying) {
                return ItemHandle.Empty;
            }
            
#if USE_UNITY_DEBUG_MENU
            var splitPaths = path.Split('/');
            var window = new Window(splitPaths[splitPaths.Length - 1], onDrawHeader, onDraw, onDrawFooter,
                windowRect, enableScrollBar);
            return AddItem(path, _ => window.Open(path), _ => window.Close());
#else
            return ItemHandle.Empty;
#endif
        }

        /// <summary>
        /// ウィンドウの追加
        /// </summary>
        /// <param name="path">追加するパス</param>
        /// <param name="onDrawHeader">ヘッダー部分のGUI描画用コール</param>
        /// <param name="onDraw">GUI描画用コールバック</param>
        /// <param name="onDrawFooter">フッター部分のGUI描画用コール</param>
        public static ItemHandle AddWindowItem(string path, DrawHeaderHandler onDrawHeader, DrawHandler onDraw, DrawFooterHandler onDrawFooter) {
            return AddWindowItem(path, onDrawHeader, onDraw, onDrawFooter, Window.DefaultRect);
        }

        /// <summary>
        /// ウィンドウの追加
        /// </summary>
        /// <param name="path">追加するパス</param>
        /// <param name="onDrawHeader">ヘッダー部分のGUI描画用コール</param>
        /// <param name="onDraw">GUI描画用コールバック</param>
        /// <param name="onDrawFooter">フッター部分のGUI描画用コール</param>
        /// <param name="windowScale">Windowのサイズに対するスケール値</param>
        public static ItemHandle AddWindowItem(string path, DrawHeaderHandler onDrawHeader, DrawHandler onDraw, DrawFooterHandler onDrawFooter, Vector2 windowScale) {
            var defaultRect = Window.DefaultRect;
            return AddWindowItem(path, onDrawHeader, onDraw, onDrawFooter, new Rect(defaultRect.min, new Vector2(defaultRect.width * windowScale.x, defaultRect.height * windowScale.y)));
        }

        /// <summary>
        /// コンテンツの削除
        /// </summary>
        [System.Diagnostics.Conditional(UseDefineSymbol)]
        public static void RemoveItem(string path) {
            if (s_instance == null || !Application.isPlaying) {
                return;
            }

            Instance.RemoveItemInternal(path);
        }

        /// <summary>
        /// コンテンツを開く
        /// </summary>
        [System.Diagnostics.Conditional(UseDefineSymbol)]
        public static void OpenItem(string path) {
            if (s_instance == null || !Application.isPlaying) {
                return;
            }

            Instance.OpenItemInternal(path);
        }

        /// <summary>
        /// ウィンドウを閉じる
        /// </summary>
        /// <param name="path">Windowを開いたときのパス</param>
        [System.Diagnostics.Conditional(UseDefineSymbol)]
        public static void CloseWindowItem(string path) {
            if (s_instance == null || !Application.isPlaying) {
                return;
            }

            Instance.CloseWindowInternal(path);
        }

        /// <summary>
        /// デバッグウィンドウの登録
        /// </summary>
        [System.Diagnostics.Conditional(UseDefineSymbol)]
        public static void SetVisible(bool isVisible) {
            if (s_instance == null || !Application.isPlaying) {
                return;
            }

            Instance.SetMenuVisible(isVisible);
        }

        /// <summary>
        /// DebugMenu制御用ハンドラを設定
        /// </summary>
        [System.Diagnostics.Conditional(UseDefineSymbol)]
        public static void SetHandler(IDebugMenuHandler handler) {
            Instance.SetHandlerInternal(handler);
        }

        /// <summary>
        /// DebugMenu制御用ハンドラにデフォルトの物を設定
        /// </summary>
        public static void SetDefaultHandler() {
            SetHandler(new DefaultDebugMenuHandler(Instance.Config));
        }

        /// <summary>
        /// コンテンツの追加
        /// </summary>
        private void AddItemInternal(string path, ExecuteHandler onExecute, DisposeHandler onDispose) {
            var lastItem = default(Item);
            var lastIndex = -1;
            var splitPaths = path.Split('/');

            if (_rootItem.FindItem(out lastItem, out lastIndex, splitPaths)) {
                // 既に存在するメニュー
                lastItem.OnExecute += onExecute;
                lastItem.OnDispose += onDispose;
            }
            else {
                // メニューの追加
                var baseItem = lastItem;

                for (var i = lastIndex; i < splitPaths.Length; i++) {
                    var childItem = new Item(splitPaths[i], baseItem);
                    baseItem.Children.Add(childItem);
                    baseItem = childItem;

                    if (i == splitPaths.Length - 1) {
                        baseItem.OnExecute += onExecute;
                        baseItem.OnDispose += onDispose;
                    }
                }
            }
        }

        /// <summary>
        /// コンテンツの追加
        /// </summary>
        private void SetDefaultViewInternal(Action action) {
            _defaultViewAction = action;
        }

        /// <summary>
        /// スタイル取得
        /// </summary>
        private GUIStyle GetCustomStyle(string styleName) {
            if (DebugGuiSkin == null) {
                return GUIStyle.none;
            }

            var style = DebugGuiSkin.customStyles.FirstOrDefault(x => x.name == styleName);

            if (style == null) {
                return GUIStyle.none;
            }

            return style;
        }

        /// <summary>
        /// コンテンツの削除
        /// </summary>
        private void RemoveItemInternal(string path) {
            var splitPaths = path.Split('/');

            if (_rootItem.FindItem(out var lastItem, out _, splitPaths)) {
                // 存在するPathなら階層的に削除
                var child = lastItem;
                var parent = lastItem.Parent;

                do {
                    // 開いていたら困るので閉じる処理を呼ぶ
                    CloseItem(child);

                    // 親から外れる
                    child.Dispose();
                    parent.Children.Remove(child);
                    child = parent;
                    parent = parent.Parent;
                }

                // 子がいなくなった親は連動して削除
                while (parent != null && child.Children.Count <= 0);
            }
        }

        /// <summary>
        /// 該当Itemを開く
        /// </summary>
        private void OpenItemInternal(string path) {
            var splitPaths = path.Split('/');

            if (_rootItem.FindItem(out var lastItem, out _, splitPaths)) {
                OpenItem(lastItem);
                lastItem.Open();
            }
        }

        /// <summary>
        /// Windowを閉じる
        /// </summary>
        private void CloseWindowInternal(string path) {
            foreach (var window in _debugWindows) {
                if (window.IsOpen && window.OpenPath == path) {
                    window.Close();
                }
            }
        }

        /// <summary>
        /// Handlerの設定
        /// </summary>
        private void SetHandlerInternal(IDebugMenuHandler handler) {
            if (!Application.isPlaying) {
                return;
            }

            _handler = handler;
        }

        /// <summary>
        /// 項目を開く
        /// </summary>
        private void OpenItem(Item item) {
            if (item.IsLeaf) {
                return;
            }

            _openItem = item;
        }

        /// <summary>
        /// 項目を閉じる
        /// </summary>
        private void CloseItem(Item item) {
            if (item != _openItem) {
                return;
            }

            _openItem = _rootItem;
        }

        /// <summary>
        /// デバッグウィンドウの登録
        /// </summary>
        private void RegisterWindow(Window window) {
            if (!Application.isPlaying) {
                return;
            }

            _debugWindows.Add(window);
        }

        /// <summary>
        /// メニューの描画
        /// </summary>
        private void DrawMenu() {
            GUILayout.BeginVertical("Box", GUILayout.Width(ScreenRect.width));

            DrawHeader();

            if (ViewMode == Mode.All) {
                _openItem?.Draw();
                _defaultViewAction?.Invoke();
            }

            GUILayout.EndVertical();
        }

        /// <summary>
        /// ウィンドウの描画
        /// </summary>
        private void DrawWindow() {
            if (_debugWindows == null) {
                return;
            }

            foreach (var window in _debugWindows) {
                window?.Draw();
            }
        }

        /// <summary>
        /// 描画前処理
        /// </summary>
        private void BeginDraw() {
            // Matrix設定
            GUIUtility.ScaleAroundPivot(Vector2.one * DrawScale, Vector2.zero);

#if UNITY_EDITOR
            // Resolutionの更新
            CalcScreenRect();
#endif
        }

        /// <summary>
        /// 描画後処理
        /// </summary>
        private void EndDraw() {
            // スケール戻す
            GUI.matrix = Matrix4x4.identity;
        }

        /// <summary>
        /// 描画用のScreen領域計算
        /// </summary>
        private void CalcScreenRect() {
            var drawScale = DrawScale;

            // ScreenRect(DrawScale考慮済み描画領域)の計算
            var safeArea = Screen.safeArea;
#if UNITY_EDITOR || UNITY_STANDALONE
            var screenWidth = Screen.width;
            var screenHeight = Screen.height;
#else
			var screenWidth = Screen.currentResolution.width;
			var screenHeight = Screen.currentResolution.height;
#endif

            if (safeArea.width <= float.Epsilon || safeArea.height <= float.Epsilon) {
                safeArea = new Rect(0.0f, 0.0f, screenWidth, screenHeight);
            }

#if !UNITY_EDITOR && !UNITY_STANDALONE
			// StatusBarを避ける
			if( safeArea.yMin < 40.0f )
			{
				safeArea.yMin = 40.0f;
			}

#endif

            var screenRect = safeArea;
            screenRect.min /= drawScale;
            screenRect.max /= drawScale;
            ScreenRect = screenRect;
        }

        /// <summary>
        /// メニューのヘッダを描画
        /// </summary>
        private void DrawHeader() {
            GUILayout.BeginHorizontal();

            if (GUILayout.Button("", GetCustomStyle($"backbutton{(_openItem?.Parent == null ? " off" : "")}"), GUILayout.Width(ButtonSize), GUILayout.Height(ButtonSize))) {
                _openItem?.Back();
            }

            var currentFontSize = DebugGuiSkin.label.fontSize;
            GUI.skin.label.fontSize = (int)ButtonSize - DebugGuiSkin.label.margin.vertical;
            var str = _openItem != null ? _openItem.FullPath : "";
            GUILayout.Label(str, GUILayout.Width(ScreenRect.width - 230.0f),
                GUILayout.Height(ButtonSize));
            GUI.skin.label.fontSize = currentFontSize;

            GUILayout.FlexibleSpace();

            if (_onChangeActiveBackground != null) {
                var prevColor = GUI.color;
                GUI.color = _backgroundActive ? Color.green : Color.gray;
                var buttonName = _backgroundActive ? "タッチ有効" : "タッチ無効";

                if (GUILayout.Button(buttonName, GUILayout.Height(ButtonSize))) {
                    SetActiveBackground(!_backgroundActive);
                }

                GUI.color = prevColor;
            }

            if (GUILayout.Button("",
                    ViewMode == Mode.All ? GetCustomStyle("minusbutton") : GetCustomStyle("plusbutton"),
                    GUILayout.Width(ButtonSize), GUILayout.Height(ButtonSize))) {
                ViewMode = ViewMode == Mode.All ? Mode.ContentsOnly : Mode.All;
            }

            if (GUILayout.Button("", GetCustomStyle("closebutton"), GUILayout.Width(ButtonSize), GUILayout.Height(ButtonSize))) {
                Reset();
            }

            GUILayout.EndHorizontal();
        }

        /// <summary>
        /// メニューの状態を初期化
        /// </summary>
        private void Reset() {
            CalcScreenRect();
            SetMenuVisible(false);
            ViewMode = Mode.All;
            _openItem = _rootItem;
        }

        /// <summary>
        /// 生成処理
        /// </summary>
        private void Awake() {
            _rootItem = new Item("", null);
            _debugWindows = new List<Window>();

            // 設定ファイルを取得
            Config = DebugMenuConfig.Instance;

            // Handlerの初期設定
            SetHandlerInternal(new DefaultDebugMenuHandler(Config));

            Reset();
        }

        /// <summary>
        /// 廃棄時処理
        /// </summary>
        private void OnDestroy() {
            if (s_instance == this) {
                s_instance = null;
            }
        }

        /// <summary>
        /// 更新処理
        /// </summary>
        private void Update() {
#if !USE_UNITY_DEBUG_MENU
            return;
#endif

            // 表示モード切替
            if (_handler != null && _handler.CheckVisible(this, Time.unscaledDeltaTime)) {
                SetMenuVisible(!_visible);
            }

            if (_debugWindows != null) {
                foreach (var window in _debugWindows) {
                    window?.UpdateWindow();
                }
            }
        }

        /// <summary>
        /// 表示切替時の処理
        /// </summary>
        private void SetMenuVisible(bool isVisible) {
            _visible = isVisible;

            // 開き直してもタッチ無効フラグを維持したい
            if (!_backgroundActive) {
                _onChangeActiveBackground?.Invoke(!_visible);
            }
        }

        /// <summary>
        /// デバッグメニューに重なっている部分他UIへのタッチを無効にするかどうかを設定する
        /// </summary>
        private void SetActiveBackground(bool enable) {
            _backgroundActive = enable;
            _onChangeActiveBackground?.Invoke(enable);
        }

        /// <summary>
        /// GUI描画
        /// </summary>
        private void OnGUI() {
#if !USE_UNITY_DEBUG_MENU
            return;
#endif

            // 描画開始
            BeginDraw();

            using (new GUILayout.HorizontalScope()) {
                // SafeArea
                GUILayout.Space(ScreenRect.xMin);

                using (new GUILayout.VerticalScope()) {
                    // SafeArea
                    GUILayout.Space(ScreenRect.yMin);

                    if (_visible) {
                        // メニュー描画
                        DrawMenu();
                        // ウィンドウ描画
                        DrawWindow();
                    }
                }
            }

            // 描画終了
            EndDraw();
        }
    }
}