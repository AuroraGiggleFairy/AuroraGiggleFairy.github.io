using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using Platform;
using Unity.Collections;
using UnityEngine;

public static class Utils
{
	public enum EnumHitDirection : byte
	{
		Front,
		Back,
		Left,
		Right,
		Explosion,
		None
	}

	public enum ERestartAntiCheatMode
	{
		KeepAntiCheatMode,
		ForceOff,
		ForceOn
	}

	public static string[] ExcludeLayerZoom = new string[1] { "VisibleOnZoom" };

	[PublicizedFrom(EAccessModifier.Private)]
	public static CultureInfo ci;

	public static readonly Regex hexAlphaMatcher = new Regex("[0-9a-fA-F]{2}");

	public static readonly Regex hexRgbMatcher = new Regex("[0-9a-fA-F]{6}");

	public static readonly Regex hexRgbaMatcher = new Regex("[0-9a-fA-F]{8}");

	public static readonly Regex nestedEscapePattern = new Regex("(\\[+\\/c\\](?:\\/c\\])*)");

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly List<Transform> setLayerRecursivelyList = new List<Transform>();

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly List<Collider> setColliderLayerRecursivelyList = new List<Collider>();

	[PublicizedFrom(EAccessModifier.Private)]
	public static Action<IntPtr, byte, int> MemsetDelegate;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly Vector3[] tan1Temp = new Vector3[786432];

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly Vector3[] tan2Temp = new Vector3[786432];

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly List<Vector3> tempNormals = new List<Vector3>();

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly List<Material> tempMats = new List<Material>();

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly char[] ipSeparatorChars = new char[2] { '.', ':' };

	public static CultureInfo StandardCulture => ci;

	public static uint CurrentUnixTime => (uint)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;

	public static void InitStatic()
	{
		ci = (CultureInfo)CultureInfo.CurrentCulture.Clone();
		ci = (CultureInfo)CultureInfo.InvariantCulture.Clone();
		ci.NumberFormat.CurrencyDecimalSeparator = ".";
		ci.NumberFormat.NumberDecimalSeparator = ".";
		ci.NumberFormat.NumberGroupSeparator = ",";
		ci.DateTimeFormat.ShortDatePattern = "yyyy-MM-dd";
		ci.DateTimeFormat.LongDatePattern = "yyyy-MM-dd";
		ci.DateTimeFormat.ShortTimePattern = "HH:mm:ss";
		ci.DateTimeFormat.LongTimePattern = "HH:mm:ss";
		if (!GlobalCultureInfo.SetDefaultCulture(ci))
		{
			Log.Warning("Setting global culture failed!");
		}
		DynamicMethod dynamicMethod = new DynamicMethod("Memset", MethodAttributes.Public | MethodAttributes.Static, CallingConventions.Standard, null, new Type[3]
		{
			typeof(IntPtr),
			typeof(byte),
			typeof(int)
		}, typeof(Utils), skipVisibility: true);
		ILGenerator iLGenerator = dynamicMethod.GetILGenerator();
		iLGenerator.Emit(OpCodes.Ldarg_0);
		iLGenerator.Emit(OpCodes.Ldarg_1);
		iLGenerator.Emit(OpCodes.Ldarg_2);
		iLGenerator.Emit(OpCodes.Initblk);
		iLGenerator.Emit(OpCodes.Ret);
		MemsetDelegate = (Action<IntPtr, byte, int>)dynamicMethod.CreateDelegate(typeof(Action<IntPtr, byte, int>));
	}

	public static string GetChildPath(this Transform t, Transform child)
	{
		Transform transform = child;
		string text = transform.name;
		transform = transform.parent;
		while (transform != null && transform != t)
		{
			text = transform.name + "/" + text;
			transform = transform.parent;
		}
		if (transform == null)
		{
			throw new Exception(t.name + " does not have a child " + child.name);
		}
		return text;
	}

	public static void SetTagsRecursively(Transform t, string tag)
	{
		t.gameObject.tag = tag;
		for (int num = t.childCount - 1; num >= 0; num--)
		{
			SetTagsRecursively(t.GetChild(num), tag);
		}
	}

	public static void SetTagsIfNoneRecursively(Transform t, string tag)
	{
		if (t.gameObject.CompareTag("Untagged"))
		{
			t.gameObject.tag = tag;
		}
		for (int num = t.childCount - 1; num >= 0; num--)
		{
			SetTagsIfNoneRecursively(t.GetChild(num), tag);
		}
	}

	public static void SetTagsIfMatchRecursively(Transform t, string matchTag, string tag)
	{
		if (t.gameObject.CompareTag(matchTag))
		{
			t.gameObject.tag = tag;
		}
		for (int num = t.childCount - 1; num >= 0; num--)
		{
			SetTagsIfMatchRecursively(t.GetChild(num), matchTag, tag);
		}
	}

	public static void DestroyWithTagRecursively(Transform t, string tag)
	{
		for (int num = t.childCount - 1; num >= 0; num--)
		{
			DestroyWithTagRecursively(t.GetChild(num), tag);
		}
		if (t.gameObject.CompareTag(tag))
		{
			UnityEngine.Object.Destroy(t.gameObject);
		}
	}

	public static void SetLayerRecursively(GameObject go, int newLayer)
	{
		go.GetComponentsInChildren(includeInactive: true, setLayerRecursivelyList);
		List<Transform> list = setLayerRecursivelyList;
		if (newLayer == 2)
		{
			for (int num = list.Count - 1; num >= 0; num--)
			{
				list[num].gameObject.layer = newLayer;
			}
		}
		else
		{
			for (int num2 = list.Count - 1; num2 >= 0; num2--)
			{
				Transform transform = list[num2];
				if (transform.CompareTag("LargeEntityBlocker"))
				{
					transform.gameObject.layer = 19;
				}
				else
				{
					transform.gameObject.layer = newLayer;
				}
			}
		}
		setLayerRecursivelyList.Clear();
	}

	public static void SetLayerRecursively(GameObject go, int newLayer, string[] excludeTags)
	{
		go.GetComponentsInChildren(includeInactive: true, setLayerRecursivelyList);
		List<Transform> list = setLayerRecursivelyList;
		if (newLayer == 2)
		{
			for (int num = list.Count - 1; num >= 0; num--)
			{
				SetLayerWithExclusionList(list[num].gameObject, newLayer, excludeTags);
			}
		}
		else
		{
			for (int num2 = list.Count - 1; num2 >= 0; num2--)
			{
				Transform transform = list[num2];
				if (transform.CompareTag("LargeEntityBlocker"))
				{
					SetLayerWithExclusionList(transform.gameObject, 19, excludeTags);
				}
				else
				{
					SetLayerWithExclusionList(transform.gameObject, newLayer, excludeTags);
				}
			}
		}
		setLayerRecursivelyList.Clear();
	}

	public static void SetLayerWithExclusionList(GameObject go, int layer, string[] excludeTags)
	{
		if (excludeTags != null)
		{
			foreach (string tag in excludeTags)
			{
				if (go.CompareTag(tag))
				{
					return;
				}
			}
		}
		go.layer = layer;
	}

	public static void SetColliderLayerRecursively(GameObject go, int newLayer)
	{
		go.GetComponentsInChildren(includeInactive: true, setColliderLayerRecursivelyList);
		for (int num = setColliderLayerRecursivelyList.Count - 1; num >= 0; num--)
		{
			setColliderLayerRecursivelyList[num].gameObject.layer = newLayer;
		}
	}

	public static void MoveTaggedToLayer(GameObject go, string tag, int newLayer)
	{
		go.GetComponentsInChildren(includeInactive: true, setLayerRecursivelyList);
		List<Transform> list = setLayerRecursivelyList;
		for (int num = list.Count - 1; num >= 0; num--)
		{
			Transform transform = list[num];
			if (transform.CompareTag(tag))
			{
				transform.gameObject.layer = newLayer;
			}
		}
		setLayerRecursivelyList.Clear();
	}

