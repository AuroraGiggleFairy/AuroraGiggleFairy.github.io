using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public abstract class vp_EventHandler : MonoBehaviour
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public class ScriptMethods
	{
		public List<MethodInfo> Events = new List<MethodInfo>();

		public ScriptMethods(Type type)
		{
			Events = GetMethods(type);
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public static List<MethodInfo> GetMethods(Type type)
		{
			List<MethodInfo> list = new List<MethodInfo>();
			List<string> list2 = new List<string>();
			while (type != null)
			{
				MethodInfo[] methods = type.GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
				foreach (MethodInfo methodInfo in methods)
				{
					if (methodInfo.Name.Contains(">m__") || list2.Contains(methodInfo.Name))
					{
						continue;
					}
					string[] supportedPrefixes = m_SupportedPrefixes;
					foreach (string value in supportedPrefixes)
					{
						if (methodInfo.Name.Contains(value))
						{
							list.Add(methodInfo);
							list2.Add(methodInfo.Name);
							break;
						}
					}
				}
				type = type.BaseType;
			}
			return list;
		}
	}

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public bool m_Initialized;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public Dictionary<string, vp_Event> m_HandlerEvents = new Dictionary<string, vp_Event>();

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public List<object> m_PendingRegistrants = new List<object>();

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public static Dictionary<Type, ScriptMethods> m_StoredScriptTypes = new Dictionary<Type, ScriptMethods>();

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public static string[] m_SupportedPrefixes = new string[10] { "OnMessage_", "CanStart_", "CanStop_", "OnStart_", "OnStop_", "OnAttempt_", "get_OnValue_", "set_OnValue_", "OnFailStart_", "OnFailStop_" };

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void Awake()
	{
		StoreHandlerEvents();
		m_Initialized = true;
		for (int num = m_PendingRegistrants.Count - 1; num > -1; num--)
		{
			Register(m_PendingRegistrants[num]);
			m_PendingRegistrants.Remove(m_PendingRegistrants[num]);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void StoreHandlerEvents()
	{
		object obj = null;
		List<FieldInfo> fields = GetFields();
		if (fields == null || fields.Count == 0)
		{
			return;
		}
		foreach (FieldInfo item in fields)
		{
			try
			{
				obj = Activator.CreateInstance(item.FieldType, item.Name);
			}
			catch (Exception ex)
			{
				Debug.LogError("Error: (" + this?.ToString() + ") does not support the type of '" + item.Name + "' in '" + item.DeclaringType?.ToString() + "'. Exception: " + ex.Message);
				continue;
			}
			if (obj == null)
			{
				continue;
			}
			item.SetValue(this, obj);
			foreach (string key in ((vp_Event)obj).Prefixes.Keys)
			{
				m_HandlerEvents.Add(key + item.Name, (vp_Event)obj);
			}
		}
	}

	public List<FieldInfo> GetFields()
	{
		List<FieldInfo> list = new List<FieldInfo>();
		Type type = GetType();
		Type type2 = null;
		do
		{
			if (type2 != null)
			{
				type = type2;
			}
			list.AddRange(type.GetFields(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic));
			if (type.BaseType != typeof(vp_StateEventHandler) && type.BaseType != typeof(vp_EventHandler))
			{
				type2 = type.BaseType;
			}
		}
		while (type.BaseType != typeof(vp_StateEventHandler) && type.BaseType != typeof(vp_EventHandler) && type != null);
		if (list == null || list.Count == 0)
		{
			Debug.LogWarning("Warning: (" + this?.ToString() + ") Found no fields to store as events.");
		}
		return list;
	}

	public void Register(object target)
	{
		if (target == null)
		{
			Debug.LogError("Error: (" + this?.ToString() + ") Target object was null.");
			return;
		}
		if (!m_Initialized)
		{
			m_PendingRegistrants.Add(target);
			return;
		}
		ScriptMethods scriptMethods = GetScriptMethods(target);
		if (scriptMethods == null)
		{
			Debug.LogError("Error: (" + this?.ToString() + ") could not get script methods for '" + target?.ToString() + "'.");
			return;
		}
		foreach (MethodInfo @event in scriptMethods.Events)
		{
			if (m_HandlerEvents.TryGetValue(@event.Name, out var value))
			{
				value.Prefixes.TryGetValue(@event.Name.Substring(0, @event.Name.IndexOf('_', 4) + 1), out var value2);
				if (CompareMethodSignatures(@event, value.GetParameterType(value2), value.GetReturnType(value2)))
				{
					value.Register(target, @event.Name, value2);
				}
			}
		}
	}

	public void Unregister(object target)
	{
		if (target == null)
		{
			Debug.LogError("Error: (" + this?.ToString() + ") Target object was null.");
			return;
		}
		foreach (vp_Event value2 in m_HandlerEvents.Values)
		{
			if (value2 == null)
			{
				continue;
			}
			string[] invokerFieldNames = value2.InvokerFieldNames;
			foreach (string text in invokerFieldNames)
			{
				FieldInfo field = value2.GetType().GetField(text);
				if (field == null)
				{
					continue;
				}
				object value = field.GetValue(value2);
				if (value == null)
				{
					continue;
				}
				Delegate obj = (Delegate)value;
				if ((object)obj == null)
				{
					continue;
				}
				Delegate[] invocationList = obj.GetInvocationList();
				for (int j = 0; j < invocationList.Length; j++)
				{
					if (invocationList[j].Target == target)
					{
						value2.Unregister(target);
					}
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool CompareMethodSignatures(MethodInfo scriptMethod, Type handlerParameterType, Type handlerReturnType)
	{
		if (scriptMethod.ReturnType != handlerReturnType)
		{
			Debug.LogError("Error: (" + scriptMethod.DeclaringType?.ToString() + ") Return type (" + vp_Utility.GetTypeAlias(scriptMethod.ReturnType) + ") is not valid for '" + scriptMethod.Name + "'. Return type declared in event handler was: (" + vp_Utility.GetTypeAlias(handlerReturnType) + ").");
			return false;
		}
		if (scriptMethod.GetParameters().Length == 1)
		{
			if (((ParameterInfo)scriptMethod.GetParameters().GetValue(0)).ParameterType != handlerParameterType)
			{
				Debug.LogError("Error: (" + scriptMethod.DeclaringType?.ToString() + ") Parameter type (" + vp_Utility.GetTypeAlias(((ParameterInfo)scriptMethod.GetParameters().GetValue(0)).ParameterType) + ") is not valid for '" + scriptMethod.Name + "'. Parameter type declared in event handler was: (" + vp_Utility.GetTypeAlias(handlerParameterType) + ").");
				return false;
			}
		}
		else if (scriptMethod.GetParameters().Length == 0)
		{
			if (handlerParameterType != typeof(void))
			{
				Debug.LogError("Error: (" + scriptMethod.DeclaringType?.ToString() + ") Can't register method '" + scriptMethod.Name + "' with 0 parameters. Expected: 1 parameter of type (" + vp_Utility.GetTypeAlias(handlerParameterType) + ").");
				return false;
			}
		}
		else if (scriptMethod.GetParameters().Length > 1)
		{
			Debug.LogError("Error: (" + scriptMethod.DeclaringType?.ToString() + ") Can't register method '" + scriptMethod.Name + "' with " + scriptMethod.GetParameters().Length + " parameters. Max parameter count: 1 of type (" + vp_Utility.GetTypeAlias(handlerParameterType) + ").");
			return false;
		}
		return true;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public ScriptMethods GetScriptMethods(object target)
	{
		if (!m_StoredScriptTypes.TryGetValue(target.GetType(), out var value))
		{
			value = new ScriptMethods(target.GetType());
			m_StoredScriptTypes.Add(target.GetType(), value);
		}
		return value;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public vp_EventHandler()
	{
	}
}
