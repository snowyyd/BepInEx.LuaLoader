using System.Reflection;

namespace LuaLoader;

public static class ReflectionExtensions
{
#if CPP
	public static object Il2CppCast(this object obj, Type castTo)
	{
		return Helpers.ReflectionHelpers.Il2CppCast(obj, castTo);
	}
#endif

	public static IEnumerable<Type> TryGetTypes(this Assembly asm)
	{
		try
		{
			return asm.GetTypes();
		}
		catch (ReflectionTypeLoadException e)
		{
			try
			{
				return asm.GetExportedTypes();
			}
			catch
			{
				return e.Types.Where(static t => t != null);
			}
		}
		catch
		{
			return [];
		}
	}
}
