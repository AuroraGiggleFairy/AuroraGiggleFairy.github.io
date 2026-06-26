using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

public sealed class SaveDataPrefsFile : ISaveDataPrefs
{
	[PublicizedFrom(EAccessModifier.Private)]
	public enum PrefType
	{
		Float,
		Int,
		String
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public class Pref
	{
		[StructLayout(LayoutKind.Explicit)]
		[PublicizedFrom(EAccessModifier.Private)]
		public struct PrefValues
		{
			[FieldOffset(0)]
			public float Float;

			[FieldOffset(0)]
			public int Int;
		}

		[StructLayout(LayoutKind.Explicit)]
		[PublicizedFrom(EAccessModifier.Private)]
		public struct PrefRefs
		{
			[FieldOffset(0)]
			public string String;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public PrefType m_type;

		[PublicizedFrom(EAccessModifier.Private)]
		public PrefValues m_values;

		[PublicizedFrom(EAccessModifier.Private)]
		public PrefRefs m_refs;

		public PrefType Type => m_type;

		public Pref(float value)
		{
			Set(value);
		}

		public Pref(int value)
		{
			Set(value);
		}

		public Pref(string value)
		{
			Set(value);
		}

		public bool TryGet(out float value)
		{
			if (m_type != PrefType.Float)
			{
				value = 0f;
				return false;
			}
			value = m_values.Float;
			return true;
		}

		public bool TryGet(out int value)
		{
			if (m_type != PrefType.Int)
			{
				value = 0;
				return false;
			}
			value = m_values.Int;
			return true;
		}

		public bool TryGet(out string value)
		{
			if (m_type != PrefType.String)
			{
				value = null;
				return false;
			}
			value = m_refs.String;
			return true;
		}

		public void Set(float value)
		{
			m_type = PrefType.Float;
			m_values.Float = value;
			m_refs.String = null;
		}

		public void Set(int value)
		{
			m_type = PrefType.Int;
			m_values.Int = value;
			m_refs.String = null;
		}

		public void Set(string value)
		{
			m_type = PrefType.String;
			m_refs.String = value;
			m_values.Int = 0;
		}

		public bool TryToString(out string stringRepresentation)
		{
			if (!PrefTypeMapping.TryGetValue(m_type, out var value))
			{
				Log.Warning(string.Format("[{0}] No char mapping for pref type '{1}'.", "SaveDataPrefsFile", m_type));
				stringRepresentation = null;
				return false;
			}
			switch (m_type)
			{
			case PrefType.Float:
				stringRepresentation = $"{value}{':'}{m_values.Float:R}";
				return true;
			case PrefType.Int:
				stringRepresentation = $"{value}{':'}{m_values.Int}";
				return true;
			case PrefType.String:
				stringRepresentation = $"{value}{':'}{m_refs.String}";
				return true;
			default:
				Log.Error(string.Format("[{0}] Missing to string implementation for '{1}'.", "SaveDataPrefsFile", m_type));
				stringRepresentation = null;
				return false;
			}
		}

		public static bool TryParse(ReadOnlySpan<char> stringRepresentation, out Pref pref)
		{
			if (stringRepresentation.Length < 2 || stringRepresentation[1] != ':')
			{
				pref = null;
				return false;
			}
			if (!PrefTypeUnmapping.TryGetValue(stringRepresentation[0], out var value))
			{
				pref = null;
				return false;
			}
			ReadOnlySpan<char> readOnlySpan = stringRepresentation;
			ReadOnlySpan<char> readOnlySpan2 = readOnlySpan.Slice(2, readOnlySpan.Length - 2);
			switch (value)
			{
			case PrefType.Float:
			{
				if (!float.TryParse(readOnlySpan2, out var result2))
				{
					pref = null;
					return false;
				}
				pref = new Pref(result2);
				return true;
			}
			case PrefType.Int:
			{
				if (!int.TryParse(readOnlySpan2, out var result))
				{
					pref = null;
					return false;
				}
				pref = new Pref(result);
				return true;
			}
			case PrefType.String:
				pref = new Pref(new string(readOnlySpan2));
				return true;
			default:
				Log.Error(string.Format("[{0}] Missing parse implementation for '{1}'.", "SaveDataPrefsFile", value));
				pref = null;
				return false;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static SaveDataPrefsFile s_instance;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly Dictionary<char, char> EscapeMapping = new Dictionary<char, char>
	{
		{ '\0', '0' },
		{ '\r', 'r' },
		{ '\n', 'n' },
		{ '=', '=' },
		{ '\\', '\\' }
	};

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly Dictionary<char, char> UnescapeMapping = EscapeMapping.ToDictionary([PublicizedFrom(EAccessModifier.Internal)] (KeyValuePair<char, char> pair) => pair.Value, [PublicizedFrom(EAccessModifier.Internal)] (KeyValuePair<char, char> pair) => pair.Key);

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly Dictionary<PrefType, char> PrefTypeMapping = new Dictionary<PrefType, char>
	{
		{
			PrefType.Float,
			'F'
		},
		{
			PrefType.Int,
			'I'
		},
		{
			PrefType.String,
			'S'
		}
	};

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly Dictionary<char, PrefType> PrefTypeUnmapping = PrefTypeMapping.ToDictionary([PublicizedFrom(EAccessModifier.Internal)] (KeyValuePair<PrefType, char> kv) => kv.Value, [PublicizedFrom(EAccessModifier.Internal)] (KeyValuePair<PrefType, char> kv) => kv.Key);

	[PublicizedFrom(EAccessModifier.Private)]
	public const string StorageFileName = "prefs.cfg";

	[PublicizedFrom(EAccessModifier.Private)]
	public const char KeyValueSeparator = '=';

	[PublicizedFrom(EAccessModifier.Private)]
	public const char EscapePrefix = '\\';

	[PublicizedFrom(EAccessModifier.Private)]
	public const char PrefTypeSeparator = ':';

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly string m_storageFilePath;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Dictionary<string, Pref> m_storage;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly object m_storageLock = new object();

	[PublicizedFrom(EAccessModifier.Private)]
	public bool m_dirty;

	public static SaveDataPrefsFile INSTANCE => s_instance ?? (s_instance = new SaveDataPrefsFile());

	public bool CanLoad => true;

	[PublicizedFrom(EAccessModifier.Private)]
	public SaveDataPrefsFile()
	{
		foreach (PrefType item in EnumUtils.Values<PrefType>())
		{
			if (!PrefTypeMapping.ContainsKey(item))
			{
				throw new KeyNotFoundException(string.Format("Expected {0} to have key '{1}'.", "PrefTypeMapping", item));
			}
		}
		m_storageFilePath = GameIO.GetNormalizedPath(Path.Join(GameIO.GetUserGameDataDir(), "prefs.cfg"));
		m_storage = new Dictionary<string, Pref>();
		Load();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void Escape(TextWriter writer, ReadOnlySpan<char> raw, bool ignoreSeparator)
	{
		bool flag = false;
		ReadOnlySpan<char> readOnlySpan = raw;
		for (int i = 0; i < readOnlySpan.Length; i++)
		{
			char c = readOnlySpan[i];
			if ((!ignoreSeparator || c != '=') && EscapeMapping.ContainsKey(c))
			{
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			writer.Write(raw);
			return;
		}
		readOnlySpan = raw;
		for (int i = 0; i < readOnlySpan.Length; i++)
		{
			char c2 = readOnlySpan[i];
			if ((ignoreSeparator && c2 == '=') || !EscapeMapping.TryGetValue(c2, out var value))
			{
				writer.Write(c2);
				continue;
			}
			writer.Write('\\');
			writer.Write(value);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void Unescape(StringBuilder builder, ReadOnlySpan<char> escaped)
	{
		if (escaped.IndexOf('\\') < 0)
		{
			builder.Append(escaped);
			return;
		}
		bool flag = false;
		int num = -1;
		ReadOnlySpan<char> readOnlySpan = escaped;
		for (int i = 0; i < readOnlySpan.Length; i++)
		{
			char c = readOnlySpan[i];
			num++;
			if (flag)
			{
				flag = false;
				if (!UnescapeMapping.TryGetValue(c, out var value))
				{
					Log.Warning($"Unexpected character after escape prefix at offset {num} (will be taken as-is): {c}");
					builder.Append(c);
				}
				builder.Append(value);
			}
			else if (c == '\\')
			{
				flag = true;
			}
			else
			{
				builder.Append(c);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static int IndexOfFirstUnescapedSeparator(ReadOnlySpan<char> search)
	{
		bool flag = false;
		int num = -1;
		ReadOnlySpan<char> readOnlySpan = search;
		for (int i = 0; i < readOnlySpan.Length; i++)
		{
			char c = readOnlySpan[i];
			num++;
			if (flag)
			{
				flag = false;
				continue;
			}
			switch (c)
			{
			case '\\':
				flag = true;
				break;
			case '=':
				return num;
			}
		}
		return -1;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SaveInternal()
	{
		lock (m_storageLock)
		{
			if (!m_dirty)
			{
				return;
			}
			try
			{
				using StreamWriter streamWriter = SdFile.CreateText(m_storageFilePath);
				int num = 0;
				foreach (var (text2, pref2) in m_storage)
				{
					if (!pref2.TryToString(out var stringRepresentation))
					{
						Log.Out(string.Format("[{0}] Failed to convert pref '{1}' of type {2} to a string representation.", "SaveDataPrefsFile", text2, pref2.Type));
						continue;
					}
					Escape(streamWriter, text2, ignoreSeparator: false);
					streamWriter.Write('=');
					Escape(streamWriter, stringRepresentation, ignoreSeparator: true);
					streamWriter.WriteLine();
					num++;
				}
				m_dirty = false;
				Log.Out(string.Format("[{0}] Saved {1} player pref(s) to: {2}", "SaveDataPrefsFile", num, m_storageFilePath));
			}
			catch (IOException ex)
			{
				Log.Error("[SaveDataPrefsFile] Failed to Save: " + ex.Message);
				Log.Exception(ex);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void LoadInternal()
	{
		lock (m_storageLock)
		{
			m_storage.Clear();
			if (SdFile.Exists(m_storageFilePath))
			{
				try
				{
					using StreamReader streamReader = SdFile.OpenText(m_storageFilePath);
					StringBuilder stringBuilder = new StringBuilder();
					int num = 0;
					int num2 = 0;
					while (true)
					{
						string text = streamReader.ReadLine();
						if (text == null)
						{
							break;
						}
						num2++;
						ReadOnlySpan<char> readOnlySpan = text;
						int num3 = IndexOfFirstUnescapedSeparator(readOnlySpan);
						if (num3 < 0)
						{
							Log.Error(string.Format("[{0}] Skipping line {1} since is missing unescaped separator '{2}'. Contents: {3}", "SaveDataPrefsFile", num2, '=', text));
						}
						else
						{
							ReadOnlySpan<char> readOnlySpan2 = readOnlySpan;
							Unescape(stringBuilder, readOnlySpan2.Slice(0, num3));
							string text2 = stringBuilder.ToString();
							stringBuilder.Clear();
							readOnlySpan2 = readOnlySpan;
							int num4 = num3 + 1;
							Unescape(stringBuilder, readOnlySpan2.Slice(num4, readOnlySpan2.Length - num4));
							string text3 = stringBuilder.ToString();
							stringBuilder.Clear();
							if (!Pref.TryParse(text3, out var pref))
							{
								Log.Error("[SaveDataPrefsFile] Failed to parse pref '" + text2 + "' with string representation: " + text3);
							}
							else
							{
								m_storage[text2] = pref;
								num++;
							}
						}
					}
					Log.Out(string.Format("[{0}] Loaded {1} player pref(s) from: {2}", "SaveDataPrefsFile", num, m_storageFilePath));
					return;
				}
				catch (IOException ex)
				{
					Log.Error("[SaveDataPrefsFile] Failed to Load: " + ex.Message);
					Log.Exception(ex);
					return;
				}
			}
			Log.Out("[SaveDataPrefsFile] Using empty player prefs, as none exists at: " + m_storageFilePath);
		}
	}

	public float GetFloat(string key, float defaultValue)
	{
		lock (m_storageLock)
		{
			Pref value;
			float value2;
			return (m_storage.TryGetValue(key, out value) && value.TryGet(out value2)) ? value2 : defaultValue;
		}
	}

	public void SetFloat(string key, float value)
	{
		lock (m_storageLock)
		{
			if (m_storage.TryGetValue(key, out var value2))
			{
				if (!value2.TryGet(out float value3) || value3 != value)
				{
					value2.Set(value);
					m_dirty = true;
				}
			}
			else
			{
				value2 = new Pref(value);
				m_storage[key] = value2;
				m_dirty = true;
			}
		}
	}

	public int GetInt(string key, int defaultValue)
	{
		lock (m_storageLock)
		{
			Pref value;
			int value2;
			return (m_storage.TryGetValue(key, out value) && value.TryGet(out value2)) ? value2 : defaultValue;
		}
	}

	public void SetInt(string key, int value)
	{
		lock (m_storageLock)
		{
			if (m_storage.TryGetValue(key, out var value2))
			{
				if (!value2.TryGet(out int value3) || value3 != value)
				{
					value2.Set(value);
					m_dirty = true;
				}
			}
			else
			{
				value2 = new Pref(value);
				m_storage[key] = value2;
				m_dirty = true;
			}
		}
	}

	public string GetString(string key, string defaultValue)
	{
		lock (m_storageLock)
		{
			Pref value;
			string value2;
			return (m_storage.TryGetValue(key, out value) && value.TryGet(out value2)) ? value2 : defaultValue;
		}
	}

	public void SetString(string key, string value)
	{
		lock (m_storageLock)
		{
			if (m_storage.TryGetValue(key, out var value2))
			{
				if (!value2.TryGet(out string value3) || !(value3 == value))
				{
					value2.Set(value);
					m_dirty = true;
				}
			}
			else
			{
				value2 = new Pref(value);
				m_storage[key] = value2;
				m_dirty = true;
			}
		}
	}

	public bool HasKey(string key)
	{
		lock (m_storageLock)
		{
			return m_storage.ContainsKey(key);
		}
	}

	public void DeleteKey(string key)
	{
		lock (m_storageLock)
		{
			m_storage.Remove(key);
		}
	}

	public void DeleteAll()
	{
		lock (m_storageLock)
		{
			m_storage.Clear();
		}
	}

	public void Save()
	{
		SaveInternal();
	}

	public void Load()
	{
		LoadInternal();
	}
}
