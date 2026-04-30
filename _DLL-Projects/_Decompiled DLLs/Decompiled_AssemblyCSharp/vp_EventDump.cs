using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine.Scripting;

[Preserve]
public class vp_EventDump
{
	public static string Dump(vp_EventHandler handler, string[] eventTypes)
	{
		string text = "";
		for (int i = 0; i < eventTypes.Length; i++)
		{
			switch (eventTypes[i])
			{
			case "vp_Message":
				text += DumpEventsOfType("vp_Message", (eventTypes.Length > 1) ? "MESSAGES:\n\n" : "", handler);
				break;
			case "vp_Attempt":
				text += DumpEventsOfType("vp_Attempt", (eventTypes.Length > 1) ? "ATTEMPTS:\n\n" : "", handler);
				break;
			case "vp_Value":
				text += DumpEventsOfType("vp_Value", (eventTypes.Length > 1) ? "VALUES:\n\n" : "", handler);
				break;
			case "vp_Activity":
				text += DumpEventsOfType("vp_Activity", (eventTypes.Length > 1) ? "ACTIVITIES:\n\n" : "", handler);
				break;
			}
		}
		return text;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static string DumpEventsOfType(string type, string caption, vp_EventHandler handler)
	{
		string text = caption.ToUpper();
		foreach (FieldInfo field in handler.GetFields())
		{
			string text2 = null;
			switch (type)
			{
			case "vp_Message":
				if (field.FieldType.ToString().Contains("vp_Message"))
				{
					text2 = DumpEventListeners((vp_Message)field.GetValue(handler), new string[1] { "Send" });
				}
				break;
			case "vp_Attempt":
				if (field.FieldType.ToString().Contains("vp_Attempt"))
				{
					text2 = DumpEventListeners((vp_Event)field.GetValue(handler), new string[1] { "Try" });
				}
				break;
			case "vp_Value":
				if (field.FieldType.ToString().Contains("vp_Value"))
				{
					text2 = DumpEventListeners((vp_Event)field.GetValue(handler), new string[2] { "Get", "Set" });
				}
				break;
			case "vp_Activity":
				if (field.FieldType.ToString().Contains("vp_Activity"))
				{
					text2 = DumpEventListeners((vp_Event)field.GetValue(handler), new string[6] { "StartConditions", "StopConditions", "StartCallbacks", "StopCallbacks", "FailStartCallbacks", "FailStopCallbacks" });
				}
				break;
			}
			if (!string.IsNullOrEmpty(text2))
			{
				text = text + "\t\t" + field.Name + "\n" + text2 + "\n";
			}
		}
		return text;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static string DumpEventListeners(object e, string[] invokers)
	{
		Type type = e.GetType();
		string text = "";
		foreach (string text2 in invokers)
		{
			FieldInfo field = type.GetField(text2);
			if (field == null)
			{
				return "";
			}
			Delegate obj = (Delegate)field.GetValue(e);
			string[] array = null;
			if ((object)obj != null)
			{
				array = GetMethodNames(obj.GetInvocationList());
			}
			text += "\t\t\t\t";
			text = (type.ToString().Contains("vp_Value") ? ((text2 == "Get") ? (text + "Get") : ((!(text2 == "Set")) ? (text + "Unsupported listener: ") : (text + "Set"))) : (type.ToString().Contains("vp_Attempt") ? (text + "Try") : (type.ToString().Contains("vp_Message") ? (text + "Send") : ((!type.ToString().Contains("vp_Activity")) ? (text + "Unsupported listener") : (text2 switch
			{
				"StartConditions" => text + "TryStart", 
				"StopConditions" => text + "TryStop", 
				"StartCallbacks" => text + "Start", 
				"StopCallbacks" => text + "Stop", 
				"FailStartCallbacks" => text + "FailStart", 
				"FailStopCallbacks" => text + "FailStop", 
				_ => text + "Unsupported listener: ", 
			})))));
			if (array != null)
			{
				text = ((array.Length <= 2) ? (text + ": ") : (text + ":\n"));
				text += DumpDelegateNames(array);
			}
		}
		return text;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static string[] GetMethodNames(Delegate[] list)
	{
		list = RemoveDelegatesFromList(list);
		string[] array = new string[list.Length];
		if (list.Length == 1)
		{
			array[0] = ((list[0].Target == null) ? "" : ("(" + list[0].Target?.ToString() + ") ")) + list[0].Method.Name;
		}
		else
		{
			for (int i = 1; i < list.Length; i++)
			{
				array[i] = ((list[i].Target == null) ? "" : ("(" + list[i].Target?.ToString() + ") ")) + list[i].Method.Name;
			}
		}
		return array;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static Delegate[] RemoveDelegatesFromList(Delegate[] list)
	{
		List<Delegate> list2 = new List<Delegate>(list);
		for (int num = list2.Count - 1; num > -1; num--)
		{
			if ((object)list2[num] != null && list2[num].Method.Name.Contains("m_"))
			{
				list2.RemoveAt(num);
			}
		}
		return list2.ToArray();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static string DumpDelegateNames(string[] array)
	{
		string text = "";
		foreach (string text2 in array)
		{
			if (!string.IsNullOrEmpty(text2))
			{
				text = text + ((array.Length > 2) ? "\t\t\t\t\t\t\t" : "") + text2 + "\n";
			}
		}
		return text;
	}
}
