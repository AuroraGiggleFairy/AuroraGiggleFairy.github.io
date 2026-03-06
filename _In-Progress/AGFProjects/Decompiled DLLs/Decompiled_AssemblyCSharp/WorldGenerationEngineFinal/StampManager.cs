using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Collections;
using UnityEngine;

namespace WorldGenerationEngineFinal;

[BurstCompile(CompileSynchronously = true)]
public class StampManager
{
	[PublicizedFrom(EAccessModifier.Private)]
	public struct RotateParams
	{
		public float sine;

		public float cosine;

		public int width;

		public int height;

		public float halfWidth;

		public float halfHeight;

		public bool isWater;
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public delegate void DrawStamp_0000A306_0024PostfixBurstDelegate(ref NativeArray<float> _dest, ref RawStamp.Data _src, int _x, int _y, int _destWidth, int _destHeight, float _alphaScale, bool _additive, float _scale, ref Color32 _customColor, float _alphaCutoff, float _angle);

	[PublicizedFrom(EAccessModifier.Internal)]
	public static class DrawStamp_0000A306_0024BurstDirectCall
	{
		[PublicizedFrom(EAccessModifier.Private)]
		public static IntPtr Pointer;

		[PublicizedFrom(EAccessModifier.Private)]
		public static IntPtr DeferredCompilation;

