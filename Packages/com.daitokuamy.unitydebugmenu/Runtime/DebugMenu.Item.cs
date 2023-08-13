using System;
using UnityEngine;
using System.Collections.Generic;

namespace UnityDebugMenu {
    /// <summary>
    /// DebugMenu
    /// </summary>
    partial class DebugMenu {
        /// 項目実行ハンドラ
        public delegate void ExecuteHandler(Item item);
        /// 項目削除ハンドラ
        public delegate void DisposeHandler(Item item);

        /// <summary>
        /// 項目を表すクラス
        /// </summary>
        public class Item : IDisposable {
            // 項目実行時の処理
            public event ExecuteHandler OnExecute;
            // 項目削除時の処理
            public event DisposeHandler OnDispose;
            // 親項目
            public Item Parent { get; private set; }
            // 子項目
            public List<Item> Children { get; private set; }
            // 項目名
            public string Name { get; private set; }
            // 最終項目か
            public bool IsLeaf => Children.Count <= 0;
            // 項目のフルパス
            public string FullPath {
                get {
                    string fullPath = Name;
                    Item parent = Parent;

                    while (parent != null) {
                        fullPath = parent.Name + "/" + fullPath;
                        parent = parent.Parent;
                    }

                    return fullPath;
                }
            }

            /// <summary>
            /// コンストラクタ
            /// </summary>
            public Item(string name, Item parent) {
                Parent = parent;
                Children = new List<Item>();
                Name = name;
            }

            /// <summary>
            /// 廃棄時処理
            /// </summary>
            public void Dispose() {
                OnDispose?.Invoke(this);
            }

            /// <summary>
            /// 項目を開く
            /// </summary>
            public void Open() {
                if (IsLeaf) {
                    Execute();
                }
                else {
                    Instance.OpenItem(this);
                }
            }

            /// <summary>
            /// 項目を閉じる
            /// </summary>
            public void Back() {
                Instance.CloseItem(this);
                Parent?.Open();
            }

            /// <summary>
            /// 項目を実行する
            /// </summary>
            private void Execute() {
                if (OnExecute == null) {
                    return;
                }

                OnExecute(this);
            }

            /// <summary>
            /// 項目の描画
            /// </summary>
            public void Draw() {
                var screenWidth = Instance.ScreenRect.width;
                GUILayout.BeginVertical();

                for (var i = 0; i < Children.Count;) {
                    var rowWidth = 0.0f;
                    GUILayout.BeginHorizontal();

                    while (i < Children.Count) {
                        var child = Children[i];
                        rowWidth += child.GetButtonWidth() + 18;

                        // 幅が最大値を超えたら止める
                        if (rowWidth > screenWidth) {
                            break;
                        }

                        // 項目を並べる
                        child.DrawButton();
                        i++;
                    }

                    GUILayout.EndHorizontal();
                    GUILayout.Space(5);
                }

                GUILayout.EndVertical();
            }

            /// <summary>
            /// 項目を表すボタンを描画
            /// </summary>
            public void DrawButton() {
                var content = new GUIContent(Name);

                if (GUILayout.Button(content, GUILayout.Height(Instance.ButtonSize))) {
                    Open();
                }
            }

            /// <summary>
            /// ボタンの幅を取得
            /// </summary>
            public float GetButtonWidth() {
                var style = Instance.DebugGuiSkin.button;
                var content = new GUIContent(Name);
                var size = style.CalcSize(content);
                return size.x;
            }

            /// <summary>
            /// 階層情報を元にItemを探す
            /// </summary>
            public bool FindItem(out Item lastItem, out int lastIndex, string[] splitPathNames) {
                return FindItemInternal(out lastItem, out lastIndex, splitPathNames, 0);
            }

            /// <summary>
            /// 子Itemを探す（再帰用）
            /// </summary>
            private bool FindItemInternal(out Item lastItem, out int lastIndex, string[] splitPathNames, int nameIndex) {
                lastItem = this;
                lastIndex = nameIndex;

                if (nameIndex >= splitPathNames.Length) {
                    return true;
                }

                var findName = splitPathNames[nameIndex];

                foreach (var child in Children) {
                    if (child.Name != findName) {
                        continue;
                    }

                    return child.FindItemInternal(out lastItem, out lastIndex, splitPathNames, nameIndex + 1);
                }

                return false;
            }
        }
    }
}