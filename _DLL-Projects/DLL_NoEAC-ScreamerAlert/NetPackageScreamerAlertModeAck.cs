public class NetPackageScreamerAlertModeAck : NetPackage
{
    private int _entityId = -1;
    private int _effectiveMode = (int)ScreamerAlertMode.On;
    private bool _countAvailable;

    public NetPackageScreamerAlertModeAck Setup(int entityId, ScreamerAlertMode effectiveMode, bool countAvailable)
    {
        _entityId = entityId;
        _effectiveMode = (int)effectiveMode;
        _countAvailable = countAvailable;
        return this;
    }

    public override void read(PooledBinaryReader reader)
    {
        _entityId = reader.ReadInt32();
        _effectiveMode = reader.ReadInt32();
        _countAvailable = reader.ReadBoolean();
    }

    public override void write(PooledBinaryWriter writer)
    {
        base.write(writer);
        writer.Write(_entityId);
        writer.Write(_effectiveMode);
        writer.Write(_countAvailable);
    }

    public override void ProcessPackage(World world, GameManager callbacks)
    {
        _ = world;
        _ = callbacks;

        if (GameManager.IsDedicatedServer)
        {
            return;
        }

        ScreamerAlertMode mode = ParseMode(_effectiveMode);
        if (!_countAvailable && mode == ScreamerAlertMode.OnWithNumbers)
        {
            mode = ScreamerAlertMode.On;
        }

        XUiC_ScreamerAlertOptions.OnAuthoritativeModeAck(mode, _countAvailable);
    }

    public override int GetLength()
    {
        return 4 + 4 + 1;
    }

    private static ScreamerAlertMode ParseMode(int value)
    {
        switch (value)
        {
            case 0:
                return ScreamerAlertMode.Off;
            case 1:
                return ScreamerAlertMode.On;
            case 2:
                return ScreamerAlertMode.OnWithNumbers;
            default:
                return ScreamerAlertMode.On;
        }
    }
}
