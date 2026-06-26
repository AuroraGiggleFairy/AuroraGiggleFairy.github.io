using UnityEngine.Scripting;

[Preserve]
public class NetPackageBloodmoonMusic : NetPackage
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public bool IsBloodMoonMusicEligible;

	public override NetPackageDirection PackageDirection => NetPackageDirection.ToClient;

	public NetPackageBloodmoonMusic Setup(bool _isBloodmoonMusicEligible)
	{
		IsBloodMoonMusicEligible = _isBloodmoonMusicEligible;
		return this;
	}

	public override void read(PooledBinaryReader _reader)
	{
		IsBloodMoonMusicEligible = _reader.ReadBoolean();
	}

	public override void write(PooledBinaryWriter _writer)
	{
		base.write(_writer);
		_writer.Write(IsBloodMoonMusicEligible);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (GameManager.Instance.World != null && GameManager.Instance.World.dmsConductor != null)
		{
			GameManager.Instance.World.dmsConductor.IsBloodmoonMusicEligible = IsBloodMoonMusicEligible;
		}
	}

	public override int GetLength()
	{
		return 1;
	}
}
