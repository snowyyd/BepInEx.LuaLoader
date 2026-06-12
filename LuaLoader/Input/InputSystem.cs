using System.Reflection;
using LuaLoader.Helpers;
using UnityEngine;

namespace LuaLoader.Input;

public class InputSystem : IAbstractInput
{
	public static Type TKeyboard => m_tKeyboard ??= ReflectionHelpers.GetTypeByName("UnityEngine.InputSystem.Keyboard");
	private static Type m_tKeyboard;

	public static Type TMouse => m_tMouse ??= ReflectionHelpers.GetTypeByName("UnityEngine.InputSystem.Mouse");
	private static Type m_tMouse;

	public static Type TKey => m_tKey ??= ReflectionHelpers.GetTypeByName("UnityEngine.InputSystem.Key");
	private static Type m_tKey;

	private static PropertyInfo m_btnIsPressedProp;
	private static PropertyInfo m_btnWasPressedProp;

	private static object CurrentKeyboard => m_currentKeyboard ??= m_kbCurrentProp.GetValue(null, null);
	private static object m_currentKeyboard;
	private static PropertyInfo m_kbCurrentProp;
	private static PropertyInfo m_kbIndexer;

	private static object CurrentMouse => m_currentMouse ??= m_mouseCurrentProp.GetValue(null, null);
	private static object m_currentMouse;
	private static PropertyInfo m_mouseCurrentProp;

	private static object LeftMouseButton => m_lmb ??= m_leftButtonProp.GetValue(CurrentMouse, null);
	private static object m_lmb;
	private static PropertyInfo m_leftButtonProp;

	private static object RightMouseButton => m_rmb ??= m_rightButtonProp.GetValue(CurrentMouse, null);
	private static object m_rmb;
	private static PropertyInfo m_rightButtonProp;

	private static object MousePositionInfo => m_pos ??= m_positionProp.GetValue(CurrentMouse, null);
	private static object m_pos;
	private static PropertyInfo m_positionProp;
	private static MethodInfo m_readVector2InputMethod;

	public Vector2 MousePosition => (Vector2)m_readVector2InputMethod.Invoke(MousePositionInfo, []);

	public bool GetKeyDown(KeyCode key)
	{
		var parsedKey = Enum.Parse(TKey, key.ToString());
		var actualKey = m_kbIndexer.GetValue(CurrentKeyboard, [parsedKey]);

		return (bool)m_btnWasPressedProp.GetValue(actualKey, null);
	}

	public bool GetKey(KeyCode key)
	{
		var parsed = Enum.Parse(TKey, key.ToString());
		var actualKey = m_kbIndexer.GetValue(CurrentKeyboard, [parsed]);

		return (bool)m_btnIsPressedProp.GetValue(actualKey, null);
	}

	public bool GetMouseButtonDown(int btn)
	{
		return btn switch
		{
			0 => (bool)m_btnWasPressedProp.GetValue(LeftMouseButton, null),
			1 => (bool)m_btnWasPressedProp.GetValue(RightMouseButton, null),
			// 2 => (bool)_btnWasPressedProp.GetValue(MiddleMouseButton, null),
			_ => throw new NotImplementedException(),
		};
	}

	public bool GetMouseButton(int btn)
	{
		return btn switch
		{
			0 => (bool)m_btnIsPressedProp.GetValue(LeftMouseButton, null),
			1 => (bool)m_btnIsPressedProp.GetValue(RightMouseButton, null),
			// 2 => (bool)_btnIsPressedProp.GetValue(MiddleMouseButton, null),
			_ => throw new NotImplementedException(),
		};
	}

	public void Init()
	{
		LuaLoader.Instance?.Logger.LogInfo("Initializing new InputSystem support...");

		m_kbCurrentProp = TKeyboard.GetProperty("current");
		m_kbIndexer = TKeyboard.GetProperty("Item", [TKey]);

		var btnControl = ReflectionHelpers.GetTypeByName("UnityEngine.InputSystem.Controls.ButtonControl");
		m_btnIsPressedProp = btnControl.GetProperty("isPressed");
		m_btnWasPressedProp = btnControl.GetProperty("wasPressedThisFrame");

		m_mouseCurrentProp = TMouse.GetProperty("current");
		m_leftButtonProp = TMouse.GetProperty("leftButton");
		m_rightButtonProp = TMouse.GetProperty("rightButton");

		m_positionProp = ReflectionHelpers.GetTypeByName("UnityEngine.InputSystem.Pointer").GetProperty("position");

		m_readVector2InputMethod = ReflectionHelpers.GetTypeByName("UnityEngine.InputSystem.InputControl`1").MakeGenericType(typeof(Vector2)).GetMethod("ReadValue");
	}
}
