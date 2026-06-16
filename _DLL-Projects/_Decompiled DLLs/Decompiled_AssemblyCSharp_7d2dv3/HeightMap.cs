using System;
using UnityEngine;

public sealed class HeightMap : IDisposable
{
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly int w;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly int h;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly int scaleShift;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly int scalePixs;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly float maxHeight;

	[PublicizedFrom(EAccessModifier.Private)]
	public IBackedArrayView<ushort> data;

	public HeightMap(int _w, int _h, float _maxHeight, IBackedArray<ushort> _data, int _targetSize = 0)
	{
		w = _w;
		h = _h;
		scaleShift = ((_targetSize != 0) ? ((int)Mathf.Log(_targetSize / _w, 2f)) : 0);
		scalePixs = _targetSize / _w;
		maxHeight = _maxHeight;
		data = BackedArrays.CreateSingleView(_data, BackedArrayHandleMode.ReadOnly, 16 * _w);
	}

	public void Dispose()
	{
		data?.Dispose();
		data = null;
	}

	public float GetAt(int _x, int _z)
	{
		ushort num = ((scaleShift != 0 || _x + _z * w >= data.Length) ? getInterpolatedHeight(_x, _z) : data[_x + _z * w]);
		return (float)(int)num * (maxHeight / 65535f);
	}

	public ushort getInterpolatedHeight(int xf, int zf)
	{
		int num = ((xf >= 0) ? (xf >> scaleShift) : (xf - scalePixs + 1 >> scaleShift));
		int num2 = ((zf >= 0) ? (zf >> scaleShift) : (zf - scalePixs + 1 >> scaleShift));
		int num3 = num + num2 * w;
		ushort num4 = data[(num3 + w) & (data.Length - 1)];
		ushort num5 = data[num3 & (data.Length - 1)];
		ushort num6 = data[(num3 + 1) & (data.Length - 1)];
		ushort num7 = data[(num3 + 1 + w) & (data.Length - 1)];
		int num8 = num << scaleShift;
		int num9 = num2 << scaleShift;
		float num10 = 1f - (float)(zf - num9) / (float)scalePixs;
		float num11 = (float)(xf - num8) / (float)scalePixs;
		return (ushort)((1f - num10) * ((1f - num11) * (float)(int)num4 + num11 * (float)(int)num7) + num10 * ((1f - num11) * (float)(int)num5 + num11 * (float)(int)num6));
	}

	public float GetAt(int _offs)
	{
		ushort num;
		if (scaleShift == 0)
		{
			num = data[_offs];
		}
		else
		{
			int zf = _offs / w;
			int xf = _offs % w;
			num = getInterpolatedHeight(xf, zf);
		}
		return (float)(int)num * maxHeight / 65535f;
	}

	public int CalcOffset(int _x, int _z)
	{
		_x >>= scaleShift;
		_z >>= scaleShift;
		return _x + _z * w;
	}

	public int GetWidth()
	{
		return w;
	}

	public int GetHeight()
	{
		return h;
	}

	public int GetScaleSteps()
	{
		return scalePixs;
	}

	public int GetScaleShift()
	{
		return scaleShift;
	}
}
