using System.IO;

public class PowerGenerator : PowerSource
{
	public ushort CurrentFuel;

	public ushort MaxFuel;

	public float OutputPerFuel;

	public override PowerItemTypes PowerItemType => PowerItemTypes.Generator;

	public override string OnSound => "generator_start";

	public override string OffSound => "generator_stop";

	public override void read(BinaryReader _br, byte _version)
	{
		base.read(_br, _version);
		CurrentFuel = _br.ReadUInt16();
	}

	public override void write(BinaryWriter _bw)
	{
		base.write(_bw);
		_bw.Write(CurrentFuel);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool ShouldAutoTurnOff()
	{
		return CurrentFuel <= 0;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void TickPowerGeneration()
	{
		if ((float)(MaxPower - CurrentPower) >= OutputPerFuel && CurrentFuel > 0)
		{
			CurrentFuel--;
			CurrentPower += (ushort)OutputPerFuel;
		}
	}

	public override void SetValuesFromBlock()
	{
		base.SetValuesFromBlock();
		Block block = Block.list[BlockID];
		if (block.Properties.Values.ContainsKey("MaxPower"))
		{
			MaxPower = ushort.Parse(block.Properties.Values["MaxPower"]);
		}
		if (block.Properties.Values.ContainsKey("MaxFuel"))
		{
			MaxFuel = ushort.Parse(block.Properties.Values["MaxFuel"]);
		}
		else
		{
			MaxFuel = 1000;
		}
		if (block.Properties.Values.ContainsKey("OutputPerFuel"))
		{
			OutputPerFuel = StringParsers.ParseFloat(block.Properties.Values["OutputPerFuel"]);
		}
		else
		{
			OutputPerFuel = 100f;
		}
	}
}