	public static void DrawBounds(Bounds _bounds, Color _color, float _duration)
	{
		Vector3 center = _bounds.center;
		Vector3 extents = _bounds.extents;
		Vector3 vector = new Vector3(center.x - extents.x, center.y + extents.y, center.z - extents.z);
		Vector3 vector2 = new Vector3(center.x + extents.x, center.y + extents.y, center.z - extents.z);
		Vector3 vector3 = new Vector3(center.x - extents.x, center.y - extents.y, center.z - extents.z);
		Vector3 vector4 = new Vector3(center.x + extents.x, center.y - extents.y, center.z - extents.z);
		Vector3 vector5 = new Vector3(center.x - extents.x, center.y + extents.y, center.z + extents.z);
		Vector3 vector6 = new Vector3(center.x + extents.x, center.y + extents.y, center.z + extents.z);
		Vector3 vector7 = new Vector3(center.x - extents.x, center.y - extents.y, center.z + extents.z);
		Vector3 vector8 = new Vector3(center.x + extents.x, center.y - extents.y, center.z + extents.z);
		UnityEngine.Debug.DrawLine(vector, vector2, _color, _duration);
		UnityEngine.Debug.DrawLine(vector2, vector4, _color, _duration);
		UnityEngine.Debug.DrawLine(vector4, vector3, _color, _duration);
		UnityEngine.Debug.DrawLine(vector3, vector, _color, _duration);
		UnityEngine.Debug.DrawLine(vector5, vector6, _color, _duration);
		UnityEngine.Debug.DrawLine(vector6, vector8, _color, _duration);
		UnityEngine.Debug.DrawLine(vector8, vector7, _color, _duration);
		UnityEngine.Debug.DrawLine(vector7, vector5, _color, _duration);
		UnityEngine.Debug.DrawLine(vector, vector5, _color, _duration);
		UnityEngine.Debug.DrawLine(vector2, vector6, _color, _duration);
		UnityEngine.Debug.DrawLine(vector4, vector8, _color, _duration);
		UnityEngine.Debug.DrawLine(vector3, vector7, _color, _duration);
	}

	public static void DrawAxisLines(Vector3 centerPos, float radius, Color color, float duration)
	{
		Vector3 start = centerPos;
		Vector3 end = centerPos;
		start.x -= radius;
		end.x += radius;
		UnityEngine.Debug.DrawLine(start, end, color, duration);
		start.x = centerPos.x;
		end.x = centerPos.x;
		start.z -= radius;
		end.z += radius;
		UnityEngine.Debug.DrawLine(start, end, color, duration);
		start.z = centerPos.z;
		end.z = centerPos.z;
		start.y -= radius;
		end.y += radius;
		UnityEngine.Debug.DrawLine(start, end, color, duration);
	}

	public static void DrawBoxLines(Vector3 minPos, Vector3 maxPos, Color color, float duration)
	{
		DrawBoxLinesHorizontal(minPos, maxPos, color, duration);
		DrawBoxLinesVerticle(minPos, maxPos, color, duration);
	}

	public static void DrawBoxLinesHorizontal(Vector3 minPos, Vector3 maxPos, Color color, float duration)
	{
		Vector3 maxPos2 = new Vector3(maxPos.x, minPos.y, maxPos.z);
		Vector3 minPos2 = new Vector3(minPos.x, maxPos.y, minPos.z);
		DrawRectLinesHorzontal(minPos, maxPos2, color, duration);
		DrawRectLinesHorzontal(minPos2, maxPos, color, duration);
	}

	public static void DrawBoxLinesVerticle(Vector3 minPos, Vector3 maxPos, Color color, float duration)
	{
		Vector3 vector = new Vector3(0f, maxPos.y - minPos.y);
		UnityEngine.Debug.DrawRay(minPos, vector, color, duration);
		UnityEngine.Debug.DrawRay(new Vector3(minPos.x, minPos.y, maxPos.z), vector, color, duration);
		UnityEngine.Debug.DrawRay(maxPos, -vector, color, duration);
		UnityEngine.Debug.DrawRay(new Vector3(maxPos.x, minPos.y, minPos.z), vector, color, duration);
	}

	public static void DrawCubeLines(Vector3 minPos, float size, Color color, float duration)
	{
		Vector3 maxPos = minPos;
		maxPos.x += size;
		maxPos.y += size;
		maxPos.z += size;
		DrawBoxLines(minPos, maxPos, color, duration);
	}

	public static void DrawCubeLinesCenter(Vector3 centerPos, float radius, Color color, float duration)
	{
		Vector3 minPos = centerPos;
		minPos.x -= radius;
		minPos.y -= radius;
		minPos.z -= radius;
		Vector3 maxPos = centerPos;
		maxPos.x += radius;
		maxPos.y += radius;
		maxPos.z += radius;
		DrawBoxLines(minPos, maxPos, color, duration);
	}

	public static void DrawRectLinesHorzontal(Vector3 minPos, Vector3 maxPos, Color color, float duration)
	{
		Vector3 vector = new Vector3(maxPos.x, minPos.y, minPos.z);
		Vector3 vector2 = new Vector3(minPos.x, minPos.y, maxPos.z);
		UnityEngine.Debug.DrawLine(minPos, vector2, color, duration);
		UnityEngine.Debug.DrawLine(vector2, maxPos, color, duration);
		UnityEngine.Debug.DrawLine(maxPos, vector, color, duration);
		UnityEngine.Debug.DrawLine(vector, minPos, color, duration);
	}

	public static void DrawCylinderLinesHorzontal(Vector3 centerPos, float radius, float height, Color startColor, Color endColor, int segments, float duration)
	{
		DrawCircleLinesHorzontal(centerPos, radius, startColor, endColor, segments, duration);
		centerPos.y += height;
		DrawCircleLinesHorzontal(centerPos, radius, startColor, endColor, segments, duration);
	}

	public static void DrawCircleLinesHorzontal(Vector3 centerPos, float radius, Color startColor, Color endColor, int segments, float duration)
	{
		if (segments < 3)
		{
			segments = 3;
		}
		float num = MathF.PI * -2f / (float)segments;
		float num2 = MathF.PI / 2f;
		float num3 = 0f;
		float num4 = 1f / (float)(segments - 1);
		Vector3 start = default(Vector3);
		start.y = centerPos.y;
		start.x = centerPos.x + Mathf.Cos(num2) * radius;
		start.z = centerPos.z + Mathf.Sin(num2) * radius;
		Vector3 end = default(Vector3);
		end.y = centerPos.y;
		for (int i = 0; i < segments; i++)
		{
			num2 += num;
			end.x = centerPos.x + Mathf.Cos(num2) * radius;
			end.z = centerPos.z + Mathf.Sin(num2) * radius;
			Color color = Color.Lerp(startColor, endColor, num3);
			UnityEngine.Debug.DrawLine(start, end, color, duration);
			start.x = end.x;
			start.z = end.z;
			num3 += num4;
		}
	}

	public static void DrawLine(Vector3 startPos, Vector3 endPos, Color startColor, Color endColor, int segments, float duration = 0f)
	{
		if (segments <= 1)
		{
			UnityEngine.Debug.DrawLine(startPos, endPos, startColor, duration);
			return;
		}
		Vector3 vector = (endPos - startPos) * (1f / (float)segments);
		float num = 0f;
		float num2 = 1f / (float)(segments - 1);
		for (int i = 0; i < segments; i++)
		{
			Color color = Color.Lerp(startColor, endColor, num);
			UnityEngine.Debug.DrawRay(startPos, vector, color, duration);
			startPos += vector;
			num += num2;
		}
	}

	public static void DrawRay(Vector3 startPos, Vector3 dir, Color startColor, Color endColor, int segments, float duration = 0f)
	{
		DrawLine(startPos, startPos + dir, startColor, endColor, segments, duration);
	}

