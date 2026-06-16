using System;

public class WorldBlockFiller
{
	[PublicizedFrom(EAccessModifier.Private)]
	public int m_iMaxX;

	[PublicizedFrom(EAccessModifier.Private)]
	public int m_iMinX;

	[PublicizedFrom(EAccessModifier.Private)]
	public int m_iMaxY;

	[PublicizedFrom(EAccessModifier.Private)]
	public int m_iMinY;

	[PublicizedFrom(EAccessModifier.Private)]
	public int m_iMaxZ;

	[PublicizedFrom(EAccessModifier.Private)]
	public int m_iMinZ;

	[PublicizedFrom(EAccessModifier.Private)]
	public byte[] m_BlocksToFill;

	[PublicizedFrom(EAccessModifier.Private)]
	public GameRandom m_RandomGenerator;

	[PublicizedFrom(EAccessModifier.Private)]
	public int m_iChunkDimension;

	[PublicizedFrom(EAccessModifier.Private)]
	public int m_iFillCount;

	[PublicizedFrom(EAccessModifier.Private)]
	public int m_iAreaCount;

	[PublicizedFrom(EAccessModifier.Private)]
	public int m_iThisBiomeColorId;

	[PublicizedFrom(EAccessModifier.Private)]
	public WorldBiomes m_GenRules;

	public WorldBlockFiller(int iBiomeColorId, WorldBiomeProviderFromImage _biomeProvider, GameRandom _rand, WorldBiomes _rules)
	{
		m_RandomGenerator = _rand;
		m_iChunkDimension = 65536;
		m_BlocksToFill = new byte[m_iChunkDimension];
		m_iThisBiomeColorId = iBiomeColorId;
		m_GenRules = _rules;
	}

	public void resetBlockInformation()
	{
		for (int i = 0; i < m_iChunkDimension; i++)
		{
			m_BlocksToFill[i] = byte.MaxValue;
		}
		m_iMaxX = 0;
		m_iMinX = 16;
		m_iMaxY = 0;
		m_iMinY = 256;
		m_iMaxZ = 0;
		m_iMinZ = 16;
		m_iFillCount = 0;
		m_iAreaCount = 0;
	}

	public void setBlockToFill(int x, int y, int z, byte top)
	{
		m_iMaxX = ((x > m_iMaxX) ? x : m_iMaxX);
		m_iMaxY = ((x > m_iMaxY) ? x : m_iMaxY);
		m_iMaxZ = ((x > m_iMaxZ) ? x : m_iMaxZ);
		m_iMinX = ((x < m_iMinX) ? x : m_iMinX);
		m_iMinY = ((x < m_iMinY) ? x : m_iMinY);
		m_iMinZ = ((x < m_iMinZ) ? x : m_iMinZ);
		setBlockArrayValue(x, y, z, top);
		m_iFillCount++;
		if (y == 0)
		{
			m_iAreaCount++;
		}
	}

	public void fillChunk(Chunk c)
	{
		if (m_iAreaCount == 0)
		{
			return;
		}
		BiomeDefinition value = null;
		m_GenRules.GetBiomeMap().TryGetValue((uint)m_iThisBiomeColorId, out value);
		if (value != null)
		{
			int iAvailableCount = m_iAreaCount;
			int iLayerDepth = -1;
			for (int i = 0; i < value.m_DecoBlocks.Count; i++)
			{
				BiomeBlockDecoration bb = value.m_DecoBlocks[i];
				fillLevel(c, bb, iLayerDepth, ref iAvailableCount);
			}
			iAvailableCount = m_iAreaCount;
			for (int j = 0; j < value.m_Layers.Count; j++)
			{
				iLayerDepth = value.m_Layers[j].m_Depth;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void fillLevel(Chunk c, BiomeBlockDecoration bb, int iLayerDepth, ref int iAvailableCount)
	{
		double num = bb.prob;
		double probability = bb.clusterProb;
		int num2 = (int)((double)m_iAreaCount * num);
		int num3 = m_RandomGenerator.RandomRange(m_iMinX, m_iMaxX + 1);
		int num4 = m_RandomGenerator.RandomRange(m_iMinZ, m_iMaxZ + 1);
		byte blockArrayValue = getBlockArrayValue(num3, 0, num4);
		if (blockArrayValue == byte.MaxValue)
		{
			return;
		}
		while (iAvailableCount >= 0 && num2 >= 0)
		{
			bool flag = false;
			if (getBlockArrayValue(num3, blockArrayValue + 1, num4) == byte.MaxValue)
			{
				int num5 = 0;
				while (!flag && num5 < 9)
				{
					int num6 = Math.Max(0, num3 - num5);
					while (!flag && num6 < Math.Min(16, num3 + num5))
					{
						int num7 = Math.Max(0, num4 - num5);
						while (!flag && num7 < Math.Min(16, num4 + num5))
						{
							blockArrayValue = getBlockArrayValue(num6, 0, num7);
							if (blockArrayValue != byte.MaxValue)
							{
								flag = true;
								num3 = num6;
								num4 = num7;
							}
							num7++;
						}
						num6++;
					}
					num5++;
				}
			}
			else
			{
				flag = true;
			}
			if (flag)
			{
				int num8 = setDecorationBlock(c, num3, blockArrayValue, iLayerDepth, num4, probability, bb.blockValues[0]);
				iAvailableCount -= num8;
				num2 -= num8;
				do
				{
					num3 = m_RandomGenerator.RandomRange(m_iMinX, m_iMaxX + 1);
					num4 = m_RandomGenerator.RandomRange(m_iMinZ, m_iMaxZ + 1);
					blockArrayValue = getBlockArrayValue(num3, 0, num4);
				}
				while (blockArrayValue == byte.MaxValue);
				continue;
			}
			Log.Error("did not find spot to place decoration");
			break;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int setDecorationBlock(Chunk c, int x, int y, int d, int z, double probability, BlockValue blockValue)
	{
		int num = 1;
		int num2 = ((d >= 0) ? m_RandomGenerator.RandomRange(0, d) : d);
		if (num2 >= y)
		{
			return 0;
		}
		if (probability > 0.0)
		{
			for (int i = -1; i <= 1; i++)
			{
				for (int j = -1; j <= 1; j++)
				{
					for (int k = ((d > 1) ? (-1) : 0); k <= ((d > 1) ? 1 : 0) && y + k - num2 > 0 && y + k - num2 < y; k++)
					{
						if (i + x >= 0 && i + x < 16 && j + z >= 0 && j + z < 16 && m_RandomGenerator.RandomDouble < probability)
						{
							c.SetBlockRaw(i + x, y + k - num2, j + z, blockValue);
							setBlockArrayValue(i + x, y + k - num2, j + z, byte.MaxValue);
							num++;
						}
					}
				}
			}
		}
		c.SetBlockRaw(x, y - num2, z, blockValue);
		setBlockArrayValue(x, y - num2, z, byte.MaxValue);
		return num;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public byte getBlockArrayValue(int x, int y, int z)
	{
		return m_BlocksToFill[((x << 4) + z << 8) + y];
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void setBlockArrayValue(int x, int y, int z, byte value)
	{
		m_BlocksToFill[((x << 4) + z << 8) + y] = value;
	}
}
