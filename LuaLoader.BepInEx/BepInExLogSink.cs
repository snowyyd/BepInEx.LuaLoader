using BepInEx.Logging;

namespace LuaLoader.BepInEx;

public sealed class BepInExLogSink(ManualLogSource logger) : ILogSink
{
	public void LogFatal(object data) => logger.LogFatal(data);
	public void LogError(object data) => logger.LogError(data);
	public void LogWarning(object data) => logger.LogWarning(data);
	public void LogMessage(object data) => logger.LogMessage(data);
	public void LogInfo(object data) => logger.LogInfo(data);
	public void LogDebug(object data) => logger.LogDebug(data);
}
