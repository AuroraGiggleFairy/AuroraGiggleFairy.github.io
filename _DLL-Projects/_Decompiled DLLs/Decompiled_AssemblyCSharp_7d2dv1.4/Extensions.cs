using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

public static class Extensions
{
	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly Regex StringSeparationRegex = new Regex("((?<=\\p{Ll})\\p{Lu}|\\p{Lu}(?=\\p{Ll}))", RegexOptions.IgnorePatternWhitespace);

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly Regex unindentEmptyBeginning = new Regex("^\\s*\r?\n", RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.CultureInvariant);

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly Regex unindentEmptyEnd = new Regex("\r?\n\\s*$", RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.CultureInvariant);

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly Regex unindentIndentationNoLinebreak = new Regex("\\s*\r?\n\\s*([^\\s|])", RegexOptions.Multiline | RegexOptions.Compiled | RegexOptions.CultureInvariant);

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly Regex unindentIndentationRegularLinebreak = new Regex("^\\s*\\|", RegexOptions.Multiline | RegexOptions.Compiled | RegexOptions.CultureInvariant);

	public static Transform FindInChildren(this Transform _t, string _name)
	{
		int childCount = _t.childCount;
		if (childCount == 0)
		{
			return null;
		}
		Transform transform = _t.Find(_name);
		if (!transform)
		{
			for (int i = 0; i < childCount; i++)
			{
				transform = _t.GetChild(i).FindInChildren(_name);
				if ((bool)transform)
				{
					break;
				}
			}
		}
		return transform;
	}

	public static Transform FindInChilds(this Transform target, string name, bool onlyActive = false)
	{
		if (!target || name == null)
		{
			return null;
		}
		if (!onlyActive || ((bool)target.gameObject && target.gameObject.activeSelf))
		{
			if (target.name == name)
			{
				return target;
			}
			for (int i = 0; i < target.childCount; i++)
			{
				Transform transform = target.GetChild(i).FindInChilds(name, onlyActive);
				if (transform != null)
				{
					return transform;
				}
			}
			return null;
		}
		return null;
	}

	public static T GetComponentInChildren<T>(this GameObject o, bool searchInactive, bool avoidGC = false) where T : Component
	{
		return o.transform.GetComponentInChildren<T>(searchInactive, avoidGC);
	}

	public static T GetComponentInChildren<T>(this Component c, bool searchInactive, bool avoidGC = false) where T : Component
	{
		return c.transform.GetComponentInChildren<T>(searchInactive, avoidGC);
	}

	public static T GetComponentInChildren<T>(this Transform t, bool searchInactive, bool avoidGC = false) where T : Component
	{
		if (!searchInactive)
		{
			return t.GetComponentInChildren<T>();
		}
		if (!avoidGC)
		{
			T[] componentsInChildren = t.GetComponentsInChildren<T>(includeInactive: true);
			if (componentsInChildren.Length == 0)
			{
				return null;
			}
			return componentsInChildren[0];
		}
		T val = t.GetComponent<T>();
		if (val == null)
		{
			for (int i = 0; i < t.childCount; i++)
			{
				val = t.GetChild(i).GetComponentInChildren<T>(searchInactive, avoidGC);
				if (val != null)
				{
					break;
				}
			}
		}
		return val;
	}

	public static T GetOrAddComponent<T>(this GameObject go) where T : Component
	{
		T val = go.GetComponent<T>();
		if (val == null)
		{
			val = go.AddComponent<T>();
		}
		return val;
	}

	public static string GetGameObjectPath(this GameObject _obj)
	{
		string text = "/" + _obj.name;
		while (_obj.transform.parent != null)
		{
			_obj = _obj.transform.parent.gameObject;
			text = "/" + _obj.name + text;
		}
		return text;
	}

	public static bool ContainsWithComparer<T>(this List<T> _list, T _item, IEqualityComparer<T> _comparer)
	{
		if (_list == null)
		{
			throw new ArgumentNullException("_list");
		}
		if (_comparer == null)
		{
			_comparer = EqualityComparer<T>.Default;
		}
		for (int i = 0; i < _list.Count; i++)
		{
			if (_comparer.Equals(_list[i], _item))
			{
				return true;
			}
		}
		return false;
	}

	public static bool ContainsCaseInsensitive(this IList<string> _list, string _item)
	{
		if (_item == null)
		{
			for (int i = 0; i < _list.Count; i++)
			{
				if (_list[i] == null)
				{
					return true;
				}
			}
			return false;
		}
		for (int j = 0; j < _list.Count; j++)
		{
			if (StringComparer.OrdinalIgnoreCase.Equals(_list[j], _item))
			{
				return true;
			}
		}
		return false;
	}

