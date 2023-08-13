using System;
using UnityDebugMenu;
using UnityEngine;

namespace Samples {
    public class Sample : MonoBehaviour {
        private void Start() {
            DebugMenu.AddWindow("Test/Foo/Window", _ => {
                if (GUILayout.Button("Test")) {
                    Debug.Log("Test");
                }
                DebugMenuUtil.LabelField("TestLabel", "Hoge");
                if (DebugMenuUtil.ButtonField("TestButton", "Foo")) {
                    Debug.Log("Test Button");
                }
            });

            for (var i = 0; i < 30; i++) {
                DebugMenu.AddItem($"Test/Sample_{i}", item => {
                    Debug.Log(item.Name);
                });
            }
        }

        private void OnDestroy() {
            DebugMenu.RemoveItem("Test");
        }
    }
}