using System.Collections.Generic;

public class BiomeLayer
{
	public BiomeBlockDecoration m_Block;

	public int m_Depth;

	public List<BiomeBlockDecoration> m_Resources;

	public List<float> SumResourceProbs;

	public float MaxResourceProb;

	public BiomeLayer(int _depth, BiomeBlockDecoration _bb)
	{
		m_Block = _bb;
		m_Depth = _depth;
		m_Resources = new List<BiomeBlockDecoration>();
		SumResourceProbs = new List<float>();
		MaxResourceProb = 0f;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	~BiomeLayer()
	{
	}

	public void AddResource(BiomeBlockDecoration _res)
	{
		m_Resources.Add(_res);
		MaxResourceProb = Utils.FastMax(_res.prob, MaxResourceProb);
		int count = SumResourceProbs.Count;
		SumResourceProbs.Add((count > 0) ? (SumResourceProbs[count - 1] + _res.prob) : _res.prob);
	}
}
