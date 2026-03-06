using System;
using System.Collections.Generic;
using Platform;
using UnityEngine;

public class Vehicle
{
	public enum Event
	{
		Start,
		Started,
		Stop,
		Stopped,
		SimulationUpdate,
		HealthChanged
	}

	public static Dictionary<string, DynamicProperties> PropertyMap;

	public DynamicProperties Properties;

	public ItemValue itemValue;

	public List<PlatformUserIdentifierAbs> AllowedUsers;

	public int PasswordHash;

	public EntityVehicle entity;

	[PublicizedFrom(EAccessModifier.Private)]
	public PlatformUserIdentifierAbs m_ownerId;

	[PublicizedFrom(EAccessModifier.Private)]
	public string vehicleName;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<VehiclePart> vehicleParts;

	[PublicizedFrom(EAccessModifier.Private)]
	public Transform meshT;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector2 cameraDistance = new Vector2(1f, 1f);

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector2 cameraTurnRate = new Vector2(1f, 1f);

	[PublicizedFrom(EAccessModifier.Private)]
	public float upAngleMax = 70f;

	[PublicizedFrom(EAccessModifier.Private)]
	public float upForce = 5f;

	[PublicizedFrom(EAccessModifier.Private)]
	public float steerAngleMax = 25f;

	[PublicizedFrom(EAccessModifier.Private)]
	public float steerRate = 130f;

	[PublicizedFrom(EAccessModifier.Private)]
	public float steerCenteringRate = 90f;

	[PublicizedFrom(EAccessModifier.Private)]
	public float tiltAngleMax = 20f;

	[PublicizedFrom(EAccessModifier.Private)]
	public float tiltThreshold = 3f;

	[PublicizedFrom(EAccessModifier.Private)]
	public float tiltDampening = 0.22f;

	[PublicizedFrom(EAccessModifier.Private)]
	public float tiltDampenThreshold = 8f;

	[PublicizedFrom(EAccessModifier.Private)]
	public float tiltUpForce = 5f;

	public float MotorTorqueForward = 300f;

	public float MotorTorqueBackward = 300f;

	public float MotorTorqueTurboForward;

	public float MotorTorqueTurboBackward;

	public float VelocityMaxForward = 10f;

	public float VelocityMaxBackward = 10f;

	public float VelocityMaxTurboForward = 10f;

	public float VelocityMaxTurboBackward = 10f;

	[PublicizedFrom(EAccessModifier.Private)]
	public float brakeTorque = 4000f;

	public bool CanTurbo = true;

	public bool IsTurbo;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector2 hopForce;

	[PublicizedFrom(EAccessModifier.Private)]
	public float unstickForce = 1f;

	public float AirDragVelScale = 0.997f;

	public float AirDragAngVelScale = 1f;

	public float WaterDragY;

	public float WaterDragVelScale = 1f;

	public float WaterDragVelMaxScale = 1f;

	public float WaterLiftY;

	public float WaterLiftDepth;

	public float WaterLiftForce;

	public float WheelPtlScale;

	public float CurrentForwardVelocity;

	public bool CurrentIsAccel;

	public bool CurrentIsBreak;

	public float CurrentMotorTorquePercent;

	public float CurrentSteeringPercent;

	public Vector3 CurrentVelocity;

	public string RecipeName;

	public Material mainEmissiveMat;

	public float EffectEntityDamagePer;

	public float EffectBlockDamagePer;

	public float EffectSelfDamagePer;

	public float EffectStrongSelfDamagePer;

	public float EffectLightIntensity;

	public float EffectFuelMaxPer = 1f;

	public float EffectFuelUsePer;

	public float EffectMotorTorquePer;

	public float EffectVelocityMaxPer;

	[PublicizedFrom(EAccessModifier.Private)]
	public float effectUpdateDelay;

	public FastTags<TagGroup.Global> ModTags;

	public Vector2 CameraDistance => cameraDistance;

