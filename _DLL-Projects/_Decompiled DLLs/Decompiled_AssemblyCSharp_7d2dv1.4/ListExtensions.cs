using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

public static class ListExtensions
{
	[PublicizedFrom(EAccessModifier.Private)]
	public static class ArrayAccessor<T>
	{
		public static Func<List<T>, T[]> Getter;

		[PublicizedFrom(EAccessModifier.Private)]
		static ArrayAccessor()
		{
			DynamicMethod dynamicMethod = new DynamicMethod("get", MethodAttributes.Public | MethodAttributes.Static, CallingConventions.Standard, typeof(T[]), new Type[1] { typeof(List<T>) }, typeof(ArrayAccessor<T>), skipVisibility: true);
			ILGenerator iLGenerator = dynamicMethod.GetILGenerator();
			iLGenerator.Emit(OpCodes.Ldarg_0);
			iLGenerator.Emit(OpCodes.Ldfld, typeof(List<T>).GetField("_items", BindingFlags.Instance | BindingFlags.NonPublic));
			iLGenerator.Emit(OpCodes.Ret);
			Getter = (Func<List<T>, T[]>)dynamicMethod.CreateDelegate(typeof(Func<List<T>, T[]>));
		}
	}

	public static T[] GetInternalArray<T>(this List<T> list)
	{
		return ArrayAccessor<T>.Getter(list);
	}
}
