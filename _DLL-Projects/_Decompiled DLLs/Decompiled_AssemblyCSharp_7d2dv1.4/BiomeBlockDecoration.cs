public class BiomeBlockDecoration
{
	public string blockName;

	public float prob;

	public float clusterProb;

	public BlockValue blockValue;

	public int randomRotateMax;

	public int checkResourceOffsetY;

	public BiomeBlockDecoration(string _name, float _prob, float _clusprob, BlockValue _blockValue, int _randomRotateMax, int _checkResource = int.MaxValue)
	{
		blockName = _name;
		prob = _prob;
		clusterProb = _clusprob;
		blockValue = _blockValue;
		randomRotateMax = _randomRotateMax;
		checkResourceOffsetY = _checkResource;
	}

	public static byte GetRandomRotation(float _rnd, int _randomRotateMax)
	{
		byte b = (byte)(_rnd * (float)_randomRotateMax + 0.5f);
		if (b >= 4 && b <= 7)
		{
			b = (byte)(b - 4 + 24);
		}
		return b;
	}
}