	public Vector2 CameraTurnRate => cameraTurnRate;

	public float BrakeTorque => brakeTorque;

	public float SteerAngleMax => steerAngleMax;

	public float SteerRate => steerRate;

	public float SteerCenteringRate => steerCenteringRate;

	public float TiltAngleMax => tiltAngleMax;

	public float TiltThreshold => tiltThreshold;

	public float TiltDampening => tiltDampening;

	public float TiltDampenThreshold => tiltDampenThreshold;

	public float TiltUpForce => tiltUpForce;

	public Vector2 HopForce => hopForce;

	public float UpAngleMax => upAngleMax;

	public float UpForce => upForce;

	public float UnstickForce => unstickForce;

	public float MaxPossibleSpeed => VelocityMaxTurboForward;

	public PlatformUserIdentifierAbs OwnerId
	{
		get
		{
			return m_ownerId;
		}
		set
		{
			m_ownerId = value;
			int belongsPlayerId = -1;
			PersistentPlayerData playerData = GameManager.Instance.persistentPlayers.GetPlayerData(m_ownerId);
			if (playerData != null)
			{
				belongsPlayerId = playerData.EntityId;
			}
			entity.belongsPlayerId = belongsPlayerId;
		}
	}

	public Vehicle(string _vehicleName, EntityVehicle _entity)
	{
		vehicleName = _vehicleName.ToLower();
		entity = _entity;
		SetupProperties();
		meshT = entity.ModelTransform.Find("Mesh");
		if (!meshT)
		{
			meshT = entity.ModelTransform;
		}
		vehicleParts = new List<VehiclePart>();
		OwnerId = null;
		AllowedUsers = new List<PlatformUserIdentifierAbs>();
		PasswordHash = 0;
		MakeItemValue();
		CreateParts();
	}

	public void MakeItemValue()
	{
		string name = GetName();
		int type = 0;
		ItemClass itemClass = ItemClass.GetItemClass(name + "Placeable", _caseInsensitive: true);
		if (itemClass != null)
		{
			type = itemClass.Id;
		}
		itemValue = new ItemValue(type, 1, 6);
		SetItemValue(itemValue);
	}

	public void SetItemValue(ItemValue _itemValue)
	{
		itemValue = _itemValue;
		if (itemValue.CosmeticMods.Length == 0)
		{
			itemValue.CosmeticMods = new ItemValue[1];
		}
		int num = itemValue.MaxUseTimes;
		if (itemValue.type == 0)
		{
			num = 5555;
		}
		int health = num - (int)itemValue.UseTimes;
		entity.Stats.Health.BaseMax = num;
		entity.Stats.Health.OriginalMax = num;
		entity.Health = health;
		CalcEffects();
		SetFuelLevel((float)itemValue.Meta / 50f);
		CalcMods();
		SetColors();
		SetSeats();
	}

	public void SetItemValueMods(ItemValue _itemValue)
	{
		ItemValue itemValue = _itemValue.Clone();
		this.itemValue.Modifications = itemValue.Modifications;
		this.itemValue.CosmeticMods = itemValue.CosmeticMods;
		CalcEffects();
		CalcMods();
		SetColors();
		SetSeats();
	}

	public void SetColors()
	{
		Color white = Color.white;
		Vector3 vector = Block.StringToVector3(itemValue.GetPropertyOverride(Block.PropTintColor, "255,255,255"));
		white.r = vector.x;
		white.g = vector.y;
		white.b = vector.z;
		for (int i = 0; i < vehicleParts.Count; i++)
		{
			vehicleParts[i].SetColors(white);
		}
	}

