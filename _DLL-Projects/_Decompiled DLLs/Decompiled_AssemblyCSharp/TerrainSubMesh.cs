using System.Collections.Generic;
using UnityEngine;

public struct TerrainSubMesh(List<TerrainSubMesh> _others, int _minSize = 0)
{
	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly Color[] vertexColors = new Color[3]
	{
		new Color(0f, 0f, 0f, 0f),
		new Color(1f, 0f, 0f, 0f),
		new Color(0f, 1f, 0f, 0f)
	};

	public ArrayDynamicFast<int> textureIds = new ArrayDynamicFast<int>(vertexColors.Length);

	public ArrayListMP<int> triangles = new ArrayListMP<int>(MemoryPools.poolInt, _minSize);

	[PublicizedFrom(EAccessModifier.Private)]
	public ArrayListMP<int> needToAdd = new ArrayListMP<int>(MemoryPools.poolInt);

	[PublicizedFrom(EAccessModifier.Private)]
	public List<TerrainSubMesh> others = _others;

	public bool Contains(IList<int> _texIds)
	{
		for (int i = 0; i < _texIds.Count; i++)
		{
			if (_texIds[i] != -1 && textureIds.Contains(_texIds[i]) == -1)
			{
				return false;
			}
		}
		return true;
	}

	public bool CanAdd(IList<int> _texIds)
	{
		needToAdd.Clear();
		for (int i = 0; i < _texIds.Count; i++)
		{
			if (_texIds[i] != -1 && textureIds.Contains(_texIds[i]) == -1)
			{
				needToAdd.Add(_texIds[i]);
			}
		}
		if (needToAdd.Count == 0)
		{
			return true;
		}
		if (needToAdd.Count <= vertexColors.Length - textureIds.Count)
		{
			Add(needToAdd);
			return true;
		}
		return false;
	}

	public void Add(int[] _texIds)
	{
		foreach (int num in _texIds)
		{
			if (num == -1)
			{
				continue;
			}
			int num2 = -1;
			int num3 = 0;
			while (num2 == -1 && num3 < others.Count)
			{
				num2 = others[num3].textureIds.Contains(num);
				if (num2 != -1 && textureIds.DataAvail[num2])
				{
					num2 = -1;
				}
				num3++;
			}
			textureIds.Add(num2, num);
		}
	}

	public void Add(ArrayListMP<int> _texIds)
	{
		for (int i = 0; i < _texIds.Count; i++)
		{
			int num = _texIds[i];
			if (num == -1)
			{
				continue;
			}
			int num2 = -1;
			int num3 = 0;
			while (num2 == -1 && num3 < others.Count)
			{
				num2 = others[num3].textureIds.Contains(num);
				if (num2 != -1 && textureIds.DataAvail[num2])
				{
					num2 = -1;
				}
				num3++;
			}
			textureIds.Add(num2, num);
		}
	}

	public Color GetColorForTextureId(int _texId)
	{
		int num = textureIds.Contains(_texId);
		if (num != -1)
		{
			return vertexColors[num];
		}
		return vertexColors[0];
	}

	public int GetTextureIdCount()
	{
		return textureIds.Count;
	}

	public int GetTextureId(int _idx)
	{
		for (int i = 0; i < textureIds.Size; i++)
		{
			if (textureIds.DataAvail[i] && _idx == 0)
			{
				return textureIds.Data[i];
			}
			_idx--;
		}
		return 0;
	}
}
