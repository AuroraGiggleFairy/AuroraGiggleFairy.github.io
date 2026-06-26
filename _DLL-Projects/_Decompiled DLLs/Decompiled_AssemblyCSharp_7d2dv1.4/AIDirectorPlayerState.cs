using System;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class AIDirectorPlayerState : IMemoryPoolableObject
{
	public const double kSmellEmitTime = 1.0;

	public const float kCheckUndergroundTime = 5f;

	public const int kNumBlocksUnderground = 10;

	public EntityPlayer Player;

	[PublicizedFrom(EAccessModifier.Private)]
	public AIDirectorPlayerInventory m_inventory;

	[PublicizedFrom(EAccessModifier.Private)]
	public double m_smellEmitTime;

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
			if (m_dead && !value)
			{
				m_smellEmitTime = 2.0;
			}
			m_dead = value;
		}
	}

	public AIDirectorPlayerState Construct(EntityPlayer _player)
	{
		Player = _player;
		m_smellEmitTime = 1.0;
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

	public void EmitSmell(double dt)
	{
		m_smellEmitTime -= dt;
		if (m_smellEmitTime <= 0.0)
		{
			UpdateSmell();
			m_smellEmitTime += 1.0;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateSmell()
	{
		float num = 0f;
		if (m_inventory.bag != null)
		{
			for (int i = 0; i < m_inventory.bag.Count; i++)
			{
				ItemClass forId = ItemClass.GetForId(m_inventory.bag[i].id);
				if (forId != null && forId.Smell != null)
				{
					num = Math.Max(forId.Smell.range, num);
				}
			}
		}
		if (m_inventory.belt != null)
		{
			for (int j = 0; j < m_inventory.belt.Count; j++)
			{
				ItemClass forId2 = ItemClass.GetForId(m_inventory.belt[j].id);
				if (forId2 != null && forId2.Smell != null)
				{
					num = Math.Max(forId2.Smell.beltRange, num);
				}
			}
		}
		Player.Stealth.smell = Mathf.FloorToInt(num);
	}
}