	public void SetSeats()
	{
		int num = (int)EffectManager.GetValue(PassiveEffects.VehicleSeats, itemValue);
		int num2 = 0;
		if (Properties != null)
		{
			int num3 = 0;
			for (int i = 0; i < 99; i++)
			{
				if (!Properties.Classes.TryGetValue("seat" + i, out var _value))
				{
					break;
				}
				if (_value.GetString("mod").Length > 0)
				{
					if (num3 >= num)
					{
						break;
					}
					num3++;
				}
				num2++;
			}
		}
		entity.SetAttachMaxCount(num2);
	}

	public int GetSeatPose(int _seatIndex)
	{
		if (Properties != null && Properties.Classes.TryGetValue("seat" + _seatIndex, out var _value))
		{
			string text = _value.GetString("pose");
			if (text.Length > 0)
			{
				return int.Parse(text);
			}
		}
		return 0;
	}

	public ItemValue GetUpdatedItemValue()
	{
		itemValue.UseTimes = (int)entity.Stats.Health.BaseMax - entity.Health;
		itemValue.Meta = (int)(GetFuelLevel() * 50f);
		return itemValue;
	}

	public void LoadItems(ItemStack[] _items)
	{
		SetItemValue(_items[0].itemValue);
	}

	public ItemStack[] GetItems()
	{
		return new ItemStack[1]
		{
			new ItemStack(GetUpdatedItemValue(), 1)
		};
	}