	public static void DrawOutline(Rect position, string text, GUIStyle style, Color outColor, Color inColor)
	{
		style.normal.textColor = outColor;
		position.x--;
		GUI.Label(position, text, style);
		position.x += 2f;
		GUI.Label(position, text, style);
		position.x--;
		position.y--;
		GUI.Label(position, text, style);
		position.y += 2f;
		GUI.Label(position, text, style);
		position.y--;
		style.normal.textColor = inColor;
		GUI.Label(position, text, style);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int FastAbsInt(int _x)
	{
		if (_x < 0)
		{
			if (_x == int.MinValue)
			{
				return int.MaxValue;
			}
			return -_x;
		}
		return _x;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float FastAbs(float _x)
	{
		if (!(_x >= 0f))
		{
			return 0f - _x;
		}
		return _x;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int Fastfloor(float x)
	{
		if (!(x >= 0f))
		{
			return (int)(x - 0.9999999f);
		}
		return (int)x;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int Fastfloor(double x)
	{
		if (!(x >= 0.0))
		{
			return (int)(x - 0.9999998807907104);
		}
		return (int)x;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int FastMax(int v1, int v2)
	{
		if (v1 < v2)
		{
			return v2;
		}
		return v1;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float FastMax(float v1, float v2)
	{
		if (!(v1 >= v2))
		{
			return v2;
		}
		return v1;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static byte FastMax(byte v1, byte v2)
	{
		if (v1 < v2)
		{
			return v2;
		}
		return v1;
	}

	public static float FastMax(float v1, float v2, float v3, float v4)
	{
		if (v1 >= v2 && v1 >= v3 && v1 >= v4)
		{
			return v1;
		}
		if (v2 >= v1 && v2 >= v3 && v2 >= v4)
		{
			return v2;
		}
		if (v3 >= v1 && v3 >= v2 && v3 >= v4)
		{
			return v3;
		}
		return v4;
	}

	public static int FastMax(int v1, int v2, int v3, int v4)
	{
		if (v1 >= v2 && v1 >= v3 && v1 >= v4)
		{
			return v1;
		}
		if (v2 >= v1 && v2 >= v3 && v2 >= v4)
		{
			return v2;
		}
		if (v3 >= v1 && v3 >= v2 && v3 >= v4)
		{
			return v3;
		}
		return v4;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float FastMin(float v1, float v2)
	{
		if (!(v1 < v2))
		{
			return v2;
		}
		return v1;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int FastMin(int v1, int v2)
	{
		if (v1 >= v2)
		{
			return v2;
		}
		return v1;
	}

	public static int FastMin(int v1, int v2, int v3, int v4)
	{
		if (v1 <= v2 && v1 <= v3 && v1 <= v4)
		{
			return v1;
		}
		if (v2 <= v1 && v2 <= v3 && v2 <= v4)
		{
			return v2;
		}
		if (v3 <= v1 && v3 <= v2 && v3 <= v4)
		{
			return v3;
		}
		return v4;
	}

	public static float FastMin(float v1, float v2, float v3, float v4)
	{
		if (v1 <= v2 && v1 <= v3 && v1 <= v4)
		{
			return v1;
		}
		if (v2 <= v1 && v2 <= v3 && v2 <= v4)
		{
			return v2;
		}
		if (v3 <= v1 && v3 <= v2 && v3 <= v4)
		{
			return v3;
		}
		return v4;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float FastClamp(float _v, float _min, float _max)
	{
		if (_v <= _min)
		{
			return _min;
		}
		if (_v >= _max)
		{
			return _max;
		}
		return _v;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int FastClamp(int _v, int _min, int _max)
	{
		if (_v <= _min)
		{
			return _min;
		}
		if (_v >= _max)
		{
			return _max;
		}
		return _v;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float FastClamp01(float _v)
	{
		if (_v <= 0f)
		{
			return 0f;
		}
		if (_v >= 1f)
		{
			return 1f;
		}
		return _v;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static double FastClamp01(double _v)
	{
		if (_v <= 0.0)
		{
			return 0.0;
		}
		if (_v >= 1.0)
		{
			return 1.0;
		}
		return _v;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float FastLerp(float a, float b, float t)
	{
		return a + (b - a) * FastClamp01(t);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float FastLerpUnclamped(float a, float b, float t)
	{
		return a + (b - a) * t;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float FastMoveTowards(float current, float target, float maxDelta)
	{
		if (FastAbs(target - current) <= maxDelta)
		{
			return target;
		}
		return current + Mathf.Sign(target - current) * maxDelta;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int FastRoundToInt(float f)
	{
		return (int)Math.Round(f);
	}

	public static int FastRoundToIntAndMod(float _f, int _mod)
	{
		if (_mod == 0)
		{
			return 0;
		}
		if (!(_f > 0f))
		{
			return (_mod + (int)(_f - 0.5f) % _mod) % _mod;
		}
		return (int)(_f + 0.5f) % _mod;
	}

	public static Texture2D LoadRawStampFile(string _filepath)
	{
		using Stream input = SdFile.OpenRead(_filepath);
		using BinaryReader binaryReader = new BinaryReader(input);
		binaryReader.BaseStream.Seek(0L, SeekOrigin.Begin);
		int num = (int)Mathf.Sqrt(binaryReader.BaseStream.Length / 6);
		Texture2D texture2D = new Texture2D(num, num, TextureFormat.RGBAFloat, mipChain: false, linear: false);
		Color[] array = new Color[num * num];
		for (int i = 0; i < array.Length; i++)
		{
			float num2 = (float)(int)binaryReader.ReadUInt16() / 65535f;
			float a = (float)(int)binaryReader.ReadUInt16() / 65535f;
			float b = (float)(int)binaryReader.ReadUInt16() / 65535f;
			array[i] = new Color(num2, num2, b, a);
		}
		texture2D.SetPixels(array);
		texture2D.Apply();
		return texture2D;
	}

	public static Color[] LoadRawStampFileArray(string _filepath)
	{
		using Stream input = SdFile.OpenRead(_filepath);
		using BinaryReader binaryReader = new BinaryReader(input);
		binaryReader.BaseStream.Seek(0L, SeekOrigin.Begin);
		byte[] array = new byte[binaryReader.BaseStream.Length];
		binaryReader.Read(array, 0, array.Length);
		int num = (int)(binaryReader.BaseStream.Length / 6);
		Color[] array2 = new Color[num];
		Color black = Color.black;
		int num2 = 0;
		for (int i = 0; i < num; i++)
		{
			black.r = (float)(array[num2] | (array[num2 + 1] << 8)) / 65535f;
			black.a = (float)(array[num2 + 2] | (array[num2 + 3] << 8)) / 65535f;
			black.b = (float)(array[num2 + 4] | (array[num2 + 5] << 8)) / 65535f;
			array2[i] = black;
			num2 += 6;
		}
		return array2;
	}

	public static void SaveRawStampFile(string _filepath, Texture2D _image)
	{
		using Stream output = SdFile.Open(_filepath, FileMode.CreateNew);
		using BinaryWriter binaryWriter = new BinaryWriter(output);
		binaryWriter.BaseStream.Seek(0L, SeekOrigin.Begin);
		Color[] pixels = _image.GetPixels();
		for (int i = 0; i < pixels.Length; i++)
		{
			binaryWriter.Write((ushort)(pixels[i].r * 65535f));
			binaryWriter.Write((ushort)(pixels[i].g * 65535f));
			binaryWriter.Write((ushort)(pixels[i].b * 65535f));
		}
	}

	public static void SaveRawStampFile(string _filepath, Color[] _image)
	{
		using Stream output = SdFile.Open(_filepath, FileMode.CreateNew);
		using BinaryWriter binaryWriter = new BinaryWriter(output);
		binaryWriter.BaseStream.Seek(0L, SeekOrigin.Begin);
		for (int i = 0; i < _image.Length; i++)
		{
			binaryWriter.Write((ushort)(_image[i].r * 65535f));
			binaryWriter.Write((ushort)(_image[i].g * 65535f));
			binaryWriter.Write((ushort)(_image[i].b * 65535f));
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float Repeat(float _value, float _length)
	{
		return FastClamp(_value - (float)Fastfloor(_value / _length) * _length, 0f, _length);
	}

	public static float DeltaAngle(float _angle1, float _angle2)
	{
		float num = Repeat(_angle2 - _angle1, 360f);
		if (num > 180f)
		{
			num -= 360f;
		}
		return num;
	}

	public static float GetAngleBetween(Vector3 _dir1, Vector3 _dir2)
	{
		float num = Mathf.Atan2(_dir1.z, _dir1.x) * 57.29578f;
		float num2 = Mathf.Atan2(_dir2.z, _dir2.x) * 57.29578f;
		float num3 = num - num2;
		if (num3 > 180f)
		{
			num3 -= 360f;
		}
		if (num3 < -180f)
		{
			num3 += 360f;
		}
		return num3;
	}

	public static int Get4HitDirectionAsInt(Vector3 _direction, Vector3 _myLook)
	{
		float angleBetween = GetAngleBetween(_direction, _myLook);
		if (angleBetween > -45f && angleBetween <= 45f)
		{
			return 1;
		}
		if (angleBetween < -45f && angleBetween >= -135f)
		{
			return 3;
		}
		if (angleBetween > 45f && angleBetween <= 135f)
		{
			return 2;
		}
		return 0;
	}

	public static int Get6HitDirectionAsInt(Vector3 _direction, Vector3 _myLook)
	{
		float angleBetween = GetAngleBetween(_direction, _myLook);
		if (angleBetween > -45f && angleBetween <= 45f)
		{
			return 1;
		}
		if (angleBetween < -45f && angleBetween >= -120f)
		{
			return 3;
		}
		if (angleBetween > 45f && angleBetween <= 120f)
		{
			return 2;
		}
		if (angleBetween > 120f && angleBetween < 160f)
		{
			return 4;
		}
		if (angleBetween < -120f && angleBetween > -160f)
		{
			return 5;
		}
		return 0;
	}

	public static EnumHitDirection GetHitDirection4Sides(Vector3 fwd, Vector3 targetDir, Vector3 up)
	{
		targetDir = targetDir.normalized;
		float num = Vector3.Dot(Vector3.Cross(fwd, targetDir), up);
		if (num > 0.75f)
		{
			return EnumHitDirection.Left;
		}
		if (num < -0.75f)
		{
			return EnumHitDirection.Right;
		}
		if ((double)Mathf.Abs(targetDir.y) > 0.05 && (double)Mathf.Abs(targetDir.x / targetDir.y) < 0.1 && (double)Mathf.Abs(targetDir.z / targetDir.y) < 0.1)
		{
			return EnumHitDirection.Back;
		}
		if (Mathf.Abs(Mathf.Atan2(targetDir.z, targetDir.x) - Mathf.Atan2(fwd.z, fwd.x)) > 1f)
		{
			return EnumHitDirection.Front;
		}
		return EnumHitDirection.Back;
	}

	public static Quaternion BlockFaceToRotation(BlockFace _blockFace)
	{
		return _blockFace switch
		{
			BlockFace.Top => Quaternion.identity, 
			BlockFace.Bottom => Quaternion.AngleAxis(180f, Vector3.forward), 
			BlockFace.North => Quaternion.AngleAxis(90f, Vector3.right), 
			BlockFace.West => Quaternion.AngleAxis(90f, Vector3.forward), 
			BlockFace.South => Quaternion.AngleAxis(-90f, Vector3.right), 
			BlockFace.East => Quaternion.AngleAxis(-90f, Vector3.forward), 
			_ => Quaternion.identity, 
		};
	}

	public static Vector3 BlockFaceToVector(BlockFace _face)
	{
		return _face switch
		{
			BlockFace.Top => new Vector3(0f, 1f, 0f), 
			BlockFace.Bottom => new Vector3(0f, -1f, 0f), 
			BlockFace.North => new Vector3(0f, 0f, 1f), 
			BlockFace.West => new Vector3(-1f, 0f, 0f), 
			BlockFace.South => new Vector3(0f, 0f, -1f), 
			BlockFace.East => new Vector3(1f, 0f, 0f), 
			_ => Vector3.zero, 
		};
	}

	public static void MoveInBlockFaceDirection(Vector3[] _vertices, BlockFace _face, float d)
	{
		Vector3 vector = Vector3.zero;
		switch (_face)
		{
		case BlockFace.Top:
			vector = new Vector3(0f, d, 0f);
			break;
		case BlockFace.Bottom:
			vector = new Vector3(0f, 0f - d, 0f);
			break;
		case BlockFace.North:
			vector = new Vector3(0f, 0f, d);
			break;
		case BlockFace.West:
			vector = new Vector3(0f - d, 0f, 0f);
			break;
		case BlockFace.South:
			vector = new Vector3(0f, 0f, 0f - d);
			break;
		case BlockFace.East:
			vector = new Vector3(d, 0f, 0f);
			break;
		}
		for (int i = 0; i < _vertices.Length; i++)
		{
			_vertices[i] += vector;
		}
	}

	public static string EncryptOrDecrypt(string text, string key)
	{
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < text.Length; i++)
		{
			stringBuilder.Append((char)(text[i] ^ key[i % key.Length]));
		}
		return stringBuilder.ToString();
	}

	public static string Crypt(string text)
	{
		return EncryptOrDecrypt(text, "X");
	}

	public static string UnCryptFromBase64(char[] text)
	{
		return Crypt(FromBase64(text));
	}

	public static string ToBase64(string _str)
	{
		return Convert.ToBase64String(Encoding.UTF8.GetBytes(_str));
	}

	public static string FromBase64(string _encodedStr)
	{
		try
		{
			byte[] bytes = Convert.FromBase64String(_encodedStr);
			return Encoding.UTF8.GetString(bytes);
		}
		catch (FormatException e)
		{
			Log.Error("FBase64 Exception");
			Log.Exception(e);
			return string.Empty;
		}
	}

	public static string FromBase64(char[] _bytes)
	{
		try
		{
			byte[] bytes = Convert.FromBase64CharArray(_bytes, 0, _bytes.Length);
			return Encoding.UTF8.GetString(bytes);
		}
		catch (FormatException e)
		{
			Log.Error("FBase64A Exception");
			Log.Exception(e);
			return string.Empty;
		}
	}

	public static string HashString(string password)
	{
		if (password.Length == 0)
		{
			return password;
		}
		return password.GetHashCode().ToString();
	}

	public static string ColorToHex(Color32 color)
	{
		return color.r.ToString("X2") + color.g.ToString("X2") + color.b.ToString("X2");
	}

	public static Color Saturate(Color _c)
	{
		if (_c.r < 0f)
		{
			_c.r = 0f;
		}
		if (_c.r > 1f)
		{
			_c.r = 1f;
		}
		if (_c.g < 0f)
		{
			_c.g = 0f;
		}
		if (_c.g > 1f)
		{
			_c.g = 1f;
		}
		if (_c.b < 0f)
		{
			_c.b = 0f;
		}
		if (_c.b > 1f)
		{
			_c.b = 1f;
		}
		return _c;
	}

	public static ushort ToColor5(Color col)
	{
		return (ushort)(((int)(col.r * 31f + 0.5f) << 10) | ((int)(col.g * 31f + 0.5f) << 5) | (int)(col.b * 31f + 0.5f));
	}

	public static Color FromColor5(ushort col)
	{
		return new Color((float)((col >> 10) & 0x1F) / 31f, (float)((col >> 5) & 0x1F) / 31f, (float)(col & 0x1F) / 31f);
	}

	public static Color32 FromColor5To32(ushort col)
	{
		return new Color32((byte)((float)((col >> 10) & 0x1F) / 31f * 255f), (byte)((float)((col >> 5) & 0x1F) / 31f * 255f), (byte)((float)(col & 0x1F) / 31f * 255f), byte.MaxValue);
	}

	public static GameRandom RandomFromSeedOnPos(int x, int y, int seed)
	{
		GameRandom gameRandom = GameRandomManager.Instance.CreateGameRandom(seed);
		long num = (long)gameRandom.RandomInt / 2L * 2 + 1;
		long num2 = (long)gameRandom.RandomInt / 2L * 2 + 1;
		int seed2 = (int)(((x << 4) * num + (y << 4) * num2) ^ seed);
		gameRandom.SetSeed(seed2);
		return gameRandom;
	}

	public static GameRandom RandomFromSeedOnPos(int _x, int _y, int _z, int _seed)
	{
		int seed = _seed + _x + (_z << 14) + (_y << 24);
		return GameRandomManager.Instance.CreateGameRandom(seed);
	}

	public static string DescribeTimeSince(DateTime _newer, DateTime _older)
	{
		TimeSpan timeSpan = _newer - _older;
		if (timeSpan.TotalMinutes < 1.0)
		{
			return Localization.Get("xuiLastSeenNow");
		}
		if (timeSpan.TotalMinutes < 60.0)
		{
			int num = (int)timeSpan.TotalMinutes;
			if (num != 1)
			{
				return string.Format(Localization.Get("xuiLastSeenMinutes"), num);
			}
			return Localization.Get("xuiLastSeen1Minute");
		}
		if (timeSpan.TotalHours < 24.0)
		{
			int num2 = (int)timeSpan.TotalHours;
			if (num2 != 1)
			{
				return string.Format(Localization.Get("xuiLastSeenHours"), num2);
			}
			return Localization.Get("xuiLastSeen1Hour");
		}
		int num3 = (int)timeSpan.TotalDays;
		if (num3 != 1)
		{
			return string.Format(Localization.Get("xuiLastSeenDays"), num3);
		}
		return Localization.Get("xuiLastSeen1Day");
	}

	public static float ToCelsius(float fromFahrenheit)
	{
		return (fromFahrenheit - 32f) * (5f / 9f);
	}

	public static float ToRelativeCelsius(float fromFahrenheit)
	{
		return fromFahrenheit * (5f / 9f);
	}

	public static string EscapeBbCodes(string _input, bool _removeInstead = false, bool _allowUrls = false)
	{
		if (string.IsNullOrEmpty(_input))
		{
			return _input;
		}
		StringBuilder stringBuilder = new StringBuilder();
		int num = 0;
		int num2 = 0;
		while (num < _input.Length)
		{
			var (num3, num4, _) = FindNextBbCode(_input, num, _allowUrls);
			if (num3 == -1)
			{
				stringBuilder.Append(_input.Substring(num2));
				break;
			}
			stringBuilder.Append(_input.Substring(num2, num3 - num2));
			if (!_removeInstead)
			{
				int num5 = 0;
				while (_input[num3 + num5] == '[')
				{
					num5++;
					stringBuilder.Append("[");
				}
				stringBuilder.Append("[/c]");
				stringBuilder.Append(_input.Substring(num3 + num5, num4 - num5));
			}
			num = num3 + num4;
			num2 = num;
		}
		return stringBuilder.ToString();
	}

	public static (int, int, bool) FindNextBbCode(string _input, int _startIndex, bool _allowUrls = false)
	{
		int num = _startIndex;
		int length = _input.Length;
		while (num < length)
		{
			if (_input[num] != '[')
			{
				num++;
				continue;
			}
			Match match = nestedEscapePattern.Match(_input.Substring(num, length - num));
			if (match.Success)
			{
				switch (match.Index)
				{
				case 0:
					return (num, match.Length, true);
				case 1:
				{
					int num2 = _input.IndexOf(']', num + match.Length);
					if (num2 == num + match.Length)
					{
						return (num, num2 + 1 - num, true);
					}
					if (num2 != -1)
					{
						string text = "[" + _input.Substring(num + match.Length, num2 - (num + match.Length)) + "]";
						var (num3, num4, _) = FindNextBbCode(text, 0);
						if (num3 == 0 && num4 == text.Length)
						{
							return (num, num2 + 1 - num, true);
						}
						break;
					}
					num += 4;
					continue;
				}
				}
			}
			if (num + 3 > length || _input[num] != '[')
			{
				num++;
				continue;
			}
			if (_input[num + 2] == ']')
			{
				switch (_input[num + 1])
				{
				case '-':
				case 'B':
				case 'C':
				case 'I':
				case 'S':
				case 'T':
				case 'U':
				case 'b':
				case 'c':
				case 'i':
				case 's':
				case 't':
				case 'u':
					return (num, 3, false);
				}
			}
			if (num + 4 <= length && _input[num + 1] == '/' && _input[num + 3] == ']')
			{
				switch (_input[num + 2])
				{
				case '-':
				case 'B':
				case 'C':
				case 'I':
				case 'S':
				case 'T':
				case 'U':
				case 'b':
				case 'c':
				case 'i':
				case 's':
				case 't':
				case 'u':
					return (num, 4, false);
				}
			}
			if (num + 4 <= length && _input[num + 3] == ']' && hexAlphaMatcher.IsMatch(_input.Substring(num + 1, 2)))
			{
				return (num, 4, false);
			}
			if (num + 5 <= length && _input[num + 4] == ']' && _input[num + 1] == 's' && _input[num + 2] == 'u' && (_input[num + 3] == 'b' || _input[num + 3] == 'p'))
			{
				return (num, 5, false);
			}
			if (num + 6 <= length && _input[num + 5] == ']')
			{
				string a = _input.Substring(num, 6);
				if (a.EqualsCaseInsensitive("[/sub]") || a.EqualsCaseInsensitive("[/sup]"))
				{
					return (num, 6, false);
				}
			}
			if (num + 5 < length && _input.Substring(num, 5).EqualsCaseInsensitive("[url="))
			{
				int num5 = _input.IndexOf(']', num + 5);
				if (num5 != -1)
				{
					if (_allowUrls)
					{
						num = num5;
						continue;
					}
					return (num, num5 - num + 1, false);
				}
			}
			if (!_allowUrls && num + 6 <= length && _input.Substring(num, 6).EqualsCaseInsensitive("[/url]"))
			{
				return (num, 6, false);
			}
			if (num + 8 <= length && _input[num + 7] == ']' && hexRgbMatcher.IsMatch(_input.Substring(num + 1, 6)))
			{
				return (num, 8, false);
			}
			if (num + 10 <= length && _input[num + 9] == ']' && hexRgbaMatcher.IsMatch(_input.Substring(num + 1, 8)))
			{
				return (num, 10, false);
			}
			num++;
		}
		return (-1, 0, false);
	}

	public static string GetVisibileTextWithBbCodes(string _input)
	{
		int num = 0;
		StringBuilder stringBuilder = new StringBuilder();
		while (num < _input.Length)
		{
			var (num2, num3, flag) = FindNextBbCode(_input, num);
			if (num2 == -1)
			{
				stringBuilder.Append(_input, num, _input.Length - num);
				break;
			}
			stringBuilder.Append(_input, num, num2 - num);
			if (flag)
			{
				string value = nestedEscapePattern.Match(_input.Substring(num)).Value;
				value.Replace("[/c]", "");
				stringBuilder.Append(value);
				stringBuilder.Append(_input, value.Length, num3 - value.Length);
			}
			num = num2 + num3;
		}
		return stringBuilder.ToString();
	}

	public static string CreateGameMessage(string _senderName, string _message)
	{
		if (string.IsNullOrEmpty(_senderName))
		{
			return _message;
		}
		return _senderName + ": " + _message;
	}

	public static string GetCancellationMessage()
	{
		if (PlatformManager.NativePlatform.Input.CurrentInputStyle == PlayerInputManager.InputStyle.Keyboard)
		{
			return Localization.Get("msgConnectingToServerCancel");
		}
		return string.Format(Localization.Get("msgConnectingToServerCancelTemplate"), LocalPlayerUI.primaryUI.playerInput.PermanentActions.Cancel.GetBindingString(_forController: true, PlatformManager.NativePlatform.Input.CurrentControllerInputStyle));
	}

	public static void Fill<T>(this T[] array, T value)
	{
		int num = array.Length;
		for (int i = 0; i < num; i++)
		{
			array[i] = value;
		}
	}

	public static void Memset(byte[] array, byte what, int length)
	{
		GCHandle gCHandle = GCHandle.Alloc(array, GCHandleType.Pinned);
		MemsetDelegate(gCHandle.AddrOfPinnedObject(), what, length);
		gCHandle.Free();
	}

	public static void CalculateMeshTangents(List<Vector3> _vertices, List<int> _indices, List<Vector3> _normals, List<Vector2> _uvs, List<Vector4> _tangents, Mesh mesh, bool _bIgnoreUvs = false)
	{
		int count = _vertices.Count;
		if (count > 786432)
		{
			return;
		}
		Array.Clear(tan1Temp, 0, count);
		Array.Clear(tan2Temp, 0, count);
		int count2 = _indices.Count;
		for (int i = 0; i < count2; i += 3)
		{
			int num = _indices[i];
			int num2 = _indices[i + 1];
			int num3 = _indices[i + 2];
			Vector3 vector = _vertices[num];
			Vector3 vector2 = _vertices[num2];
			Vector3 vector3 = _vertices[num3];
			Vector2 vector4;
			Vector2 vector5;
			Vector2 vector6;
			if (_bIgnoreUvs)
			{
				vector4 = Vector2.zero;
				vector5 = Vector2.zero;
				vector6 = Vector2.zero;
			}
			else
			{
				vector4 = _uvs[num];
				vector5 = _uvs[num2];
				vector6 = _uvs[num3];
			}
			float num4 = vector2.x - vector.x;
			float num5 = vector3.x - vector.x;
			float num6 = vector2.y - vector.y;
			float num7 = vector3.y - vector.y;
			float num8 = vector2.z - vector.z;
			float num9 = vector3.z - vector.z;
			float num10 = vector5.x - vector4.x;
			float num11 = vector6.x - vector4.x;
			float num12 = vector5.y - vector4.y;
			float num13 = vector6.y - vector4.y;
			float num14 = num10 * num13 - num11 * num12;
			float num15 = ((num14 == 0f) ? 0f : (1f / num14));
			float num16 = (num13 * num4 - num12 * num5) * num15;
			float num17 = (num13 * num6 - num12 * num7) * num15;
			float num18 = (num13 * num8 - num12 * num9) * num15;
			float num19 = (num10 * num5 - num11 * num4) * num15;
			float num20 = (num10 * num7 - num11 * num6) * num15;
			float num21 = (num10 * num9 - num11 * num8) * num15;
			tan1Temp[num].x += num16;
			tan1Temp[num].y += num17;
			tan1Temp[num].z += num18;
			tan1Temp[num2].x += num16;
			tan1Temp[num2].y += num17;
			tan1Temp[num2].z += num18;
			tan1Temp[num3].x += num16;
			tan1Temp[num3].y += num17;
			tan1Temp[num3].z += num18;
			tan2Temp[num].x += num19;
			tan2Temp[num].y += num20;
			tan2Temp[num].z += num21;
			tan2Temp[num2].x += num19;
			tan2Temp[num2].y += num20;
			tan2Temp[num2].z += num21;
			tan2Temp[num3].x += num19;
			tan2Temp[num3].y += num20;
			tan2Temp[num3].z += num21;
		}
		if (_normals.Count == 0)
		{
			tempNormals.Clear();
			mesh.GetNormals(tempNormals);
		}
		for (int j = 0; j < count; j++)
		{
			Vector3 normal = ((_normals.Count > 0) ? _normals[j] : tempNormals[j]);
			Vector3 tangent = tan1Temp[j];
			Vector3.OrthoNormalize(ref normal, ref tangent);
			Vector4 item = new Vector4(tangent.x, tangent.y, tangent.z, (Vector3.Dot(Vector3.Cross(normal, tangent), tan2Temp[j]) < 0f) ? (-1f) : 1f);
			_tangents.Add(item);
		}
	}

	public static void CalculateMeshTangents(ArrayListMP<Vector3> _vertices, ArrayListMP<int> _indices, ArrayListMP<Vector3> _normals, ArrayListMP<Vector2> _uvs, ArrayListMP<Vector4> _tangents, Mesh _mesh, bool _bIgnoreUvs = false)
	{
		int count = _vertices.Count;
		if (count > 786432)
		{
			return;
		}
		Array.Clear(tan1Temp, 0, count);
		Array.Clear(tan2Temp, 0, count);
		int count2 = _indices.Count;
		for (int i = 0; i < count2; i += 3)
		{
			int num = _indices[i];
			int num2 = _indices[i + 1];
			int num3 = _indices[i + 2];
			Vector3 vector = _vertices[num];
			Vector3 vector2 = _vertices[num2];
			Vector3 vector3 = _vertices[num3];
			Vector2 vector4;
			Vector2 vector5;
			Vector2 vector6;
			if (_bIgnoreUvs)
			{
				vector4 = Vector2.zero;
				vector5 = Vector2.zero;
				vector6 = Vector2.zero;
			}
			else
			{
				vector4 = _uvs[num];
				vector5 = _uvs[num2];
				vector6 = _uvs[num3];
			}
			float num4 = vector2.x - vector.x;
			float num5 = vector3.x - vector.x;
			float num6 = vector2.y - vector.y;
			float num7 = vector3.y - vector.y;
			float num8 = vector2.z - vector.z;
			float num9 = vector3.z - vector.z;
			float num10 = vector5.x - vector4.x;
			float num11 = vector6.x - vector4.x;
			float num12 = vector5.y - vector4.y;
			float num13 = vector6.y - vector4.y;
			float num14 = num10 * num13 - num11 * num12;
			float num15 = ((num14 == 0f) ? 0f : (1f / num14));
			float num16 = (num13 * num4 - num12 * num5) * num15;
			float num17 = (num13 * num6 - num12 * num7) * num15;
			float num18 = (num13 * num8 - num12 * num9) * num15;
			float num19 = (num10 * num5 - num11 * num4) * num15;
			float num20 = (num10 * num7 - num11 * num6) * num15;
			float num21 = (num10 * num9 - num11 * num8) * num15;
			tan1Temp[num].x += num16;
			tan1Temp[num].y += num17;
			tan1Temp[num].z += num18;
			tan1Temp[num2].x += num16;
			tan1Temp[num2].y += num17;
			tan1Temp[num2].z += num18;
			tan1Temp[num3].x += num16;
			tan1Temp[num3].y += num17;
			tan1Temp[num3].z += num18;
			tan2Temp[num].x += num19;
			tan2Temp[num].y += num20;
			tan2Temp[num].z += num21;
			tan2Temp[num2].x += num19;
			tan2Temp[num2].y += num20;
			tan2Temp[num2].z += num21;
			tan2Temp[num3].x += num19;
			tan2Temp[num3].y += num20;
			tan2Temp[num3].z += num21;
		}
		if (_normals.Count == 0)
		{
			tempNormals.Clear();
			_mesh.GetNormals(tempNormals);
		}
		for (int j = 0; j < count; j++)
		{
			Vector3 normal = ((_normals.Count > 0) ? _normals[j] : tempNormals[j]);
			Vector3 tangent = tan1Temp[j];
			Vector3.OrthoNormalize(ref normal, ref tangent);
			Vector4 item = new Vector4(tangent.x, tangent.y, tangent.z, (Vector3.Dot(Vector3.Cross(normal, tangent), tan2Temp[j]) < 0f) ? (-1f) : 1f);
			_tangents.Add(item);
		}
	}

	public static void CalculateMeshTangents(VoxelMesh _vm, bool _bIgnoreUvs = false)
	{
		int count = _vm.m_Vertices.Count;
		if (count > 786432)
		{
			return;
		}
		Array.Clear(tan1Temp, 0, count);
		Array.Clear(tan2Temp, 0, count);
		Vector2 vector = Vector2.zero;
		Vector2 vector2 = Vector2.zero;
		Vector2 vector3 = Vector2.zero;
		int count2 = _vm.m_Indices.Count;
		for (int i = 0; i < count2; i += 3)
		{
			int num = _vm.m_Indices[i];
			int num2 = _vm.m_Indices[i + 1];
			int num3 = _vm.m_Indices[i + 2];
			Vector3 vector4 = _vm.m_Vertices[num];
			Vector3 vector5 = _vm.m_Vertices[num2];
			Vector3 vector6 = _vm.m_Vertices[num3];
			if (!_bIgnoreUvs)
			{
				vector = _vm.m_Uvs[num];
				vector2 = _vm.m_Uvs[num2];
				vector3 = _vm.m_Uvs[num3];
			}
			float num4 = vector5.x - vector4.x;
			float num5 = vector6.x - vector4.x;
			float num6 = vector5.y - vector4.y;
			float num7 = vector6.y - vector4.y;
			float num8 = vector5.z - vector4.z;
			float num9 = vector6.z - vector4.z;
			float num10 = vector2.x - vector.x;
			float num11 = vector3.x - vector.x;
			float num12 = vector2.y - vector.y;
			float num13 = vector3.y - vector.y;
			float num14 = num10 * num13 - num11 * num12;
			float num15 = ((num14 == 0f) ? 0f : (1f / num14));
			float num16 = (num13 * num4 - num12 * num5) * num15;
			float num17 = (num13 * num6 - num12 * num7) * num15;
			float num18 = (num13 * num8 - num12 * num9) * num15;
			float num19 = (num10 * num5 - num11 * num4) * num15;
			float num20 = (num10 * num7 - num11 * num6) * num15;
			float num21 = (num10 * num9 - num11 * num8) * num15;
			tan1Temp[num].x += num16;
			tan1Temp[num].y += num17;
			tan1Temp[num].z += num18;
			tan1Temp[num2].x += num16;
			tan1Temp[num2].y += num17;
			tan1Temp[num2].z += num18;
			tan1Temp[num3].x += num16;
			tan1Temp[num3].y += num17;
			tan1Temp[num3].z += num18;
			tan2Temp[num].x += num19;
			tan2Temp[num].y += num20;
			tan2Temp[num].z += num21;
			tan2Temp[num2].x += num19;
			tan2Temp[num2].y += num20;
			tan2Temp[num2].z += num21;
			tan2Temp[num3].x += num19;
			tan2Temp[num3].y += num20;
			tan2Temp[num3].z += num21;
		}
		Vector3[] items = _vm.m_Normals.Items;
		int num22 = _vm.m_Tangents.Alloc(count);
		Vector4[] items2 = _vm.m_Tangents.Items;
		Vector4 vector7 = default(Vector4);
		for (int j = 0; j < count; j++)
		{
			Vector3 normal = items[j];
			Vector3 tangent = tan1Temp[j];
			Vector3.OrthoNormalize(ref normal, ref tangent);
			vector7.x = tangent.x;
			vector7.y = tangent.y;
			vector7.z = tangent.z;
			vector7.w = ((!(Vector3.Dot(Vector3.Cross(normal, tangent), tan2Temp[j]) < 0f)) ? 1 : (-1));
			items2[num22++] = vector7;
		}
	}

	public static void CalculateMeshTangents(ReadOnlySpan<Vector3> _vertices, ReadOnlySpan<int> _indices, ReadOnlySpan<Vector3> _normals, ReadOnlySpan<Vector2> _uvs, Span<Vector4> _tangents, bool _bIgnoreUvs = false)
	{
		int length = _vertices.Length;
		if (length > 786432)
		{
			return;
		}
		using NativeArray<Vector3> source = new NativeArray<Vector3>(length, Allocator.TempJob);
		using NativeArray<Vector3> source2 = new NativeArray<Vector3>(length, Allocator.TempJob);
		Span<Vector3> span = source;
		Span<Vector3> span2 = source2;
		int length2 = _indices.Length;
		for (int i = 0; i < length2; i += 3)
		{
			int index = _indices[i];
			int index2 = _indices[i + 1];
			int index3 = _indices[i + 2];
			Vector3 vector = _vertices[index];
			Vector3 vector2 = _vertices[index2];
			Vector3 vector3 = _vertices[index3];
			Vector2 vector4;
			Vector2 vector5;
			Vector2 vector6;
			if (_bIgnoreUvs)
			{
				vector4 = Vector2.zero;
				vector5 = Vector2.zero;
				vector6 = Vector2.zero;
			}
			else
			{
				vector4 = _uvs[index];
				vector5 = _uvs[index2];
				vector6 = _uvs[index3];
			}
			float num = vector2.x - vector.x;
			float num2 = vector3.x - vector.x;
			float num3 = vector2.y - vector.y;
			float num4 = vector3.y - vector.y;
			float num5 = vector2.z - vector.z;
			float num6 = vector3.z - vector.z;
			float num7 = vector5.x - vector4.x;
			float num8 = vector6.x - vector4.x;
			float num9 = vector5.y - vector4.y;
			float num10 = vector6.y - vector4.y;
			float num11 = num7 * num10 - num8 * num9;
			float num12 = ((num11 == 0f) ? 0f : (1f / num11));
			float num13 = (num10 * num - num9 * num2) * num12;
			float num14 = (num10 * num3 - num9 * num4) * num12;
			float num15 = (num10 * num5 - num9 * num6) * num12;
			float num16 = (num7 * num2 - num8 * num) * num12;
			float num17 = (num7 * num4 - num8 * num3) * num12;
			float num18 = (num7 * num6 - num8 * num5) * num12;
			span[index].x += num13;
			span[index].y += num14;
			span[index].z += num15;
			span[index2].x += num13;
			span[index2].y += num14;
			span[index2].z += num15;
			span[index3].x += num13;
			span[index3].y += num14;
			span[index3].z += num15;
			span2[index].x += num16;
			span2[index].y += num17;
			span2[index].z += num18;
			span2[index2].x += num16;
			span2[index2].y += num17;
			span2[index2].z += num18;
			span2[index3].x += num16;
			span2[index3].y += num17;
			span2[index3].z += num18;
		}
		for (int j = 0; j < length; j++)
		{
			Vector3 normal = _normals[j];
			Vector3 tangent = span[j];
			Vector3.OrthoNormalize(ref normal, ref tangent);
			Vector4 vector7 = new Vector4(tangent.x, tangent.y, tangent.z, (Vector3.Dot(Vector3.Cross(normal, tangent), span2[j]) < 0f) ? (-1f) : 1f);
			_tangents[j] = vector7;
		}
	}

	public static int WrapInt(int value, int min, int max)
	{
		int num = max - min;
		while (value < min)
		{
			value += num;
		}
		while (value > max)
		{
			value -= num;
		}
		return value;
	}

	public static int WrapIndex(int index, int arraySize)
	{
		while (index < 0)
		{
			index += arraySize;
		}
		while (index >= arraySize)
		{
			index -= arraySize;
		}
		return index;
	}

	public static float WrapFloat(float value, float min, float max)
	{
		float num = max - min;
		if (value < min)
		{
			value += num;
		}
		else if (value > max)
		{
			value -= num;
		}
		return value;
	}

	public static void CleanupMaterialsOfRenderers<T>(T renderers) where T : IList<Renderer>
	{
		for (int i = 0; i < renderers.Count; i++)
		{
			renderers[i].GetSharedMaterials(tempMats);
			CleanupMaterials(tempMats);
		}
		tempMats.Clear();
	}

	public static void CleanupMaterialsOfRenderer(Renderer renderer)
	{
		CleanupMaterials(renderer.sharedMaterials);
	}

	public static void CleanupMaterials<T>(T mats) where T : IList<Material>
	{
		for (int i = 0; i < mats.Count; i++)
		{
			Material material = mats[i];
			if ((bool)material && material.GetInstanceID() < 0)
			{
				UnityEngine.Object.Destroy(material);
			}
		}
	}

	public static void MarkMaterialAsSafeForManualCleanup(Material mat)
	{
		mat.name += " (Instance)";
	}

	public static void RestartGame(ERestartAntiCheatMode _antiCheatMode = ERestartAntiCheatMode.KeepAntiCheatMode)
	{
		Regex regex = new Regex("^(.*)(output_log_client__)(\\d{4}-\\d{2}-\\d{2}__\\d{2}-\\d{2}-\\d{2})(\\.txt)$");
		Regex regex2 = new Regex("^(.*)(output_log__)(\\d{4}-\\d{2}-\\d{2}__\\d{2}-\\d{2}-\\d{2})(\\.txt)$", RegexOptions.IgnoreCase);
		string[] commandLineArgs = GameStartupHelper.GetCommandLineArgs();
		string text = commandLineArgs[0];
		bool flag = _antiCheatMode switch
		{
			ERestartAntiCheatMode.KeepAntiCheatMode => PlatformManager.MultiPlatform.AntiCheatClient?.ClientAntiCheatEnabled() ?? false, 
			ERestartAntiCheatMode.ForceOn => true, 
			_ => false, 
		};
		if (flag)
		{
			switch (Application.platform)
			{
			case RuntimePlatform.WindowsPlayer:
			case RuntimePlatform.WindowsServer:
				text = Path.GetDirectoryName(text);
				text = ((text.Length > 0) ? (text + "/") : "") + "7DaysToDie_EAC.exe";
				break;
			case RuntimePlatform.LinuxPlayer:
			case RuntimePlatform.LinuxServer:
				text = Path.GetDirectoryName(text);
				text = ((text.Length > 0) ? (text + "/") : "") + "7DaysToDie_EAC";
				break;
			case RuntimePlatform.OSXPlayer:
			case RuntimePlatform.OSXServer:
				text = Path.GetDirectoryName(text);
				text = ((text.Length > 0) ? (text + "/") : "") + "7DaysToDie_EAC";
				break;
			default:
				Log.Error($"Restarting the game not supported on this platform ({Application.platform})");
				return;
			}
		}
		StringBuilder stringBuilder = new StringBuilder();
		if (!flag)
		{
			stringBuilder.Append("-noeac");
		}
		for (int i = 1; i < commandLineArgs.Length; i++)
		{
			if (!flag || i > 1)
			{
				stringBuilder.Append(' ');
			}
			string text2 = commandLineArgs[i];
			if (text2.EqualsCaseInsensitive("-logfile"))
			{
				stringBuilder.Append(text2);
				stringBuilder.Append(' ');
				i++;
				text2 = commandLineArgs[i];
				Match match;
				if ((match = regex.Match(text2)).Success)
				{
					string text3 = DateTime.Now.ToString("yyyy-MM-dd__HH-mm-ss");
					text2 = match.Groups[1].Value + match.Groups[2].Value + text3 + match.Groups[4].Value;
				}
				else if ((match = regex2.Match(text2)).Success)
				{
					string text4 = DateTime.Now.ToString("yyyy-MM-dd__HH-mm-ss");
					text2 = match.Groups[1].Value + match.Groups[2].Value + text4 + match.Groups[4].Value;
				}
				else
				{
					text2 += ".restarted.txt";
				}
			}
			if (text2.IndexOf(' ') >= 0)
			{
				stringBuilder.Append('"');
				stringBuilder.Append(text2);
				stringBuilder.Append('"');
			}
			else
			{
				stringBuilder.Append(text2);
			}
		}
		stringBuilder.Append(" -skipintro");
		stringBuilder.Append(" -skipnewsscreen=true");
		Process.Start(new ProcessStartInfo(text)
		{
			UseShellExecute = true,
			WorkingDirectory = SdDirectory.GetCurrentDirectory(),
			Arguments = stringBuilder.ToString()
		});
		Log.Out("Restarting game: " + text + " " + stringBuilder.ToString());
		Application.Quit();
	}

	public unsafe static void GetBytes(int _value, byte[] _target, int _targetOffset = 0)
	{
		GetBytes((byte*)(&_value), 4, _target, _targetOffset);
	}

	public unsafe static void GetBytes(uint _value, byte[] _target, int _targetOffset = 0)
	{
		GetBytes((byte*)(&_value), 4, _target, _targetOffset);
	}

	public unsafe static void GetBytes(long _value, byte[] _target, int _targetOffset = 0)
	{
		GetBytes((byte*)(&_value), 8, _target, _targetOffset);
	}

	public unsafe static void GetBytes(byte* _ptr, int _count, byte[] _target, int _targetOffset = 0)
	{
		for (int i = 0; i < _count; i++)
		{
			_target[_targetOffset + i] = _ptr[i];
		}
	}

	public static string MaskIp(string _input)
	{
		if (string.IsNullOrEmpty(_input))
		{
			return _input;
		}
		StringBuilder stringBuilder = new StringBuilder(_input);
		int num;
		if ((num = _input.IndexOfAny(ipSeparatorChars)) < 0)
		{
			return _input;
		}
		while (num >= 0)
		{
			if (num > 0)
			{
				stringBuilder[num - 1] = '*';
			}
			num = _input.IndexOfAny(ipSeparatorChars, num + 1);
		}
		stringBuilder[_input.Length - 1] = '*';
		return stringBuilder.ToString();
	}

	public static void ForceMaterialsInstance(GameObject go)
	{
		Renderer[] componentsInChildren = go.GetComponentsInChildren<Renderer>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			_ = componentsInChildren[i].materials;
		}
	}

	public static bool IsValidWebUrl(ref string _input)
	{
		if (string.IsNullOrEmpty(_input))
		{
			return false;
		}
		if (_input.StartsWith("http://") || _input.StartsWith("https://"))
		{
			return true;
		}
		if (_input.Contains("://"))
		{
			return false;
		}
		_input = "http://" + _input;
		return true;
	}

	public static bool OpenSystemBrowser(string _url)
	{
		if (!IsValidWebUrl(ref _url))
		{
			return false;
		}
		Application.OpenURL(_url);
		return true;
	}

	public static bool ArrayEquals(byte[] _a, byte[] _b)
	{
		if (_a == _b)
		{
			return true;
		}
		if (_a == null || _b == null)
		{
			return false;
		}
		if (_a.Length != _b.Length)
		{
			return false;
		}
		for (int i = 0; i < _a.Length; i++)
		{
			if (_a[i] != _b[i])
			{
				return false;
			}
		}
		return true;
	}

	public static bool ArrayEquals(int[] _a, int[] _b)
	{
		if (_a == _b)
		{
			return true;
		}
		if (_a == null || _b == null)
		{
			return false;
		}
		if (_a.Length != _b.Length)
		{
			return false;
		}
		for (int i = 0; i < _a.Length; i++)
		{
			if (_a[i] != _b[i])
			{
				return false;
			}
		}
		return true;
	}

	public static string GenerateGuid()
	{
		byte[] array = Guid.NewGuid().ToByteArray();
		StringBuilder stringBuilder = new StringBuilder(32);
		for (int i = 0; i < array.Length; i++)
		{
			stringBuilder.Append(array[i].ToString("X2"));
		}
		return stringBuilder.ToString();
	}
}
