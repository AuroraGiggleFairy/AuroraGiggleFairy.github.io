using System.Collections;
using UnityEngine;
using UnityEngine.Scripting;

namespace GameEvent.SequenceActions;

[Preserve]
public class ActionTeleportToSpecial : ActionBaseClientAction
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public enum SpecialPointTypes
	{
		Bedroll,
		Landclaim,
		Backpack
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 position;

	[PublicizedFrom(EAccessModifier.Protected)]
	public float teleportDelay = 0.1f;

	[PublicizedFrom(EAccessModifier.Protected)]
	public SpecialPointTypes pointType;

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropSpecialType = "special_type";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropTeleportDelay = "teleport_delay";

	public override void OnClientPerform(Entity target)
	{
		if (!(target is EntityPlayer entityPlayer))
		{
			return;
		}
		_ = GameManager.Instance.World;
		position = Vector3.zero;
		switch (pointType)
		{
		case SpecialPointTypes.Bedroll:
			if (entityPlayer.SpawnPoints.Count == 0)
			{
				return;
			}
			position = entityPlayer.SpawnPoints[0];
			break;
		case SpecialPointTypes.Landclaim:
		{
			PersistentPlayerData playerDataFromEntityID = GameManager.Instance.persistentPlayers.GetPlayerDataFromEntityID(entityPlayer.entityId);
			if (playerDataFromEntityID.LPBlocks == null || playerDataFromEntityID.LPBlocks.Count == 0)
			{
				return;
			}
			position = playerDataFromEntityID.LPBlocks[0];
			break;
		}
		case SpecialPointTypes.Backpack:
		{
			Vector3i lastDroppedBackpackPosition = entityPlayer.GetLastDroppedBackpackPosition();
			if (lastDroppedBackpackPosition == Vector3i.zero)
			{
				return;
			}
			position = lastDroppedBackpackPosition;
			break;
		}
		}
		GameManager.Instance.StartCoroutine(handleTeleport(entityPlayer));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator handleTeleport(EntityPlayer player)
	{
		yield return new WaitForSeconds(teleportDelay);
		if (position.y > 0f)
		{
			position += Vector3.up * 2f;
			if (player.isEntityRemote)
			{
				SingletonMonoBehaviour<ConnectionManager>.Instance.Clients.ForEntityId(player.entityId).SendPackage(NetPackageManager.GetPackage<NetPackageTeleportPlayer>().Setup(position));
				yield break;
			}
			((EntityPlayerLocal)player).PlayerUI.windowManager.CloseAllOpenWindows();
			((EntityPlayerLocal)player).TeleportToPosition(position);
		}
	}

	public override void ParseProperties(DynamicProperties properties)
	{
		base.ParseProperties(properties);
		properties.ParseEnum(PropSpecialType, ref pointType);
		properties.ParseFloat(PropTeleportDelay, ref teleportDelay);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override BaseAction CloneChildSettings()
	{
		return new ActionTeleportToSpecial
		{
			targetGroup = targetGroup,
			pointType = pointType,
			teleportDelay = teleportDelay
		};
	}
}
