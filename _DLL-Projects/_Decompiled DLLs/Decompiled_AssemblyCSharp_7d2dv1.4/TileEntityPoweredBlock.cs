public class TileEntityPoweredBlock(Chunk _chunk) : TileEntityPowered(_chunk)
{
	[PublicizedFrom(EAccessModifier.Private)]
	public bool isToggled = true;

	public float DelayTime;

	[PublicizedFrom(EAccessModifier.Protected)]
	public float updateTime;

	public bool IsToggled
	{
		get
		{
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer && PowerItem is PowerConsumerToggle)
			{
				return (PowerItem as PowerConsumerToggle).IsToggled;
			}
			return isToggled;
		}
		set
		{
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				if (PowerItem is PowerConsumerToggle)
				{
					(PowerItem as PowerConsumerToggle).IsToggled = value;
				}
				isToggled = value;
				SetModified();
			}
			else
			{
				isToggled = value;
				SetModified();
			}
		}
	}

	public override int PowerUsed
	{
		get
		{
			if (IsToggled)
			{
				return base.PowerUsed;
			}
			return 0;
		}
	}

	public override void UpdateTick(World world)
	{
		base.UpdateTick(world);
	}

	public override bool Activate(bool activated)
	{
		World world = GameManager.Instance.World;
		BlockValue block = chunk.GetBlock(base.localChunkPos);
		return block.Block.ActivateBlock(world, GetClrIdx(), ToWorldPos(), block, activated, activated);
	}

	public override bool ActivateOnce()
	{
		World world = GameManager.Instance.World;
		BlockValue block = chunk.GetBlock(base.localChunkPos);
		return block.Block.ActivateBlockOnce(world, GetClrIdx(), ToWorldPos(), block);
	}

	public override void OnRemove(World world)
	{
		base.OnRemove(world);
		if (PowerManager.Instance.ClientUpdateList.Contains(this))
		{
			PowerManager.Instance.ClientUpdateList.Remove(this);
		}
	}

	public override void OnUnload(World world)
	{
		base.OnUnload(world);
		if (PowerManager.Instance.ClientUpdateList.Contains(this))
		{
			PowerManager.Instance.ClientUpdateList.Remove(this);
		}
	}

	public override void OnSetLocalChunkPosition()
	{
		base.OnSetLocalChunkPosition();
		if (GameManager.Instance == null)
		{
			return;
		}
		World world = GameManager.Instance.World;
		if (chunk != null)
		{
			BlockValue block = chunk.GetBlock(base.localChunkPos);
			Block block2 = block.Block;
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				block2.ActivateBlock(world, GetClrIdx(), ToWorldPos(), block, base.IsPowered, base.IsPowered);
			}
		}
	}

	public virtual void ClientUpdate()
	{
	}

	public override void read(PooledBinaryReader _br, StreamModeRead _eStreamMode)
	{
		base.read(_br, _eStreamMode);
		switch (_eStreamMode)
		{
		case StreamModeRead.FromClient:
			isToggled = _br.ReadBoolean();
			if (PowerItem is PowerConsumerToggle)
			{
				(PowerItem as PowerConsumerToggle).IsToggled = isToggled;
			}
			break;
		default:
			isToggled = _br.ReadBoolean();
			break;
		case StreamModeRead.Persistency:
			break;
		}
	}

	public override void write(PooledBinaryWriter _bw, StreamModeWrite _eStreamMode)
	{
		base.write(_bw, _eStreamMode);
		if (_eStreamMode != StreamModeWrite.Persistency)
		{
			_bw.Write(IsToggled);
		}
	}
}
