using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class DecoOccupiedMap
{
	[PublicizedFrom(EAccessModifier.Private)]
	public EnumDecoOccupied[] occupiedMap;

	[PublicizedFrom(EAccessModifier.Private)]
	public int width;

	[PublicizedFrom(EAccessModifier.Private)]
	public int height;

	[PublicizedFrom(EAccessModifier.Private)]
	public int widthHalf;

	[PublicizedFrom(EAccessModifier.Private)]
	public int heightHalf;

	public DecoOccupiedMap(int _worldWidth, int _worldHeight)
	{
		width = _worldWidth;
		height = _worldHeight;
		widthHalf = width / 2;
		heightHalf = height / 2;
		occupiedMap = new EnumDecoOccupied[width * height];
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public EnumDecoOccupied Get(int _offs)
	{
		return occupiedMap[_offs];
	}

	public EnumDecoOccupied Get(int _x, int _z)
	{
		int num = DecoManager.CheckPosition(width, height, _x, _z);
		if (num >= 0)
		{
			return occupiedMap[num];
		}
		return EnumDecoOccupied.NoneAllowed;
	}

	public void Set(int _offs, EnumDecoOccupied _v)
	{
		occupiedMap[_offs] = _v;
	}

	public void Set(int _x, int _z, EnumDecoOccupied _v)
	{
		int num = DecoManager.CheckPosition(width, height, _x, _z);
		if (num >= 0)
		{
			occupiedMap[num] = _v;
		}
	}

	public bool CheckArea(int _x, int _z, EnumDecoOccupied _v, int _rectSizeX, int _rectSizeZ)
	{
		int num = DecoManager.CheckPosition(width, height, _x, _z);
		if (num < 0)
		{
			return true;
		}
		for (int i = 0; i < _rectSizeZ; i++)
		{
			for (int j = 0; j < _rectSizeX; j++)
			{
				if (num >= occupiedMap.Length)
				{
					return true;
				}
				if ((int)occupiedMap[num] >= (int)_v)
				{
					return true;
				}
				num++;
			}
			num += width - _rectSizeX;
		}
		return false;
	}

	public void SetArea(int _x, int _z, EnumDecoOccupied _v, int _rectSizeX, int _rectSizeZ)
	{
		int num = _x + widthHalf + (_z + heightHalf) * width;
		for (int i = 0; i < _rectSizeZ; i++)
		{
			for (int j = 0; j < _rectSizeX; j++)
			{
				if (num < 0 || num >= occupiedMap.Length)
				{
					num++;
					continue;
				}
				if ((int)occupiedMap[num] < (int)_v)
				{
					occupiedMap[num] = _v;
				}
				num++;
			}
			num += width - _rectSizeX;
		}
	}

	public EnumDecoOccupied[] GetData()
	{
		return occupiedMap;
	}

	public void SaveAsTexture(string path, bool includeFlatAreas = false, List<FlatArea> flatAreas = null)
	{
		Color32[] array = new Color32[occupiedMap.Length];
		for (int i = 0; i < occupiedMap.Length; i++)
		{
			Color color = Color.black;
			switch (occupiedMap[i])
			{
			case EnumDecoOccupied.SmallSlope:
				color = Color.blue;
				break;
			case EnumDecoOccupied.Stop_BigDeco:
				color = Color.gray;
				break;
			case EnumDecoOccupied.Perimeter:
				if (!includeFlatAreas)
				{
					color = Color.red;
				}
				break;
			case EnumDecoOccupied.Stop_AnyDeco:
				color = Color.cyan;
				break;
			case EnumDecoOccupied.Deco:
				if (!includeFlatAreas)
				{
					color = Color.green;
				}
				break;
			case EnumDecoOccupied.POI:
				color = Color.magenta;
				break;
			case EnumDecoOccupied.BigSlope:
				color = Color.yellow;
				break;
			case EnumDecoOccupied.NoneAllowed:
				color = Color.white;
				break;
			}
			array[i] = color;
		}
		if (includeFlatAreas)
		{
			if (flatAreas == null || flatAreas.Count == 0)
			{
				flatAreas = GameManager.Instance.World.FlatAreaManager?.GetAllFlatAreas();
			}
			if (flatAreas != null)
			{
				foreach (FlatArea flatArea in flatAreas)
				{
					foreach (Vector2i position in flatArea.GetPositions())
					{
						Color color2 = ((position.x == flatArea.position.x && position.y == flatArea.position.z) ? Color.red : ((flatArea.size != 16) ? new Color(0.5f, 0.5f, 0.5f, 1f) : new Color(0.75f, 0.75f, 0.75f, 1f)));
						int num = DecoManager.CheckPosition(width, height, position.x, position.y);
						array[num] = color2;
					}
				}
			}
		}
		Texture2D texture2D = new Texture2D(width, height);
		texture2D.SetPixels32(array);
		texture2D.Apply();
		TextureUtils.SaveTexture(texture2D, path);
		Log.Out("Saved deco texture to {0}", path);
		Object.Destroy(texture2D);
	}
}
