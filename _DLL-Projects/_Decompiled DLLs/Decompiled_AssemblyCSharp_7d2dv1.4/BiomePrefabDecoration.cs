public class BiomePrefabDecoration
{
	public string prefabName;

	public float prob;

	public int checkResourceOffsetY;

	public bool isDecorateOnSlopes;

	public BiomePrefabDecoration(string _prefabName, float _prob, bool _isDecorateOnSlopes, int _checkResource = int.MaxValue)
	{
		prefabName = _prefabName;
		prob = _prob;
		checkResourceOffsetY = _checkResource;
		isDecorateOnSlopes = _isDecorateOnSlopes;
	}
}
