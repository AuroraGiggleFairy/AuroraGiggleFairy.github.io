using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public sealed class vp_ComponentPreset
{
	[PublicizedFrom(EAccessModifier.Private)]
	public enum ReadMode
	{
		Normal,
		LineComment,
		BlockComment
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public class Field
	{
		public RuntimeFieldHandle FieldHandle;

		public object Args;

		public Field(RuntimeFieldHandle fieldHandle, object args)
		{
			FieldHandle = fieldHandle;
			Args = args;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static string m_FullPath = null;

	[PublicizedFrom(EAccessModifier.Private)]
	public static int m_LineNumber = 0;

	[PublicizedFrom(EAccessModifier.Private)]
	public static Type m_Type = null;

	public static bool LogErrors = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public static ReadMode m_ReadMode = ReadMode.Normal;

	[PublicizedFrom(EAccessModifier.Private)]
	public Type m_ComponentType;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<Field> m_Fields = new List<Field>();

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<string, string[]> MovedParameters = new Dictionary<string, string[]>
	{
		{
			"vp_FPCamera.MouseAcceleration",
			new string[2] { "vp_FPInput", "MouseLookAcceleration" }
		},
		{
			"vp_FPCamera.MouseSensitivity",
			new string[2] { "vp_FPInput", "MouseLookSensitivity" }
		},
		{
			"vp_FPCamera.MouseSmoothSteps",
			new string[2] { "vp_FPInput", "MouseLookSmoothSteps" }
		},
		{
			"vp_FPCamera.MouseSmoothWeight",
			new string[2] { "vp_FPInput", "MouseLookSmoothWeight" }
		},
		{
			"vp_FPCamera.MouseAccelerationThreshold",
			new string[2] { "vp_FPInput", "MouseLookAccelerationThreshold" }
		},
		{
			"vp_FPInput.ForceCursor",
			new string[2] { "vp_FPInput", "MouseCursorForced" }
		}
	};

	public Type ComponentType
	{
		get
		{
			return m_ComponentType;
		}
		set
		{
			m_ComponentType = value;
		}
	}

	public static string Save(Component component, string fullPath)
	{
		vp_ComponentPreset obj = new vp_ComponentPreset();
		obj.InitFromComponent(component);
		return Save(obj, fullPath);
	}

	public static string Save(vp_ComponentPreset savePreset, string fullPath, bool isDifference = false)
	{
		m_FullPath = fullPath;
		bool logErrors = LogErrors;
		LogErrors = false;
		vp_ComponentPreset vp_ComponentPreset2 = new vp_ComponentPreset();
		vp_ComponentPreset2.LoadTextStream(m_FullPath);
		LogErrors = logErrors;
		if (vp_ComponentPreset2 != null)
		{
			if (vp_ComponentPreset2.m_ComponentType != null)
			{
				if (vp_ComponentPreset2.ComponentType != savePreset.ComponentType)
				{
					return "'" + ExtractFilenameFromPath(m_FullPath) + "' has the WRONG component type: " + vp_ComponentPreset2.ComponentType.ToString() + ".\n\nDo you want to replace it with a " + savePreset.ComponentType.ToString() + "?";
				}
				if (File.Exists(m_FullPath))
				{
					if (isDifference)
					{
						return "This will update '" + ExtractFilenameFromPath(m_FullPath) + "' with only the values modified since pressing Play or setting a state.\n\nContinue?";
					}
					return "'" + ExtractFilenameFromPath(m_FullPath) + "' already exists.\n\nDo you want to replace it?";
				}
			}
			if (File.Exists(m_FullPath))
			{
				return "'" + ExtractFilenameFromPath(m_FullPath) + "' has an UNKNOWN component type.\n\nDo you want to replace it?";
			}
		}
		ClearTextFile();
		Append("///////////////////////////////////////////////////////////");
		Append("// Component Preset Script");
		Append("///////////////////////////////////////////////////////////\n");
		Append("ComponentType " + savePreset.ComponentType.Name);
		foreach (Field field in savePreset.m_Fields)
		{
			string text = "";
			string text2 = "";
			FieldInfo fieldFromHandle = FieldInfo.GetFieldFromHandle(field.FieldHandle);
			if (fieldFromHandle.FieldType == typeof(float))
			{
				text2 = ((float)field.Args).ToCultureInvariantString("0.#######");
			}
			else if (fieldFromHandle.FieldType == typeof(Vector4))
			{
				Vector4 vector = (Vector4)field.Args;
				text2 = vector.x.ToCultureInvariantString("0.#######") + " " + vector.y.ToCultureInvariantString("0.#######") + " " + vector.z.ToCultureInvariantString("0.#######") + " " + vector.w.ToCultureInvariantString("0.#######");
			}
			else if (fieldFromHandle.FieldType == typeof(Vector3))
			{
				Vector3 vector2 = (Vector3)field.Args;
				text2 = vector2.x.ToCultureInvariantString("0.#######") + " " + vector2.y.ToCultureInvariantString("0.#######") + " " + vector2.z.ToCultureInvariantString("0.#######");
			}
			else if (fieldFromHandle.FieldType == typeof(Vector2))
			{
				Vector2 vector3 = (Vector2)field.Args;
				text2 = vector3.x.ToCultureInvariantString("0.#######") + " " + vector3.y.ToCultureInvariantString("0.#######");
			}
			else if (fieldFromHandle.FieldType == typeof(int))
			{
				text2 = ((int)field.Args).ToString();
			}
			else if (fieldFromHandle.FieldType == typeof(bool))
			{
				text2 = ((bool)field.Args).ToString();
			}
			else if (fieldFromHandle.FieldType == typeof(string))
			{
				text2 = (string)field.Args;
			}
			else
			{
				text = "//";
				text2 = "<NOTE: Type '" + fieldFromHandle.FieldType.Name.ToString() + "' can't be saved to preset.>";
			}
			if (!string.IsNullOrEmpty(text2) && fieldFromHandle.Name != "Persist")
			{
				Append(text + fieldFromHandle.Name + " " + text2);
			}
		}
		return null;
	}

	public static string SaveDifference(vp_ComponentPreset initialStatePreset, Component modifiedComponent, string fullPath, vp_ComponentPreset diskPreset)
	{
		if (initialStatePreset.ComponentType != modifiedComponent.GetType())
		{
			Error("Tried to save difference between different type components in 'SaveDifference'");
			return null;
		}
		vp_ComponentPreset vp_ComponentPreset2 = new vp_ComponentPreset();
		vp_ComponentPreset2.InitFromComponent(modifiedComponent);
		vp_ComponentPreset vp_ComponentPreset3 = new vp_ComponentPreset();
		vp_ComponentPreset3.m_ComponentType = vp_ComponentPreset2.ComponentType;
		for (int i = 0; i < vp_ComponentPreset2.m_Fields.Count; i++)
		{
			if (!initialStatePreset.m_Fields[i].Args.Equals(vp_ComponentPreset2.m_Fields[i].Args))
			{
				vp_ComponentPreset3.m_Fields.Add(vp_ComponentPreset2.m_Fields[i]);
			}
		}
		foreach (Field field in diskPreset.m_Fields)
		{
			bool flag = true;
			foreach (Field field2 in vp_ComponentPreset3.m_Fields)
			{
				if (field.FieldHandle == field2.FieldHandle)
				{
					flag = false;
				}
			}
			bool flag2 = false;
			foreach (Field field3 in vp_ComponentPreset2.m_Fields)
			{
				if (field.FieldHandle == field3.FieldHandle)
				{
					flag2 = true;
				}
			}
			if (!flag2)
			{
				flag = false;
			}
			if (flag)
			{
				vp_ComponentPreset3.m_Fields.Add(field);
			}
		}
		return Save(vp_ComponentPreset3, fullPath, isDifference: true);
	}

	public void InitFromComponent(Component component)
	{
		m_ComponentType = component.GetType();
		m_Fields.Clear();
		FieldInfo[] fields = m_ComponentType.GetFields();
		foreach (FieldInfo fieldInfo in fields)
		{
			if (fieldInfo.IsPublic && (fieldInfo.FieldType == typeof(float) || fieldInfo.FieldType == typeof(Vector4) || fieldInfo.FieldType == typeof(Vector3) || fieldInfo.FieldType == typeof(Vector2) || fieldInfo.FieldType == typeof(int) || fieldInfo.FieldType == typeof(bool) || fieldInfo.FieldType == typeof(string)))
			{
				m_Fields.Add(new Field(fieldInfo.FieldHandle, fieldInfo.GetValue(component)));
			}
		}
	}

	public static vp_ComponentPreset CreateFromComponent(Component component)
	{
		vp_ComponentPreset vp_ComponentPreset2 = new vp_ComponentPreset();
		vp_ComponentPreset2.m_ComponentType = component.GetType();
		FieldInfo[] fields = vp_ComponentPreset2.m_ComponentType.GetFields();
		foreach (FieldInfo fieldInfo in fields)
		{
			if (fieldInfo.IsPublic && (fieldInfo.FieldType == typeof(float) || fieldInfo.FieldType == typeof(Vector4) || fieldInfo.FieldType == typeof(Vector3) || fieldInfo.FieldType == typeof(Vector2) || fieldInfo.FieldType == typeof(int) || fieldInfo.FieldType == typeof(bool) || fieldInfo.FieldType == typeof(string)))
			{
				vp_ComponentPreset2.m_Fields.Add(new Field(fieldInfo.FieldHandle, fieldInfo.GetValue(component)));
			}
		}
		return vp_ComponentPreset2;
	}

	public int TryMakeCompatibleWithComponent(vp_Component component)
	{
		m_ComponentType = component.GetType();
		List<FieldInfo> list = new List<FieldInfo>(m_ComponentType.GetFields());
		for (int num = m_Fields.Count - 1; num > -1; num--)
		{
			foreach (FieldInfo item in list)
			{
				if (item.Name.Contains("PositionOffset") || item.Name.Contains("RotationOffset"))
				{
					break;
				}
				if (!(m_Fields[num].FieldHandle == item.FieldHandle))
				{
					continue;
				}
				goto IL_00b8;
			}
			m_Fields.Remove(m_Fields[num]);
			IL_00b8:;
		}
		return m_Fields.Count;
	}

	public bool LoadTextStream(string fullPath)
	{
		m_FullPath = fullPath;
		FileInfo fileInfo = null;
		TextReader textReader = null;
		fileInfo = new FileInfo(m_FullPath);
		if (fileInfo != null && fileInfo.Exists)
		{
			textReader = fileInfo.OpenText();
			List<string> list = new List<string>();
			string item;
			while ((item = textReader.ReadLine()) != null)
			{
				list.Add(item);
			}
			textReader.Close();
			if (list == null)
			{
				Error("Preset is empty. '" + m_FullPath + "'");
				return false;
			}
			ParseLines(list);
			return true;
		}
		Error("Failed to read file. '" + m_FullPath + "'");
		return false;
	}

	public static bool Load(vp_Component component, string fullPath)
	{
		vp_ComponentPreset vp_ComponentPreset2 = new vp_ComponentPreset();
		vp_ComponentPreset2.LoadTextStream(fullPath);
		return Apply(component, vp_ComponentPreset2);
	}

	public bool LoadFromResources(string resourcePath)
	{
		m_FullPath = resourcePath;
		TextAsset textAsset = Resources.Load(m_FullPath) as TextAsset;
		if (textAsset == null)
		{
			Error("Failed to read file. '" + m_FullPath + "'");
			return false;
		}
		return LoadFromTextAsset(textAsset);
	}

	public static vp_ComponentPreset LoadFromResources(vp_Component component, string resourcePath)
	{
		vp_ComponentPreset vp_ComponentPreset2 = new vp_ComponentPreset();
		vp_ComponentPreset2.LoadFromResources(resourcePath);
		Apply(component, vp_ComponentPreset2);
		return vp_ComponentPreset2;
	}

	public bool LoadFromTextAsset(TextAsset file)
	{
		m_FullPath = file.name;
		List<string> list = new List<string>();
		string[] array = file.text.Split('\n');
		foreach (string item in array)
		{
			list.Add(item);
		}
		if (list == null)
		{
			Error("Preset is empty. '" + m_FullPath + "'");
			return false;
		}
		ParseLines(list);
		return true;
	}

	public static vp_ComponentPreset LoadFromTextAsset(vp_Component component, TextAsset file)
	{
		vp_ComponentPreset vp_ComponentPreset2 = new vp_ComponentPreset();
		vp_ComponentPreset2.LoadFromTextAsset(file);
		Apply(component, vp_ComponentPreset2);
		return vp_ComponentPreset2;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void Append(string str)
	{
		str = str.Replace("\n", Environment.NewLine);
		StreamWriter streamWriter = null;
		try
		{
			streamWriter = new StreamWriter(m_FullPath, append: true);
			streamWriter.WriteLine(str);
			streamWriter?.Close();
		}
		catch
		{
			Error("Failed to write to file: '" + m_FullPath + "'");
		}
		streamWriter?.Close();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void ClearTextFile()
	{
		StreamWriter streamWriter = null;
		try
		{
			streamWriter = new StreamWriter(m_FullPath, append: false);
			streamWriter?.Close();
		}
		catch
		{
			Error("Failed to clear file: '" + m_FullPath + "'");
		}
		streamWriter?.Close();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ParseLines(List<string> lines)
	{
		m_LineNumber = 0;
		foreach (string line in lines)
		{
			m_LineNumber++;
			string text = RemoveComments(line);
			if (!string.IsNullOrEmpty(text) && !Parse(text))
			{
				return;
			}
		}
		m_LineNumber = 0;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool Parse(string line)
	{
		line = line.Trim();
		if (string.IsNullOrEmpty(line))
		{
			return true;
		}
		string[] array = line.Split((string[])null, StringSplitOptions.RemoveEmptyEntries);
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = array[i].Trim();
		}
		if (m_ComponentType == null)
		{
			if (array[0] == "ComponentType" && array.Length == 2)
			{
				m_Type = Type.GetType(array[1]);
				if (m_Type == null)
				{
					PresetError("No such ComponentType: '" + array[1] + "'");
					return false;
				}
				m_ComponentType = m_Type;
				return true;
			}
			PresetError("Unknown ComponentType.");
			return false;
		}
		FieldInfo fieldInfo = null;
		FieldInfo[] fields = m_Type.GetFields();
		foreach (FieldInfo fieldInfo2 in fields)
		{
			if (fieldInfo2.Name == array[0])
			{
				fieldInfo = fieldInfo2;
			}
		}
		if (fieldInfo == null)
		{
			if (array[0] != "ComponentType")
			{
				string[] array2 = FindMovedParameter(m_Type.Name, array[0]);
				if (array2 != null && array2.Length == 2)
				{
					if ((array2[0] == null || (!string.IsNullOrEmpty(array2[0]) && array2[0] == m_Type.Name)) && !string.IsNullOrEmpty(array2[1]) && array2[1] != array[0])
					{
						PresetWarning("The parameter '" + array[0] + "' has been renamed to '" + array2[1] + "'. Please update your presets.");
					}
					else if (array2[0] != null && array2[0] != m_Type.Name && (string.IsNullOrEmpty(array2[1]) || array2[1] == array[0]))
					{
						PresetWarning("The parameter '" + array[0] + "' has been moved to the '" + array2[0] + "' component. Please update your presets.");
					}
					else if (array2[0] != null && array2[0] != m_Type.Name && !string.IsNullOrEmpty(array2[1]) && array2[1] != array[0])
					{
						PresetWarning("The parameter '" + array[0] + "' has been moved to the '" + array2[0] + "' component and renamed to '" + array2[1] + "'. Please update your presets.");
					}
					else
					{
						PresetWarning("'" + m_Type.Name + "' no longer supports the parameter: '" + array[0] + "'. Please update your presets.");
					}
				}
				else
				{
					PresetError("'" + m_Type.Name + "' has no such field: '" + array[0] + "'");
				}
			}
			return true;
		}
		Field item = new Field(fieldInfo.FieldHandle, TokensToObject(fieldInfo, array));
		m_Fields.Add(item);
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public string[] FindMovedParameter(string type, string field)
	{
		if (!MovedParameters.TryGetValue(type + "." + field, out var value))
		{
			return null;
		}
		return value;
	}

	public static bool Apply(vp_Component component, vp_ComponentPreset preset)
	{
		if (preset == null)
		{
			Error("Tried to apply a preset that was null in '" + vp_Utility.GetErrorLocation() + "'");
			return false;
		}
		if (preset.m_ComponentType == null)
		{
			Error("Preset ComponentType was null in '" + vp_Utility.GetErrorLocation() + "'");
			return false;
		}
		if (component == null)
		{
			Error("Component was null when attempting to apply preset in '" + vp_Utility.GetErrorLocation() + "'");
			return false;
		}
		if (component.Type != preset.m_ComponentType)
		{
			string text = "a '" + preset.m_ComponentType?.ToString() + "' preset";
			if (preset.m_ComponentType == null)
			{
				text = "an unknown preset type";
			}
			Error("Applied " + text + " to a '" + component.Type.ToString() + "' component in '" + vp_Utility.GetErrorLocation() + "'");
			return false;
		}
		foreach (Field field in preset.m_Fields)
		{
			FieldInfo[] fields = component.Fields;
			foreach (FieldInfo fieldInfo in fields)
			{
				if (fieldInfo.FieldHandle == field.FieldHandle)
				{
					fieldInfo.SetValue(component, field.Args);
				}
			}
		}
		return true;
	}

	public static Type GetFileType(string fullPath)
	{
		bool logErrors = LogErrors;
		LogErrors = false;
		vp_ComponentPreset vp_ComponentPreset2 = new vp_ComponentPreset();
		vp_ComponentPreset2.LoadTextStream(fullPath);
		LogErrors = logErrors;
		if (vp_ComponentPreset2 != null && vp_ComponentPreset2.m_ComponentType != null)
		{
			return vp_ComponentPreset2.m_ComponentType;
		}
		return null;
	}

	public static Type GetFileTypeFromAsset(TextAsset asset)
	{
		bool logErrors = LogErrors;
		LogErrors = false;
		vp_ComponentPreset vp_ComponentPreset2 = new vp_ComponentPreset();
		vp_ComponentPreset2.LoadFromTextAsset(asset);
		LogErrors = logErrors;
		if (vp_ComponentPreset2 != null && vp_ComponentPreset2.m_ComponentType != null)
		{
			return vp_ComponentPreset2.m_ComponentType;
		}
		return null;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static object TokensToObject(FieldInfo field, string[] tokens)
	{
		if (field.FieldType == typeof(float))
		{
			return ArgsToFloat(tokens);
		}
		if (field.FieldType == typeof(Vector4))
		{
			return ArgsToVector4(tokens);
		}
		if (field.FieldType == typeof(Vector3))
		{
			return ArgsToVector3(tokens);
		}
		if (field.FieldType == typeof(Vector2))
		{
			return ArgsToVector2(tokens);
		}
		if (field.FieldType == typeof(int))
		{
			return ArgsToInt(tokens);
		}
		if (field.FieldType == typeof(bool))
		{
			return ArgsToBool(tokens);
		}
		if (field.FieldType == typeof(string))
		{
			return ArgsToString(tokens);
		}
		return null;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static string RemoveComments(string str)
	{
		string text = "";
		for (int i = 0; i < str.Length; i++)
		{
			switch (m_ReadMode)
			{
			case ReadMode.Normal:
				if (str[i] == '/' && str[i + 1] == '*')
				{
					m_ReadMode = ReadMode.BlockComment;
					i++;
				}
				else if (str[i] == '/' && str[i + 1] == '/')
				{
					m_ReadMode = ReadMode.LineComment;
					i++;
				}
				else
				{
					text += str[i];
				}
				break;
			case ReadMode.LineComment:
				if (i == str.Length - 1)
				{
					m_ReadMode = ReadMode.Normal;
				}
				break;
			case ReadMode.BlockComment:
				if (str[i] == '*' && str[i + 1] == '/')
				{
					m_ReadMode = ReadMode.Normal;
					i++;
				}
				break;
			}
		}
		return text;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static Vector4 ArgsToVector4(string[] args)
	{
		if (args.Length - 1 != 4)
		{
			PresetError("Wrong number of fields for '" + args[0] + "'");
			return Vector4.zero;
		}
		try
		{
			return new Vector4(Convert.ToSingle(args[1], CultureInfo.InvariantCulture), Convert.ToSingle(args[2], CultureInfo.InvariantCulture), Convert.ToSingle(args[3], CultureInfo.InvariantCulture), Convert.ToSingle(args[4], CultureInfo.InvariantCulture));
		}
		catch
		{
			PresetError("Illegal value: '" + args[1] + ", " + args[2] + ", " + args[3] + ", " + args[4] + "'");
			return Vector4.zero;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static Vector3 ArgsToVector3(string[] args)
	{
		if (args.Length - 1 != 3)
		{
			PresetError("Wrong number of fields for '" + args[0] + "'");
			return Vector3.zero;
		}
		try
		{
			return new Vector3(Convert.ToSingle(args[1], CultureInfo.InvariantCulture), Convert.ToSingle(args[2], CultureInfo.InvariantCulture), Convert.ToSingle(args[3], CultureInfo.InvariantCulture));
		}
		catch
		{
			PresetError("Illegal value: '" + args[1] + ", " + args[2] + ", " + args[3] + "'");
			return Vector3.zero;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static Vector2 ArgsToVector2(string[] args)
	{
		if (args.Length - 1 != 2)
		{
			PresetError("Wrong number of fields for '" + args[0] + "'");
			return Vector2.zero;
		}
		try
		{
			return new Vector2(Convert.ToSingle(args[1], CultureInfo.InvariantCulture), Convert.ToSingle(args[2], CultureInfo.InvariantCulture));
		}
		catch
		{
			PresetError("Illegal value: '" + args[1] + ", " + args[2] + "'");
			return Vector2.zero;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static float ArgsToFloat(string[] args)
	{
		if (args.Length - 1 != 1)
		{
			PresetError("Wrong number of fields for '" + args[0] + "'");
			return 0f;
		}
		try
		{
			return Convert.ToSingle(args[1], CultureInfo.InvariantCulture);
		}
		catch
		{
			PresetError("Illegal value: '" + args[1] + "'");
			return 0f;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static int ArgsToInt(string[] args)
	{
		if (args.Length - 1 != 1)
		{
			PresetError("Wrong number of fields for '" + args[0] + "'");
			return 0;
		}
		try
		{
			return Convert.ToInt32(args[1], CultureInfo.InvariantCulture);
		}
		catch
		{
			PresetError("Illegal value: '" + args[1] + "'");
			return 0;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool ArgsToBool(string[] args)
	{
		if (args.Length - 1 != 1)
		{
			PresetError("Wrong number of fields for '" + args[0] + "'");
			return false;
		}
		if (args[1].ToLower() == "true")
		{
			return true;
		}
		if (args[1].ToLower() == "false")
		{
			return false;
		}
		PresetError("Illegal value: '" + args[1] + "'");
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static string ArgsToString(string[] args)
	{
		string text = "";
		for (int i = 1; i < args.Length; i++)
		{
			text += args[i];
			if (i < args.Length - 1)
			{
				text += " ";
			}
		}
		return text;
	}

	public Type GetFieldType(string fieldName)
	{
		Type result = null;
		foreach (Field field in m_Fields)
		{
			FieldInfo fieldFromHandle = FieldInfo.GetFieldFromHandle(field.FieldHandle);
			if (fieldFromHandle.Name == fieldName)
			{
				result = fieldFromHandle.FieldType;
			}
		}
		return result;
	}

	public object GetFieldValue(string fieldName)
	{
		object result = null;
		foreach (Field field in m_Fields)
		{
			if (FieldInfo.GetFieldFromHandle(field.FieldHandle).Name == fieldName)
			{
				result = field.Args;
			}
		}
		return result;
	}

	public void SetFieldValue(string fieldName, object value)
	{
		foreach (Field field in m_Fields)
		{
			if (FieldInfo.GetFieldFromHandle(field.FieldHandle).Name == fieldName)
			{
				field.Args = value;
				break;
			}
		}
	}

	public static string ExtractFilenameFromPath(string path)
	{
		int num = Math.Max(path.LastIndexOf('/'), path.LastIndexOf('\\'));
		if (num == -1)
		{
			return path;
		}
		if (num == path.Length - 1)
		{
			return "";
		}
		return path.Substring(num + 1, path.Length - num - 1);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void PresetError(string message)
	{
		if (LogErrors)
		{
			Debug.LogError("Preset Error: " + m_FullPath + " (at " + m_LineNumber + ") " + message);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void PresetWarning(string message)
	{
		if (LogErrors)
		{
			Debug.LogWarning("Preset Warning: " + m_FullPath + " (at " + m_LineNumber + ") " + message);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void Error(string message)
	{
		if (LogErrors)
		{
			Debug.LogError("Error: " + message);
		}
	}
}
