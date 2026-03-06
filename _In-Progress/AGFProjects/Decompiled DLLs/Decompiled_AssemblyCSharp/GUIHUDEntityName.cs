using System;
using Platform;
using UnityEngine;

public class GUIHUDEntityName : MonoBehaviour
{
	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const int cRaycastFrameDelay = 5;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const int cVisibleDistance = 8;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const float cHeadOffset = 0.6f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public EntityAlive entity;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public EntityPlayer entityPlayer;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Renderer[] renderers;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public int updatePhysicsVisibilityCounter;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool bShowHUDText;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool bLastShowHUDText;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float hideCountdownTime;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static GameManager gameManager;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static Camera mainCamera;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public NGuiHUDText hudText;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public GameObject hudTextObj;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public NGuiUIFollowTarget followTarget;

	[PublicizedFrom(EAccessModifier.Protected)]
	public void Start()
	{
		if (GameManager.IsDedicatedServer)
		{
			UnityEngine.Object.Destroy(this);
			return;
		}
		entity = GetComponent<EntityAlive>();
		entityPlayer = entity as EntityPlayer;
		if (gameManager == null)
		{
			gameManager = (GameManager)UnityEngine.Object.FindObjectOfType(typeof(GameManager));
		}
		if (NGuiHUDRoot.go == null)
		{
			UnityEngine.Object.Destroy(this);
			return;
		}
		GameObject gameObject = Resources.Load("Prefabs/prefabPlayerHUDText", typeof(GameObject)) as GameObject;
		if (gameObject != null)
		{
			GameObject gameObject2 = NGuiHUDRoot.go.AddChild(gameObject);
			gameObject2.name = ((entityPlayer != null) ? entityPlayer.PlayerDisplayName : entity.EntityName);
			hudText = gameObject2.GetComponentInChildren<NGuiHUDText>();
			hudTextObj = hudText.gameObject;
			if (hudText.ambigiousFont == null)
			{
				Log.Error("GUIHUDEntityName font null");
			}
			followTarget = gameObject2.AddComponent<NGuiUIFollowTarget>();
			followTarget.offset = new Vector3(0f, 0.6f, 0f);
			followTarget.target = null;
			hudText.Add(string.Empty, Color.white, float.MaxValue);
			hudText.Add(string.Empty, Color.white, float.MaxValue);
			hudText.Add(string.Empty, Color.white, float.MaxValue);
			hudTextObj.SetActive(value: false);
			updatePhysicsVisibilityCounter = 9999;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void OnDestroy()
	{
		if (hudText != null)
		{
			UnityEngine.Object.Destroy(hudTextObj);
			hudText = null;
			hudTextObj = null;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void findRenderers()
	{
		if ((bool)entity.emodel)
		{
			Transform modelTransform = entity.emodel.GetModelTransform();
			if ((bool)modelTransform)
			{
				renderers = modelTransform.GetComponentsInChildren<Renderer>();
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void OnGUI()
	{
		if (GameManager.IsDedicatedServer)
		{
			return;
		}
		if (mainCamera == null)
		{
			mainCamera = Camera.main;
			if (mainCamera == null)
			{
				return;
			}
		}
		Vector3 direction = entity.getHeadPosition() - Origin.position - mainCamera.transform.position;
		float magnitude = direction.magnitude;
		bool flag = entityPlayer != null;
		if (magnitude > 8f && flag)
		{
			setActiveIfDifferent(_active: false);
			return;
		}
		if (renderers == null || renderers.Length == 0)
		{
			if (++updatePhysicsVisibilityCounter > 100)
			{
				updatePhysicsVisibilityCounter = 0;
				findRenderers();
			}
			return;
		}
		bool flag2 = false;
		for (int i = 0; i < renderers.Length; i++)
		{
			Renderer renderer = renderers[i];
			if (!renderer)
			{
				renderers = null;
				return;
			}
			if (renderer.isVisible)
			{
				flag2 = true;
				break;
			}
		}
		if (!flag2)
		{
			setActiveIfDifferent(_active: false);
			return;
		}
		if (followTarget.target == null)
		{
			followTarget.target = entity.ModelTransform;
			followTarget.offset = new Vector3(0f, entity.GetEyeHeight() + 0.6f, 0f);
		}
		if (++updatePhysicsVisibilityCounter > 5)
		{
			updatePhysicsVisibilityCounter = 0;
			EntityPlayerLocal primaryPlayer = gameManager.World.GetPrimaryPlayer();
			if (primaryPlayer == null || !primaryPlayer.Spawned)
			{
				bShowHUDText = false;
				setActiveIfDifferent(_active: false);
				return;
			}
			if (!primaryPlayer.PlayerUI.windowManager.IsHUDEnabled())
			{
				bShowHUDText = false;
				setActiveIfDifferent(_active: false);
				return;
			}
			bShowHUDText = Physics.Raycast(new Ray(mainCamera.transform.position + direction.normalized * 0.15f, direction), out var hitInfo, 9.6f, -538480645);
			bShowHUDText = bShowHUDText && hitInfo.distance < 8f;
			Transform hitRootTransform = hitInfo.transform;
			if (bShowHUDText && hitRootTransform.tag.StartsWith("E_BP_"))
			{
				hitRootTransform = GameUtils.GetHitRootTransform(hitRootTransform.tag, hitRootTransform);
			}
			bShowHUDText &= hitRootTransform == entity.transform;
			if (!flag)
			{
				bShowHUDText = true;
			}
			if (!bShowHUDText && bLastShowHUDText && hideCountdownTime <= 0f)
			{
				hideCountdownTime = 0.4f;
			}
			int num = 45;
			string text = ((entityPlayer != null) ? entityPlayer.PlayerDisplayName : entity.EntityName);
			if (entity.DebugNameInfo.Length > 0 && !entity.IsDead())
			{
				text += entity.DebugNameInfo;
				num = (int)(150f / Utils.FastClamp(magnitude, 3.333f, 8f));
			}
			string text2 = string.Empty;
			if (GameManager.Instance.persistentPlayers.EntityToPlayerMap.TryGetValue(entity.entityId, out var value))
			{
				GameServerInfo obj = (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer ? SingletonMonoBehaviour<ConnectionManager>.Instance.LocalServerInfo : SingletonMonoBehaviour<ConnectionManager>.Instance.LastGameServerInfo);
				if (obj != null && obj.AllowsCrossplay)
				{
					EPlayGroup playGroup = GameManager.Instance.persistentPlayers.Players[value.PrimaryId].PlayGroup;
					text2 = PlatformManager.NativePlatform.Utils.GetCrossplayPlayerIcon(playGroup, _fetchGenericIcons: false, value.PlatformData.NativeId.PlatformIdentifier);
				}
			}
			UIAtlas atlasByName = primaryPlayer.PlayerUI.xui.GetAtlasByName("SymbolAtlas", text2);
			hudText.SetEntry(0, text2, _isSprite: true, atlasByName);
			hudText.SetEntrySize(0, num - 5);
			hudText.SetEntryOffset(0, new Vector3(-0.1f, 0f, 0f));
			hudText.SetEntry(1, text, _isSprite: false);
			hudText.SetEntrySize(1, num);
			IPartyVoice.EVoiceMemberState playerVoiceState = VoiceHelpers.GetPlayerVoiceState(entityPlayer);
			if (playerVoiceState != IPartyVoice.EVoiceMemberState.Disabled)
			{
				UIAtlas atlasByName2 = primaryPlayer.PlayerUI.xui.GetAtlasByName("UIAtlas", "ui_game_symbol_talk");
				hudText.SetEntry(2, "ui_game_symbol_talk", _isSprite: true, atlasByName2);
				hudText.SetEntrySize(2, num - 5);
				hudText.SetEntryOffset(2, new Vector3(0.1f, 0f, 0f));
				hudText.SetEntryColor(2, playerVoiceState switch
				{
					IPartyVoice.EVoiceMemberState.VoiceActive => Color.white, 
					IPartyVoice.EVoiceMemberState.Muted => Color.red, 
					_ => Color.grey, 
				});
			}
			else
			{
				hudText.SetEntry(2, string.Empty, _isSprite: true);
				hudText.SetEntrySize(2, num - 5);
				hudText.SetEntryOffset(2, new Vector3(0.1f, 0f, 0f));
			}
		}
		if (hideCountdownTime > 0f)
		{
			hideCountdownTime -= Time.deltaTime;
		}
		if (hideCountdownTime <= 0f)
		{
			setActiveIfDifferent(bShowHUDText);
			bLastShowHUDText = bShowHUDText;
		}
		else if (bShowHUDText)
		{
			setActiveIfDifferent(bShowHUDText);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void setActiveIfDifferent(bool _active)
	{
		if (hudTextObj.activeSelf != _active)
		{
			hudTextObj.SetActive(_active);
		}
	}
}
