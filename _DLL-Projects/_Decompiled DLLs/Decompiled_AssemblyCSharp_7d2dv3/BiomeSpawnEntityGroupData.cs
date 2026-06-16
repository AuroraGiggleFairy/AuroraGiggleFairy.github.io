public class BiomeSpawnEntityGroupData
{
	public int idHash;

	public string entityGroupName;

	public int maxCount;

	public int[] respawnDelayInWorldTime;

	public EDaytime daytime;

	public FastTags<TagGroup.Poi> POITags;

	public FastTags<TagGroup.Poi> noPOITags;

	public bool isAnimal;

	public BiomeSpawnEntityGroupData(int _idHash, int _maxCount, int[] _respawndelay, EDaytime _daytime, bool _isAnimal)
	{
		idHash = _idHash;
		maxCount = _maxCount;
		daytime = _daytime;
		respawnDelayInWorldTime = _respawndelay;
		isAnimal = _isAnimal;
	}
}
