using UnityEngine;

namespace LuaLoader.Helpers;

public static class UnityHelpers
{
	public static Camera? MainCamera
	{
		get
		{
			if (!field)
				field = Camera.main;

			return field;
		}
	}

	public static string ActiveSceneName => UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
}
