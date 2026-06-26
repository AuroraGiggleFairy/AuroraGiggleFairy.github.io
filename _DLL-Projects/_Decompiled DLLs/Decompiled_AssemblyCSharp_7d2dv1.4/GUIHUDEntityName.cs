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
	public EntityAlive entity;

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

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float headOffset = 0.6f;

	[PublicizedFrom(EAccessModifier.Protected)]
	public void Start()
	{
		if (GameManager.IsDedicatedServer)
		{
			UnityEngine.Object.Destroy(this);
			return;
		}
		entity = GetComponent<EntityAlive>();
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
			hudText = gameObject2.GetComponentInChildren<NGuiHUDText>();
			hudTextObj = hudText.gameObject;
			if (hudText.ambigiousFont == null)
			{
				Log.Error("GUIHUDEntityName font null");
			}
			followTarget = gameObject2.AddComponent<NGuiUIFollowTarget>();
			followTarget.offset = new Vector3(0f, headOffset, 0f);
			followTarget.target = null;
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
		bool flag = entity is EntityPlayer;
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
			followTarget.offset = new Vector3(0f, entity.GetEyeHeight() + headOffset, 0f);
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
			string text = ((entity is EntityPlayer entityPlayer) ? entityPlayer.PlayerDisplayName : entity.EntityName);
			if (!entity.IsDead())
			{
				text += entity.DebugNameInfo;
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
			if (!string.IsNullOrEmpty(text2))
			{
				UIAtlas atlasByName = primaryPlayer.PlayerUI.xui.GetAtlasByName("SymbolAtlas", text2);
				hudText.SetEntry(0, text2, _isSprite: true, atlasByName);
				hudText.SetEntrySize(0, 40);
				hudText.SetEntryOffset(0, new Vector3(-0.1f, 0f, 0f));
				hudText.SetEntry(1, text, _isSprite: false);
				hudText.SetEntrySize(1, 45);
			}
			else
			{
				hudText.SetEntry(0, text, _isSprite: false);
				hudText.SetEntrySize(0, 45);
				hudText.SetEntryOffset(0, default(Vector3));
				hudText.SetEntry(1, string.Empty, _isSprite: false);
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
