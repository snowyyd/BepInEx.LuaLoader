namespace LuaLoader;

public interface ILogSink
{
	void LogFatal(object data);
	void LogError(object data);
	void LogWarning(object data);
	void LogMessage(object data);
	void LogInfo(object data);
	void LogDebug(object data);
}

internal sealed class NullLogSink : ILogSink
{
	public static readonly NullLogSink Instance = new();

	public void LogFatal(object data) { }
	public void LogError(object data) { }
	public void LogWarning(object data) { }
	public void LogMessage(object data) { }
	public void LogInfo(object data) { }
	public void LogDebug(object data) { }
}
