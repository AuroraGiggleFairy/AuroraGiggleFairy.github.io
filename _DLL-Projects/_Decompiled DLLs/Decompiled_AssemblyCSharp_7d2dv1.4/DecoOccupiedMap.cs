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

	public EnumDecoOccupied Get(int _offs)
	{
		return occupiedMap[_offs];
	}

	public EnumDecoOccupied Get(int _x, int _z)
	{
		int num;
		if ((num = DecoManager.CheckPosition(width, height, _x, _z)) < 0)
		{
			return EnumDecoOccupied.NoneAllowed;
		}
		return occupiedMap[num];
	}

	public void Set(int _offs, EnumDecoOccupied _v)
	{
		occupiedMap[_offs] = _v;
	}

	public void Set(int _x, int _z, EnumDecoOccupied _v)
	{
		int num;
		if ((num = DecoManager.CheckPosition(width, height, _x, _z)) >= 0)
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

	public void SaveAsTexture(string _filename)
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
				color = Color.red;
				break;
			case EnumDecoOccupied.Stop_AnyDeco:
				color = Color.cyan;
				break;
			case EnumDecoOccupied.Deco:
				color = Color.green;
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
		Texture2D texture2D = new Texture2D(width, height);
		texture2D.SetPixels32(array);
		texture2D.Apply();
		TextureUtils.SaveTexture(texture2D, _filename);
		Object.Destroy(texture2D);
	}
}
