namespace Platform.GameCore.Save.MasterFileTable;

public interface IMigratable
{
	ushort Version { get; }

	void Write(PooledBinaryWriter writer);

	void Read(PooledBinaryReader reader);
}
