using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using NLua;

namespace LuaLoader.LuaClass;

public class LuaHarmony
{
	private readonly string id;
	private readonly Harmony harmony;
	private readonly TypeBuilder typeBuilder;
	private Type? type;
	private int methodId;

	public LuaHarmony(string id)
	{
		this.id = id;

		this.methodId = 0;
		this.harmony = new Harmony(id);

		var assName = new AssemblyName("LuaHarmony_" + id);
		var assBuilder = AssemblyBuilder.DefineDynamicAssembly(assName, AssemblyBuilderAccess.Run);
		var moduleBuilder = assBuilder.DefineDynamicModule(assName.Name);

		this.typeBuilder = moduleBuilder.DefineType("LuaHarmony_" + id, TypeAttributes.Public);
	}

	public void CreateMethod(LuaTable table, LuaFunction func)
	{
		var methodBuilder = this.typeBuilder.DefineMethod("LuaHarmonyMethod_" + this.methodId, MethodAttributes.Public | MethodAttributes.Static, null, [typeof(int)]);

		this.methodId++;

		// https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.typebuilder?view=net-6.0
		// https://github.com/pardeike/Harmony/blob/56ac45a7e79bf0575298ab7cbbc20ded5418d796/Harmony/Internal/MethodPatcher.cs

		var methIL = methodBuilder.GetILGenerator();

		//emitter.Emit(OpCodes.Ldnull)
		// To retrieve the private instance field, load the instance it
		// belongs to (argument zero). After loading the field, load the
		// argument one and then multiply. Return from the method with
		// the return value (the product of the two numbers) on the
		// execution stack.
		//methIL.Emit(OpCodes.Ldarg_0);
		//methIL.Emit(OpCodes.Ldfld, fbNumber);
		//methIL.Emit(OpCodes.Ldarg_1);
		//methIL.Emit(OpCodes.Mul);
		//methIL.Emit(OpCodes.Ret);

	}

	public Type CreateType()
	{
		this.type = this.typeBuilder.CreateType();

		return this.type;
	}

	/* public void Patch(LuaTable table)
	{
		//int priority = -1, string[] before = null, string[] after = null, bool? debug = null
		//this.priority = priority;
		//this.before = before;
		//this.after = after;
		//this.debug = debug;

		//PatchProcessor patchProcessor = harmony.CreateProcessor(original);
		////patchProcessor.AddPrefix(prefix);
		////patchProcessor.AddPostfix(postfix);
		////patchProcessor.AddTranspiler(transpiler);
		////patchProcessor.AddFinalizer(finalizer);
		////patchProcessor.AddILManipulator(ilmanipulator);
		////return patchProcessor.Patch();
	} */
}
