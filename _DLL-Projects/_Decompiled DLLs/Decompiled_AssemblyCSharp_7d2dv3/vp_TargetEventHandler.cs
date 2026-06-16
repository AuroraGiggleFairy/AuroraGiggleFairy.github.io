using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

[PublicizedFrom(EAccessModifier.Internal)]
public static class vp_TargetEventHandler
{
	[PublicizedFrom(EAccessModifier.Private)]
	public static List<Dictionary<object, Dictionary<string, Delegate>>> m_TargetDict;

	public static List<Dictionary<object, Dictionary<string, Delegate>>> TargetDict
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			if (m_TargetDict == null)
			{
				m_TargetDict = new List<Dictionary<object, Dictionary<string, Delegate>>>(100);
				for (int i = 0; i < 8; i++)
				{
					m_TargetDict.Add(new Dictionary<object, Dictionary<string, Delegate>>(100));
				}
			}
			return m_TargetDict;
		}
	}

	public static void Register(object target, string eventName, Delegate callback, int dictionary)
	{
		if (target == null)
		{
			Debug.LogWarning("Warning: (" + vp_Utility.GetErrorLocation(2) + " -> vp_TargetEvent.Register) Target object is null.");
			return;
		}
		if (string.IsNullOrEmpty(eventName))
		{
			Debug.LogWarning("Warning: (" + vp_Utility.GetErrorLocation(2) + " -> vp_TargetEvent.Register) Event name is null or empty.");
			return;
		}
		if ((object)callback == null)
		{
			Debug.LogWarning("Warning: (" + vp_Utility.GetErrorLocation(2) + " -> vp_TargetEvent.Register) Callback is null.");
			return;
		}
		if (callback.Method.Name.StartsWith("<"))
		{
			Debug.LogWarning("Warning: (" + vp_Utility.GetErrorLocation(2) + " -> vp_TargetEvent.Register) Target events can only be registered to declared methods.");
			return;
		}
		if (!TargetDict[dictionary].ContainsKey(target))
		{
			TargetDict[dictionary].Add(target, new Dictionary<string, Delegate>(100));
		}
		TargetDict[dictionary].TryGetValue(target, out var value);
		Delegate value2;
		while (true)
		{
			value.TryGetValue(eventName, out value2);
			if ((object)value2 == null)
			{
				value.Add(eventName, callback);
				return;
			}
			if (!(value2.GetType() != callback.GetType()))
			{
				break;
			}
			eventName += "_";
		}
		callback = Delegate.Combine(value2, callback);
		if ((object)callback != null)
		{
			value.Remove(eventName);
			value.Add(eventName, callback);
		}
	}

	public static void Unregister(object target, string eventName = null, Delegate callback = null)
	{
		if ((eventName != null || (object)callback == null) && ((object)callback != null || eventName == null))
		{
			for (int i = 0; i < 8; i++)
			{
				Unregister(target, i, eventName, callback);
			}
		}
	}

	public static void Unregister(Component component)
	{
		if (!(component == null))
		{
			for (int i = 0; i < 8; i++)
			{
				Unregister(i, component);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void Unregister(int dictionary, Component component)
	{
		if (component == null)
		{
			return;
		}
		if (TargetDict[dictionary].TryGetValue(component, out var value))
		{
			TargetDict[dictionary].Remove(component);
		}
		object transform = component.transform;
		if (transform == null || !TargetDict[dictionary].TryGetValue(transform, out value))
		{
			return;
		}
		foreach (string item in new List<string>(value.Keys))
		{
			if (item == null || !value.TryGetValue(item, out var value2) || (object)value2 == null)
			{
				continue;
			}
			Delegate[] invocationList = value2.GetInvocationList();
			if (invocationList == null || invocationList.Length < 1)
			{
				continue;
			}
			for (int num = invocationList.Length - 1; num > -1; num--)
			{
				if (invocationList[num].Target as Component == component)
				{
					value.Remove(item);
					Delegate obj = Delegate.Remove(value2, invocationList[num]);
					if (obj.GetInvocationList().Length != 0)
					{
						value.Add(item, obj);
					}
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void Unregister(object target, int dictionary, string eventName, Delegate callback)
	{
		if (target == null || !TargetDict[dictionary].TryGetValue(target, out var value) || value == null || value.Count == 0)
		{
			return;
		}
		if (eventName == null && (object)callback == null)
		{
			TargetDict[dictionary].Remove(value);
		}
		else
		{
			if (!value.TryGetValue(eventName, out var value2))
			{
				return;
			}
			if ((object)value2 != null)
			{
				value.Remove(eventName);
				value2 = Delegate.Remove(value2, callback);
				if ((object)value2 != null && value2.GetInvocationList() != null)
				{
					value.Add(eventName, value2);
				}
			}
			else
			{
				value.Remove(eventName);
			}
			if (value.Count <= 0)
			{
				TargetDict[dictionary].Remove(target);
			}
		}
	}

	public static void UnregisterAll()
	{
		m_TargetDict = null;
	}

	public static Delegate GetCallback(object target, string eventName, bool upwards, int d, vp_TargetEventOptions options)
	{
		if (target == null)
		{
			return null;
		}
		if (string.IsNullOrEmpty(eventName))
		{
			return null;
		}
		Delegate value;
		do
		{
			value = null;
			if ((options & vp_TargetEventOptions.IncludeInactive) == vp_TargetEventOptions.IncludeInactive)
			{
				goto IL_005f;
			}
			GameObject gameObject = target as GameObject;
			if (gameObject != null)
			{
				if (vp_Utility.IsActive(gameObject))
				{
					goto IL_005f;
				}
				if (!upwards)
				{
					return null;
				}
			}
			else
			{
				Behaviour behaviour = target as Behaviour;
				if (!(behaviour != null) || (behaviour.enabled && vp_Utility.IsActive(behaviour.gameObject)))
				{
					goto IL_005f;
				}
				if (!upwards)
				{
					return null;
				}
			}
			goto IL_008b;
			IL_008b:
			if (!((object)value == null && upwards))
			{
				break;
			}
			target = vp_Utility.GetParent(target as Component);
			continue;
			IL_005f:
			Dictionary<string, Delegate> value2 = null;
			if (!TargetDict[d].TryGetValue(target, out value2))
			{
				if (!upwards)
				{
					return null;
				}
			}
			else if (!value2.TryGetValue(eventName, out value) && !upwards)
			{
				return null;
			}
			goto IL_008b;
		}
		while (target != null);
		return value;
	}

	public static void OnNoReceiver(string eventName, vp_TargetEventOptions options)
	{
		if ((options & vp_TargetEventOptions.RequireReceiver) == vp_TargetEventOptions.RequireReceiver)
		{
			Debug.LogError("Error: (" + vp_Utility.GetErrorLocation(2) + ") vp_TargetEvent '" + eventName + "' has no receiver!");
		}
	}

	public static string Dump()
	{
		Dictionary<object, string> dictionary = new Dictionary<object, string>();
		foreach (Dictionary<object, Dictionary<string, Delegate>> item in TargetDict)
		{
			foreach (object key in item.Keys)
			{
				string text = "";
				if (key == null)
				{
					continue;
				}
				if (item.TryGetValue(key, out var value))
				{
					foreach (string key2 in value.Keys)
					{
						text = text + "        \"" + key2 + "\" -> ";
						bool flag = false;
						if (string.IsNullOrEmpty(key2) || !value.TryGetValue(key2, out var value2))
						{
							continue;
						}
						if (value2.GetInvocationList().Length > 1)
						{
							flag = true;
							text += "\n";
						}
						Delegate[] invocationList = value2.GetInvocationList();
						foreach (Delegate obj in invocationList)
						{
							text = text + (flag ? "                        " : "") + obj.Method.ReflectedType?.ToString() + ".cs -> ";
							string text2 = "";
							ParameterInfo[] parameters = obj.Method.GetParameters();
							foreach (ParameterInfo parameterInfo in parameters)
							{
								text2 = text2 + vp_Utility.GetTypeAlias(parameterInfo.ParameterType) + " " + parameterInfo.Name + ", ";
							}
							if (text2.Length > 0)
							{
								text2 = text2.Remove(text2.LastIndexOf(", "));
							}
							text = text + vp_Utility.GetTypeAlias(obj.Method.ReturnType) + " ";
							if (obj.Method.Name.Contains("m_"))
							{
								string text3 = obj.Method.Name.TrimStart('<');
								text3 = text3.Remove(text3.IndexOf('>'));
								text = text + text3 + " -> delegate";
							}
							else
							{
								text += obj.Method.Name;
							}
							text = text + "(" + text2 + ")\n";
						}
					}
				}
				if (!dictionary.TryGetValue(key, out var value3))
				{
					dictionary.Add(key, text);
					continue;
				}
				dictionary.Remove(key);
				dictionary.Add(key, value3 + text);
			}
		}
		string text4 = "--- TARGET EVENT DUMP ---\n\n";
		foreach (object key3 in dictionary.Keys)
		{
			if (key3 != null)
			{
				text4 = text4 + key3.ToString() + ":\n";
				if (dictionary.TryGetValue(key3, out var value4))
				{
					text4 += value4;
				}
			}
		}
		return text4;
	}
}
