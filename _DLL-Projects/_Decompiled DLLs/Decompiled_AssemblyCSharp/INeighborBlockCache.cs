public interface INeighborBlockCache
{
	void Init(int _bX, int _bZ);

	void Clear();

	IChunk GetChunk(int x, int z);

	IChunk GetNeighborChunk(int x, int z);

	BlockValue Get(int relx, int absy, int relz);

	byte GetStab(int relx, int absy, int relz);

	bool IsWater(int relx, int absy, int relz);

	bool IsAir(int relx, int absy, int relz);
}
