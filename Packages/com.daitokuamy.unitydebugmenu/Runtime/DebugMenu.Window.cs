using UnityEngine;

namespace UnityDebugMenu {
    /// <summary>
    /// DebugMenu
    /// </summary>
    partial class DebugMenu {
        // Window内描画ハンドラ
        public delegate void DrawHeaderHandler(Rect windowRect);
        public delegate void DrawHandler(Rect windowRect);
        public delegate void DrawFooterHandler(Rect windowRect);

        // 次の生成されるWindowのID
        private static int s_nextWindowID = 0;

        /// <summary>
        /// デバッグウィンドウクラス
        /// </summary>
        public class Window {
            // 最小のウィンドウサイズ
            private static float MinWindowWidth => Instance.Config.minWindowWidth;
            private static float MinWindowHeight => Instance.Config.minWindowHeight;
            // ウィンドウ内のボタンサイズ
            private static float WindowButtonWidth => 24;
            private static float WindowButtonHeight => 24;
            // ウィンドウの初期マージン
            private static float WindowSizeMargin => 5.0f;

            // デフォルトのウィンドウサイズ
            private static Rect? s_defaultRect = null;

            public static Rect DefaultRect {
                get {
                    if (!s_defaultRect.HasValue) {
                        var screenRect = Instance.ScreenRect;
                        var rect = screenRect;
                        rect.min += new Vector2(WindowSizeMargin, Instance.ButtonSize * 2);
                        rect.max -= new Vector2(WindowSizeMargin, WindowSizeMargin);
                        s_defaultRect = rect;
                    }

                    return s_defaultRect.Value;
                }
            }

            // 識別ID
            public int ID { get; private set; }
            // タイトル名
            public string Title { get; private set; }
            // 開いているか
            public bool IsOpen { get; private set; }
            // ウィンドウサイズ
            public Rect WindowRect { get; set; }
            // 描画処理
            public DrawHeaderHandler OnDrawHeader { get; set; }
            public DrawHandler OnDraw { get; set; }
            public DrawFooterHandler OnDrawFooter { get; set; }
            // マウスポジション保存用
            private Vector3 _beforeMousePosition;
            // 拡縮領域をタッチしているかどうか
            private bool _isTouchScaleArea;
            // 拡縮領域をタッチしているかどうか
            private bool _enableScrollBar;
            // スクロールバーの位置
            private Vector2 _scrollBarPos;
            // スクロール領域をタッチしているかどうか
            private bool _isTouchScrollArea;
            // windowのスタイル
            private GUIStyle _windowStyle;
            // 閉じるボタンのスタイル
            private GUIStyle _closeButtonStyle;

            // タッチスクロールの変異量
            private Vector2 _diffTouchScrollPosition;

            /// <summary>
            /// コンストラクタ
            /// </summary>
            /// <param name="title">Windowタイトル</param>
            /// <param name="onDraw">GUI描画用コールバック</param>
            /// <param name="windowRect">Windowのサイズを表すRect</param>
            /// <param name="enableScrollBar">Scrollbarを使うか</param>
            public Window(string title, DrawHandler onDraw, Rect windowRect, bool enableScrollBar) {
                ID = s_nextWindowID++;
                Title = title;
                IsOpen = false;
                WindowRect = windowRect;
                OnDraw = onDraw;
                _beforeMousePosition = Vector2.zero;
                _isTouchScaleArea = false;
                // DebugMenuに登録
                Instance.RegisterWindow(this);
                // スクロールバーを使うかどうか
                _enableScrollBar = enableScrollBar;
                // スクロールバーの初期位置
                _scrollBarPos = Vector2.zero;
                // windowスタイル
                _windowStyle = Instance.GetCustomStyle("debugwindow");
                // 閉じるボタン
                _closeButtonStyle = Instance.GetCustomStyle("closebutton");
            }

            /// <summary>
            /// コンストラクタ
            /// </summary>
            /// <param name="title">Windowタイトル</param>
            /// <param name="onDraw">GUI描画用コールバック</param>
            /// <param name="windowRect">Windowのサイズを表すRect</param>
            public Window(string title, DrawHandler onDraw, Rect windowRect)
                : this(title, onDraw, windowRect, true) {
            }

            /// <summary>
            /// コンストラクタ
            /// </summary>
            /// <param name="title">Windowタイトル</param>
            /// <param name="onDraw">GUI描画用コールバック</param>
            public Window(string title, DrawHandler onDraw)
                : this(title, onDraw, DefaultRect) {
            }

