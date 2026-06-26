using System.IO;
using UnityEngine;

public class PowerSolarPanel : PowerSource
{
	public ushort InputFromSun;

	[PublicizedFrom(EAccessModifier.Private)]
	public byte sunLight;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool lastHasLight;

	[PublicizedFrom(EAccessModifier.Private)]
	public string runningSound = "solarpanel_idle";

	[PublicizedFrom(EAccessModifier.Private)]
	public float lightUpdateTime;

	[field: PublicizedFrom(EAccessModifier.Private)]
	public bool HasLight
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	public override PowerItemTypes PowerItemType => PowerItemTypes.SolarPanel;

	public override string OnSound => "solarpanel_on";

	public override string OffSound => "solarpanel_off";

	[PublicizedFrom(EAccessModifier.Private)]
	public void CheckLightLevel()
	{
		if (TileEntity != null)
		{
			Chunk chunk = TileEntity.GetChunk();
			Vector3i localChunkPos = TileEntity.localChunkPos;
			sunLight = chunk.GetLight(localChunkPos.x, localChunkPos.y, localChunkPos.z, Chunk.LIGHT_TYPE.SUN);
		}
		lastHasLight = HasLight;
		HasLight = sunLight == 15 && GameManager.Instance.World.IsDaytime();
		if (lastHasLight != HasLight)
		{
			HandleOnOffSound();
			if (!HasLight)
			{
				CurrentPower = 0;
				HandleDisconnect();
			}
			else
			{
				SendHasLocalChangesToRoot();
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void TickPowerGeneration()
	{
		if (HasLight)
		{
			CurrentPower = MaxOutput;
		}
	}

	public override void HandleSendPower()
	{
		if (!base.IsOn)
		{
			return;
		}
		if (Time.time > lightUpdateTime)
		{
			lightUpdateTime = Time.time + 2f;
			CheckLightLevel();
		}
		if (!HasLight)
		{
			return;
		}
		if (CurrentPower < MaxPower)
		{
			TickPowerGeneration();
		}
		else if (CurrentPower > MaxPower)
		{
			CurrentPower = MaxPower;
		}
		if (ShouldAutoTurnOff())
		{
			CurrentPower = 0;
			base.IsOn = false;
		}
		if (hasChangesLocal)
		{
			LastPowerUsed = 0;
			ushort num = (ushort)Mathf.Min(MaxOutput, CurrentPower);
			ushort power = num;
			_ = GameManager.Instance.World;
			for (int i = 0; i < Children.Count; i++)
			{
				num = power;
				Children[i].HandlePowerReceived(ref power);
				LastPowerUsed += (ushort)(num - power);
			}
		}
		if (LastPowerUsed >= CurrentPower)
		{
			SendHasLocalChangesToRoot();
			CurrentPower = 0;
		}
		else
		{
			CurrentPower -= LastPowerUsed;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool ShouldClearPower()
	{
		if (sunLight != 15 || !GameManager.Instance.World.IsDaytime())
		{
			return true;
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void RefreshPowerStats()
	{
		base.RefreshPowerStats();
		MaxPower = MaxOutput;
	}

	public override void read(BinaryReader _br, byte _version)
	{
		base.read(_br, _version);
		if (PowerManager.Instance.CurrentFileVersion >= 2)
		{
			sunLight = _br.ReadByte();
		}
	}

	public override void write(BinaryWriter _bw)
	{
		base.write(_bw);
		_bw.Write(sunLight);
	}
}
