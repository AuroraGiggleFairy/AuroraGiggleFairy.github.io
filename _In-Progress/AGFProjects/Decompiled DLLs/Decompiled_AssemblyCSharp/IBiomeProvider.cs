using System.Collections;

public interface IBiomeProvider
{
	IEnumerator InitData();

	void Init(int _seed, string _worldName, WorldBiomes _biomes, string _params1, string _params2);

	int GetSubBiomeIdxAt(BiomeDefinition bd, int _x, int _y, int _z);

	BiomeDefinition GetBiomeAt(int _x, int _z);

	BiomeDefinition GetBiomeAt(int _x, int _z, out float _intensity);

	BiomeDefinition GetBiomeOrSubAt(int x, int z)
	{
		return null;
	}

	float GetHumidityAt(int x, int z);

	float GetTemperatureAt(int x, int z);

	float GetRadiationAt(int x, int z);

	string GetWorldName();

	BlockValue GetTopmostBlockValue(int xWorld, int zWorld);

	void Cleanup()
	{
	}
}
