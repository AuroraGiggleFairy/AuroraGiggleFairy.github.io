using Audio;

public class PowerConsumer : PowerItem
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public string StartSound = "";

	[PublicizedFrom(EAccessModifier.Protected)]
	public string EndSound = "";

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool lastActivate;

	public override void HandlePowerUpdate(bool isOn)
	{
		bool flag = isPowered && isOn;
		if (TileEntity != null)
		{
			TileEntity.Activate(flag);
			if (flag && lastActivate != flag)
			{
				TileEntity.ActivateOnce();
			}
			TileEntity.SetModified();
		}
		lastActivate = flag;
		if (PowerChildren())
		{
			for (int i = 0; i < Children.Count; i++)
			{
				Children[i].HandlePowerUpdate(isOn);
			}
		}
	}

	public override void SetValuesFromBlock()
	{
		base.SetValuesFromBlock();
		Block block = Block.list[BlockID];
		if (block.Properties.Values.ContainsKey("RequiredPower"))
		{
			RequiredPower = ushort.Parse(block.Properties.Values["RequiredPower"]);
		}
		if (block.Properties.Values.ContainsKey("StartSound"))
		{
			StartSound = block.Properties.Values["StartSound"];
		}
		if (block.Properties.Values.ContainsKey("EndSound"))
		{
			EndSound = block.Properties.Values["EndSound"];
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void IsPoweredChanged(bool newPowered)
	{
		Manager.BroadcastPlay(Position.ToVector3(), newPowered ? StartSound : EndSound);
	}
}
