using System;
using System.Reflection;
using UnityEngine;

public static class CloneModelAccess
{
	static MethodInfo clone3;
	static MethodInfo clone6;
	static FieldInfo instanceField;
	static object purpose3;
	static object purpose6;
	static object defaultTexture;
	static PropertyInfo worldProperty;
	static FieldInfo worldField;
	static PropertyInfo managerProperty;
	static FieldInfo managerField;
	static bool ready;

	public static Transform TransformOf(object item, object itemValue, Transform parent)
	{
		if (!ready)
		{
			Bind(item);
		}

		if (clone3 != null)
		{
			object handle = clone3.Invoke(item, new object[] { itemValue, parent, purpose3 });
			object instance = instanceField.GetValue(handle);
			GameObject go = instance as GameObject;
			return go != null ? go.transform : null;
		}

		object world = World();
		return (Transform)clone6.Invoke(item, new object[] { world, itemValue, Vector3.zero, parent, purpose6, defaultTexture });
	}

	static void Bind(object item)
	{
		ready = true;
		Type type = item.GetType();
		clone3 = Find(type, 3);
		clone6 = Find(type, 6);
		if (clone3 != null)
		{
			ParameterInfo[] parameters = clone3.GetParameters();
			purpose3 = Enum.ToObject(parameters[2].ParameterType, 0);
			instanceField = clone3.ReturnType.GetField("Instance", BindingFlags.Instance | BindingFlags.Public);
			return;
		}

		ParameterInfo[] oldParameters = clone6.GetParameters();
		purpose6 = Enum.ToObject(oldParameters[4].ParameterType, 0);
		defaultTexture = Activator.CreateInstance(oldParameters[5].ParameterType);
		Type manager = FindType("GameManager");
		managerProperty = manager.GetProperty("Instance", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
		if (managerProperty == null)
		{
			managerField = manager.GetField("Instance", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
		}

		object instance = managerProperty != null ? managerProperty.GetValue(null, null) : managerField.GetValue(null);
		Type worldType = instance.GetType();
		worldProperty = worldType.GetProperty("World", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		if (worldProperty == null)
		{
			worldField = worldType.GetField("World", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		}
	}

	static object World()
	{
		object instance = managerProperty != null ? managerProperty.GetValue(null, null) : managerField.GetValue(null);
		return worldProperty != null ? worldProperty.GetValue(instance, null) : worldField.GetValue(instance);
	}

	static MethodInfo Find(Type type, int count)
	{
		while (type != null)
		{
			foreach (MethodInfo method in type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly))
			{
				if (method.Name == "CloneModel" && method.GetParameters().Length == count)
				{
					return method;
				}
			}

			type = type.BaseType;
		}

		return null;
	}

	static Type FindType(string name)
	{
		foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
		{
			Type type = assembly.GetType(name);
			if (type != null)
			{
				return type;
			}
		}

		return null;
	}
}
