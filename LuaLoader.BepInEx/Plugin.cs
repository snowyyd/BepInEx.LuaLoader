using System.Reflection;
using BepInEx;
using HarmonyLib;
using LuaLoader.UI;

namespace LuaLoader.BepInEx;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin
{
	// public static MelonPreferences_ReflectiveCategory Category;
	// public static bool IsLoadMonoCSharp;

	public Plugin()
	{
		LuaEngine.Logger = new BepInExLogSink(this.Logger);
		// Category = MelonPreferences.CreateCategory<Config>(BuildInfo.Name);
		// Category.SetFilePath("Lua/LuaLoader.cfg");

		// TODO
		/* var path = Path.Combine(MelonUtils.BaseDirectory, "MelonLoader", "Managed", "Mono.CSharp.dll");

		if (File.Exists(path))
		{
			try
			{
				Assembly.LoadFrom(path);
				IsLoadMonoCSharp = true;
			}
			catch (Exception e)
			{
				LuaEngine.Logger.LogWarning(e);
			}
		}
		else
		{
			LuaEngine.Logger.LogWarning("Assembly 'Mono.CSharp.dll' not found");
		} */

		LuaEngine.Initialize();
	}

	public void Awake()
	{
		this.Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");

		LuaEngine.HarmonyInstance = Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());

		LuaEngine.LuaCall("Awake");
	}

	public void Start()
	{
		ForceUnlockCursor.Init();

		LuaEngine.LuaCall("Start");
	}

	public void Quit() => LuaEngine.LuaCall("Quit");

	private void OnDestroy()
	{
		LuaEngine.LuaCall("OnDestroy");
		this.Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID}'s OnDestroy() got called!");
		LuaEngine.HarmonyInstance?.UnpatchSelf();
	}

	public void Update()
	{
		if (LuaClass.Loader.ShowMouse)
		{
			ForceUnlockCursor.Update();
		}

		LuaEngine.LuaCall("Update");
	}

	public void LateUpdate() => LuaEngine.LuaCall("LateUpdate");
	public void FixedUpdate() => LuaEngine.LuaCall("FixedUpdate");
	public void OnGUI() => LuaEngine.LuaCall("OnGUI");
}
