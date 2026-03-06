public interface ILightProcessor
{
	void LightChunk(Chunk _chunk);

	void GenerateSunlight(Chunk chunk, bool bSpreadLight);

	void SpreadLight(Chunk c, int blockX, int blockY, int blockZ, byte lightValue, Chunk.LIGHT_TYPE type, bool bSetAtStarterPos = true);

	void UnspreadLight(Chunk c, int x, int y, int z, byte lightValue, Chunk.LIGHT_TYPE type);

	void RefreshSunlightAtLocalPos(Chunk c, int x, int y, bool _isSpread);

	byte RefreshLightAtLocalPos(Chunk c, int x, int y, int z, Chunk.LIGHT_TYPE type);
}
