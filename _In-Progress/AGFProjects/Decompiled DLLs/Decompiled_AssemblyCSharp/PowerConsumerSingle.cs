public class PowerConsumerSingle : PowerItem
{
	[PublicizedFrom(EAccessModifier.Private)]
	public bool lastActivate;

	public override PowerItemTypes PowerItemType => PowerItemTypes.ConsumerToggle;

	public override void HandlePowerUpdate(bool isOn)
	{
		bool flag = isPowered;
		if (flag && lastActivate != flag && TileEntity != null)
		{
			TileEntity.ActivateOnce();
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
	}
}
