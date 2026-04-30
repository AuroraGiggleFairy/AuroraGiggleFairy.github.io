using UnityEngine.Scripting;

[Preserve]
public class AIDirectorPlayerState : IMemoryPoolableObject
{
	public const float kCheckUndergroundTime = 5f;

	public const int kNumBlocksUnderground = 10;

	public EntityPlayer Player;

	[PublicizedFrom(EAccessModifier.Private)]
	public AIDirectorPlayerInventory m_inventory;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool m_dead;

	public AIDirectorPlayerInventory Inventory
	{
		get
		{
			return m_inventory;
		}
		set
		{
			m_inventory = value;
		}
	}

	public bool Dead
	{
		get
		{
			return m_dead;
		}
		set
		{
			m_dead = value;
		}
	}

	public AIDirectorPlayerState Construct(EntityPlayer _player)
	{
		Player = _player;
		m_dead = false;
		return this;
	}

	public void Reset()
	{
		Player = null;
	}

	public void Cleanup()
	{
	}
}