	public void Update(float _deltaTime)
	{
		UpdateEffects(_deltaTime);
		for (int i = 0; i < vehicleParts.Count; i++)
		{
			vehicleParts[i].Update(_deltaTime);
		}
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer && !HasStorage())
		{
			GameManager.Instance.DropContentOfLootContainerServer(BlockValue.Air, new Vector3i(entity.position), entity.entityId);
		}
	}

	public void UpdateSimulation()
	{
		FireEvent(Event.SimulationUpdate);
	}

	public void FireEvent(Event _event)
	{
		for (int i = 0; i < vehicleParts.Count; i++)
		{
			vehicleParts[i].HandleEvent(_event, 0f);
		}
	}

	public void FireEvent(VehiclePart.Event _event, VehiclePart _fromPart, float _arg)
	{
		for (int i = 0; i < vehicleParts.Count; i++)
		{
			vehicleParts[i].HandleEvent(_event, _fromPart, _arg);
		}
	}

	public void SetupProperties()
	{
		if (!PropertyMap.TryGetValue(vehicleName, out Properties))
		{
			Log.Error("Vehicle properties for '{0}' not found!", vehicleName);
		}
	}

	public DynamicProperties GetPropertiesForClass(string className)
	{
		if (Properties == null)
		{
			return null;
		}
		Properties.Classes.TryGetValue(className, out var _value);
		return _value;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ParseGeneralProperties(DynamicProperties properties)
	{
		properties.ParseVec("cameraDistance", ref cameraDistance);
		properties.ParseVec("cameraTurnRate", ref cameraTurnRate);
		properties.ParseFloat("steerAngleMax", ref steerAngleMax);
		properties.ParseFloat("steerRate", ref steerRate);
		properties.ParseFloat("steerCenteringRate", ref steerCenteringRate);
		properties.ParseFloat("tiltAngleMax", ref tiltAngleMax);
		properties.ParseFloat("tiltThreshold", ref tiltThreshold);
		properties.ParseFloat("tiltDampening", ref tiltDampening);
		properties.ParseFloat("tiltDampenThreshold", ref tiltDampenThreshold);
		properties.ParseFloat("tiltUpForce", ref tiltUpForce);
		properties.ParseFloat("upAngleMax", ref upAngleMax);
		properties.ParseFloat("upForce", ref upForce);
		properties.ParseVec("motorTorque_turbo", ref MotorTorqueForward, ref MotorTorqueBackward, ref MotorTorqueTurboForward, ref MotorTorqueTurboBackward);
		properties.ParseVec("velocityMax_turbo", ref VelocityMaxForward, ref VelocityMaxBackward, ref VelocityMaxTurboForward, ref VelocityMaxTurboBackward);
		properties.ParseFloat("brakeTorque", ref brakeTorque);
		properties.ParseVec("hopForce", ref hopForce);
		properties.ParseFloat("unstickForce", ref unstickForce);
		properties.ParseVec("airDrag_velScale_angVelScale", ref AirDragVelScale, ref AirDragAngVelScale);
		properties.ParseVec("waterDrag_y_velScale_velMaxScale", ref WaterDragY, ref WaterDragVelScale, ref WaterDragVelMaxScale);
		properties.ParseVec("waterLift_y_depth_force", ref WaterLiftY, ref WaterLiftDepth, ref WaterLiftForce);
		properties.ParseFloat("wheelPtlScale", ref WheelPtlScale);
		properties.ParseString("recipeName", ref RecipeName);
	}

	public void OnXMLChanged()
	{
		SetupProperties();
		DynamicProperties properties = Properties;
		if (properties == null)
		{
			return;
		}
		ParseGeneralProperties(properties);
		foreach (KeyValuePair<string, DynamicProperties> item in properties.Classes.Dict)
		{
			VehiclePart vehiclePart = FindPart(item.Key);
			if (vehiclePart != null)
			{
				DynamicProperties value = item.Value;
				vehiclePart.SetProperties(value);
			}
		}
	}

	public void TriggerUpdateEffects()
	{
		effectUpdateDelay = 0f;
	}

	public void UpdateEffects(float _deltaTime)
	{
		effectUpdateDelay -= _deltaTime;
		if (!(effectUpdateDelay > 0f))
		{
			effectUpdateDelay = 2f;
			GetUpdatedItemValue();
			CalcEffects();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CalcEffects()
	{
		EntityAlive entityAlive = entity.AttachedMainEntity as EntityAlive;
		FastTags<TagGroup.Global> entityTags = entity.EntityTags;
		EffectEntityDamagePer = EffectManager.GetValue(PassiveEffects.VehicleEntityDamage, itemValue, 1f, entityAlive, null, entityTags);
		EffectBlockDamagePer = EffectManager.GetValue(PassiveEffects.VehicleBlockDamage, itemValue, 1f, entityAlive, null, entityTags);
		EffectSelfDamagePer = EffectManager.GetValue(PassiveEffects.VehicleSelfDamage, itemValue, 1f, entityAlive, null, entityTags);
		EffectStrongSelfDamagePer = EffectManager.GetValue(PassiveEffects.VehicleStrongSelfDamage, itemValue, 1f, entityAlive, null, entityTags);
		EffectLightIntensity = EffectManager.GetValue(PassiveEffects.LightIntensity, itemValue, 1f, entityAlive, null, entityTags);
		EffectFuelMaxPer = EffectManager.GetValue(PassiveEffects.VehicleFuelMaxPer, itemValue, 1f, entityAlive, null, entityTags);
		EffectFuelUsePer = EffectManager.GetValue(PassiveEffects.VehicleFuelUsePer, itemValue, 1f, entityAlive, null, entityTags);
		EffectMotorTorquePer = EffectManager.GetValue(PassiveEffects.VehicleMotorTorquePer, itemValue, 1f, entityAlive, null, entityTags);
		EffectVelocityMaxPer = EffectManager.GetValue(PassiveEffects.VehicleVelocityMaxPer, itemValue, 1f, entityAlive, null, entityTags);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CalcMods()
	{
		int num = 0;
		ModTags = FastTags<TagGroup.Global>.none;
		ItemValue[] modifications = this.itemValue.Modifications;
		if (modifications != null)
		{
			foreach (ItemValue itemValue in modifications)
			{
				if (itemValue != null && itemValue.ItemClass is ItemClassModifier itemClassModifier)
				{
					ModTags |= itemClassModifier.ItemTags;
					if (itemClassModifier.ItemTags.Test_AnySet(EntityVehicle.StorageModifierTags))
					{
						num++;
					}
				}
			}
		}
		for (int j = 0; j < vehicleParts.Count; j++)
		{
			vehicleParts[j].SetMods();
		}
		entity.UpdateStorageModCount(num);
		entity.UpdateContainerSize();
	}

	public void CreateParts()
	{
		DynamicProperties properties = Properties;
		if (properties == null)
		{
			return;
		}
		ParseGeneralProperties(properties);
		foreach (KeyValuePair<string, DynamicProperties> item in properties.Classes.Dict)
		{
			DynamicProperties value = item.Value;
			string text = value.GetString("class");
			if (text.Length > 0)
			{
				try
				{
					VehiclePart vehiclePart = (VehiclePart)Activator.CreateInstance(ReflectionHelpers.GetTypeWithPrefix("VP", text));
					vehiclePart.SetVehicle(this);
					vehiclePart.SetTag(item.Key);
					vehiclePart.SetProperties(value);
					vehicleParts.Add(vehiclePart);
				}
				catch (Exception ex)
				{
					Log.Out(ex.Message);
					Log.Out(ex.StackTrace);
					throw new Exception("No vehicle part class 'VP" + text + "' found!");
				}
			}
		}
		for (int i = 0; i < vehicleParts.Count; i++)
		{
			vehicleParts[i].InitPrefabConnections();
		}
	}

	public VehiclePart FindPart(string _tag)
	{
		for (int i = 0; i < vehicleParts.Count; i++)
		{
			if (vehicleParts[i].tag == _tag)
			{
				return vehicleParts[i];
			}
		}
		return null;
	}

	public string GetPartProperty(string _tag, string _propertyName)
	{
		VehiclePart vehiclePart = FindPart(_tag);
		if (vehiclePart == null)
		{
			return string.Empty;
		}
		return vehiclePart.GetProperty(_propertyName);
	}

	public List<VehiclePart> GetParts()
	{
		return vehicleParts;
	}

	public static void SetupPreview(Transform rootT)
	{
		Transform transform = rootT.Find("Physics");
		if ((bool)transform)
		{
			UnityEngine.Object.Destroy(transform.gameObject);
		}
		ParticleSystem[] componentsInChildren = rootT.GetComponentsInChildren<ParticleSystem>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].gameObject.SetActive(value: false);
		}
	}

	public Transform GetMeshTransform()
	{
		return meshT;
	}

	public string GetName()
	{
		return vehicleName;
	}

	public static void Cleanup()
	{
		PropertyMap = new Dictionary<string, DynamicProperties>();
	}

	public string GetFuelItem()
	{
		if (HasEnginePart())
		{
			return "ammoGasCan";
		}
		return "";
	}

	public float GetFuelPercent()
	{
		for (int i = 0; i < vehicleParts.Count; i++)
		{
			if (vehicleParts[i].GetType() == typeof(VPFuelTank))
			{
				float num = ((VPFuelTank)vehicleParts[i]).GetFuelLevelPercent();
				if (num > 0.993f)
				{
					num = 1f;
				}
				return num;
			}
		}
		return 0f;
	}

	public float GetFuelLevel()
	{
		for (int i = 0; i < vehicleParts.Count; i++)
		{
			if (vehicleParts[i].GetType() == typeof(VPFuelTank))
			{
				return ((VPFuelTank)vehicleParts[i]).GetFuelLevel();
			}
		}
		return 0f;
	}

	public float GetMaxFuelLevel()
	{
		for (int i = 0; i < vehicleParts.Count; i++)
		{
			if (vehicleParts[i].GetType() == typeof(VPFuelTank))
			{
				return ((VPFuelTank)vehicleParts[i]).GetMaxFuelLevel();
			}
		}
		return 0f;
	}

	public void SetFuelLevel(float _fuelLevel)
	{
		for (int i = 0; i < vehicleParts.Count; i++)
		{
			if (vehicleParts[i].GetType() == typeof(VPFuelTank))
			{
				((VPFuelTank)vehicleParts[i]).SetFuelLevel(_fuelLevel);
				break;
			}
		}
	}

	public float GetBatteryLevel()
	{
		return 0f;
	}

	public void SetBatteryLevel(float _amount)
	{
	}

	public void AddFuel(float _fuelLevel)
	{
		for (int i = 0; i < vehicleParts.Count; i++)
		{
			if (vehicleParts[i].GetType() == typeof(VPFuelTank))
			{
				((VPFuelTank)vehicleParts[i]).AddFuel(_fuelLevel);
				break;
			}
		}
	}

	public int GetRepairAmountNeeded()
	{
		return GetMaxHealth() - entity.Health;
	}

	public void RepairParts(int _add, float _percent)
	{
		int v = _add + (int)((float)GetMaxHealth() * _percent);
		v = Utils.FastMin(v, GetRepairAmountNeeded());
		entity.Health += v;
	}

	public bool IsDriveable()
	{
		return HasSteering();
	}

	public bool HasEnginePart()
	{
		return FindPart("engine") != null;
	}

	public float GetEngineQualityPercent()
	{
		return 0f;
	}

	public bool HasStorage()
	{
		return FindPart("storage") != null;
	}

	public bool HasSteering()
	{
		return true;
	}

	public bool IsSteeringBroken()
	{
		if (HasSteering())
		{
			return FindPart("handlebars").IsBroken();
		}
		return true;
	}

	public bool HasLock()
	{
		return FindPart("lock") != null;
	}

	public bool IsLockBroken()
	{
		return FindPart("lock").GetHealthPercentage() == 0f;
	}

	public string GetHornSoundName()
	{
		return Properties.GetString("hornSound");
	}

	public bool HasHorn()
	{
		return GetHornSoundName().Length > 0;
	}

	public List<IKController.Target> GetIKTargets(int slot)
	{
		List<IKController.Target> list = new List<IKController.Target>();
		if (slot == 0)
		{
			VehiclePart vehiclePart = FindPart("handlebars");
			if (vehiclePart != null)
			{
				list.AddRange(vehiclePart.ikTargets);
			}
			VehiclePart vehiclePart2 = FindPart("pedals");
			if (vehiclePart2 != null)
			{
				list.AddRange(vehiclePart2.ikTargets);
			}
		}
		VehiclePart vehiclePart3 = FindPart("seat" + slot);
		if (vehiclePart3 != null && vehiclePart3.ikTargets != null)
		{
			list.AddRange(vehiclePart3.ikTargets);
		}
		if (list.Count <= 0)
		{
			return null;
		}
		return list;
	}

	public List<string> GetParticleTransformPaths()
	{
		List<string> list = new List<string>();
		for (int i = 0; i < vehicleParts.Count; i++)
		{
			string property = vehicleParts[i].GetProperty("particle_transform");
			if (property != string.Empty)
			{
				list.Add(property);
			}
		}
		return list;
	}

	public int GetVehicleQuality()
	{
		return itemValue.Quality;
	}

	public int GetHealth()
	{
		int health = entity.Health;
		if (health <= 1)
		{
			return 0;
		}
		return health;
	}

	public int GetMaxHealth()
	{
		return entity.GetMaxHealth();
	}

	public float GetHealthPercent()
	{
		return (float)GetHealth() / (float)entity.GetMaxHealth();
	}

	public float GetPlayerDamagePercent()
	{
		return 0.1f;
	}

	public float GetNoise()
	{
		return 0.5f;
	}

	public void SetLocked(bool isLocked, EntityPlayerLocal player)
	{
		if (!(player == null))
		{
			if (isLocked)
			{
				entity.SetOwner(PlatformManager.InternalLocalUserIdentifier);
				entity.isLocked = true;
				PasswordHash = 0;
			}
			else
			{
				entity.isLocked = false;
				PasswordHash = 0;
			}
		}
	}
}
