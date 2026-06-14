using System.Reflection;
using LuaLoader.Helpers;
using UnityEngine;

namespace LuaLoader.Input;

public class LegacyInput : IAbstractInput
{
	public static Type TInput => m_tInput ??= ReflectionHelpers.GetTypeByName("UnityEngine.Input");
	private static Type? m_tInput;

	private static PropertyInfo? m_mousePositionProp;
	private static MethodInfo? m_getKeyMethod;
	private static MethodInfo? m_getKeyDownMethod;
	private static MethodInfo? m_getMouseButtonMethod;
	private static MethodInfo? m_getMouseButtonDownMethod;

	public Vector2 MousePosition => (Vector3)m_mousePositionProp.GetValue(null, null);

	public bool GetKey(KeyCode key) => (bool)m_getKeyMethod.Invoke(null, [key]);

	public bool GetKeyDown(KeyCode key) => (bool)m_getKeyDownMethod.Invoke(null, [key]);

	public bool GetMouseButton(int btn) => (bool)m_getMouseButtonMethod.Invoke(null, [btn]);

	public bool GetMouseButtonDown(int btn) => (bool)m_getMouseButtonDownMethod.Invoke(null, [btn]);

	public void Init()
	{
		LuaLoader.Instance.Logger.LogInfo("Initializing Legacy Input support...");

		m_mousePositionProp = TInput.GetProperty("mousePosition");
		m_getKeyMethod = TInput.GetMethod("GetKey", [typeof(KeyCode)]);
		m_getKeyDownMethod = TInput.GetMethod("GetKeyDown", [typeof(KeyCode)]);
		m_getMouseButtonMethod = TInput.GetMethod("GetMouseButton", [typeof(int)]);
		m_getMouseButtonDownMethod = TInput.GetMethod("GetMouseButtonDown", [typeof(int)]);
	}
}
