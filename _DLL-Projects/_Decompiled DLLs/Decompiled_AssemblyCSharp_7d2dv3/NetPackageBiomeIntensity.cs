using UnityEngine.Scripting;

[Preserve]
public class NetPackageBiomeIntensity : NetPackage
{
	[PublicizedFrom(EAccessModifier.Private)]
	public BiomeIntensity bi;

	public NetPackageBiomeIntensity Setup(BiomeIntensity _bi)
	{
		bi = _bi;
		return this;
	}

	public override void read(PooledBinaryReader _reader)
	{
		bi = BiomeIntensity.Default;
		bi.Read(_reader);
	}

	public override void write(PooledBinaryWriter _writer)
	{
		base.write(_writer);
		bi.Write(_writer);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (_world != null)
		{
			_world.LocalPlayerBiomeIntensityStandingOn = bi;
		}
	}

	public override int GetLength()
	{
		return 8;
	}
}
