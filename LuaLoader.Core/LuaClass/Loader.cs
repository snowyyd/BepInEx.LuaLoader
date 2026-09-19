using LuaLoader.UI;

namespace LuaLoader.LuaClass;

public static class Loader
{
	public static void LogFatal(object obj) => LuaEngine.Logger.LogFatal(obj);
	public static void LogError(object obj) => LuaEngine.Logger.LogError(obj);
	public static void LogWarning(object obj) => LuaEngine.Logger.LogWarning(obj);
	public static void LogMessage(object obj) => LuaEngine.Logger.LogMessage(obj);
	public static void LogInfo(object obj) => LuaEngine.Logger.LogInfo(obj);
	public static void LogDebug(object obj) => LuaEngine.Logger.LogDebug(obj);

	public static bool ShowMouse
	{
		get => m_showMouse;
		set => SetShowMouse(value);
	}

	private static bool m_showMouse;

	private static void SetShowMouse(bool show)
	{
		m_showMouse = show;
		ForceUnlockCursor.UpdateCursorControl();
	}

	// TODO
	/* public static Config? GetConfig()
	{
		var config = LuaLoader.Category.GetValue<Config>();

		return config;
	} */

	public static void ReloadLua() => LuaEngine.ReloadLua();

	public static FileSystemWatcher CreateFileSystemWatcher(string path, NLua.LuaTable table)
	{
		var fileSystemWatcher = new FileSystemWatcher(path)
		{
			NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName,
			Filter = (string)(table["Filter"] ?? "*.lua"),
		};

		if (table["Changed"] != null)
		{
			fileSystemWatcher.Changed += (sender, args) => ((NLua.LuaFunction)table["Changed"]).TryCall(sender, args);
		}

		if (table["Created"] != null)
		{
			fileSystemWatcher.Created += (sender, args) => ((NLua.LuaFunction)table["Created"]).TryCall(sender, args);
		}

		if (table["Deleted"] != null)
		{
			fileSystemWatcher.Deleted += (sender, args) => ((NLua.LuaFunction)table["Deleted"]).TryCall(sender, args);
		}

		if (table["Disposed"] != null)
		{
			fileSystemWatcher.Disposed += (sender, args) => ((NLua.LuaFunction)table["Disposed"]).TryCall(sender, args);
		}

		if (table["Error"] != null)
		{
			fileSystemWatcher.Error += (sender, args) => ((NLua.LuaFunction)table["Error"]).TryCall(sender, args);
		}

		if (table["Renamed"] != null)
		{
			fileSystemWatcher.Renamed += (sender, args) => ((NLua.LuaFunction)table["Renamed"]).TryCall(sender, args);
		}

		fileSystemWatcher.EnableRaisingEvents = true;

		return fileSystemWatcher;
	}
}
