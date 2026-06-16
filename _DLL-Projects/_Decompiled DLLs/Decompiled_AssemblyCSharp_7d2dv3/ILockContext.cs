public interface ILockContext
{
	void Write(PooledBinaryWriter _bw);

	void Read(PooledBinaryReader _br);
}
