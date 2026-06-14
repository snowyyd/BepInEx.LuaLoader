using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using LuaLoader.Helpers;
using LuaLoader.LuaClass;
using LuaLoader.UI;

#if CPP
using UnhollowerRuntimeLib;
#endif

namespace LuaLoader;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class LuaLoader : BaseUnityPlugin
{
	public static NLua.Lua? Lua;
	public static LuaLoader Instance { get; private set; } = null!;

	public new ManualLogSource Logger => base.Logger;
	public Harmony? HarmonyInstance;

	// public static MelonPreferences_ReflectiveCategory Category;
	// public static bool IsLoadMonoCSharp;

	public LuaLoader()
	{
		Instance = this;
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
				Instance.Logger.LogWarning(e);
			}
		}
		else
		{
			Instance.Logger.LogWarning("Assembly 'Mono.CSharp.dll' not found");
		} */

		InputManager.Init();
		Instance.Logger.LogInfo("Initializing the lua environment...");
		this.InitializationLua();
		LoadingLua();
		Instance.Logger.LogInfo("The lua environment has been initialized!");
	}

	public void Awake()
	{
		this.Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");

		this.HarmonyInstance = Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());

		LuaCall("Awake");
	}

	public void Start()
	{
		ForceUnlockCursor.Init();

		LuaCall("Start");
	}

	public void Quit() => LuaCall("Quit");

	private void OnDestroy()
	{
		LuaCall("OnDestroy");
		this.Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID}'s OnDestroy() got called!");
		this.HarmonyInstance?.UnpatchSelf();
	}

	public void Update()
	{
		if (Loader.ShowMouse)
		{
			ForceUnlockCursor.Update();
		}

		LuaCall("Update");
	}

	public void LateUpdate() => LuaCall("LateUpdate");

	public void FixedUpdate() => LuaCall("FixedUpdate");

	public void OnGUI() => LuaCall("OnGUI");

	private static readonly string dirpath = Directory.GetCurrentDirectory().Replace("\\", "/", StringComparison.Ordinal);
	private static readonly string packagepath = "package.path = package.path .. ';" + EscapeLuaString(dirpath) + "/Lua/includes/modules/?.lua;" + EscapeLuaString(dirpath) + "/Lua/includes/modules/?.luac'";
	private static readonly string cpackagepath = "package.cpath = package.cpath .. ';" + EscapeLuaString(dirpath) + "/Lua/bin/?.dll'";
	private const string importloader = "import('LuaLoader', 'LuaLoader.LuaClass');import('LuaLoader', 'LuaLoader.Helpers')";
	private static bool loadingincludes;
	private static bool luaenvloaded;

	public static void LuaCall(string name) => Lua?.GetFunction("hook.Call").TryCall(name);

	public static void LuaCall(string name, object arg1) => Lua?.GetFunction("hook.Call").TryCall(name, arg1);

	public static void LuaCall(string name, object arg1, object arg2) => Lua?.GetFunction("hook.Call").TryCall(name, arg1, arg2);

	public static void LuaCall(string name, object arg1, object arg2, object arg3) => Lua?.GetFunction("hook.Call").TryCall(name, arg1, arg2, arg3);

	public static void LuaCall(string name, params object[] args)
	{
		if (args == null)
			args = [name];
		else
		{
			var objs = new object[args.Length + 1];

			objs[0] = name;

			for (var i = 0; i < args.Length; i++)
				objs[i + 1] = args[i];

			args = objs;
		}

		Lua?.GetFunction("hook.Call").TryCall(args);
	}

	private static void LoadingLuaDir(string dir)
	{
		foreach (var f in Directory.GetFiles(dir, "*.lua"))
		{
			IncludeLuaFile(f);
		}
	}

	public void InitializationLua()
	{
		Instance.Logger.LogInfo($"dirpath: {dirpath}");

		Lua = new NLua.Lua();
		Lua.State.Encoding = Encoding.UTF8;

#if CPP
		Lua["CPP"] = true;
#endif

		Lua.RegisterFunction("include", this.GetType().GetMethod(nameof(IncludeLuaFile)));
		Lua.RegisterFunction("typeof", typeof(ReflectionHelpers).GetMethod(nameof(ReflectionHelpers.GetActualType)));
		Lua.RegisterFunction("ctype", this.GetType().GetMethod(nameof(GetTypeName)));
		Lua.RegisterFunction("cprint", this.GetType().GetMethod(nameof(LuaCPrint)));

		// TODO
		// if (IsLoadMonoCSharp) Lua.RegisterFunction("Evaluator", this.GetType().GetMethod(nameof(CreateEvaluator)));

		Instance.Logger.LogDebug($"packagepath = {packagepath}");
		Lua.DoString(packagepath);

		Instance.Logger.LogDebug($"cpackagepath = {cpackagepath}");
		Lua.DoString(cpackagepath);

		IncludeLuaFile("Lua/includes/luanet.lua");

		Lua.DoString(importloader);

		Lua.DoString(@"
local Msg = Loader.LogMessage
local table = table

function print(...)
	local r = {}

	for i = 1, select('#', ...) do table.insert(r, tostring(select(i, ...))) end

	if #r == 0 then table.insert(r, 'nil') end

	Msg(table.concat(r, '  '))
end
");

		loadingincludes = true;

		IncludeLuaFile("Lua/includes/init.lua");

		loadingincludes = false;
		luaenvloaded = true;
	}

	public static void LoadingLua(bool reload = false)
	{
		const string path = "Lua/autorun";

		LoadingLuaDir(path);

#if DEBUG
		LuaCall("test");
#endif

		if (reload)
			return;

		static void Include(object sender, FileSystemEventArgs args)
		{
			IncludeLuaFile(Path.Combine(path, Path.GetFileName(args.Name)));
		}

		var fileSystemWatcher = new FileSystemWatcher(path)
		{
			NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName,
			Filter = "*.lua",
		};

		fileSystemWatcher.Changed += Include;
		fileSystemWatcher.Renamed += Include;
		fileSystemWatcher.EnableRaisingEvents = true;
	}

	public static void ReloadLua()
	{
		Instance.Logger.LogInfo("The lua environment is being reloading");
		LoadingLua(true);
		Instance.Logger.LogInfo("The lua environment has been reloaded!");
	}

	public static object[]? IncludeLuaFile(string name, bool isunsafe = false)
	{
		name = name.Replace("\\", "/");

		if (!isunsafe)
		{
			var ext = Path.GetExtension(name);

			if (ext != null && ext?.Length == 0)
				name += ".lua";

			if (loadingincludes && File.Exists("Lua/includes/" + name))
				name = "Lua/includes/" + name;

			if (!name.StartsWith("Lua/"))
				name = "Lua/" + name;
		}

		Instance.Logger.LogDebug($"Loading Lua file: {name}");

		try
		{
			return Lua?.DoFile(name);
		}
		catch (Exception e)
		{
			LuaError(e);
		}

		return null;
	}

	public static void LuaCPrint(params object[] args)
	{
		args ??= [];

		Instance.Logger.LogMessage(MakeString(args));
	}

	private static string MakeString(object[] args)
	{
		if (args.Length == 0 || args[0] == null)
		{
			return string.Empty;
		}

		var sb = new StringBuilder();
		var text = args[0].ToString();

		if (text != null)
			sb.Append(text);

		for (var i = 1; i < args.Length; i++)
		{
			sb.Append("  ");
			if (args[i] != null)
			{
				if (args[i] is NLua.ProxyType ntype)
					args[i] = ntype.UnderlyingSystemType;

				text = args[i].ToString();
				if (text != null)
					sb.Append(text);
			}
		}

		return sb.ToString();
	}

	private static string EscapeLuaString(string text)
	{
		return text
			.Replace("\\", "\\\\", StringComparison.Ordinal)
			.Replace("'", "\\'", StringComparison.Ordinal)
			.Replace("\"", "\\\"", StringComparison.Ordinal);
	}

	public static string? GetTypeName(object obj)
	{
		var type = ReflectionHelpers.GetActualType(obj);

		if (type == null) return null;

		return type.Name;
	}

	public static void AddMethodDynamically(TypeBuilder myTypeBld, string mthdName, Type[] mthdParams, Type returnType, string mthdAction)
	{
		var myMthdBld = myTypeBld.DefineMethod(mthdName, MethodAttributes.Public | MethodAttributes.Static, returnType, mthdParams);
		var ILout = myMthdBld.GetILGenerator();
		var numParams = mthdParams.Length;
		for (byte x = 0; x < numParams; x++)
		{
			ILout.Emit(OpCodes.Ldarg_S, x);
		}

		if (numParams > 1)
		{
			for (var y = 0; y < (numParams - 1); y++)
			{
				switch (mthdAction)
				{
					case "A":
						ILout.Emit(OpCodes.Add);
						break;
					case "M":
						ILout.Emit(OpCodes.Mul);
						break;
					default:
						ILout.Emit(OpCodes.Add);
						break;
				}
			}
		}

		ILout.Emit(OpCodes.Ret);
	}

	public static void LuaError(Exception e)
	{
		if (luaenvloaded)
		{
			try
			{
				Lua?.GetFunction("hook.Call").Call("OnLuaError", e.ToString(), e);
			}
			catch (Exception e2)
			{
				Instance.Logger.LogError(e2.ToString());
			}
		}

		Instance.Logger.LogError(e.ToString());
	}

	public static void LuaError(object e)
	{
		if (luaenvloaded)
		{
			try
			{
				Lua?.GetFunction("hook.Call").Call("OnLuaError", e.ToString());
			}
			catch (Exception e2)
			{
				Instance.Logger.LogError(e2.ToString());
			}
		}

		Instance.Logger.LogError(e.ToString());
	}

	// TODO
	/* public static Evaluator CreateEvaluator()
	{
		if (!IsLoadMonoCSharp)
		{
			Instance.Logger.LogWarning("Assembly 'Mono.CSharp.dll' was not loaded");
			return null;
		}

		var ctx = CreateContext(new StreamReportPrinter(new LoggerTextWriter()));

		return new Evaluator(ctx);
	}

	private static CompilerContext CreateContext(ReportPrinter reportPrinter)
	{
		var settings = new CompilerSettings
		{
			Version = LanguageVersion.Experimental,
			GenerateDebugInfo = false,
			StdLib = true,
			Target = Target.Library,
		};

		return new CompilerContext(settings, reportPrinter);
	} */
}

internal class LoggerTextWriter : TextWriter
{
	private readonly StringBuilder sb = new();

	public override Encoding Encoding { get; } = Encoding.UTF8;

	public override void Write(char value)
	{
		if (value == '\n')
		{
			LuaLoader.Instance.Logger.LogWarning(this.sb.ToString());
			this.sb.Length = 0;
			return;
		}

		this.sb.Append(value);
	}
}
