public class NetPackageScreamerAlertClientHello : NetPackage
{
    private int _entityId = -1;
    private string _userCombined = string.Empty;

    public NetPackageScreamerAlertClientHello Setup(int entityId, string userCombined)
    {
        _entityId = entityId;
        _userCombined = userCombined ?? string.Empty;
        return this;
    }

    public override void read(PooledBinaryReader reader)
    {
        _entityId = reader.ReadInt32();
        _userCombined = reader.ReadString();
    }

    public override void write(PooledBinaryWriter writer)
    {
        base.write(writer);
        writer.Write(_entityId);
        writer.Write(_userCombined ?? string.Empty);
    }

    public override void ProcessPackage(World world, GameManager callbacks)
    {
        _ = world;
        _ = callbacks;
        ConnectionManager manager = SingletonMonoBehaviour<ConnectionManager>.Instance;
        if (manager == null || !manager.IsServer)
        {
            return;
        }

        ScreamerAlertHybridRouting.MarkClientCapability(_entityId, _userCombined);
    }

    public override int GetLength()
    {
        return 8 + (_userCombined?.Length ?? 0) * 2;
    }
}
