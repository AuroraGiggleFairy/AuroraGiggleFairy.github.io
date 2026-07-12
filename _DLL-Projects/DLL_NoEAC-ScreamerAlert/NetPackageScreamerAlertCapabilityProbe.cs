using System;
using System.Reflection;
using HarmonyLib;

public class NetPackageScreamerAlertCapabilityProbe : NetPackage
{
    private int _nonce;

    public NetPackageScreamerAlertCapabilityProbe Setup(int nonce)
    {
        _nonce = nonce;
        return this;
    }

    public override void read(PooledBinaryReader reader)
    {
        _nonce = reader.ReadInt32();
    }

    public override void write(PooledBinaryWriter writer)
    {
        base.write(writer);
        writer.Write(_nonce);
    }

    public override void ProcessPackage(World world, GameManager callbacks)
    {
        _ = world;
        _ = callbacks;

        ConnectionManager manager = SingletonMonoBehaviour<ConnectionManager>.Instance;
        if (manager == null || manager.IsServer)
        {
            return;
        }

        int localEntityId = GameManager.Instance?.World?.GetPrimaryPlayer()?.entityId ?? -1;
        if (localEntityId < 0)
        {
            return;
        }

        TryInvokeEnhancedProbeResponder(localEntityId, _nonce);
    }

    public override int GetLength()
    {
        return 4;
    }

    private static void TryInvokeEnhancedProbeResponder(int entityId, int nonce)
    {
        try
        {
            Type helloType = AccessTools.TypeByName("ScreamerAlertEnhancedCapabilityHello");
            if (helloType == null)
            {
                return;
            }

            MethodInfo fromProbe = helloType.GetMethod(
                "TrySendFromProbe",
                BindingFlags.Public | BindingFlags.Static,
                null,
                new[] { typeof(int), typeof(int) },
                null);
            if (fromProbe != null)
            {
                fromProbe.Invoke(null, new object[] { entityId, nonce });
                return;
            }

            MethodInfo fallback = helloType.GetMethod(
                "TrySendFromCommand",
                BindingFlags.Public | BindingFlags.Static,
                null,
                new[] { typeof(int) },
                null);
            fallback?.Invoke(null, new object[] { entityId });
        }
        catch (Exception)
        {
        }
    }
}
