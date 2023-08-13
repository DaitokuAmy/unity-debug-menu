using System.Linq;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UnityDebugMenu {
    /// <summary>
    /// デフォルトのDebugMenuハンドリングクラス
    /// </summary>
    internal class DefaultDebugMenuHandler : IDebugMenuHandler {
        private float _touchTime;

        /// <summary>
        /// 切り替えチェック
        /// </summary>
        bool IDebugMenuHandler.CheckVisible(DebugMenu menu, float deltaTime) {
#if ENABLE_INPUT_SYSTEM
#if UNITY_EDITOR || UNITY_STANDALONE
        // 右クリック
        return Mouse.current.rightButton.wasReleasedThisFrame;
#else
				// N箇所以上長押しタッチ
				var touchscreen = Touchscreen.current;

				if( touchscreen == null )
				{
					return false;
				}

				var touchCount = touchscreen.touches.Count( x => x.ReadValue().isInProgress );

				if( touchCount < menu.MenuToggleTouchCount )
				{
					deltaTime = 0.0f;
					_touchTime = 0.0f;
				}
				else if( touchCount >= menu.MenuToggleFastTouchCount )
				{
					deltaTime *= menu.MenuToggleTouchTime / menu.MenuToggleFastTouchTime;
				}

				_touchTime += deltaTime;
				return _touchTime - deltaTime < menu.MenuToggleTouchTime && _touchTime >= menu.MenuToggleTouchTime;
#endif
#else
#if UNITY_EDITOR || UNITY_STANDALONE
            // 右クリック
            return Input.GetMouseButtonDown(1);
#else
				// N箇所以上長押しタッチ
				var touchCount = Input.touchCount;

				if( touchCount < menu.MenuToggleTouchCount )
				{
					deltaTime = 0.0f;
					_touchTime = 0.0f;
				}
				else if( touchCount >= menu.MenuToggleFastTouchCount )
				{
					deltaTime *= menu.MenuToggleTouchTime / menu.MenuToggleFastTouchTime;
				}

				_touchTime += deltaTime;
				return _touchTime - deltaTime < menu.MenuToggleTouchTime && _touchTime >= menu.MenuToggleTouchTime;
#endif
#endif
        }
    }
}