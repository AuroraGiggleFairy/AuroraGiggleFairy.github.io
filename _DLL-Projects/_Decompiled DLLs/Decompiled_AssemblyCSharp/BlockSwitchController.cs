using System;
using UnityEngine;

public class BlockSwitchController : MonoBehaviour
{
	public GameObject RedLight;

	public GameObject GreenLight;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool powered;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool activated;

	public bool Powered
	{
		get
		{
			return powered;
		}
		set
		{
			powered = value;
			UpdateLights();
		}
	}

	public bool Activated
	{
		get
		{
			return activated;
		}
		set
		{
			activated = value;
			UpdateLights();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Start()
	{
		UpdateLights();
	}

	public void SetState(bool _powered, bool _activated)
	{
		powered = _powered;
		activated = _activated;
		UpdateLights();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void UpdateLights()
	{
		if (!Powered)
		{
			GreenLight.SetActive(value: false);
			RedLight.SetActive(value: false);
		}
		else
		{
			GreenLight.SetActive(activated);
			RedLight.SetActive(!activated);
		}
	}
}
