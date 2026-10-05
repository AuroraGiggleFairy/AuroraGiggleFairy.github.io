using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace QuartermasterCrafting
{
	// Net package classes stay abstract. 3.3 has no GetLength; 3.2 requires it.
	// A concrete type is built at runtime so the same DLL loads on both.
	internal static class PackageEmit
	{
		private static readonly Dictionary<Type, Type> ConcreteTypes = new Dictionary<Type, Type>();
		private static readonly Dictionary<Type, Func<NetPackage>> Takers = new Dictionary<Type, Func<NetPackage>>();
		private static ModuleBuilder module;
		private static bool registered;

		internal static void Prepare(params Type[] packageTypes)
		{
			GameVersion.Initialize();
			for (int i = 0; i < packageTypes.Length; i++)
			{
				Concrete(packageTypes[i]);
			}

			Register();
		}

		internal static T Take<T>() where T : NetPackage
		{
			Type concrete = Concrete(typeof(T));
			if (!registered)
			{
				Register();
			}

			if (!Takers.TryGetValue(concrete, out Func<NetPackage> take))
			{
				MethodInfo method = typeof(NetPackageManager).GetMethod("GetPackage").MakeGenericMethod(concrete);
				take = () => (NetPackage)method.Invoke(null, null);
				Takers[concrete] = take;
			}

			return (T)take();
		}

		private static Type Concrete(Type packageType)
		{
			if (ConcreteTypes.TryGetValue(packageType, out Type existing))
			{
				return existing;
			}

			if (module == null)
			{
				AssemblyBuilder assembly = AssemblyBuilder.DefineDynamicAssembly(
					new AssemblyName(packageType.Assembly.GetName().Name + ".Packages"),
					AssemblyBuilderAccess.Run);
				module = assembly.DefineDynamicModule("packages");
			}

			TypeBuilder builder = module.DefineType(
				packageType.Name,
				TypeAttributes.Public | TypeAttributes.Class,
				packageType);
			ConstructorInfo baseCtor = packageType.GetConstructor(
				BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
				null,
				Type.EmptyTypes,
				null);
			ConstructorBuilder ctor = builder.DefineConstructor(
				MethodAttributes.Public,
				CallingConventions.Standard,
				Type.EmptyTypes);
			ILGenerator ctorIl = ctor.GetILGenerator();
			ctorIl.Emit(OpCodes.Ldarg_0);
			ctorIl.Emit(OpCodes.Call, baseCtor);
			ctorIl.Emit(OpCodes.Ret);

			if (!GameVersion.UseV33)
			{
				MethodInfo slot = typeof(NetPackage).GetMethod("GetLength", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
				MethodInfo body = packageType.GetMethod("GetLength", BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);
				MethodBuilder getLength = builder.DefineMethod(
					"GetLength",
					MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.HideBySig,
					typeof(int),
					Type.EmptyTypes);
				ILGenerator lengthIl = getLength.GetILGenerator();
				lengthIl.Emit(OpCodes.Ldarg_0);
				lengthIl.Emit(OpCodes.Call, body);
				lengthIl.Emit(OpCodes.Ret);
				builder.DefineMethodOverride(getLength, slot);
			}

			Type created = builder.CreateType();
			ConcreteTypes[packageType] = created;
			registered = false;
			return created;
		}

		private static void Register()
		{
			GameVersion.Initialize();
			FieldInfo field = typeof(NetPackageManager).GetField("knownPackageTypes", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
			IDictionary known = field?.GetValue(null) as IDictionary;
			if (known == null)
			{
				return;
			}

			foreach (KeyValuePair<Type, Type> pair in ConcreteTypes)
			{
				string name = pair.Key.Name;
				if (!known.Contains(name))
				{
					known.Add(name, pair.Value);
				}

				MapIfLive(pair.Value);
			}

			registered = true;
		}

		private static void MapIfLive(Type concrete)
		{
			const BindingFlags flags = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
			FieldInfo ids = typeof(NetPackageManager).GetField("packageIdToClass", flags);
			Array current = ids?.GetValue(null) as Array;
			if (current == null)
			{
				return;
			}

			MethodInfo add = typeof(NetPackageManager).GetMethod("AddPackageMapping", flags);
			add?.Invoke(null, new object[] { current.Length, concrete });
		}
	}
}
