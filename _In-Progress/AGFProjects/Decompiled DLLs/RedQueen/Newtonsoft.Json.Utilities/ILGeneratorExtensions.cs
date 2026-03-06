using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;

namespace Newtonsoft.Json.Utilities;

[_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(0)]
[_003C464f7c58_002D4ec4_002D4694_002Da30c_002D0ded4d74fb4d_003ENullableContext(1)]
internal static class ILGeneratorExtensions
{
	public static void PushInstance(this ILGenerator generator, Type type)
	{
		generator.Emit(OpCodes.Ldarg_0);
		if (type.IsValueType())
		{
			generator.Emit(OpCodes.Unbox, type);
		}
		else
		{
			generator.Emit(OpCodes.Castclass, type);
		}
	}

	public static void PushArrayInstance(this ILGenerator generator, int argsIndex, int arrayIndex)
	{
		generator.Emit(OpCodes.Ldarg, argsIndex);
		generator.Emit(OpCodes.Ldc_I4, arrayIndex);
		generator.Emit(OpCodes.Ldelem_Ref);
	}

	public static void BoxIfNeeded(this ILGenerator generator, Type type)
	{
		if (type.IsValueType())
		{
			generator.Emit(OpCodes.Box, type);
		}
		else
		{
			generator.Emit(OpCodes.Castclass, type);
		}
	}

	public static void UnboxIfNeeded(this ILGenerator generator, Type type)
	{
		if (type.IsValueType())
		{
			generator.Emit(OpCodes.Unbox_Any, type);
		}
		else
		{
			generator.Emit(OpCodes.Castclass, type);
		}
	}

	public static void CallMethod(this ILGenerator generator, MethodInfo methodInfo)
	{
		if (methodInfo.IsFinal || !methodInfo.IsVirtual)
		{
			generator.Emit(OpCodes.Call, methodInfo);
		}
		else
		{
			generator.Emit(OpCodes.Callvirt, methodInfo);
		}
	}

	public static void Return(this ILGenerator generator)
	{
		generator.Emit(OpCodes.Ret);
	}
}
