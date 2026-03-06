using Audio;
using UnityEngine.Scripting;

[Preserve]
public class BlockSpeakerTrader : Block
{
	[PublicizedFrom(EAccessModifier.Private)]
	public string openSound;

	[PublicizedFrom(EAccessModifier.Private)]
	public string closeSound;

	[PublicizedFrom(EAccessModifier.Private)]
	public string warningSound;

	public override void Init()
	{
		base.Init();
		if (base.Properties.Values.ContainsKey("OpenSound"))
		{
			openSound = base.Properties.Values["OpenSound"];
		}
		if (base.Properties.Values.ContainsKey("CloseSound"))
		{
			closeSound = base.Properties.Values["CloseSound"];
		}
		if (base.Properties.Values.ContainsKey("WarningSound"))
		{
			warningSound = base.Properties.Values["WarningSound"];
		}
	}

	public void PlayOpen(Vector3i _blockPos, EntityTrader _trader)
	{
		string text = openSound;
		if (string.IsNullOrEmpty(text))
		{
			text = ((_trader != null) ? (_trader.NPCInfo.VoiceSet + "_announce_open") : "");
		}
		if (text != "")
		{
			Manager.BroadcastPlay(_blockPos.ToVector3(), text);
		}
	}

	public void PlayClose(Vector3i _blockPos, EntityTrader _trader)
	{
		string text = closeSound;
		if (string.IsNullOrEmpty(text))
		{
			text = ((_trader != null) ? (_trader.NPCInfo.VoiceSet + "_announce_closed") : "");
		}
		if (text != "")
		{
			Manager.BroadcastPlay(_blockPos.ToVector3(), text);
		}
	}

	public void PlayWarning(Vector3i _blockPos, EntityTrader _trader)
	{
		string text = warningSound;
		if (string.IsNullOrEmpty(text))
		{
			text = ((_trader != null) ? (_trader.NPCInfo.VoiceSet + "_announce_closing") : "");
		}
		if (text != "")
		{
			Manager.BroadcastPlay(_blockPos.ToVector3(), text);
		}
	}
}