		[BurstDiscard]
		[PublicizedFrom(EAccessModifier.Private)]
		public unsafe static void GetFunctionPointerDiscard(ref IntPtr P_0)
		{
			if (Pointer == (IntPtr)0)
			{
				Pointer = (nint)BurstCompiler.GetILPPMethodFunctionPointer2(DeferredCompilation, (RuntimeMethodHandle)/*OpCode not supported: LdMemberToken*/, typeof(DrawStamp_0000A306_0024PostfixBurstDelegate).TypeHandle);
			}
			P_0 = Pointer;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public static IntPtr GetFunctionPointer()
		{
			nint result = 0;
			GetFunctionPointerDiscard(ref result);
			return result;
		}

		public static void Constructor()
		{
			DeferredCompilation = BurstCompiler.CompileILPPMethod2((RuntimeMethodHandle)/*OpCode not supported: LdMemberToken*/);
		}

		public static void Initialize()
		{
		}

		[PublicizedFrom(EAccessModifier.Private)]
		static DrawStamp_0000A306_0024BurstDirectCall()
		{
			Constructor();
		}

		public unsafe static void Invoke(ref NativeArray<float> _dest, ref RawStamp.Data _src, int _x, int _y, int _destWidth, int _destHeight, float _alphaScale, bool _additive, float _scale, ref Color32 _customColor, float _alphaCutoff, float _angle)
		{
			if (BurstCompiler.IsEnabled)
			{
				IntPtr functionPointer = GetFunctionPointer();
				if (functionPointer != (IntPtr)0)
				{
					((delegate* unmanaged[Cdecl]<ref NativeArray<float>, ref RawStamp.Data, int, int, int, int, float, bool, float, ref Color32, float, float, void>)functionPointer)(ref _dest, ref _src, _x, _y, _destWidth, _destHeight, _alphaScale, _additive, _scale, ref _customColor, _alphaCutoff, _angle);
					return;
				}
			}
			DrawStamp_0024BurstManaged(ref _dest, ref _src, _x, _y, _destWidth, _destHeight, _alphaScale, _additive, _scale, ref _customColor, _alphaCutoff, _angle);
		}
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public delegate void DrawWaterStamp_0000A309_0024PostfixBurstDelegate(ref NativeArray<float> _dest, ref NativeArray<float> _src, int _x, int _y, int _destWidth, int _destHeight, int _srcWidth, int _srcHeight, float _alpha, float _scale, float _customColor, float _angle);

	[PublicizedFrom(EAccessModifier.Internal)]
	public static class DrawWaterStamp_0000A309_0024BurstDirectCall
	{
		[PublicizedFrom(EAccessModifier.Private)]
		public static IntPtr Pointer;

		[PublicizedFrom(EAccessModifier.Private)]
		public static IntPtr DeferredCompilation;

		[BurstDiscard]
		[PublicizedFrom(EAccessModifier.Private)]
		public unsafe static void GetFunctionPointerDiscard(ref IntPtr P_0)
		{
			if (Pointer == (IntPtr)0)
			{
				Pointer = (nint)BurstCompiler.GetILPPMethodFunctionPointer2(DeferredCompilation, (RuntimeMethodHandle)/*OpCode not supported: LdMemberToken*/, typeof(DrawWaterStamp_0000A309_0024PostfixBurstDelegate).TypeHandle);
			}
			P_0 = Pointer;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public static IntPtr GetFunctionPointer()
		{
			nint result = 0;
			GetFunctionPointerDiscard(ref result);
			return result;
		}

		public static void Constructor()
		{
			DeferredCompilation = BurstCompiler.CompileILPPMethod2((RuntimeMethodHandle)/*OpCode not supported: LdMemberToken*/);
		}

		public static void Initialize()
		{
		}

		[PublicizedFrom(EAccessModifier.Private)]
		static DrawWaterStamp_0000A309_0024BurstDirectCall()
		{
			Constructor();
		}

		public unsafe static void Invoke(ref NativeArray<float> _dest, ref NativeArray<float> _src, int _x, int _y, int _destWidth, int _destHeight, int _srcWidth, int _srcHeight, float _alpha, float _scale, float _customColor, float _angle)
		{
			if (BurstCompiler.IsEnabled)
			{
				IntPtr functionPointer = GetFunctionPointer();
				if (functionPointer != (IntPtr)0)
				{
					((delegate* unmanaged[Cdecl]<ref NativeArray<float>, ref NativeArray<float>, int, int, int, int, int, int, float, float, float, float, void>)functionPointer)(ref _dest, ref _src, _x, _y, _destWidth, _destHeight, _srcWidth, _srcHeight, _alpha, _scale, _customColor, _angle);
					return;
				}
			}
			DrawWaterStamp_0024BurstManaged(ref _dest, ref _src, _x, _y, _destWidth, _destHeight, _srcWidth, _srcHeight, _alpha, _scale, _customColor, _angle);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cAlphaCutoff = 1E-05f;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly WorldBuilder worldBuilder;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Dictionary<string, RawStamp> AllStamps = new Dictionary<string, RawStamp>();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<RawStamp> tempGetStampList = new List<RawStamp>();

	public StampManager(WorldBuilder _worldBuilder)
	{
		worldBuilder = _worldBuilder;
	}

	public void ClearStamps()
	{
		foreach (RawStamp value in AllStamps.Values)
		{
			value.Clear();
		}
		AllStamps.Clear();
	}

	public void DrawStampGroup(StampGroup _group, ref NativeArray<float> _dest, int size)
	{
		MicroStopwatch microStopwatch = new MicroStopwatch(_bStart: true);
		for (int i = 0; i < _group.Stamps.Count; i++)
		{
			Stamp stamp = _group.Stamps[i];
			if (stamp != null)
			{
				DrawStamp(ref _dest, stamp);
				if (worldBuilder.IsCanceled)
				{
					break;
				}
			}
		}
		Log.Out("DrawStampGroup '{0}', count {1}, in {2}", _group.Name, _group.Stamps.Count, (float)microStopwatch.ElapsedMilliseconds * 0.001f);
	}

	public void DrawStampGroup(StampGroup _group, Color32[] _image, int size, float _stampScale = 1f)
	{
		MicroStopwatch microStopwatch = new MicroStopwatch(_bStart: true);
		for (int i = 0; i < _group.Stamps.Count; i++)
		{
			Stamp stamp = _group.Stamps[i];
			if (stamp != null)
			{
				int x = (int)((float)stamp.transform.x * _stampScale);
				int y = (int)((float)stamp.transform.y * _stampScale);
				DrawStamp(_image, ref stamp.stamp.data, x, y, size, size, stamp.imageWidth, stamp.imageHeight, stamp.alpha, stamp.scale * _stampScale, stamp.customColor, stamp.alphaCutoff, stamp.transform.rotation, stamp.isWater);
				if (worldBuilder.IsCanceled)
				{
					break;
				}
			}
		}
		Log.Out("DrawStampGroup c32 '{0}', count {1}, in {2}", _group.Name, _group.Stamps.Count, (float)microStopwatch.ElapsedMilliseconds * 0.001f);
	}

	public void DrawWaterStampGroup(StampGroup _group, ref NativeArray<float> _dest, int _destSize)
	{
		MicroStopwatch microStopwatch = new MicroStopwatch(_bStart: true);
		for (int i = 0; i < _group.Stamps.Count; i++)
		{
			Stamp stamp = _group.Stamps[i];
			if (stamp != null)
			{
				DrawWaterStamp(stamp, ref _dest, _destSize);
				if (worldBuilder.IsCanceled)
				{
					break;
				}
			}
		}
		Log.Out("DrawWaterStampGroup '{0}', count {1}, in {2}", _group.Name, _group.Stamps.Count, (float)microStopwatch.ElapsedMilliseconds * 0.001f);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static float CalcRotatedValue(float x1, float y1, ref NativeArray<float> src, float sine, float cosine, int width, int height, bool isWater = false)
	{
		int num = width >> 1;
		int num2 = height >> 1;
		float num3 = cosine * (x1 - (float)num) + sine * (y1 - (float)num2) + (float)num;
		int num4 = (int)num3;
		if ((uint)num4 >= width)
		{
			return 0f;
		}
		float num5 = (0f - sine) * (x1 - (float)num) + cosine * (y1 - (float)num2) + (float)num2;
		int num6 = (int)num5;
		if ((uint)num6 >= height)
		{
			return 0f;
		}
		int num7 = num4 + num6 * width;
		float num8 = src[num7];
		float num9 = ((num4 + 1 >= width) ? num8 : src[num7 + 1]);
		float num10 = ((num6 + 1 >= height) ? num8 : src[num7 + width]);
		float num11 = ((num4 + 1 >= width || num6 + 1 >= height) ? num8 : src[num7 + width + 1]);
		if (isWater && (num8 > 0f || num10 > 0f || num9 > 0f || num11 > 0f))
		{
			return num8;
		}
		num3 -= (float)num4;
		num5 -= (float)num6;
		num8 += (num9 - num8) * num3;
		float num12 = num10 + (num11 - num10) * num3;
		return num8 + (num12 - num8) * num5;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static float CalcRotatedValue(ref RotateParams _p, float x1, float y1, ref NativeArray<float> src)
	{
		float num = _p.cosine * (x1 - _p.halfWidth) + _p.sine * (y1 - _p.halfHeight) + _p.halfWidth;
		int num2 = (int)num;
		if ((uint)num2 >= _p.width)
		{
			return 0f;
		}
		float num3 = (0f - _p.sine) * (x1 - _p.halfWidth) + _p.cosine * (y1 - _p.halfHeight) + _p.halfHeight;
		int num4 = (int)num3;
		if ((uint)num4 >= _p.height)
		{
			return 0f;
		}
		int num5 = num2 + num4 * _p.width;
		float num6 = src[num5];
		float num7 = num6;
		if (num2 + 1 < _p.width)
		{
			num7 = src[num5 + 1];
		}
		float num8 = num6;
		if (num4 + 1 < _p.height)
		{
			num8 = src[num5 + _p.width];
		}
		float num9 = num6;
		if (num2 + 1 < _p.width && num4 + 1 < _p.height)
		{
			num9 = src[num5 + _p.width + 1];
		}
		if (_p.isWater && (num6 > 0f || num8 > 0f || num7 > 0f || num9 > 0f))
		{
			return num6;
		}
		num -= (float)num2;
		num3 -= (float)num4;
		float num10 = num6 + (num7 - num6) * num;
		float num11 = num8 + (num9 - num8) * num;
		return num10 + (num11 - num10) * num3;
	}

	public void DrawStamp(ref NativeArray<float> _dest, Stamp stamp)
	{
		int x = stamp.transform.x;
		int y = stamp.transform.y;
		int worldSize = worldBuilder.WorldSize;
		float angle = stamp.transform.rotation;
		DrawStamp(ref _dest, ref stamp.stamp.data, x, y, worldSize, worldSize, stamp.alpha, stamp.additive, stamp.scale, ref stamp.customColor, stamp.alphaCutoff, angle);
	}

	[BurstCompile(CompileSynchronously = true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public static void DrawStamp(ref NativeArray<float> _dest, ref RawStamp.Data _src, int _x, int _y, int _destWidth, int _destHeight, float _alphaScale, bool _additive, float _scale, ref Color32 _customColor, float _alphaCutoff, float _angle)
	{
		DrawStamp_0000A306_0024BurstDirectCall.Invoke(ref _dest, ref _src, _x, _y, _destWidth, _destHeight, _alphaScale, _additive, _scale, ref _customColor, _alphaCutoff, _angle);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void DrawStamp(Color32[] _dest, ref RawStamp.Data _src, int _x, int _y, int _destWidth, int _destHeight, int _srcWidth, int _srcHeight, float _alpha, float _scale, Color32 _customColor, float _alphaCutoff = 0.1f, float _angle = 0f, bool isWater = false)
	{
		_x -= (int)((float)_srcWidth * _scale) / 2;
		_y -= (int)((float)_srcHeight * _scale) / 2;
		double num = MathF.PI / 180f * _angle;
		float sine = (float)Math.Sin(num);
		float cosine = (float)Math.Cos(num);
		int num2 = Mathf.FloorToInt((float)(((int)Mathf.Sqrt(_srcWidth * _srcWidth + _srcHeight * _srcHeight) - _srcWidth) / 2) * _scale);
		int num3 = Mathf.FloorToInt((float)_srcWidth * _scale + (float)num2);
		num2 = -num2;
		int num4 = num2;
		int num5 = _x + num2;
		if (num5 < 0)
		{
			num4 -= num5;
		}
		int num6 = num3;
		num5 = _x + num3;
		if (num5 >= _destWidth)
		{
			num6 -= num5 - _destWidth;
		}
		int num7 = num2;
		int num8 = _y + num2;
		if (num8 < 0)
		{
			num7 -= num8;
		}
		int num9 = num3;
		num8 = _y + num3;
		if (num8 >= _destHeight)
		{
			num9 -= num8 - _destHeight;
		}
		for (int i = num7; i < num9; i++)
		{
			int num10 = (_y + i) * _destWidth;
			float y = (float)i / _scale;
			for (int j = num4; j < num6; j++)
			{
				float num11 = _src.alphaConst;
				if (_src.alphaPixels.IsCreated)
				{
					num11 = CalcRotatedValue((float)j / _scale, y, ref _src.alphaPixels, sine, cosine, _srcWidth, _srcHeight, isWater);
				}
				if (num11 > _alphaCutoff)
				{
					int num12 = _x + j + num10;
					_dest[num12] = _customColor;
				}
			}
		}
	}

	public static void DrawWaterStamp(Stamp stamp, ref NativeArray<float> _dest, int _destSize)
	{
		if (!stamp.isWater)
		{
			throw new ArgumentException("DrawWaterStamp called with non-water stamp " + stamp.Name);
		}
		NativeArray<float> _src = stamp.stamp.data.waterPixels;
		if (!_src.IsCreated)
		{
			_src = stamp.stamp.data.alphaPixels;
			if (!_src.IsCreated)
			{
				return;
			}
		}
		int x = stamp.transform.x;
		int y = stamp.transform.y;
		float angle = stamp.transform.rotation;
		DrawWaterStamp(ref _dest, ref _src, x, y, _destSize, _destSize, stamp.imageWidth, stamp.imageHeight, stamp.alpha, stamp.scale, (int)stamp.customColor.b, angle);
	}

	[BurstCompile(CompileSynchronously = true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public static void DrawWaterStamp(ref NativeArray<float> _dest, ref NativeArray<float> _src, int _x, int _y, int _destWidth, int _destHeight, int _srcWidth, int _srcHeight, float _alpha, float _scale, float _customColor, float _angle)
	{
		DrawWaterStamp_0000A309_0024BurstDirectCall.Invoke(ref _dest, ref _src, _x, _y, _destWidth, _destHeight, _srcWidth, _srcHeight, _alpha, _scale, _customColor, _angle);
	}

	public static void DrawBiomeStamp(Color32[] _dest, ref NativeArray<float> _src, int _x, int _y, int _destWidth, int _destHeight, int _srcWidth, int _srcHeight, float _scale, Color32 _destColor, float _alphaCutoff = 0.1f, float _angle = 0f)
	{
		_x -= (int)((float)_srcWidth * _scale) / 2;
		_y -= (int)((float)_srcHeight * _scale) / 2;
		double num = MathF.PI / 180f * _angle;
		float sine = (float)Math.Sin(num);
		float cosine = (float)Math.Cos(num);
		int num2 = Mathf.FloorToInt((float)(((int)Mathf.Sqrt(_srcWidth * _srcWidth + _srcHeight * _srcHeight) - _srcWidth) / 2) * _scale);
		int num3 = Mathf.FloorToInt((float)_srcWidth * _scale + (float)num2);
		num2 = -num2;
		int num4 = num2;
		int num5 = _x + num2;
		if (num5 < 0)
		{
			num4 -= num5;
		}
		int num6 = num3;
		num5 = _x + num3;
		if (num5 >= _destWidth)
		{
			num6 -= num5 - _destWidth;
		}
		int num7 = num2;
		int num8 = _y + num2;
		if (num8 < 0)
		{
			num7 -= num8;
		}
		int num9 = num3;
		num8 = _y + num3;
		if (num8 >= _destHeight)
		{
			num9 -= num8 - _destHeight;
		}
		for (int i = num7; i < num9; i++)
		{
			int num10 = (_y + i) * _destWidth;
			float y = (float)i / _scale;
			for (int j = num4; j < num6; j++)
			{
				int num11 = _x + j + num10;
				if (CalcRotatedValue((float)j / _scale, y, ref _src, sine, cosine, _srcWidth, _srcHeight) > _alphaCutoff)
				{
					_dest[num11] = _destColor;
				}
			}
		}
	}

	public bool TryGetStamp(string terrainTypeName, string comboTypeName, out RawStamp tmp)
	{
		if (!TryGetStamp(comboTypeName, out tmp) && !TryGetStamp(terrainTypeName, out tmp))
		{
			return false;
		}
		return true;
	}

	public bool TryGetStamp(string _baseName, out RawStamp _output)
	{
		return TryGetStamp(_baseName, out _output, Rand.Instance);
	}

	public bool TryGetStamp(string _baseName, out RawStamp _output, Rand _rand)
	{
		lock (tempGetStampList)
		{
			foreach (KeyValuePair<string, RawStamp> allStamp in AllStamps)
			{
				if (allStamp.Key.StartsWith(_baseName))
				{
					tempGetStampList.Add(allStamp.Value);
				}
			}
			if (tempGetStampList.Count == 0)
			{
				_output = null;
				return false;
			}
			if (tempGetStampList.Count == 1)
			{
				_output = tempGetStampList[0];
			}
			else
			{
				_output = tempGetStampList[_rand.Range(tempGetStampList.Count)];
			}
			tempGetStampList.Clear();
			return true;
		}
	}

	public RawStamp GetStamp(string _baseName, Rand _rand = null)
	{
		if (_rand == null)
		{
			_rand = Rand.Instance;
		}
		if (!TryGetStamp(_baseName, out var _output, _rand))
		{
			return null;
		}
		return _output;
	}

	public IEnumerator LoadStamps()
	{
		if (AllStamps.Count > 0)
		{
			yield break;
		}
		MicroStopwatch ms = new MicroStopwatch(_bStart: true);
		AllStamps.Clear();
		List<PathAbstractions.AbstractedLocation> stampMaps = PathAbstractions.RwgStampsSearchPaths.GetAvailablePathsList(null, null, null, _ignoreDuplicateNames: true);
		MicroStopwatch msRestart = new MicroStopwatch(_bStart: true);
		for (int i = 0; i < stampMaps.Count; i++)
		{
			string extension = stampMaps[i].Extension;
			if (!(extension != ".exr") || !(extension != ".raw"))
			{
				LoadStamp(stampMaps[i]);
				if (msRestart.ElapsedMilliseconds > 500)
				{
					yield return null;
					msRestart.ResetAndRestart();
				}
			}
		}
		for (int i = 0; i < stampMaps.Count; i++)
		{
			LoadStamp(stampMaps[i]);
			if (msRestart.ElapsedMilliseconds > 500)
			{
				yield return null;
				msRestart.ResetAndRestart();
			}
		}
		Log.Out("LoadStamps in {0}", (float)ms.ElapsedMilliseconds * 0.001f);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void LoadStamp(PathAbstractions.AbstractedLocation _path)
	{
		string fileNameNoExtension = _path.FileNameNoExtension;
		if (AllStamps.ContainsKey(fileNameNoExtension))
		{
			return;
		}
		MicroStopwatch microStopwatch = new MicroStopwatch(_bStart: true);
		Color[] array;
		if (_path.Extension == ".raw")
		{
			array = Utils.LoadRawStampFileArray(_path.FullPath);
		}
		else
		{
			if (_path.Extension == ".exr")
			{
				return;
			}
			Texture2D texture2D = TextureUtils.LoadTexture(_path.FullPath);
			array = texture2D.GetPixels();
			if (Application.isPlaying)
			{
				UnityEngine.Object.Destroy(texture2D);
			}
		}
		if (array == null)
		{
			Log.Error("LoadStamp {0} failed", _path.FullPath);
			return;
		}
		NativeArray<float> heightPixels = default(NativeArray<float>);
		float r = array[0].r;
		for (int i = 1; i < array.Length; i++)
		{
			if (r != array[i].r)
			{
				heightPixels = new NativeArray<float>(array.Length, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
				for (int j = 0; j < array.Length; j++)
				{
					heightPixels[j] = array[j].r * 255f;
				}
				break;
			}
		}
		r *= 255f;
		NativeArray<float> alphaPixels = default(NativeArray<float>);
		float a = array[0].a;
		for (int k = 1; k < array.Length; k++)
		{
			if (a != array[k].a)
			{
				alphaPixels = new NativeArray<float>(array.Length, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
				for (int l = 0; l < array.Length; l++)
				{
					alphaPixels[l] = array[l].a;
				}
				break;
			}
		}
		NativeArray<float> waterPixels = default(NativeArray<float>);
		if (!fileNameNoExtension.Contains("rwg_tile"))
		{
			for (int m = 0; m < array.Length; m++)
			{
				if (array[m].b != 0f)
				{
					waterPixels = new NativeArray<float>(array.Length, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
					for (int n = 0; n < array.Length; n++)
					{
						waterPixels[n] = array[n].b * 255f;
					}
					break;
				}
			}
		}
		int num = (int)Mathf.Sqrt(array.Length);
		RawStamp rawStamp = new RawStamp
		{
			name = fileNameNoExtension
		};
		rawStamp.data.heightConst = r;
		rawStamp.data.heightPixels = heightPixels;
		rawStamp.data.alphaConst = a;
		rawStamp.data.alphaPixels = alphaPixels;
		rawStamp.data.waterPixels = waterPixels;
		rawStamp.data.height = num;
		rawStamp.data.width = num;
		AllStamps[fileNameNoExtension] = rawStamp;
		if (alphaPixels.IsCreated)
		{
			if (fileNameNoExtension.StartsWith("mountains_"))
			{
				RawStamp.SmoothAlpha(ref rawStamp.data, 5);
			}
			if (fileNameNoExtension.StartsWith("desert_mountains_"))
			{
				RawStamp.SmoothAlpha(ref rawStamp.data, 3);
			}
		}
		Log.Out("LoadStamp {0}, size {1}, height {2} ({3}), alpha {4} ({5}), water {6}, in {7}", _path.FullPath, num, heightPixels.IsCreated, r, alphaPixels.IsCreated, a, waterPixels.IsCreated, (float)microStopwatch.ElapsedMilliseconds * 0.001f);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void SaveEXR(string cleanname, Color[] colors, bool _replace = true)
	{
		string text = GameIO.GetGameDir("Data/Stamps") + "/" + cleanname + ".exr";
		if (_replace || !File.Exists(text))
		{
			int num = (int)Mathf.Sqrt(colors.Length);
			Texture2D texture2D = new Texture2D(num, num, TextureFormat.RGBAFloat, mipChain: false);
			texture2D.SetPixels(colors);
			File.WriteAllBytes(text, texture2D.EncodeToEXR(Texture2D.EXRFlags.None));
			Log.Out("SaveEXR {0}", text);
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[BurstCompile(CompileSynchronously = true)]
	[PublicizedFrom(EAccessModifier.Internal)]
	public static void DrawStamp_0024BurstManaged(ref NativeArray<float> _dest, ref RawStamp.Data _src, int _x, int _y, int _destWidth, int _destHeight, float _alphaScale, bool _additive, float _scale, ref Color32 _customColor, float _alphaCutoff, float _angle)
	{
		_x -= (int)((float)_src.width * _scale) / 2;
		_y -= (int)((float)_src.height * _scale) / 2;
		double num = MathF.PI / 180f * _angle;
		float sine = (float)Math.Sin(num);
		float cosine = (float)Math.Cos(num);
		int num2 = (int)Math.Floor((float)(((int)Math.Sqrt(_src.width * _src.width + _src.height * _src.height) - _src.width) / 2) * _scale);
		int num3 = (int)Math.Floor((float)_src.width * _scale + (float)num2);
		num2 = -num2;
		int num4 = num2;
		int num5 = _x + num2;
		if (num5 < 0)
		{
			num4 -= num5;
		}
		int num6 = num3;
		num5 = _x + num3;
		if (num5 >= _destWidth)
		{
			num6 -= num5 - _destWidth;
		}
		int num7 = num2;
		int num8 = _y + num2;
		if (num8 < 0)
		{
			num7 -= num8;
		}
		int num9 = num3;
		num8 = _y + num3;
		if (num8 >= _destHeight)
		{
			num9 -= num8 - _destHeight;
		}
		RotateParams _p = default(RotateParams);
		_p.sine = sine;
		_p.cosine = cosine;
		_p.width = _src.width;
		_p.height = _src.height;
		_p.halfWidth = _src.width / 2;
		_p.halfHeight = _src.height / 2;
		_p.isWater = false;
		for (int i = num7; i < num9; i++)
		{
			int num10 = (_y + i) * _destWidth;
			float y = (float)i / _scale;
			for (int j = num4; j < num6; j++)
			{
				float x = (float)j / _scale;
				float num11 = _src.alphaConst;
				if (_src.alphaPixels.IsCreated)
				{
					num11 = CalcRotatedValue(ref _p, x, y, ref _src.alphaPixels);
				}
				if (num11 < 1E-05f)
				{
					continue;
				}
				int index = _x + j + num10;
				if (_customColor.a > 0)
				{
					if (num11 > _alphaCutoff)
					{
						_dest[index] = (int)_customColor.r;
					}
					continue;
				}
				float num12 = _src.heightConst;
				if (_src.heightPixels.IsCreated)
				{
					num12 = CalcRotatedValue(ref _p, x, y, ref _src.heightPixels);
				}
				float num13 = num11 * _alphaScale;
				float num14 = _dest[index];
				num14 = ((!_additive) ? (num14 + (num12 - num14) * num13) : (num14 + num12 * num13));
				_dest[index] = num14;
			}
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[BurstCompile(CompileSynchronously = true)]
	[PublicizedFrom(EAccessModifier.Internal)]
	public static void DrawWaterStamp_0024BurstManaged(ref NativeArray<float> _dest, ref NativeArray<float> _src, int _x, int _y, int _destWidth, int _destHeight, int _srcWidth, int _srcHeight, float _alpha, float _scale, float _customColor, float _angle)
	{
		_x -= (int)((float)_srcWidth * _scale) / 2;
		_y -= (int)((float)_srcHeight * _scale) / 2;
		double num = MathF.PI / 180f * _angle;
		float sine = (float)Math.Sin(num);
		float cosine = (float)Math.Cos(num);
		int num2 = (int)Math.Floor((float)(((int)Math.Sqrt(_srcWidth * _srcWidth + _srcHeight * _srcHeight) - _srcWidth) / 2) * _scale);
		int num3 = (int)Math.Floor((float)_srcWidth * _scale + (float)num2);
		num2 = -num2;
		int num4 = num2;
		int num5 = _x + num2;
		if (num5 < 0)
		{
			num4 -= num5;
		}
		int num6 = num3;
		num5 = _x + num3;
		if (num5 >= _destWidth)
		{
			num6 -= num5 - _destWidth;
		}
		int num7 = num2;
		int num8 = _y + num2;
		if (num8 < 0)
		{
			num7 -= num8;
		}
		int num9 = num3;
		num8 = _y + num3;
		if (num8 >= _destHeight)
		{
			num9 -= num8 - _destHeight;
		}
		RotateParams _p = default(RotateParams);
		_p.sine = sine;
		_p.cosine = cosine;
		_p.width = _srcWidth;
		_p.height = _srcHeight;
		_p.halfWidth = _srcWidth / 2;
		_p.halfHeight = _srcHeight / 2;
		_p.isWater = true;
		for (int i = num7; i < num9; i++)
		{
			int num10 = (_y + i) * _destWidth;
			float y = (float)i / _scale;
			for (int j = num4; j < num6; j++)
			{
				int index = _x + j + num10;
				if (!(_dest[index] > 0f) && !(CalcRotatedValue(ref _p, (float)j / _scale, y, ref _src) < 1E-05f))
				{
					_dest[index] = _customColor;
				}
			}
		}
	}
}
