using System.Reflection;
using LuaLoader.Helpers;
using UnityEngine;

namespace LuaLoader.Input;

public class LegacyInput : IAbstractInput
{
	public static Type? TInput => field ??= ReflectionHelpers.GetTypeByName("UnityEngine.Input");

	private static PropertyInfo m_mousePositionProp = null!;
	private static MethodInfo m_getKeyMethod = null!;
	private static MethodInfo m_getKeyDownMethod = null!;
	private static MethodInfo m_getMouseButtonMethod = null!;
	private static MethodInfo m_getMouseButtonDownMethod = null!;

	public Vector2 MousePosition => (Vector2)m_mousePositionProp.GetValue(null, null);

	public bool GetKey(KeyCode key) => (bool)m_getKeyMethod.Invoke(null, [key]);
	public bool GetKeyDown(KeyCode key) => (bool)m_getKeyDownMethod.Invoke(null, [key]);
	public bool GetMouseButton(int btn) => (bool)m_getMouseButtonMethod.Invoke(null, [btn]);
	public bool GetMouseButtonDown(int btn) => (bool)m_getMouseButtonDownMethod.Invoke(null, [btn]);

	public void Init()
	{
		LuaEngine.Logger.LogInfo("Initializing Legacy Input support...");

		var input = TInput ?? throw new TypeLoadException("UnityEngine.Input was not found.");

		m_mousePositionProp = input.GetProperty("mousePosition");
		m_getKeyMethod = input.GetMethod("GetKey", [typeof(KeyCode)]);
		m_getKeyDownMethod = input.GetMethod("GetKeyDown", [typeof(KeyCode)]);
		m_getMouseButtonMethod = input.GetMethod("GetMouseButton", [typeof(int)]);
		m_getMouseButtonDownMethod = input.GetMethod("GetMouseButtonDown", [typeof(int)]);
	}
}
