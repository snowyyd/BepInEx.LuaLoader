using HarmonyLib;
using LuaLoader.Helpers;
using LuaLoader.LuaClass;
using UnityEngine;
using BF = System.Reflection.BindingFlags;

namespace LuaLoader.UI;

public class ForceUnlockCursor
{
	public static bool Unlock
	{
		get => m_forceUnlock;
		set => SetForceUnlock(value);
	}

	private static bool m_forceUnlock;

	public static bool ShouldForceMouse => Loader.ShowMouse && Unlock;

	private static CursorLockMode m_lastLockMode;
	private static bool m_lastVisibleState;

	private static bool m_currentlySettingCursor;

	private static Type? CursorType => m_cursorType ??= ReflectionHelpers.GetTypeByName("UnityEngine.Cursor");
	private static Type? m_cursorType;

	public static void Init()
	{
		try
		{
			if (CursorType == null)
			{
				throw new Exception("Could not find Type 'UnityEngine.Cursor'!");
			}

			// Get current cursor state and enable cursor
			try
			{
				m_lastLockMode = (CursorLockMode)typeof(Cursor).GetProperty("lockState", BF.Public | BF.Static).GetValue(null, null);
				m_lastVisibleState = (bool)typeof(Cursor).GetProperty("visible", BF.Public | BF.Static).GetValue(null, null);
			}
			catch
			{
				m_lastLockMode = CursorLockMode.None;
				m_lastVisibleState = true;
			}

			// Setup Harmony Patches
			TryPatch("lockState", new HarmonyMethod(typeof(ForceUnlockCursor).GetMethod(nameof(PrefixSetLockState))), true);
			TryPatch("lockState", new HarmonyMethod(typeof(ForceUnlockCursor).GetMethod(nameof(PostfixGetLockState))), false);

			TryPatch("visible", new HarmonyMethod(typeof(ForceUnlockCursor).GetMethod(nameof(PrefixSetVisible))), true);
			TryPatch("visible", new HarmonyMethod(typeof(ForceUnlockCursor).GetMethod(nameof(PostfixGetVisible))), false);
		}
		catch (Exception e)
		{
			LuaLoader.Instance?.Logger.LogWarning($"Exception on CursorControl.Init: {e.GetType()}, {e.Message}");
		}

		Unlock = true;
	}

	private static void TryPatch(string property, HarmonyMethod patch, bool setter)
	{
		try
		{
			var harmony = LuaLoader.Instance?.HarmonyInstance;
			var prop = typeof(Cursor).GetProperty(property);

			if (setter)
			{
				// setter is prefix
				harmony?.Patch(prop.GetSetMethod(), prefix: patch);
			}
			else
			{
				// getter is postfix
				harmony?.Patch(prop.GetGetMethod(), postfix: patch);
			}
		}
		catch (Exception e)
		{
			var s = setter ? "set_" : "get_";
			LuaLoader.Instance?.Logger.LogWarning($"Unable to patch Cursor.{s}{property}: {e.Message}");
		}
	}

	private static void SetForceUnlock(bool unlock)
	{
		m_forceUnlock = unlock;
		UpdateCursorControl();
	}

	public static void Update()
	{
		// Check Force-Unlock input
		if (InputManager.GetKeyDown(KeyCode.LeftAlt))
		{
			Unlock = !Unlock;
		}
	}

	public static void UpdateCursorControl()
	{
		try
		{
			m_currentlySettingCursor = true;
			if (ShouldForceMouse)
			{
				Cursor.lockState = CursorLockMode.None;
				Cursor.visible = true;
			}
			else
			{
				Cursor.lockState = m_lastLockMode;
				Cursor.visible = m_lastVisibleState;
			}

			m_currentlySettingCursor = false;
		}
		catch (Exception e)
		{
			LuaLoader.Instance?.Logger.LogError($"Exception while setting Cursor state: {e.GetType()}, {e.Message}");
		}
	}

	// Force mouse to stay unlocked and visible while UnlockMouse and ShowMenu are true.
	// Also keep track of when anything else tries to set Cursor state, this will be the
	// value that we set back to when we close the menu or disable force-unlock.

	[HarmonyPrefix]
	public static void PrefixSetLockState(ref CursorLockMode value)
	{
		if (!m_currentlySettingCursor)
		{
			m_lastLockMode = value;

			if (ShouldForceMouse)
			{
				value = CursorLockMode.None;
			}
		}
	}

	[HarmonyPrefix]
	public static void PrefixSetVisible(ref bool value)
	{
		if (!m_currentlySettingCursor)
		{
			m_lastVisibleState = value;

			if (ShouldForceMouse)
			{
				value = true;
			}
		}
	}

	[HarmonyPostfix]
	public static void PostfixGetLockState(ref CursorLockMode __result)
	{
		if (ShouldForceMouse)
		{
			__result = m_lastLockMode;
		}
	}

	[HarmonyPostfix]
	public static void PostfixGetVisible(ref bool __result)
	{
		if (ShouldForceMouse)
		{
			__result = m_lastVisibleState;
		}
	}
}