            /// <summary>
            /// コンストラクタ
            /// </summary>
            /// <param name="title">Windowタイトル</param>
            /// <param name="onDraw">GUI描画用コールバック</param>
            /// <param name="windowScale">DefaultWindowサイズをベースにしたScale</param>
            public Window(string title, DrawHandler onDraw, Vector2 windowScale)
                : this(title, onDraw,
                    new Rect(DefaultRect.min,
                        new Vector2(DefaultRect.width * windowScale.x, DefaultRect.height * windowScale.y))) {
            }

            /// <summary>
            /// コンストラクタ
            /// </summary>
            /// <param name="title">Windowタイトル</param>
            /// <param name="onDrawHeader">ヘッダー部分のGUI描画用コール</param>
            /// <param name="onDraw">GUI描画用コールバック</param>
            /// <param name="onDrawFooter">フッター部分のGUI描画用コール</param>
            /// <param name="windowRect">Windowのサイズを表すRect</param>
            /// <param name="enableScrollBar">Scrollbarを使うか</param>
            public Window(string title, DrawHeaderHandler onDrawHeader, DrawHandler onDraw, DrawFooterHandler onDrawFooter, Rect windowRect, bool enableScrollBar)
                : this(title, onDraw, windowRect, enableScrollBar) {
                OnDrawHeader = onDrawHeader;
                OnDrawFooter = onDrawFooter;
            }

            /// <summary>
            /// コンストラクタ
            /// </summary>
            /// <param name="title">Windowタイトル</param>
            /// <param name="onDrawHeader">ヘッダー部分のGUI描画用コール</param>
            /// <param name="onDraw">GUI描画用コールバック</param>
            /// <param name="onDrawFooter">フッター部分のGUI描画用コール</param>
            /// <param name="windowRect">Windowのサイズを表すRect</param>
            public Window(string title, DrawHeaderHandler onDrawHeader, DrawHandler onDraw, DrawFooterHandler onDrawFooter, Rect windowRect)
                : this(title, onDrawHeader, onDraw, onDrawFooter, windowRect, true) {
            }

            /// <summary>
            /// コンストラクタ
            /// </summary>
            /// <param name="title">Windowタイトル</param>
            /// <param name="onDrawHeader">ヘッダー部分のGUI描画用コール</param>
            /// <param name="onDraw">GUI描画用コールバック</param>
            /// <param name="onDrawFooter">フッター部分のGUI描画用コール</param>
            public Window(string title, DrawHeaderHandler onDrawHeader, DrawHandler onDraw, DrawFooterHandler onDrawFooter)
                : this(title, onDrawHeader, onDraw, onDrawFooter, DefaultRect) {
            }

            /// <summary>
            /// コンストラクタ
            /// </summary>
            /// <param name="title">Windowタイトル</param>
            /// <param name="onDrawHeader">ヘッダー部分のGUI描画用コール</param>
            /// <param name="onDraw">GUI描画用コールバック</param>
            /// <param name="onDrawFooter">フッター部分のGUI描画用コール</param>
            /// <param name="windowScale">DefaultWindowサイズをベースにしたScale</param>
            public Window(string title, DrawHeaderHandler onDrawHeader, DrawHandler onDraw, DrawFooterHandler onDrawFooter, Vector2 windowScale)
                : this(title, onDrawHeader, onDraw, onDrawFooter,
                    new Rect(DefaultRect.min,
                        new Vector2(DefaultRect.width * windowScale.x, DefaultRect.height * windowScale.y))) {
            }

            /// <summary>
            /// 開く
            /// </summary>
            public void Open() {
                IsOpen = true;

                // ScreenRect内に修める
                var windowRect = WindowRect;
                var screenRect = Instance.ScreenRect;
                windowRect.xMin = Mathf.Max(WindowRect.xMin, screenRect.xMin + WindowSizeMargin);
                windowRect.xMax = Mathf.Min(WindowRect.xMax, screenRect.xMax - WindowSizeMargin);
                windowRect.yMin = Mathf.Max(WindowRect.yMin, screenRect.yMin + Instance.ButtonSize * 2);
                windowRect.yMax = Mathf.Min(WindowRect.yMax, screenRect.yMax - WindowSizeMargin);
                WindowRect = windowRect;
            }

