using System;
using UnityEngine;

public class LocalPlayerManager
{
	public static event Action OnLocalPlayersChanged;

	public static void LocalPlayersChanged()
	{
		LocalPlayerManager.OnLocalPlayersChanged?.Invoke();
	}

	public static void Init()
	{
		GameManager.Instance.OnLocalPlayerChanged += HandleLocalPlayerChanged;
	}

	public static void Destroy()
	{
		GameManager.Instance.OnLocalPlayerChanged -= HandleLocalPlayerChanged;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void HandleLocalPlayerChanged(EntityPlayerLocal localPlayer)
	{
		if (localPlayer != null)
		{
			return;
		}
		foreach (LocalPlayerUI playerUI in LocalPlayerUI.PlayerUIs)
		{
			if (!playerUI.isPrimaryUI)
			{
				UnityEngine.Object.Destroy(playerUI.gameObject);
			}
		}
	}
}
