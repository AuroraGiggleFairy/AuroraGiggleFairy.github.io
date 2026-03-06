using System;
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

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton spawnFiltered;

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
		spawnFiltered = GetChildById("spawnFiltered").GetChildByType<XUiC_SimpleButton>();
		spawnFiltered.OnPressed += SpawnFiltered_OnPressed;
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
	public void SpawnFiltered_OnPressed(XUiController _sender, int _mouseButton)
	{
		int num = 0;
		Vector3 hitPoint = GetHitPoint();
		float y = GameManager.Instance.World.GetPrimaryPlayer().finalCamera.transform.eulerAngles.y;
		int entryCount = entitiesList.EntryCount;
		float num2 = 0f;
		float num3 = 45f;
		float num4 = ((entryCount > 1) ? 2 : 0);
		for (int i = 0; i < entryCount; i++)
		{
			XUiC_SpawnEntitiesList.SpawnEntityEntry entry = entitiesList.GetEntry(i);
			Vector3 spawnPos = hitPoint;
			float f = (num2 + y) * (MathF.PI / 180f);
			spawnPos.x += Mathf.Sin(f) * num4;
			spawnPos.z += Mathf.Cos(f) * num4;
			num += Spawn(entry.key, spawnPos);
			if (num > 200)
			{
				break;
			}
			num2 += num3;
			if (num2 > 359f)
			{
				num2 = 0f;
				num3 /= 2f;
				num4 += 2f;
			}
		}
		Log.Out("Spawned {0}", num);
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
		Vector3 hitPoint = GetHitPoint();
		Spawn(_key, hitPoint);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 GetHitPoint()
	{
		EntityPlayerLocal primaryPlayer = GameManager.Instance.World.GetPrimaryPlayer();
		float offsetUp = (primaryPlayer.AttachedToEntity ? 2 : 0);
		Vector3 result = XUiC_LevelTools3Window.getRaycastHitPoint(100f, offsetUp);
		if (result.Equals(Vector3.zero))
		{
			Ray ray = primaryPlayer.finalCamera.ScreenPointToRay(new Vector3((float)Screen.width * 0.5f, (float)Screen.height * 0.5f, 0f));
			result = ray.origin + ray.direction * 10f + Origin.position;
		}
		result.y += 0.25f;
		return result;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int Spawn(int _key, Vector3 _spawnPos)
	{
		Camera finalCamera = GameManager.Instance.World.GetPrimaryPlayer().finalCamera;
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
				return 0;
			}
			GameManager.Instance.World.EntityLoadedDelegates += EntityDrone.OnClientSpawnRemote;
			num = 1;
		}
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			_spawnPos -= right * ((float)(num - 1) * 0.5f);
			for (int i = 0; i < num; i++)
			{
				Entity entity = EntityFactory.CreateEntity(_key, _spawnPos, rotation);
				entity.SetSpawnerSource(enumSpawnerSource);
				GameManager.Instance.World.SpawnEntityInWorld(entity);
				_spawnPos += right;
			}
		}
		else
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageConsoleCmdServer>().Setup("spawnentityat \"" + EntityClass.list[_key].entityClassName + "\" " + _spawnPos.x.ToCultureInvariantString() + " " + _spawnPos.y.ToCultureInvariantString() + " " + _spawnPos.z.ToCultureInvariantString() + " " + num + " " + rotation.x.ToCultureInvariantString() + " " + rotation.y.ToCultureInvariantString() + " " + rotation.z.ToCultureInvariantString() + " " + right.x.ToCultureInvariantString() + " " + right.y.ToCultureInvariantString() + " " + right.z.ToCultureInvariantString() + " " + enumSpawnerSource.ToStringCached()));
		}
		return num;
	}
}
