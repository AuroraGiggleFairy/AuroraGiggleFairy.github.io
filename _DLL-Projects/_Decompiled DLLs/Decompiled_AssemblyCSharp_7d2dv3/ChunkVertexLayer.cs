using UnityEngine;

public class ChunkVertexLayer : IMemoryPoolableObject
{
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3[] m_Vertices;

	[PublicizedFrom(EAccessModifier.Private)]
	public float[] yPos;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool[] valid;

	[PublicizedFrom(EAccessModifier.Private)]
	public int wPow;

	[PublicizedFrom(EAccessModifier.Private)]
	public int hPow;

	public static int InstanceCount;

	public ChunkVertexLayer()
	{
		wPow = 4;
		hPow = 4;
		int num = 1 << wPow;
		int num2 = 1 << hPow;
		m_Vertices = new Vector3[num * num2];
		yPos = new float[num * num2];
		valid = new bool[num * num2];
	}

	public void Reset()
	{
		for (int i = 0; i < valid.Length; i++)
		{
			m_Vertices[i] = Vector3.zero;
			yPos[i] = 0f;
			valid[i] = false;
		}
	}

	public void Cleanup()
	{
	}

	public bool getAt(int _x, int _y, out Vector3 _vec)
	{
		int offs = _x + (_y << wPow);
		return getAt(offs, out _vec);
	}

	public bool getAt(int _offs, out Vector3 _vec)
	{
		_vec = m_Vertices[_offs];
		return valid[_offs];
	}

	public void setAt(int _x, int _y, Vector3 _v)
	{
		int num = _x + (_y << wPow);
		m_Vertices[num] = _v;
		valid[num] = true;
	}

	public bool getYPosAt(int _x, int _y, out float _ypos)
	{
		int offs = _x + (_y << wPow);
		return getYPosAt(offs, out _ypos);
	}

	public bool getYPosAt(int _offs, out float _ypos)
	{
		_ypos = yPos[_offs];
		return valid[_offs];
	}

	public void setYPosAt(int _x, int _y, float _ypos)
	{
		int num = _x + (_y << wPow);
		yPos[num] = _ypos;
		valid[num] = true;
	}

	public void setInvalid(int _offs)
	{
		valid[_offs] = false;
	}

	public int GetUsedMem()
	{
		return m_Vertices.Length * 13 + 8;
	}
}
