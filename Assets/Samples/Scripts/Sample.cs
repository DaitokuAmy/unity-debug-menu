using System;
using UnityDebugMenu;
using UnityEngine;

namespace Samples {
    public class Sample : MonoBehaviour {
        private enum TestEnum {
            None,
            One,
            Two,
            Three,
        }

        private TestEnum _testEnum;
        private bool _on;
        
        /// <summary>
        /// 開始処理
        /// </summary>
        private void Start() {
            for (var i = 0; i < 10; i++) {
                var index = i;
                DebugMenu.AddWindow($"Sample/Windows/Window_{i}", _ => {
                    DebugMenuUtil.LabelField("TestLabel", "Hoge");
                    _testEnum = DebugMenuUtil.EnumArrowOrderField("TestEnum", _testEnum);
                    _on = DebugMenuUtil.ToggleButtonField("TestOnOff", _on);
                    if (DebugMenuUtil.ButtonField("TestButton", "Execute")) {
                        Debug.Log("Execute TestButton");
                    }
                });
            }

            for (var i = 0; i < 30; i++) {
                var index = i;
                DebugMenu.AddItem($"Test/Sample_{i}", _ => {
                    Debug.Log($"Execute Sample_{index}");
                });
            }
        }

        /// <summary>
        /// 削除処理
        /// </summary>
        private void OnDestroy() {
            // Test階層以下を全て削除
            DebugMenu.RemoveItem("Test");
        }
    }
}