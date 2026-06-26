using System.Collections.Generic;

public class PoiMapElement
{
	public uint m_uColorId;

	public string m_sModelName;

	public BlockValue m_BlockValue;

	public BlockValue m_BlockBelow;

	public int m_YPos;

	public int m_YPosFill;

	public List<PoiMapDecal> decals = new List<PoiMapDecal>();

	public List<PoiMapBlock> blocksOnTop = new List<PoiMapBlock>();

	public PoiMapElement(uint _color, string _name, BlockValue _blockValue, BlockValue _blockBelow, int _iSO, int _ypos, int _yposFill, int _iST)
	{
		m_sModelName = _name;
		m_uColorId = _color;
		m_YPos = _ypos;
		m_YPosFill = _yposFill;
		m_BlockValue = _blockValue;
		m_BlockBelow = _blockBelow;
	}

	public PoiMapDecal GetDecal(int _index)
	{
		if (_index >= 0 && _index < decals.Count)
		{
			return decals[_index];
		}
		return null;
	}

	public PoiMapDecal GetRandomDecal(GameRandom _random)
	{
		for (int i = 0; i < decals.Count; i++)
		{
			PoiMapDecal poiMapDecal = decals[i];
			if (_random.RandomFloat < poiMapDecal.m_Prob)
			{
				return poiMapDecal;
			}
		}
		return null;
	}

	public PoiMapBlock GetRandomBlockOnTop(GameRandom _random)
	{
		for (int i = 0; i < blocksOnTop.Count; i++)
		{
			PoiMapBlock poiMapBlock = blocksOnTop[i];
			if (_random.RandomFloat < poiMapBlock.m_Prob)
			{
				return poiMapBlock;
			}
		}
		return null;
	}
}