            /// <summary>
            /// 閉じる
            /// </summary>
            public void Close() {
                IsOpen = false;
                _isTouchScaleArea = false;
                _scrollBarPos = Vector2.zero;
            }

            /// <summary>
            /// 描画処理
            /// </summary>
            public void Draw() {
                if (!IsOpen) {
                    return;
                }

                WindowRect = GUI.Window(ID, WindowRect, OnWindow, Title, _windowStyle);
            }

            /// <summary>
            /// 毎フレーム処理　親クラスのUpdateで呼ぶ
            /// </summary>
            public void UpdateWindow() {
                if (IsOpen) {
                    // Unity Updateのタイミングでスクロールを反映させたい
                    _scrollBarPos += _diffTouchScrollPosition;
                    _diffTouchScrollPosition = Vector2.zero;
                }
            }

            /// <summary>
            /// GUI.Windowイベント
            /// </summary>
            private void OnWindow(int id) {
                using (new GUILayout.VerticalScope()) {
                    // 閉じるボタン描画
                    var closeButtonRect = new Rect(WindowRect.width - WindowButtonWidth - 3.0f, 2.5f,
                        WindowButtonWidth, WindowButtonHeight);

                    if (GUI.Button(closeButtonRect, "", _closeButtonStyle)) {
                        Close();
                    }

                    // 描画処理
                    var contentRect = new Rect(0.0f, 0.0f, WindowRect.width, WindowRect.height);

                    OnDrawHeader?.Invoke(contentRect);

                    if (_enableScrollBar) {
                        var skinScroll = Instance.DebugGuiSkin.horizontalScrollbar;

                        using (var scope = new GUILayout.ScrollViewScope(_scrollBarPos, skinScroll,
                                   Instance.DebugGuiSkin.verticalScrollbar)) {
                            OnDraw?.Invoke(contentRect);
                            _scrollBarPos = scope.scrollPosition;
                        }
                    }
                    else {
                        OnDraw?.Invoke(contentRect);
                    }

                    OnDrawFooter?.Invoke(contentRect);
                }

                var eventType = Event.current.type;
                var currentMousePosition = Event.current.mousePosition;

                if (eventType == EventType.MouseDown) {
                    Rect lowerRightRect = new Rect(WindowRect.width - WindowButtonWidth,
                        WindowRect.height - WindowButtonWidth, WindowRect.width, WindowRect.height);
                    _isTouchScaleArea = lowerRightRect.Contains(currentMousePosition);

                    if (_isTouchScaleArea == false) {
                        var scrollRect = new Rect(WindowRect.xMin, WindowRect.yMin, WindowRect.xMax,
                            WindowRect.yMax);

                        _isTouchScrollArea = scrollRect.Contains(currentMousePosition);
                    }

                    _beforeMousePosition = currentMousePosition;
                }
                else if (eventType == EventType.MouseUp) {
                    _isTouchScaleArea = false;
                    _isTouchScrollArea = false;
                }

                if (_isTouchScaleArea) {
                    // リサイズ処理
                    ResizeWindow(currentMousePosition);
                    _beforeMousePosition = currentMousePosition;
                }
                else if (_isTouchScrollArea) {
                    var diffY = currentMousePosition.y - _beforeMousePosition.y;
                    _diffTouchScrollPosition += new Vector2(0, -diffY);
                    _beforeMousePosition = currentMousePosition;
                }
                else {
                    // Drag処理
                    var dragRect = new Rect(0.0f, 0.0f, WindowRect.width, -_windowStyle.contentOffset.y);
                    GUI.DragWindow(dragRect);
                }

                // window裏のボタンなどを押せないようにタッチ伝搬キャンセル
                if (eventType == EventType.MouseDown || eventType == EventType.MouseDrag) {
                    Event.current.Use();
                }
            }

            /// <summary>
            /// Windowの更新
            /// </summary>
            private void ResizeWindow(Vector2 mousePosition) {
                // 差分計算
                var diffX = mousePosition.x - _beforeMousePosition.x;
                var diffY = mousePosition.y - _beforeMousePosition.y;
                // 最小の幅と高さを考慮する
                var newWidth = Mathf.Max((WindowRect.width + diffX), MinWindowWidth);
                var newHeight = Mathf.Max((WindowRect.height + diffY), MinWindowHeight);
                WindowRect = new Rect(WindowRect.x, WindowRect.y, newWidth, newHeight);
            }
        }
    }
}