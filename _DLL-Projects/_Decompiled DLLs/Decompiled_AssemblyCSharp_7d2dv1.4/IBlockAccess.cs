public interface IBlockAccess
{
	BlockValue GetBlock(int x, int y, int z);

	BlockValue GetBlock(Vector3i _pos);
}
