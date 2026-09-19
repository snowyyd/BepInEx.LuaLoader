using System.Reflection;
using LuaLoader.Helpers;
using UnityEngine;

namespace LuaLoader.Input;

public class InputSystem : IAbstractInput
{
	public static Type? TKeyboard => field ??= ReflectionHelpers.GetTypeByName("UnityEngine.InputSystem.Keyboard");
	public static Type? TMouse => field ??= ReflectionHelpers.GetTypeByName("UnityEngine.InputSystem.Mouse");
	public static Type? TKey => field ??= ReflectionHelpers.GetTypeByName("UnityEngine.InputSystem.Key");

	private static PropertyInfo m_btnIsPressedProp = null!;
	private static PropertyInfo m_btnWasPressedProp = null!;
	private static PropertyInfo m_kbIndexer = null!;

	private static PropertyInfo m_kbCurrentProp = null!;
	private static object CurrentKeyboard => field ??= m_kbCurrentProp.GetValue(null)
		?? throw new InvalidOperationException("Unity Input System keyboard is not available.");

	private static PropertyInfo m_mouseCurrentProp = null!;
	private static object CurrentMouse => field ??= m_mouseCurrentProp.GetValue(null)
		?? throw new InvalidOperationException("Unity Input System mouse is not available.");

	private static PropertyInfo m_leftButtonProp = null!;
	private static object LeftMouseButton => field ??= m_leftButtonProp.GetValue(CurrentMouse)
		?? throw new InvalidOperationException("Unity Input System left mouse button is not available.");

	private static PropertyInfo m_rightButtonProp = null!;
	private static object RightMouseButton => field ??= m_rightButtonProp.GetValue(CurrentMouse)
		?? throw new InvalidOperationException("Unity Input System right mouse button is not available.");

	private static PropertyInfo m_positionProp = null!;
	private static object MousePositionInfo => field ??= m_positionProp.GetValue(CurrentMouse)
		?? throw new InvalidOperationException("Unity Input System mouse position is not available.");

	private static MethodInfo m_readVector2InputMethod = null!;
	public Vector2 MousePosition => (Vector2)m_readVector2InputMethod.Invoke(MousePositionInfo, []);

	public bool GetKeyDown(KeyCode key)
	{
		var parsedKey = Enum.Parse(
			TKey ?? throw new InvalidOperationException("InputSystem has not been initialized."),
			key.ToString());

		var actualKey = m_kbIndexer.GetValue(CurrentKeyboard, [parsedKey])
			?? throw new InvalidOperationException($"Could not get keyboard key '{key}'.");

		return (bool)m_btnWasPressedProp.GetValue(actualKey)!;
	}

	public bool GetKey(KeyCode key)
	{
		var parsedKey = Enum.Parse(
			TKey ?? throw new InvalidOperationException("InputSystem has not been initialized."),
			key.ToString());

		var actualKey = m_kbIndexer.GetValue(CurrentKeyboard, [parsedKey])
			?? throw new InvalidOperationException($"Could not get keyboard key '{key}'.");

		return (bool)m_btnIsPressedProp.GetValue(actualKey)!;
	}

	public bool GetMouseButtonDown(int btn)
	{
		return btn switch
		{
			0 => (bool)m_btnWasPressedProp.GetValue(LeftMouseButton),
			1 => (bool)m_btnWasPressedProp.GetValue(RightMouseButton),
			_ => throw new NotImplementedException(),
		};
	}

	public bool GetMouseButton(int btn)
	{
		return btn switch
		{
			0 => (bool)m_btnIsPressedProp.GetValue(LeftMouseButton),
			1 => (bool)m_btnIsPressedProp.GetValue(RightMouseButton),
			_ => throw new NotImplementedException(),
		};
	}

	public void Init()
	{
		LuaEngine.Logger.LogInfo("Initializing new InputSystem support...");

		var keyboard = TKeyboard ?? throw new TypeLoadException("UnityEngine.InputSystem.Keyboard was not found.");
		var mouse = TMouse ?? throw new TypeLoadException("UnityEngine.InputSystem.Mouse was not found.");
		var key = TKey ?? throw new TypeLoadException("UnityEngine.InputSystem.Key was not found.");

		m_kbCurrentProp = keyboard.GetProperty("current")
			?? throw new MissingMemberException(keyboard.FullName, "current");

		m_kbIndexer = keyboard.GetProperty("Item", [key])
			?? throw new MissingMemberException(keyboard.FullName, "Item");

		var btnControl = ReflectionHelpers.GetTypeByName("UnityEngine.InputSystem.Controls.ButtonControl")
			?? throw new TypeLoadException("UnityEngine.InputSystem.Controls.ButtonControl was not found.");

		m_btnIsPressedProp = btnControl.GetProperty("isPressed")
			?? throw new MissingMemberException(btnControl.FullName, "isPressed");

		m_btnWasPressedProp = btnControl.GetProperty("wasPressedThisFrame")
			?? throw new MissingMemberException(btnControl.FullName, "wasPressedThisFrame");

		m_mouseCurrentProp = mouse.GetProperty("current")
			?? throw new MissingMemberException(mouse.FullName, "current");

		m_leftButtonProp = mouse.GetProperty("leftButton")
			?? throw new MissingMemberException(mouse.FullName, "leftButton");

		m_rightButtonProp = mouse.GetProperty("rightButton")
			?? throw new MissingMemberException(mouse.FullName, "rightButton");

		var pointer = ReflectionHelpers.GetTypeByName("UnityEngine.InputSystem.Pointer")
			?? throw new TypeLoadException("UnityEngine.InputSystem.Pointer was not found.");

		m_positionProp = pointer.GetProperty("position")
			?? throw new MissingMemberException(pointer.FullName, "position");

		var inputControl = ReflectionHelpers.GetTypeByName("UnityEngine.InputSystem.InputControl`1")
			?? throw new TypeLoadException("UnityEngine.InputSystem.InputControl`1 was not found.");

		m_readVector2InputMethod = inputControl.MakeGenericType(typeof(Vector2)).GetMethod("ReadValue") ?? throw new MissingMethodException("InputControl<Vector2>.ReadValue was not found.");
	}
}
