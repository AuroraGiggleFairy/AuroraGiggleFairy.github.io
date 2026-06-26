using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_SpawnMenu : XUiController
{
	public static string ID = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SpawnEntitiesList entitiesList;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ToggleButton toggleLookAtYou;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ToggleButton toggleSpawn25;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ToggleButton toggleFromDynamic;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ToggleButton toggleFromStatic;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ToggleButton toggleFromBiome;

	public override void Init()
	{
		base.Init();
		ID = base.WindowGroup.ID;
		toggleLookAtYou = GetChildById("toggleLookAtYou").GetChildByType<XUiC_ToggleButton>();
		toggleLookAtYou.OnValueChanged += ToggleLookAtYou_OnValueChanged;
		toggleSpawn25 = GetChildById("toggleSpawn25").GetChildByType<XUiC_ToggleButton>();
		toggleSpawn25.OnValueChanged += ToggleSpawn25_OnValueChanged;
		toggleFromDynamic = GetChildById("toggleFromDynamic").GetChildByType<XUiC_ToggleButton>();
		toggleFromDynamic.OnValueChanged += ToggleFromDynamic_OnValueChanged;
		toggleFromStatic = GetChildById("toggleFromStatic").GetChildByType<XUiC_ToggleButton>();
		toggleFromStatic.OnValueChanged += ToggleFromStatic_OnValueChanged;
		toggleFromBiome = GetChildById("toggleFromBiome").GetChildByType<XUiC_ToggleButton>();
		toggleFromBiome.OnValueChanged += ToggleFromBiome_OnValueChanged;
		entitiesList = (XUiC_SpawnEntitiesList)GetChildById("entities");
		entitiesList.SelectionChanged += EntitiesList_SelectionChanged;
		toggleFromDynamic.Value = true;
	}

	public override void OnOpen()
	{
		base.OnOpen();
		XUiC_FocusedBlockHealth.SetData(base.xui.playerUI, null, 0f);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void EntitiesList_SelectionChanged(XUiC_ListEntry<XUiC_SpawnEntitiesList.SpawnEntityEntry> _previousEntry, XUiC_ListEntry<XUiC_SpawnEntitiesList.SpawnEntityEntry> _newEntry)
	{
		if (_newEntry != null)
		{
			entitiesList.ClearSelection();
			if (_newEntry.GetEntry() != null)
			{
				XUiC_SpawnEntitiesList.SpawnEntityEntry entry = _newEntry.GetEntry();
				BtnSpawns_OnPress(entry.key);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ToggleLookAtYou_OnValueChanged(XUiC_ToggleButton _sender, bool _newValue)
	{
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ToggleSpawn25_OnValueChanged(XUiC_ToggleButton _sender, bool _newValue)
	{
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ToggleFromDynamic_OnValueChanged(XUiC_ToggleButton _sender, bool _newValue)
	{
		if (_newValue)
		{
			toggleFromStatic.Value = false;
			toggleFromBiome.Value = false;
		}
		else
		{
			toggleFromDynamic.Value = true;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ToggleFromStatic_OnValueChanged(XUiC_ToggleButton _sender, bool _newValue)
	{
		if (_newValue)
		{
			toggleFromDynamic.Value = false;
			toggleFromBiome.Value = false;
		}
		else
		{
			toggleFromStatic.Value = true;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ToggleFromBiome_OnValueChanged(XUiC_ToggleButton _sender, bool _newValue)
	{
		if (_newValue)
		{
			toggleFromDynamic.Value = false;
			toggleFromStatic.Value = false;
		}
		else
		{
			toggleFromBiome.Value = true;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnSpawns_OnPress(int _key)
	{
		EntityPlayerLocal primaryPlayer = GameManager.Instance.World.GetPrimaryPlayer();
		Camera finalCamera = primaryPlayer.finalCamera;
		float offsetUp = (primaryPlayer.AttachedToEntity ? 2 : 0);
		Vector3 transformPos = XUiC_LevelTools3Window.getRaycastHitPoint(100f, offsetUp);
		if (transformPos.Equals(Vector3.zero))
		{
			Ray ray = finalCamera.ScreenPointToRay(new Vector3((float)Screen.width * 0.5f, (float)Screen.height * 0.5f, 0f));
			transformPos = ray.origin + ray.direction * 10f + Origin.position;
		}
		transformPos.y += 0.25f;
		Vector3 rotation = new Vector3(0f, toggleLookAtYou.Value ? (finalCamera.transform.eulerAngles.y + 180f) : finalCamera.transform.eulerAngles.y, 0f);
		EnumSpawnerSource enumSpawnerSource = EnumSpawnerSource.Unknown;
		if (toggleFromDynamic.Value)
		{
			enumSpawnerSource = EnumSpawnerSource.Dynamic;
		}
		if (toggleFromStatic.Value)
		{
			enumSpawnerSource = EnumSpawnerSource.StaticSpawner;
		}
		if (toggleFromBiome.Value)
		{
			enumSpawnerSource = EnumSpawnerSource.Biome;
		}
		int num = ((!toggleSpawn25.Value) ? 1 : 25);
		if (InputUtils.ShiftKeyPressed)
		{
			num = 5;
		}
		Vector3 right = finalCamera.transform.right;
		if (!InputUtils.AltKeyPressed)
		{
			right *= 0.01f;
		}
		if (EntityClass.list[_key].entityClassName == "entityJunkDrone")
		{
			if (!EntityDrone.IsValidForLocalPlayer())
			{
				return;
			}
			GameManager.Instance.World.EntityLoadedDelegates += EntityDrone.OnClientSpawnRemote;
			num = 1;
		}
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			transformPos -= right * ((float)(num - 1) * 0.5f);
			for (int i = 0; i < num; i++)
			{
				Entity entity = EntityFactory.CreateEntity(_key, transformPos, rotation);
				entity.SetSpawnerSource(enumSpawnerSource);
				setUpEntity(entity);
				GameManager.Instance.World.SpawnEntityInWorld(entity);
				transformPos += right;
			}
			return;
		}
		SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageConsoleCmdServer>().Setup("spawnentityat \"" + EntityClass.list[_key].entityClassName + "\" " + transformPos.x.ToCultureInvariantString() + " " + transformPos.y.ToCultureInvariantString() + " " + transformPos.z.ToCultureInvariantString() + " " + num + " " + rotation.x.ToCultureInvariantString() + " " + rotation.y.ToCultureInvariantString() + " " + rotation.z.ToCultureInvariantString() + " " + right.x.ToCultureInvariantString() + " " + right.y.ToCultureInvariantString() + " " + right.z.ToCultureInvariantString() + " " + enumSpawnerSource.ToStringCached()));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void setUpEntity(Entity _entity)
	{
	}
}
