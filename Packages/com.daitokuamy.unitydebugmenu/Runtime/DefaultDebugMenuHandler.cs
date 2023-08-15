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
	    private DebugMenuConfig _config;
        private float _touchTime;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public DefaultDebugMenuHandler(DebugMenuConfig config) {
	        _config = config;
        }

        /// <summary>
        /// 切り替えチェック
        /// </summary>
        bool IDebugMenuHandler.CheckVisible(DebugMenu menu, float deltaTime) {
	        var menuOpen = _config.editorMenuOpen;
#if ENABLE_INPUT_SYSTEM
#if UNITY_EDITOR || UNITY_STANDALONE
	        var currentKeyboard = Keyboard.current;
	        if (menuOpen.shift && !(currentKeyboard.leftShiftKey.isPressed || currentKeyboard.rightShiftKey.isPressed)) {
		        return false;
	        }
	        
	        if (menuOpen.alt && !currentKeyboard.altKey.isPressed) {
		        return false;
	        }
	        
	        if (menuOpen.controlOrCommand &&
	            !(currentKeyboard.leftCtrlKey.isPressed || currentKeyboard.rightCtrlKey.isPressed ||
	              currentKeyboard.leftCommandKey.isPressed || currentKeyboard.rightCommandKey.isPressed)) {
		        return false;
	        }

	        if (!currentKeyboard[menuOpen.keycode].wasPressedThisFrame) {
		        return false;
	        }

	        return true;
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
	        if (menuOpen.shift && !(Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))) {
		        return false;
	        }
	        
	        if (menuOpen.alt && !(Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt))) {
		        return false;
	        }

	        if (menuOpen.controlOrCommand && 
	            !(Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl) ||
	              Input.GetKey(KeyCode.LeftCommand) || Input.GetKey(KeyCode.RightCommand))) {
		        return false;
	        }

	        if (!Input.GetKeyDown(menuOpen.keycode)) {
		        return false;
	        }

	        return true;
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