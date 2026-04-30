using System.Collections.Generic;

public class BlockMaskRules<O, M>
{
	[PublicizedFrom(EAccessModifier.Private)]
	public List<BlockRule<O, M>> blockRules;

	[PublicizedFrom(EAccessModifier.Private)]
	public M wildcard;

	public BlockMaskRules(M _wildcard)
	{
		blockRules = new List<BlockRule<O, M>>();
		wildcard = _wildcard;
	}

	public void AddRule(BlockRule<O, M> blockRule)
	{
		if (!blockRules.Contains(blockRule))
		{
			blockRules.Add(blockRule);
			RotateLocalRangeYAxis(blockRule);
		}
	}

	public O GetOutput(M[] mask)
	{
		for (int i = 0; i < blockRules.Count; i++)
		{
			BlockRule<O, M> blockRule = blockRules[i];
			M[] mask2 = blockRule.Mask;
			bool flag = true;
			for (byte b = 0; b < mask2.Length; b++)
			{
				ref readonly M reference = ref mask2[b];
				object obj = wildcard;
				if (!reference.Equals(obj))
				{
					ref readonly M reference2 = ref mask2[b];
					object obj2 = mask[b];
					if (!reference2.Equals(obj2))
					{
						flag = false;
						break;
					}
				}
			}
			if (flag)
			{
				return blockRule.Output;
			}
		}
		return default(O);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void RotateLocalRangeYAxis(BlockRule<O, M> blockRule)
	{
		M[] mask = blockRule.Mask;
		List<M> list = new List<M>
		{
			mask[0],
			mask[1],
			mask[2],
			mask[3],
			mask[4],
			mask[5],
			mask[6],
			mask[7],
			mask[8],
			mask[9],
			mask[10],
			mask[11],
			mask[12],
			mask[13],
			mask[14],
			mask[15],
			mask[16],
			mask[17],
			mask[18],
			mask[19],
			mask[20],
			mask[21],
			mask[22],
			mask[23],
			mask[24],
			mask[25],
			mask[26]
		};
		for (int i = 0; i < 3; i++)
		{
			list = new List<M>
			{
				list[6],
				list[3],
				list[0],
				list[7],
				list[4],
				list[1],
				list[8],
				list[5],
				list[2],
				list[15],
				list[12],
				list[9],
				list[16],
				list[13],
				list[10],
				list[17],
				list[14],
				list[11],
				list[24],
				list[21],
				list[18],
				list[25],
				list[22],
				list[19],
				list[26],
				list[23],
				list[20]
			};
			BlockRule<O, M> blockRule2 = new BlockRule<O, M>();
			blockRule2.Output = blockRule.Output;
			blockRule2.Mask = list.ToArray();
			if (!blockRules.Contains(blockRule2))
			{
				blockRules.Add(blockRule2);
			}
		}
	}
}
