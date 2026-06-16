using System;
using System.IO;
using UnityEngine;

public class PNG2TGA
{
	[PublicizedFrom(EAccessModifier.Private)]
	public struct Vector2Int(int x, int y)
	{
		public int x = x;

		public int y = y;

		public static Vector2Int operator +(Vector2Int a, Vector2Int b)
		{
			return new Vector2Int(a.x + b.x, a.y + b.y);
		}

		public static Vector2Int operator -(Vector2Int a, Vector2Int b)
		{
			return new Vector2Int(a.x - b.x, a.y - b.y);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static PNG2TGA _window;

	[PublicizedFrom(EAccessModifier.Private)]
	public Texture2D _texture;

	[PublicizedFrom(EAccessModifier.Private)]
	public int _width;

	[PublicizedFrom(EAccessModifier.Private)]
	public int _height;

	[PublicizedFrom(EAccessModifier.Private)]
	public byte[] _data;

	[PublicizedFrom(EAccessModifier.Private)]
	public static byte[] header = new byte[18]
	{
		0, 0, 2, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 32, 8
	};

	[PublicizedFrom(EAccessModifier.Private)]
	public static byte[] footer = new byte[26]
	{
		0, 0, 0, 0, 0, 0, 0, 0, 84, 82,
		85, 69, 86, 73, 83, 73, 79, 78, 45, 88,
		70, 73, 76, 69, 46, 0
	};

	[PublicizedFrom(EAccessModifier.Private)]
	public int gwidth;

	[PublicizedFrom(EAccessModifier.Private)]
	public int gheight;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector2Int IndexToVector2Int(int index)
	{
		return new Vector2Int(index % _width, index / _width);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int XYToIndex(int x, int y)
	{
		return x + y * _width;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int Vector2IntToIndex(Vector2Int pos)
	{
		return XYToIndex(pos.x, pos.y);
	}

	public void Process(Texture2D texture)
	{
		_texture = texture;
		Color[] pixels = _texture.GetPixels(0);
		int[] array = new int[pixels.Length];
		_width = _texture.width;
		_height = _texture.height;
		bool flag = false;
		for (int i = 0; i < pixels.Length; i++)
		{
			if (pixels[i].a == 0f)
			{
				pixels[i].r = (pixels[i].g = (pixels[i].b = 0f));
				array[i] = 0;
			}
			else
			{
				array[i] = 1;
				flag = true;
			}
		}
		if (flag)
		{
			bool flag2 = true;
			int num = 1;
			while (flag2)
			{
				flag2 = false;
				for (int j = 0; j < _height; j++)
				{
					for (int k = 0; k < _width; k++)
					{
						if (array[XYToIndex(k, j)] == 0)
						{
							flag2 = true;
							if ((k > 0 && array[XYToIndex(k - 1, j)] == num) || (k < _width - 1 && array[XYToIndex(k + 1, j)] == num) || (j > 0 && array[XYToIndex(k, j - 1)] == num) || (j < _height - 1 && array[XYToIndex(k, j + 1)] == num))
							{
								array[XYToIndex(k, j)] = num + 1;
							}
						}
					}
				}
				num++;
			}
			for (int l = 2; l < num; l++)
			{
				for (int m = 0; m < _height; m++)
				{
					for (int n = 0; n < _width; n++)
					{
						if (array[XYToIndex(n, m)] == l)
						{
							float num2 = 0f;
							Color black = Color.black;
							if (n > 0 && array[XYToIndex(n - 1, m)] == l - 1)
							{
								num2 += 1f;
								black += 0.125f * pixels[XYToIndex(n - 1, m)];
							}
							if (n < _width - 1 && array[XYToIndex(n + 1, m)] == l - 1)
							{
								num2 += 1f;
								black += 0.125f * pixels[XYToIndex(n + 1, m)];
							}
							if (m > 0 && array[XYToIndex(n, m - 1)] == l - 1)
							{
								num2 += 1f;
								black += 0.125f * pixels[XYToIndex(n, m - 1)];
							}
							if (m < _height - 1 && array[XYToIndex(n, m + 1)] == l - 1)
							{
								num2 += 1f;
								black += 0.125f * pixels[XYToIndex(n, m + 1)];
							}
							black *= 8f / num2;
							black.a = 0f;
							pixels[XYToIndex(n, m)] = black;
						}
					}
				}
			}
		}
		_texture.SetPixels(pixels, 0);
		_data = new byte[pixels.Length * 4];
		int num3 = pixels.Length;
		Mathf.Max(num3 / 200, 1);
		for (int num4 = 0; num4 < num3; num4++)
		{
			_data[4 * num4] = (byte)(pixels[num4].b * 255f);
			_data[4 * num4 + 1] = (byte)(pixels[num4].g * 255f);
			_data[4 * num4 + 2] = (byte)(pixels[num4].r * 255f);
			_data[4 * num4 + 3] = (byte)(pixels[num4].a * 255f);
		}
		_texture.Apply();
	}

	public void Process2(Color[] pixels, int _width, int _height)
	{
		gwidth = _width;
		gheight = _height;
		int[] array = new int[pixels.Length];
		bool flag = false;
		for (int i = 0; i < pixels.Length; i++)
		{
			array[i] = 1;
			flag = true;
		}
		if (flag)
		{
			bool flag2 = true;
			int num = 1;
			while (flag2)
			{
				flag2 = false;
				for (int j = 0; j < this._height; j++)
				{
					for (int k = 0; k < this._width; k++)
					{
						if (array[XYToIndex(k, j)] == 0)
						{
							flag2 = true;
							if ((k > 0 && array[XYToIndex(k - 1, j)] == num) || (k < this._width - 1 && array[XYToIndex(k + 1, j)] == num) || (j > 0 && array[XYToIndex(k, j - 1)] == num) || (j < this._height - 1 && array[XYToIndex(k, j + 1)] == num))
							{
								array[XYToIndex(k, j)] = num + 1;
							}
						}
					}
				}
				num++;
			}
			for (int l = 2; l < num; l++)
			{
				for (int m = 0; m < this._height; m++)
				{
					for (int n = 0; n < this._width; n++)
					{
						if (array[XYToIndex(n, m)] == l)
						{
							float num2 = 0f;
							Color black = Color.black;
							if (n > 0 && array[XYToIndex(n - 1, m)] == l - 1)
							{
								num2 += 1f;
								black += 0.125f * pixels[XYToIndex(n - 1, m)];
							}
							if (n < this._width - 1 && array[XYToIndex(n + 1, m)] == l - 1)
							{
								num2 += 1f;
								black += 0.125f * pixels[XYToIndex(n + 1, m)];
							}
							if (m > 0 && array[XYToIndex(n, m - 1)] == l - 1)
							{
								num2 += 1f;
								black += 0.125f * pixels[XYToIndex(n, m - 1)];
							}
							if (m < this._height - 1 && array[XYToIndex(n, m + 1)] == l - 1)
							{
								num2 += 1f;
								black += 0.125f * pixels[XYToIndex(n, m + 1)];
							}
							black *= 8f / num2;
							black.a = 0f;
							pixels[XYToIndex(n, m)] = black;
						}
					}
				}
			}
		}
		_data = new byte[pixels.Length * 4];
		int num3 = pixels.Length;
		Mathf.Max(num3 / 200, 1);
		for (int num4 = 0; num4 < num3; num4++)
		{
			_data[4 * num4] = (byte)(pixels[num4].b * 255f);
			_data[4 * num4 + 1] = (byte)(pixels[num4].g * 255f);
			_data[4 * num4 + 2] = (byte)(pixels[num4].r * 255f);
			_data[4 * num4 + 3] = (byte)(pixels[num4].a * 255f);
		}
	}

	public void Process3(Color32[] pixels, int _width, int _height)
	{
		gwidth = _width;
		gheight = _height;
		int[] array = new int[pixels.Length];
		bool flag = false;
		for (int i = 0; i < pixels.Length; i++)
		{
			array[i] = 1;
			flag = true;
		}
		if (flag)
		{
			bool flag2 = true;
			int num = 1;
			while (flag2)
			{
				flag2 = false;
				for (int j = 0; j < this._height; j++)
				{
					for (int k = 0; k < this._width; k++)
					{
						if (array[XYToIndex(k, j)] == 0)
						{
							flag2 = true;
							if ((k > 0 && array[XYToIndex(k - 1, j)] == num) || (k < this._width - 1 && array[XYToIndex(k + 1, j)] == num) || (j > 0 && array[XYToIndex(k, j - 1)] == num) || (j < this._height - 1 && array[XYToIndex(k, j + 1)] == num))
							{
								array[XYToIndex(k, j)] = num + 1;
							}
						}
					}
				}
				num++;
			}
			for (int l = 2; l < num; l++)
			{
				for (int m = 0; m < this._height; m++)
				{
					for (int n = 0; n < this._width; n++)
					{
						if (array[XYToIndex(n, m)] == l)
						{
							float num2 = 0f;
							Color black = Color.black;
							if (n > 0 && array[XYToIndex(n - 1, m)] == l - 1)
							{
								num2 += 1f;
								black += 0.125f * (Color)pixels[XYToIndex(n - 1, m)];
							}
							if (n < this._width - 1 && array[XYToIndex(n + 1, m)] == l - 1)
							{
								num2 += 1f;
								black += 0.125f * (Color)pixels[XYToIndex(n + 1, m)];
							}
							if (m > 0 && array[XYToIndex(n, m - 1)] == l - 1)
							{
								num2 += 1f;
								black += 0.125f * (Color)pixels[XYToIndex(n, m - 1)];
							}
							if (m < this._height - 1 && array[XYToIndex(n, m + 1)] == l - 1)
							{
								num2 += 1f;
								black += 0.125f * (Color)pixels[XYToIndex(n, m + 1)];
							}
							black *= 8f / num2;
							black.a = 0f;
							pixels[XYToIndex(n, m)] = black;
						}
					}
				}
			}
		}
		_data = new byte[pixels.Length * 4];
		int num3 = pixels.Length;
		Mathf.Max(num3 / 200, 1);
		for (int num4 = 0; num4 < num3; num4++)
		{
			_data[4 * num4] = pixels[num4].b;
			_data[4 * num4 + 1] = pixels[num4].g;
			_data[4 * num4 + 2] = pixels[num4].r;
			_data[4 * num4 + 3] = pixels[num4].a;
		}
	}

	public bool Save(string path)
	{
		try
		{
			Stream stream = SdFile.Open(path, FileMode.Create, FileAccess.Write, FileShare.Read);
			int width = _texture.width;
			int height = _texture.height;
			header[12] = (byte)(width % 256);
			header[13] = (byte)(width / 256);
			header[14] = (byte)(height % 256);
			header[15] = (byte)(height / 256);
			stream.Write(header, 0, header.Length);
			stream.Write(_data, 0, _data.Length);
			stream.Write(footer, 0, footer.Length);
			stream.Close();
			return true;
		}
		catch (Exception e)
		{
			Log.Exception(e);
		}
		return false;
	}

	public bool Save2(string path)
	{
		try
		{
			Stream stream = SdFile.Open(path, FileMode.Create, FileAccess.Write, FileShare.Read);
			header[12] = (byte)(gwidth % 256);
			header[13] = (byte)(gwidth / 256);
			header[14] = (byte)(gheight % 256);
			header[15] = (byte)(gheight / 256);
			stream.Write(header, 0, header.Length);
			stream.Write(_data, 0, _data.Length);
			stream.Write(footer, 0, footer.Length);
			stream.Close();
			return true;
		}
		catch (Exception e)
		{
			Log.Exception(e);
		}
		return false;
	}
}
