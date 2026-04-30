using System;
using UnityEngine;

public class BlockSwitchSingleController : MonoBehaviour
{
	public GameObject ItemPrefab;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool activated;

	public bool Activated
	{
		get
		{
			return activated;
		}
		set
		{
			activated = value;
			SetState();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Start()
	{
		SetState();
	}

	public void SetState(bool _activated)
	{
		activated = _activated;
		SetState();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void SetState()
	{
		if (ItemPrefab != null)
		{
			ItemPrefab.SetActive(!activated);
		}
	}
}