	public static void CopyTo<T>(this IList<T> _srcList, IList<T> _dest)
	{
		foreach (T _src in _srcList)
		{
			_dest.Add(_src);
		}
	}

	public static bool ColorEquals(this Color32 _a, Color32 _b)
	{
		if (_a.r == _b.r && _a.g == _b.g && _a.b == _b.b)
		{
			return _a.a == _b.a;
		}
		return false;
	}

	public static string ToHexCode(this Color _color, bool _includeAlpha = false)
	{
		return ((Color32)_color).ToHexCode(_includeAlpha);
	}

	public static string ToHexCode(this Color32 _color, bool _includeAlpha = false)
	{
		if (!_includeAlpha)
		{
			return $"{_color.r:X02}{_color.g:X02}{_color.b:X02}";
		}
		return $"{_color.r:X02}{_color.g:X02}{_color.b:X02}{_color.a:X02}";
	}

	public static bool EqualsCaseInsensitive(this string _a, string _b)
	{
		return string.Equals(_a, _b, StringComparison.OrdinalIgnoreCase);
	}

	public static bool ContainsCaseInsensitive(this string _a, string _b)
	{
		return _a.IndexOf(_b, StringComparison.OrdinalIgnoreCase) >= 0;
	}

	public static string SeparateCamelCase(this string _value)
	{
		return StringSeparationRegex.Replace(_value, " $1").Trim();
	}

	public static string ToHexString(this byte[] _bytes, string _separator = "")
	{
		return BitConverter.ToString(_bytes).Replace("-", _separator).ToUpperInvariant();
	}

	public static string RemoveLineBreaks(this string _value)
	{
		return _value.Replace("\r\n", " ").Replace("\r", " ").Replace("\n", " ");
	}

	public static int GetStableHashCode(this string _str)
	{
		return _str.AsSpan().GetStableHashCode();
	}

	public static int GetStableHashCode(this ReadOnlySpan<char> _str)
	{
		int num = 5381;
		int num2 = num;
		for (int i = 0; i < _str.Length && _str[i] != 0; i += 2)
		{
			num = ((num << 5) + num) ^ _str[i];
			if (i == _str.Length - 1 || _str[i + 1] == '\0')
			{
				break;
			}
			num2 = ((num2 << 5) + num2) ^ _str[i + 1];
		}
		return num + num2 * 1566083941;
	}

	public static string Unindent(this string _indented, bool _trimEmptyLines = true)
	{
		if (_trimEmptyLines)
		{
			_indented = unindentEmptyBeginning.Replace(_indented, string.Empty);
			_indented = unindentEmptyEnd.Replace(_indented, string.Empty);
		}
		_indented = unindentIndentationNoLinebreak.Replace(_indented, " $1");
		_indented = unindentIndentationRegularLinebreak.Replace(_indented, string.Empty);
		return _indented;
	}

	public static StringBuilder TrimEnd(this StringBuilder _sb)
	{
		if (_sb == null || _sb.Length == 0)
		{
			return _sb;
		}
		int num = _sb.Length - 1;
		while (num >= 0 && char.IsWhiteSpace(_sb[num]))
		{
			num--;
		}
		if (num < _sb.Length - 1)
		{
			_sb.Length = num + 1;
		}
		return _sb;
	}

	public static StringBuilder TrimStart(this StringBuilder _sb)
	{
		if (_sb == null || _sb.Length == 0)
		{
			return _sb;
		}
		int i;
		for (i = 0; i < _sb.Length && char.IsWhiteSpace(_sb[i]); i++)
		{
		}
		if (i > 0)
		{
			_sb.Remove(0, i);
		}
		return _sb;
	}

	public static StringBuilder Trim(this StringBuilder _sb)
	{
		if (_sb == null || _sb.Length == 0)
		{
			return _sb;
		}
		return _sb.TrimEnd().TrimStart();
	}

	public static string ToCultureInvariantString(this float _value)
	{
		return _value.ToString(Utils.StandardCulture);
	}

	public static string ToCultureInvariantString(this double _value)
	{
		return _value.ToString(Utils.StandardCulture);
	}

	public static string ToCultureInvariantString(this float _value, string _format)
	{
		return _value.ToString(_format, Utils.StandardCulture);
	}

	public static string ToCultureInvariantString(this double _value, string _format)
	{
		return _value.ToString(_format, Utils.StandardCulture);
	}

	public static string ToCultureInvariantString(this decimal _value)
	{
		return _value.ToString(Utils.StandardCulture);
	}

	public static string ToCultureInvariantString(this decimal _value, string _format)
	{
		return _value.ToString(_format, Utils.StandardCulture);
	}

	public static string ToCultureInvariantString(this DateTime _value)
	{
		return _value.ToString(Utils.StandardCulture);
	}

