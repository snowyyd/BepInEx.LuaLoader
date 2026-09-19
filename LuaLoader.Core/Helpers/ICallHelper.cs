#if CPP

using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace LuaLoader.Helpers;

public static class ICallHelper
{
	private static readonly Dictionary<string, Delegate> iCallCache = [];

	public static T GetICall<T>(string iCallName) where T : Delegate
	{
		if (iCallCache.TryGetValue(iCallName, out var value))
		{
			return (T)value;
		}

		var ptr = il2cppResolveIcall(iCallName);

		if (ptr == IntPtr.Zero)
		{
			throw new MissingMethodException($"Could not resolve internal call by name '{iCallName}'!");
		}

		var iCall = Marshal.GetDelegateForFunctionPointer<T>(ptr);
		iCallCache.Add(iCallName, iCall);

		return iCall;
	}

	[DllImport("GameAssembly", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	private static extern IntPtr il2cppResolveIcall([MarshalAs(UnmanagedType.LPStr)] string name);
}

#endif
