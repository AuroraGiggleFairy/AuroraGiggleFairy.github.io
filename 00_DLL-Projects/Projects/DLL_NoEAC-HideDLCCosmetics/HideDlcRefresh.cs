public static class HideDlcRefresh
{
	public static void Apply()
	{
		if (GameManager.IsDedicatedServer)
		{
			return;
		}

		ApplyLocal();
		RefreshRemotePlayerMeshes();
	}

	public static void ApplyLocal()
	{
		if (GameManager.IsDedicatedServer)
		{
			return;
		}

		ClearHiddenCosmeticsFromLocalPlayer();
		RefreshLocalEquipmentAndWardrobe();
	}

	private static EntityPlayerLocal GetLocalPlayer()
	{
		World world = GameManager.Instance != null ? GameManager.Instance.World : null;
		return world != null ? world.GetPrimaryPlayer() as EntityPlayerLocal : null;
	}

	private static void ClearHiddenCosmeticsFromLocalPlayer()
	{
		try
		{
			EntityPlayerLocal local = GetLocalPlayer();
			if (local == null || local.equipment == null)
			{
				return;
			}

			Equipment equipment = local.equipment;
			ItemClass[] slots = equipment.CosmeticSlots;
			if (slots != null)
			{
				for (int i = 0; i < slots.Length; i++)
				{
					if (HideDlcCatalog.IsHiddenItem(slots[i]))
					{
						equipment.SetCosmeticSlot(i, 0);
					}
				}
			}

			if (HideDlcCatalog.IsHiddenItem(equipment.tempCosmeticSlot))
			{
				equipment.ClearTempCosmeticSlot();
			}
		}
		catch
		{
		}
	}

	private static void RefreshLocalEquipmentAndWardrobe()
	{
		try
		{
			EntityPlayerLocal local = GetLocalPlayer();
			if (local == null || local.playerUI == null || local.playerUI.xui == null)
			{
				return;
			}

			XUiC_CharacterCosmeticsListWindow listWindow = local.playerUI.xui.GetChildByType<XUiC_CharacterCosmeticsListWindow>();
			if (listWindow != null)
			{
				listWindow.IsDirty = true;
			}

			XUiM_PlayerEquipment playerEquipment = local.playerUI.xui.PlayerEquipment;
			if (playerEquipment != null)
			{
				playerEquipment.RefreshEquipment();
			}
		}
		catch
		{
		}
	}

	private static void RefreshRemotePlayerMeshes()
	{
		try
		{
			World world = GameManager.Instance != null ? GameManager.Instance.World : null;
			if (world == null || world.Players == null || world.Players.list == null)
			{
				return;
			}

			foreach (EntityPlayer player in world.Players.list)
			{
				if (player == null || player is EntityPlayerLocal)
				{
					continue;
				}

				EModelSDCS emodel = player.emodel as EModelSDCS;
				if (emodel != null)
				{
					emodel.GenerateMeshes();
				}
			}
		}
		catch
		{
		}
	}
}