	public static string ToCultureInvariantString(this Vector2 _value)
	{
		return "(" + _value.x.ToCultureInvariantString("F1") + ", " + _value.y.ToCultureInvariantString("F1") + ")";
	}

	public static string ToCultureInvariantString(this Vector2 _value, string _format)
	{
		return "(" + _value.x.ToCultureInvariantString(_format) + ", " + _value.y.ToCultureInvariantString(_format) + ")";
	}

	public static string ToCultureInvariantString(this Vector3 _value)
	{
		return "(" + _value.x.ToCultureInvariantString("F1") + ", " + _value.y.ToCultureInvariantString("F1") + ", " + _value.z.ToCultureInvariantString("F1") + ")";
	}

	public static string ToCultureInvariantString(this Vector3 _value, string _format)
	{
		return "(" + _value.x.ToCultureInvariantString(_format) + ", " + _value.y.ToCultureInvariantString(_format) + ", " + _value.z.ToCultureInvariantString(_format) + ")";
	}

	public static string ToCultureInvariantString(this Vector4 _value)
	{
		return "(" + _value.x.ToCultureInvariantString("F1") + ", " + _value.y.ToCultureInvariantString("F1") + ", " + _value.z.ToCultureInvariantString("F1") + ", " + _value.w.ToCultureInvariantString("F1") + ")";
	}

	public static string ToCultureInvariantString(this Vector4 _value, string _format)
	{
		return "(" + _value.x.ToCultureInvariantString(_format) + ", " + _value.y.ToCultureInvariantString(_format) + ", " + _value.z.ToCultureInvariantString(_format) + ", " + _value.w.ToCultureInvariantString(_format) + ")";
	}

	public static string ToCultureInvariantString(this Bounds _value)
	{
		return "Center: " + _value.center.ToCultureInvariantString() + ", Extents: " + _value.extents.ToCultureInvariantString();
	}

	public static string ToCultureInvariantString(this Rect _value)
	{
		return "(x:" + _value.x.ToCultureInvariantString("F2") + ", y:" + _value.y.ToCultureInvariantString("F2") + ", width:" + _value.width.ToCultureInvariantString("F2") + ", height:" + _value.height.ToCultureInvariantString("F2") + ")";
	}

	public static string ToCultureInvariantString(this Quaternion _value)
	{
		return "(" + _value.x.ToCultureInvariantString("F1") + ", " + _value.y.ToCultureInvariantString("F1") + ", " + _value.z.ToCultureInvariantString("F1") + ", " + _value.w.ToCultureInvariantString("F1") + ")";
	}

	public static string ToCultureInvariantString(this Matrix4x4 _value)
	{
		return _value.m00.ToCultureInvariantString("F5") + "\t" + _value.m01.ToCultureInvariantString("F5") + "\t" + _value.m02.ToCultureInvariantString("F5") + "\t" + _value.m03.ToCultureInvariantString("F5") + "\n" + _value.m10.ToCultureInvariantString("F5") + "\t" + _value.m11.ToCultureInvariantString("F5") + "\t" + _value.m12.ToCultureInvariantString("F5") + "\t" + _value.m13.ToCultureInvariantString("F5") + "\n" + _value.m20.ToCultureInvariantString("F5") + "\t" + _value.m21.ToCultureInvariantString("F5") + "\t" + _value.m22.ToCultureInvariantString("F5") + "\t" + _value.m23.ToCultureInvariantString("F5") + "\n" + _value.m30.ToCultureInvariantString("F5") + "\t" + _value.m31.ToCultureInvariantString("F5") + "\t" + _value.m32.ToCultureInvariantString("F5") + "\t" + _value.m33.ToCultureInvariantString("F5") + "\n";
	}

	public static string ToCultureInvariantString(this Color _value)
	{
		return "RGBA(" + _value.r.ToCultureInvariantString("F3") + ", " + _value.g.ToCultureInvariantString("F3") + ", " + _value.b.ToCultureInvariantString("F3") + ", " + _value.a.ToCultureInvariantString("F3") + ")";
	}

	public static string ToCultureInvariantString(this Plane _value)
	{
		return "(normal:(" + _value.normal.x.ToCultureInvariantString("F1") + ", " + _value.normal.y.ToCultureInvariantString("F1") + ", " + _value.normal.z.ToCultureInvariantString("F1") + "), distance:" + _value.distance.ToCultureInvariantString("F1") + ")";
	}

	public static string ToCultureInvariantString(this Ray _value)
	{
		return "Origin: " + _value.origin.ToCultureInvariantString() + ", Dir: " + _value.direction.ToCultureInvariantString();
	}

	public static string ToCultureInvariantString(this Ray2D _value)
	{
		return "Origin: " + _value.origin.ToCultureInvariantString() + ", Dir: " + _value.direction.ToCultureInvariantString();
	}
}
