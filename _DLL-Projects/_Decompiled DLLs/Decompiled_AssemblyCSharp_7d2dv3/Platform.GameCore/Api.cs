using Platform.XBL;

namespace Platform.GameCore;

public class Api : XblPlatformApi
{
	[PublicizedFrom(EAccessModifier.Private)]
	public string scid;

	public override string SCID => scid;

	public override void Init(IPlatform owner)
	{
		Log.Out("[XBL] API Startup. SCID: " + SCID);
	}

	public override bool InitServerApis()
	{
		return false;
	}

	public override void ServerApiLoaded()
	{
	}
}
