using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using Audio;
using HarmonyLib;
using UnityEngine;
using Object = UnityEngine.Object;
using XMLData.Item;

[assembly: CompilationRelaxations(8)]
[assembly: RuntimeCompatibility(WrapNonExceptionThrows = true)]
[assembly: Debuggable(DebuggableAttribute.DebuggingModes.IgnoreSymbolStoreSequencePoints)]
[assembly: AssemblyTitle("LawnTractorCompanion")]
[assembly: AssemblyDescription("7 Days to Die Mod")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("OCBNET")]
[assembly: AssemblyProduct("LawnTractorCompanion")]
[assembly: AssemblyCopyright("Copyright Â© Marcel Greter 2022")]
[assembly: AssemblyTrademark("")]
[assembly: ComVisible(false)]
[assembly: Guid("734c1bfa-b525-474b-b915-1af3327bfe76")]
[assembly: AssemblyFileVersion("1.0.0.0")]
[assembly: AssemblyVersion("1.0.0.0")]
public class LawnTractorCompanion : IModApi
{
	[HarmonyPatch(typeof(XUiM_PlayerInventory))]
	[HarmonyPatch("GetItemCountWithMods")]
	public class PlayerInventoryGetItemCountPatch
	{
		public static bool Prefix(ItemValue _itemValue, ref int __result)
		{
			if (UseBagForItemCount == null)
			{
				return true;
			}
			__result = UseBagForItemCount.GetItemCount(_itemValue, -1, -1, true);
			return false;
		}
	}

	[HarmonyPatch(typeof(ItemValue))]
	[HarmonyPatch("GetPropertyOverride")]
	public class ItemValueGetPropertyOverridePatch
	{
		public static void Prefix(ItemValue __instance, string _propertyName, ref string _originalValue)
		{
			if (!(_propertyName != Block.PropTintColor))
			{
				__instance.ItemClass.Properties.ParseString(Block.PropTintColor, ref _originalValue);
			}
		}
	}

	// 3.2 applies installed parts here. 3.3 removed the method. Prepare skips the patch
	// instead of failing the whole mod when the method is missing.
	[HarmonyPatch(typeof(Vehicle), "SetItemValueMods")]
	public class VehicleSetItemValueModsPatch
	{
		public static bool Prepare()
		{
			return typeof(Vehicle).GetMethod("SetItemValueMods", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic) != null;
		}

		public static void Postfix(Vehicle __instance, ItemValue ___itemValue)
		{
			TractorCompat.ApplyMowerMods(__instance, ___itemValue);
		}
	}

	// 3.3 replacement for SetItemValueMods. Skipped on 3.2, where the old method still runs.
	[HarmonyPatch(typeof(Vehicle), "OnModsChanged")]
	public class VehicleOnModsChangedPatch
	{
		public static bool Prepare()
		{
			const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
			return typeof(Vehicle).GetMethod("SetItemValueMods", flags) == null
				&& typeof(Vehicle).GetMethod("OnModsChanged", flags) != null;
		}

		public static void Postfix(Vehicle __instance, ItemValue ___itemValue)
		{
			TractorCompat.ApplyMowerMods(__instance, ___itemValue);
		}
	}

	[HarmonyPatch(typeof(Vehicle))]
	[HarmonyPatch("SetItemValue")]
	public class VehicleSetItemValuePatch
	{
		public static void Postfix(Vehicle __instance, ItemValue ___itemValue)
		{
			TractorCompat.ApplyMowerMods(__instance, ___itemValue);
		}
	}

	[HarmonyPatch(typeof(VehiclePart))]
	[HarmonyPatch("SetColors")]
	public class VehiclePartSetColorsPatch
	{
		private static void Postfix(VehiclePart __instance, Color _color)
		{
			Transform transform = __instance.GetTransform("paints");
			if (transform == null)
			{
				return;
			}
			Renderer[] renderers = transform.GetComponentsInChildren<Renderer>();
			for (int i = 0; i < renderers.Length; i++)
			{
				renderers[i].material.color = _color;
			}
		}
	}

	[HarmonyPatch(typeof(Block))]
	[HarmonyPatch("CopyDroppedFrom")]
	public class BlockCopyDroppedFromPatch
	{
		public static bool Prefix(Block __instance, Block _other)
		{
			//IL_0045: Unknown result type (might be due to invalid IL or missing references)
			//IL_004a: Unknown result type (might be due to invalid IL or missing references)
			//IL_005b: Unknown result type (might be due to invalid IL or missing references)
			//IL_006c: Unknown result type (might be due to invalid IL or missing references)
			//IL_007e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0081: Invalid comparison between Unknown and I4
			//IL_0086: Unknown result type (might be due to invalid IL or missing references)
			//IL_0153: Unknown result type (might be due to invalid IL or missing references)
			//IL_0110: Unknown result type (might be due to invalid IL or missing references)
			//IL_011e: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
			bool flag = _other.Properties.GetBool("ExtendHarvestDrops");
			bool flag2 = _other.Properties.GetBool("ExtendDestroyDrops");
			if (!flag && !flag2)
			{
				return true;
			}
			foreach (KeyValuePair<EnumDropEvent, List<Block.SItemDropProb>> item in _other.itemsToDrop)
			{
				EnumDropEvent key = item.Key;
				List<Block.SItemDropProb> value = item.Value;
				if (!__instance.itemsToDrop.TryGetValue(key, out var value2))
				{
					value2 = (__instance.itemsToDrop[key] = new List<Block.SItemDropProb>());
				}
				if ((flag && (int)key == 2) || (flag2 && (int)key == 0))
				{
					int count = value2.Count;
					for (int i = 0; i < value.Count; i++)
					{
						bool flag3 = true;
						int num = 0;
						while (flag3 && num < count)
						{
							if (value2[num].name == value[i].name)
							{
								flag3 = false;
								break;
							}
							num++;
						}
						if (flag3)
						{
							value2.Add(value[i]);
						}
					}
					continue;
				}
				for (int j = 0; j < value.Count; j++)
				{
					bool flag4 = true;
					int num2 = 0;
					while (flag4 && num2 < value2.Count)
					{
						if (value2[num2].name == value[j].name)
						{
							flag4 = false;
							break;
						}
						num2++;
					}
					if (flag4)
					{
						value2.Add(value[j]);
					}
				}
			}
			return false;
		}
	}

	[HarmonyPatch(typeof(Vehicle), "SetupPreview")]
	public class VehicleSetupPreviewPatch
	{
		private static bool Prefix(Transform rootT)
		{
			if (rootT == null || rootT.GetComponentInChildren<CopyTransform>(true) == null)
			{
				return true;
			}
			Transform physics = rootT.Find("Physics");
			if (physics != null)
			{
				physics.gameObject.SetActive(false);
			}
			ParticleSystem[] particleSystems = rootT.GetComponentsInChildren<ParticleSystem>(true);
			for (int i = 0; i < particleSystems.Length; i++)
			{
				particleSystems[i].gameObject.SetActive(false);
			}
			Renderer[] renderers = rootT.GetComponentsInChildren<Renderer>(true);
			for (int j = 0; j < renderers.Length; j++)
			{
				if (renderers[j] is ParticleSystemRenderer)
				{
					continue;
				}
				renderers[j].enabled = true;
				renderers[j].gameObject.SetActive(true);
			}
			return false;
		}
	}

	[HarmonyPatch(typeof(EntityVehicle))]
	[HarmonyPatch("ApplyDamage")]
	public class EntityVehicleApplyDamagePatch
	{
		private static void Prefix(EntityVehicle __instance, ref int damage)
		{
			foreach (VehiclePart part in __instance.vehicle.GetParts())
			{
				if (part is VPMower vPMower)
				{
					damage = (int)(vPMower.DamageModifier * (float)damage);
				}
			}
		}
	}

	[HarmonyPatch(typeof(Manager), "Play")]
	[HarmonyPatch(new Type[]
	{
		typeof(Entity),
		typeof(string),
		typeof(float),
		typeof(bool)
	})]
	public class AudioManagerPlayPatch
	{
		private static void Prefix(Entity _entity, string soundGroupName, float volumeScale, bool wantHandle, ref bool __state)
		{
			if (_entity != null && _entity.entityId >= 0 && soundGroupName == "lawnmower_plant" && GameManager.Instance != null && GameManager.Instance.World != null)
			{
				Entity entity = ((WorldBase)GameManager.Instance.World).GetEntity(_entity.entityId);
				EntityAlive val = entity as EntityAlive;
				if (val != null)
				{
					__state = val.Crouching;
					val.Crouching = true;
				}
			}
		}

		private static void Postfix(Entity _entity, string soundGroupName, float volumeScale, bool wantHandle, bool __state)
		{
			if (_entity.entityId >= 0 && soundGroupName == "lawnmower_plant")
			{
				Entity entity = ((WorldBase)GameManager.Instance.World).GetEntity(_entity.entityId);
				EntityAlive val = (EntityAlive)(object)((entity is EntityAlive) ? entity : null);
				if (val != null)
				{
					val.Crouching = __state;
				}
			}
		}
	}


	[HarmonyPatch(typeof(RecipeUnlockData))]
	[HarmonyPatch("GetLevel")]
	public class RecipeUnlockDataGetLevelPatch
	{
		public static bool Prefix(RecipeUnlockData __instance, string recipeName, ref string __result)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0007: Invalid comparison between Unknown and I4
			if ((int)__instance.unlockType == 1)
			{
				string text = "UnlockFor" + char.ToUpper(__instance.perk.Name[0]) + __instance.perk.Name.Substring(1);
				ItemClass itemClass = ItemClass.GetItemClass(recipeName, false);
				if (itemClass != null)
				{
					string text2 = itemClass.Properties.GetString(text);
					if (string.IsNullOrEmpty(text2))
					{
						return true;
					}
					__result = text2;
					return false;
				}
			}
			return true;
		}
	}

	public static Bag UseBagForItemCount;

	public static bool IsTractor(Vehicle vehicle)
	{
		return vehicle != null && vehicle.GetName() == "vehiclelawntractor";
	}

	public static bool IsTractor(EntityVehicle entity)
	{
		if (entity == null)
		{
			return false;
		}
		EntityClass entityClass = EntityClass.list[entity.entityClass];
		return entityClass != null && entityClass.entityClassName == "vehicleLawnTractor";
	}

	private static Type PartType(string className)
	{
		if (className == null)
		{
			return null;
		}
		if (className.StartsWith("Mower,"))
		{
			return typeof(VPMower);
		}
		if (className == "Chassis")
		{
			return typeof(VPChassis);
		}
		if (className == "Engine")
		{
			return typeof(VPEngine);
		}
		if (className == "Headlight")
		{
			return typeof(VPHeadlight);
		}
		if (className == "FuelTank")
		{
			return typeof(VPFuelTank);
		}
		if (className == "Steering")
		{
			return typeof(VPSteering);
		}
		if (className == "Seat")
		{
			return typeof(VPSeat);
		}
		if (className == "Storage")
		{
			return typeof(VPStorage);
		}
		if (className == "Wheel")
		{
			return typeof(VPWheel);
		}
		return null;
	}

	public static bool TypeLookup(string _name, ref Type __result)
	{
		if (_name != null && _name.StartsWith("Mower,"))
		{
			__result = typeof(VPMower);
			return false;
		}
		return true;
	}

	public static bool CreatePartsEnter(Vehicle __instance)
	{
		if (!IsTractor(__instance))
		{
			return true;
		}
		DynamicProperties properties = __instance.Properties;
		if (properties == null)
		{
			return false;
		}
		AccessTools.Method(typeof(Vehicle), "ParseGeneralProperties").Invoke(__instance, new object[] { properties });
		foreach (KeyValuePair<string, DynamicProperties> pair in properties.Classes)
		{
			DynamicProperties value = pair.Value;
			string text = value.GetString("class");
			if (text.Length <= 0)
			{
				continue;
			}
			try
			{
				Type type = PartType(text);
				if (type == null)
				{
					Log.Warning("OCB missing part type " + text);
					continue;
				}
				VehiclePart vehiclePart = (VehiclePart)Activator.CreateInstance(type);
				vehiclePart.SetVehicle(__instance);
				vehiclePart.SetTag(pair.Key);
				if (vehiclePart is VPMower mowerPart)
				{
					mowerPart.ApplyProperties(value);
				}
				else
				{
					vehiclePart.SetProperties(value);
				}
				__instance.vehicleParts.Add(vehiclePart);
			}
			catch (Exception ex)
			{
				Log.Error("OCB create part failed " + text + " " + ex);
				throw;
			}
		}
		if (__instance.entity != null)
		{
			CopyEngineParticles(__instance.entity);
		}
		for (int i = 0; i < __instance.vehicleParts.Count; i++)
		{
			if (!(__instance.vehicleParts[i] is VPMower))
			{
				__instance.vehicleParts[i].InitPrefabConnections();
			}
		}
		return false;
	}

	public static void EntityInitLeave(EntityVehicle __instance)
	{
		if (!IsTractor(__instance))
		{
			return;
		}
		CopyEngineParticles(__instance);
	}

	public static void CopyEngineParticles(EntityVehicle entity)
	{
		int copied = 0;
		MonoBehaviour[] behaviours = entity.GetComponentsInChildren<MonoBehaviour>(true);
		for (int i = 0; i < behaviours.Length; i++)
		{
			MonoBehaviour behaviour = behaviours[i];
			if (behaviour != null && behaviour.GetType().Name == "CopyTransform" && CopyEngineParticle(behaviour))
			{
				copied++;
			}
		}
		if (copied > 0 && entity.vehicle != null && entity.vehicle.FindPart("engine") is VPEngine engine)
		{
			engine.ParticleEffectUpdate();
		}
	}

	private static bool CopyEngineParticle(Component component)
	{
		Type type = component.GetType();
		FieldInfo addedField = type.GetField("added", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
			?? type.GetField("Added", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		if (addedField != null && addedField.FieldType == typeof(bool) && (bool)addedField.GetValue(component))
		{
			return false;
		}
		string prefab = type.GetField("Prefab")?.GetValue(component) as string;
		string path = type.GetField("Path")?.GetValue(component) as string;
		if (string.IsNullOrEmpty(prefab))
		{
			return false;
		}
		if (addedField != null && addedField.FieldType == typeof(bool))
		{
			addedField.SetValue(component, true);
		}
		GameObject asset = DataLoader.LoadAsset<GameObject>(prefab);
		if (asset == null)
		{
			Log.Warning("OCB particle copy missing asset " + prefab);
			return false;
		}
		Transform source = string.IsNullOrEmpty(path) ? null : asset.transform.Find(path);
		if (source == null)
		{
			Log.Warning("OCB particle copy missing transform " + path);
			return false;
		}
		string name = type.GetField("Name")?.GetValue(component) as string;
		Vector3 position = Vector3.zero;
		Vector3 scale = Vector3.one;
		object positionValue = type.GetField("Position")?.GetValue(component);
		object scaleValue = type.GetField("Scale")?.GetValue(component);
		if (positionValue is Vector3 positionVector)
		{
			position = positionVector;
		}
		if (scaleValue is Vector3 scaleVector)
		{
			scale = scaleVector;
		}
		GameObject copy = Object.Instantiate(source.gameObject);
		copy.name = string.IsNullOrEmpty(name) ? source.name : name;
		copy.transform.SetParent(component.transform, false);
		copy.transform.localPosition = position;
		copy.transform.localScale = scale;
		copy.SetActive(true);
		return true;
	}

	private static bool SkipOriginalAwake()
	{
		return false;
	}

	private static void SuppressOriginalMod(Harmony harmony)
	{
		Harmony.UnpatchID("OcbLawnMowing");
		foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
		{
			if (assembly.GetName().Name != "LawnMowing")
			{
				continue;
			}
			Type copyTransform = assembly.GetType("CopyTransform");
			MethodInfo awake = copyTransform?.GetMethod("Awake", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			if (awake != null)
			{
				harmony.Patch(awake, new HarmonyMethod(typeof(LawnTractorCompanion), nameof(SkipOriginalAwake)));
			}
		}
	}

	public void InitMod(Mod mod)
	{
		TractorCompat.Initialize();
		if (!OcbLegacyXml.OriginalModLoaded())
		{
			Log.Error("[LawnTractorV3Fix] OcbLawnMowing is not loaded. This mod only patches that mod and will not run.");
			return;
		}
		Harmony harmony = new Harmony(GetType().ToString());
		OcbLegacyXml.Install(harmony, mod);
		SuppressOriginalMod(harmony);
		harmony.PatchAll(Assembly.GetExecutingAssembly());
		HarmonyMethod createParts = new HarmonyMethod(typeof(LawnTractorCompanion), nameof(CreatePartsEnter));
		createParts.priority = Priority.First;
		harmony.Patch(AccessTools.Method(typeof(Vehicle), "CreateParts"), createParts);
		harmony.Patch(AccessTools.Method(typeof(EntityVehicle), "Init"), null, new HarmonyMethod(typeof(LawnTractorCompanion), nameof(EntityInitLeave)));
	}
}

// HUDPlus asks these bindings for the two icons beside the vehicle cluster.
[HarmonyPatch]
internal static class TractorHudStatusBindings
{
	private const string LightsBinding = "ocbTractorLightsOn";
	private const string MowingBinding = "ocbTractorMowingOn";

	private static bool sampled;
	private static bool lastLights;
	private static bool lastMowing;
	private static int dirtyFrame = -1;

	[HarmonyPatch(typeof(XUiC_HUDStatBar), "GetBindingValueInternal")]
	[HarmonyPrefix]
	private static bool GetBindingValuePrefix(XUiC_HUDStatBar __instance, ref bool __result, ref string _value, string _bindingName)
	{
		if (_bindingName != LightsBinding && _bindingName != MowingBinding)
		{
			return true;
		}

		ReadStatus(__instance, out bool lightsOn, out bool mowingOn);
		_value = (_bindingName == LightsBinding ? lightsOn : mowingOn) ? "true" : "false";
		__result = true;
		return false;
	}

	[HarmonyPatch(typeof(XUiC_HUDStatBar), "hasChanged")]
	[HarmonyPostfix]
	private static void HasChangedPostfix(XUiC_HUDStatBar __instance, ref bool __result)
	{
		if (__instance == null || __instance.statGroup != HUDStatGroups.Vehicle)
		{
			return;
		}

		ReadStatus(__instance, out bool lightsOn, out bool mowingOn);
		if (!sampled || lightsOn != lastLights || mowingOn != lastMowing)
		{
			sampled = true;
			lastLights = lightsOn;
			lastMowing = mowingOn;
			dirtyFrame = Time.frameCount;
		}

		if (Time.frameCount == dirtyFrame)
		{
			__result = true;
		}
	}

	private static void ReadStatus(XUiC_HUDStatBar statBar, out bool lightsOn, out bool mowingOn)
	{
		lightsOn = false;
		mowingOn = false;
		EntityVehicle entity = statBar != null ? statBar.vehicle : null;
		if (entity == null && statBar != null)
		{
			EntityPlayerLocal player = statBar.localPlayer ?? statBar.xui?.playerUI?.entityPlayer;
			entity = player != null ? player.AttachedToEntity as EntityVehicle : null;
		}

		if (!LawnTractorCompanion.IsTractor(entity))
		{
			return;
		}

		Vehicle vehicle = entity.GetVehicle();
		if (vehicle == null)
		{
			return;
		}

		if (vehicle.FindPart("headlight") is VPHeadlight headlight)
		{
			lightsOn = headlight.IsOn();
		}

		if (vehicle.FindPart("mower") is VPMower mower)
		{
			mowingOn = mower.IsOn;
		}
	}
}

// The original mower still calls StopUIInteraction after a harvest. That method
// starts by touching the primary player UI, which does not exist on a dedicated server.
[HarmonyPatch(typeof(EntityVehicle), "StopInteraction")]
internal static class Patch_VehicleStopInteractionNoLocalUi
{
	public static bool Prefix(EntityVehicle __instance, ushort syncFlags)
	{
		if (LocalPlayerUI.GetUIForPrimaryPlayer() != null)
		{
			return true;
		}

		if (syncFlags != 0)
		{
			__instance.SendSyncData(syncFlags);
		}

		return false;
	}
}

// Closing the mower bag asks the server to drop a lock the player never held.
[HarmonyPatch(typeof(LockManager), nameof(LockManager.UnlockRequestServer))]
[HarmonyPriority(Priority.Last)]
internal static class Patch_LockManager_EmptyUnlock
{
	public static bool Prefix(LockManager __instance, int _playerId)
	{
		if (SingletonMonoBehaviour<ConnectionManager>.Instance == null || !SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			return true;
		}

		HashSet<int> held = AccessTools.Field(typeof(LockManager), "playersWithLocks")?.GetValue(__instance) as HashSet<int>;
		if (held == null)
		{
			return true;
		}

		return held.Contains(_playerId);
	}
}

// Single-player only registers these decorations for clients. The host then warns
// every time a car or other terrain-aligned model is shown again after a chunk rebuild.
[HarmonyPatch(typeof(MultiBlockManager), "SetTerrainAlignmentDirty", new System.Type[] { typeof(Vector3i) })]
internal static class Patch_TerrainAlignmentRegisterOnHost
{
	private static bool Prefix(Vector3i worldPos)
	{
		World world = GameManager.Instance != null ? GameManager.Instance.World : null;
		if (world == null)
		{
			return false;
		}

		BlockValue blockValue = world.GetBlock(worldPos);
		if (blockValue.isair || blockValue.ischild || blockValue.Block == null || blockValue.Block.terrainAlignmentMode == TerrainAlignmentMode.None)
		{
			return false;
		}

		return MultiBlockManager.Instance.TryRegisterTerrainAlignedBlock(worldPos, blockValue);
	}
}

internal class HarmonyFieldProxy<T>
{
	private readonly FieldInfo Field;

	public HarmonyFieldProxy(Type type, string name)
	{
		Field = AccessTools.Field(type, name);
	}

	public T Get(object instance)
	{
		return (T)Field.GetValue(instance);
	}

	public void Set(object instance, T value)
	{
		Field.SetValue(instance, value);
	}
}
public class VPMower : VehiclePart
{
	public bool IsOn;

	private float LastBlade;

	private float LastPlant;

	private float BladeInterval = 1f / 30f;

	private float PlantInterval = 0.125f;

	private float FuelUsePerSecond = 0.01f;

	private Vector2i Area = Vector2i.one;

	private Vector2i Reach = new Vector2i(-1, 2);

	private Handle loop;

	private float LoopFadeIn;

	private float LoopFaded;

	private int ClickCount = -1;

	public float DamageModifier = 1f;

	private float LastLightState;

	private Color ColorBrakeLight = new Color(0.89f, 0.09f, 0.03f, 1f);

	private Color ColorMowerOn = new Color(0.09f, 0.89f, 0.03f, 1f);

	private float LastBrakes;

	private static Vector3 particleOffset = new Vector3(0f, 0.15f, 0f);

	private bool DoReseed;

	private bool ProtectGrowingPlants;

	private readonly List<Material> Materials = new List<Material>();

	private readonly HashSet<string> HarvestTags = new HashSet<string>();

	private readonly HashSet<string> HarvestTools = new HashSet<string>();

	private HashSet<string> OldShownModPhysics = new HashSet<string>();

	private HashSet<string> NewShownModPhysics = new HashSet<string>();

	private HashSet<string> OldShownModTransforms = new HashSet<string>();

	private HashSet<string> NewShownModTransforms = new HashSet<string>();

	private static readonly HarmonyFieldProxy<float> WheelBrakes = new HarmonyFieldProxy<float>(typeof(EntityVehicle), "wheelBrakes");

	public override void InitPrefabConnections()
	{
	}

	public void ApplyProperties(DynamicProperties value)
	{
		properties = value;
		if (value == null)
		{
			return;
		}
		Area = ReadPair(value.GetString("area"), Area);
		Reach = ReadPair(value.GetString("reach"), Reach);
		float blade = BladeInterval;
		float plant = PlantInterval;
		float fuel = FuelUsePerSecond;
		if (float.TryParse(value.GetString("blade_interval"), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out blade))
		{
			BladeInterval = blade;
		}
		if (float.TryParse(value.GetString("plant_interval"), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out plant))
		{
			PlantInterval = plant;
		}
		if (float.TryParse(value.GetString("fuel_per_second"), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out fuel))
		{
			FuelUsePerSecond = fuel;
		}
	}

	private static Vector2i ReadPair(string text, Vector2i fallback)
	{
		if (string.IsNullOrEmpty(text))
		{
			return fallback;
		}
		string[] parts = text.Split(',');
		int x;
		int y;
		if (parts.Length < 2 || !int.TryParse(parts[0].Trim(), out x) || !int.TryParse(parts[1].Trim(), out y))
		{
			return fallback;
		}
		return new Vector2i(x, y);
	}

	public override void SetProperties(DynamicProperties value)
	{
		ApplyProperties(value);
	}

	private static string[] SplitAndTrim(string str, char delim)
	{
		string[] array = str.Split(new char[1] { delim }, StringSplitOptions.RemoveEmptyEntries);
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = array[i].Trim();
		}
		return array;
	}

	public void UpdateModifications(ItemValue[] modifications)
	{
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		Materials.Clear();
		HarvestTags.Clear();
		HarvestTools.Clear();
		NewShownModPhysics.Clear();
		NewShownModTransforms.Clear();
		if (base.vehicle == null)
		{
			return;
		}
		Transform meshTransform = base.vehicle.GetMeshTransform();
		Renderer[] array = ((meshTransform != null) ? ((Component)meshTransform).GetComponentsInChildren<Renderer>(true) : null);
		if (array != null)
		{
			foreach (Renderer val in array)
			{
				if ((Object)(object)val != (Object)null && !Materials.Contains(val.material))
				{
					Materials.Add(val.material);
				}
			}
		}
		foreach (Material material in Materials)
		{
			material.SetVector("_BrakeColor", (Vector4)(Color.black));
		}
		string str = default;
		string str2 = default;
		string str3 = default;
		string str4 = default;
		if (modifications != null)
		{
		foreach (ItemValue obj in modifications)
		{
			DynamicProperties val2 = ((obj != null) ? obj.ItemClass : null)?.Properties;
			Dictionary<string, string> val3 = val2?.Values;
			if (val3 == null)
			{
				continue;
			}
			val2.ParseFloat("DamageModifier", ref DamageModifier);
			if (val3.TryGetValue("MowerHarvestTags", out str))
			{
				string[] array2 = SplitAndTrim(str, ',');
				foreach (string item in array2)
				{
					HarvestTags.Add(item);
				}
			}
			if (val3.TryGetValue("MowerHarvestTools", out str2))
			{
				string[] array2 = SplitAndTrim(str2, ',');
				foreach (string item2 in array2)
				{
					HarvestTools.Add(item2);
				}
			}
			if (val3.TryGetValue("EnablePhysics", out str3))
			{
				string[] array2 = SplitAndTrim(str3, ',');
				foreach (string item3 in array2)
				{
					NewShownModPhysics.Add(item3);
				}
			}
			if (val3.TryGetValue("ShowTransforms", out str4))
			{
				string[] array2 = SplitAndTrim(str4, ',');
				foreach (string item4 in array2)
				{
					NewShownModTransforms.Add(item4);
				}
			}
		}
		}
		if (OldShownModPhysics.Count > 0 || NewShownModPhysics.Count > 0)
		{
			Transform val4 = (base.vehicle != null && (Object)(object)base.vehicle.entity != (Object)null) ? ((Entity)base.vehicle.entity).PhysicsTransform : null;
			if ((Object)(object)val4 == (Object)null)
			{
				return;
			}
			foreach (string oldShownModPhysic in OldShownModPhysics)
			{
				if (!NewShownModPhysics.Contains(oldShownModPhysic))
				{
					Transform val5 = val4.Find(oldShownModPhysic);
					if (val5 != null)
					{
						((Component)val5).gameObject.SetActive(false);
					}
				}
			}
			foreach (string newShownModPhysic in NewShownModPhysics)
			{
				Transform val6 = val4.Find(newShownModPhysic);
				if (val6 != null)
				{
					((Component)val6).gameObject.SetActive(true);
				}
			}
			HashSet<string> oldShownModPhysics = OldShownModPhysics;
			HashSet<string> newShownModPhysics = NewShownModPhysics;
			NewShownModPhysics = oldShownModPhysics;
			OldShownModPhysics = newShownModPhysics;
		}
		if (OldShownModTransforms.Count > 0 || NewShownModTransforms.Count > 0)
		{
			Transform meshTransform2 = base.vehicle.GetMeshTransform();
			if (meshTransform2 == null)
			{
				return;
			}
			foreach (string oldShownModTransform in OldShownModTransforms)
			{
				if (!NewShownModTransforms.Contains(oldShownModTransform))
				{
					Transform val7 = meshTransform2.Find(oldShownModTransform);
					if (val7 != null)
					{
						((Component)val7).gameObject.SetActive(false);
					}
				}
			}
			foreach (string newShownModTransform in NewShownModTransforms)
			{
				Transform val8 = meshTransform2.Find(newShownModTransform);
				if (val8 != null)
				{
					((Component)val8).gameObject.SetActive(true);
				}
			}
			HashSet<string> newShownModPhysics = OldShownModTransforms;
			HashSet<string> oldShownModPhysics = NewShownModTransforms;
			NewShownModTransforms = newShownModPhysics;
			OldShownModTransforms = oldShownModPhysics;
		}
		ProtectGrowingPlants = HarvestTags.Contains("growProtector");
		DoReseed = HarvestTags.Contains("growReseed");
	}

	private void StopMower()
	{
		if (base.vehicle != null)
		{
			if (loop != null && base.vehicle != null)
			{
				loop.Stop(((Entity)base.vehicle.entity).entityId);
				loop = null;
			}
			PlaySound(base.properties.Values["sound_shut_off"]);
		}
	}

	private void StartMower()
	{
		if (loop == null && base.vehicle != null && base.vehicle.GetHealth() > 0 && !((double)base.vehicle.GetFuelLevel() <= 0.0))
		{
			PlaySound(base.properties.Values["sound_start"]);
			string text = base.properties.GetString("sound_loop");
			loop = Manager.Play((Entity)(object)base.vehicle.entity, text, 1f, true);
			if (loop != null)
			{
				loop.SetVolume(0f);
				LoopFadeIn = 2f;
				LoopFaded = 0f;
			}
		}
	}

	private void EnableMower(bool enable)
	{
		if (base.vehicle == null)
		{
			return;
		}
		if (base.vehicle.GetHealth() <= 0)
		{
			enable = false;
		}
		if ((double)base.vehicle.GetFuelLevel() <= 0.0)
		{
			enable = false;
		}
		if (IsOn == enable)
		{
			return;
		}
		IsOn = enable;
		if (!((Object)(object)base.vehicle?.entity == (Object)null) && !((Entity)base.vehicle.entity).isEntityRemote)
		{
			if (enable)
			{
				StartMower();
			}
			else
			{
				StopMower();
			}
		}
	}

	private void PlaySound(string _sound)
	{
		if (base.vehicle != null && !((Object)(object)base.vehicle.entity == (Object)null) && !((Entity)base.vehicle.entity).isEntityRemote)
		{
			((Entity)base.vehicle.entity).PlayOneShot(_sound, false, false, false, (AnimationEvent)null, 1f);
		}
	}

	public override void HandleEvent(Event evt, VehiclePart part, float arg)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Invalid comparison between Unknown and I4
		if ((int)evt != 1)
		{
			return;
		}
		if (LastLightState != arg)
		{
			LastLightState = arg;
			if (ClickCount == 3)
			{
				ClickCount = 0;
			}
			else
			{
				ClickCount++;
			}
			EnableMower(ClickCount == 1 || ClickCount == 2);
		}
		ToggleEmission((double)arg != 0.0);
	}

	public override void HandleEvent(Vehicle.Event _event, float _arg)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Invalid comparison between Unknown and I4
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Invalid comparison between Unknown and I4
		if ((int)_event != 1)
		{
			if ((int)_event == 3 && IsOn)
			{
				StopMower();
			}
		}
		else if (IsOn)
		{
			StartMower();
		}
	}

	private void ToggleEmission(bool state)
	{
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		if (base.vehicle == null)
		{
			return;
		}
		if (((VehiclePart)this).IsBroken())
		{
			state = false;
		}
		Transform meshTransform = base.vehicle.GetMeshTransform();
		if ((Object)(object)meshTransform == (Object)null)
		{
			return;
		}
		Renderer[] array = ((meshTransform != null) ? ((Component)meshTransform).GetComponentsInChildren<Renderer>(true) : null);
		foreach (Renderer val in array)
		{
			if (!Materials.Contains(val.material))
			{
				Materials.Add(val.material);
			}
		}
		foreach (Material material in Materials)
		{
			if (!((Object)(object)material == (Object)null))
			{
				if (state)
				{
					material.EnableKeyword("EMISSION_ON");
				}
				else
				{
					material.DisableKeyword("EMISSION_ON");
				}
				Color val2 = (IsOn ? ColorMowerOn : Color.black);
				material.SetColor("_MowerOnColor", val2);
			}
		}
	}

	public override void Update(float dt)
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_01df: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0472: Unknown result type (might be due to invalid IL or missing references)
		//IL_044f: Unknown result type (might be due to invalid IL or missing references)
		//IL_042c: Unknown result type (might be due to invalid IL or missing references)
		//IL_022f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0230: Unknown result type (might be due to invalid IL or missing references)
		//IL_0232: Unknown result type (might be due to invalid IL or missing references)
		//IL_0237: Unknown result type (might be due to invalid IL or missing references)
		//IL_023c: Unknown result type (might be due to invalid IL or missing references)
		//IL_023f: Unknown result type (might be due to invalid IL or missing references)
		//IL_024b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0250: Unknown result type (might be due to invalid IL or missing references)
		//IL_032c: Unknown result type (might be due to invalid IL or missing references)
		//IL_032d: Unknown result type (might be due to invalid IL or missing references)
		//IL_032f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0334: Unknown result type (might be due to invalid IL or missing references)
		//IL_0336: Unknown result type (might be due to invalid IL or missing references)
		//IL_0340: Expected Obj, but got Unknown
		//IL_0347: Unknown result type (might be due to invalid IL or missing references)
		//IL_0315: Unknown result type (might be due to invalid IL or missing references)
		//IL_0287: Unknown result type (might be due to invalid IL or missing references)
		//IL_028c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0323: Unknown result type (might be due to invalid IL or missing references)
		//IL_0328: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d1: Expected Obj, but got Unknown
		if (base.vehicle == null)
		{
			return;
		}
		float num = WheelBrakes.Get(base.vehicle.entity);
		if (num != LastBrakes)
		{
			Color val = ColorBrakeLight * num * 0.5f;
			foreach (Material material in Materials)
			{
				if (!((Object)(object)material == (Object)null))
				{
					material.SetVector("_BrakeColor", (Vector4)(val));
				}
			}
			LastBrakes = num;
		}
		if (((VehiclePart)this).IsBroken() && IsOn)
		{
			ToggleEmission(IsOn = false);
		}
		if (!IsOn)
		{
			return;
		}
		if (loop != null && LoopFaded < LoopFadeIn)
		{
			LoopFaded += dt;
			if (LoopFaded > LoopFadeIn)
			{
				LoopFaded = LoopFadeIn;
			}
			loop.SetVolume(LoopFaded / LoopFadeIn);
		}
		LastBlade += dt;
		LastPlant += dt;
		if (LastBlade < BladeInterval)
		{
			return;
		}
		LastBlade %= BladeInterval;
		VehiclePart val2 = base.vehicle.FindPart("engine");
		VPEngine val3 = (VPEngine)(object)((val2 is VPEngine) ? val2 : null);
		if (val3 != null && !val3.isRunning)
		{
			return;
		}
		if (FuelUsePerSecond > 0f)
		{
			base.vehicle.FireEvent((Event)3, (VehiclePart)(object)this, FuelUsePerSecond * BladeInterval);
		}
		Vector3i blockPosition = ((Entity)base.vehicle.entity).GetBlockPosition();
		List<BlockChangeInfo> list = new List<BlockChangeInfo>();
		World world = ((Entity)base.vehicle.entity).world;
		GameRandom gameRandom = ((WorldBase)world).GetGameRandom();
		bool flag = base.vehicle.HasStorage();
		bool flag2 = false;
		Vector3i zero = Vector3i.zero;
		zero.x = -Area.x;
		while (zero.x <= Area.x)
		{
			zero.z = -Area.y;
			while (zero.z <= Area.y)
			{
				zero.y = Reach.x;
				for (; zero.y <= Reach.y; zero.y++)
				{
					BlockValue block = ((WorldBase)world).GetBlock(blockPosition + zero);
					if (!ShouldMowDown(block))
					{
						continue;
					}
					BlockValue val4 = BlockValue.Air;
					// Vanilla hidden mushrooms already name their sprout as DowngradeBlock.
					// Place that sprout. Do not ask the basket for it.
					bool flag3 = BlockValue.Air.type != block.Block.DowngradeBlock.type;
					if (flag3)
					{
						val4 = block.Block.DowngradeBlock;
					}
					else if (DoReseed)
					{
						string cropReplacement = GetCropReplacement(block.Block);
						if (cropReplacement != null)
						{
							val4 = Block.GetBlockValue(cropReplacement, false);
						}
						if (val4.type != BlockValue.Air.type)
						{
							if (LastPlant < PlantInterval)
							{
								continue;
							}
							LastPlant = 0f;
						}
					}
					if (flag)
					{
						flag2 |= HarvestBlockToBag(block.Block, ((EntityAlive)base.vehicle.entity).bag, gameRandom);
						if (!flag3)
						{
							if (DecrementBagItem(((EntityAlive)base.vehicle.entity).bag, val4))
							{
								flag2 = true;
							}
							else
							{
								val4 = BlockValue.Air;
							}
						}
					}
					list.Add(new BlockChangeInfo(new BlockValueRef(blockPosition + zero), val4));
					string destroyParticle = block.Block.GetDestroyParticle(block);
					string text = block.Block.blockMaterial.SurfaceCategory + "destroy";
					if (text == "plantdestroy")
					{
						text = "lawnmower_plant";
					}
					else if (text == "stonedestroy")
					{
						text = "lawnmower_stone";
					}
					ParticleEffect val5 = new ParticleEffect("blockdestroy_" + destroyParticle, World.blockToTransformPos(blockPosition + zero) + particleOffset, 15f, Color.white, text, (Transform)null, true, 1f, null);
					((WorldBase)world).GetGameManager().SpawnParticleEffectServer(val5, ((Entity)base.vehicle.entity).entityId, true, true);
					int num2 = block.Block.Properties.GetInt("LawnMowerDamage");
					if (num2 > 0)
					{
						base.vehicle.entity.ApplyDamage(num2);
					}
				}
				zero.z++;
			}
			zero.x++;
		}
		if (list.Count != 0)
		{
			((WorldBase)((Entity)base.vehicle.entity).world).SetBlocksRPC(list);
			if (flag2)
			{
				// Bag sync only. StopUIInteraction also clears the local UI and asks for an unlock.
				// A dedicated server has no local UI, so that call null-refs and logs "nothing to unlock".
				base.vehicle.entity.SetBagModified();
			}
		}
	}

	private string GetCropReplacement(Block block)
	{
		string blockName = block.GetBlockName();
		if (blockName.EndsWith("3HarvestPlayer"))
		{
			return blockName.Substring(0, blockName.Length - 14) + "1";
		}
		if (blockName.EndsWith("HarvestPlayer"))
		{
			return blockName.Substring(0, blockName.Length - 13);
		}
		string result = default;
		if (block.Properties.Values.TryGetValue("CropReplacement", out result))
		{
			return result;
		}
		return null;
	}

	private bool ShouldMowDown(BlockValue block)
	{
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		if (block.ischild)
		{
			return false;
		}
		if (block.isair)
		{
			return false;
		}
		if (IsPlayerPlant(block) || IsHiddenSprout(block))
		{
			return false;
		}
		string value = default;
		if (block.Block.Properties.Values.TryGetValue("EnableMowing", out value))
		{
			return bool.Parse(value);
		}
		if (block.Block.blockMaterial.StabilitySupport)
		{
			return false;
		}
		if (ProtectGrowingPlants && IsGrowingPlant(block))
		{
			return false;
		}
		return block.Block.blockMaterial.SurfaceCategory == "plant";
	}

	private bool IsGrowingPlant(BlockValue block)
	{
		return ((object)block.Block).GetType().Name.StartsWith("BlockPlantGrowing");
	}

	private bool IsPlayerPlant(BlockValue block)
	{
		string blockName = block.Block.GetBlockName();
		if (string.IsNullOrEmpty(blockName) || !blockName.StartsWith("planted") || blockName.StartsWith("plantedtreeGrass"))
		{
			return false;
		}
		// Seed, mid growth, and the player harvest stage. Wild POI harvest stays mowable.
		if (blockName.EndsWith("3HarvestPlayer") || blockName.EndsWith("PlantPlayer"))
		{
			return true;
		}
		return IsGrowingPlant(block);
	}

	private bool IsHiddenSprout(BlockValue block)
	{
		string model = default;
		if (!block.Block.Properties.Values.TryGetValue("Model", out model) || string.IsNullOrEmpty(model))
		{
			return false;
		}
		return model.IndexOf("SproutNoShow", StringComparison.Ordinal) >= 0;
	}

	private bool DecrementBagItem(Bag bag, BlockValue bv)
	{
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Expected Obj, but got Unknown
		if (bv.isair)
		{
			return false;
		}
		ItemValue val = bv.ToItemValue();
		bool flag = bag.DecItem(val, 1, true, (IList<ItemStack>)null) != 0;
		LawnTractorCompanion.UseBagForItemCount = bag;
		LocalPlayerUI uIForPrimaryPlayer = LocalPlayerUI.GetUIForPrimaryPlayer();
		XUiC_CollectedItemList obj = ((uIForPrimaryPlayer == null) ? null : uIForPrimaryPlayer.xui?.CollectedItemList);
		if (obj != null)
		{
			obj.AddItemStack(new ItemStack(val, flag ? (-1) : 0), false);
		}
		LawnTractorCompanion.UseBagForItemCount = null;
		return flag;
	}

	private bool HarvestBlockToBag(Block block, Bag bag, GameRandom random)
	{
		bool flag = false;
		if (bag == null)
		{
			return flag;
		}
		if (block.itemsToDrop.TryGetValue((EnumDropEvent)2, out var value))
		{
			flag |= AddDropToBag(value, bag, random);
		}
		if (block.itemsToDrop.TryGetValue((EnumDropEvent)0, out var value2))
		{
			flag |= AddDropToBag(value2, bag, random);
		}
		return flag;
	}

	private bool AddDropToBag(List<Block.SItemDropProb> drops, Bag bag, GameRandom random)
	{
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		bool flag = false;
		if (bag == null)
		{
			return flag;
		}
		if (drops == null)
		{
			return flag;
		}
		if (drops.Count == 0)
		{
			return flag;
		}
		Vehicle vehicle = base.vehicle;
		object obj;
		if (vehicle == null)
		{
			obj = null;
		}
		else
		{
			EntityVehicle entity = vehicle.entity;
			obj = ((entity != null) ? ((Entity)entity).GetAttachedPlayerLocal() : null);
		}
		EntityPlayerLocal val = (EntityPlayerLocal)obj;
		foreach (Block.SItemDropProb drop in drops)
		{
			if (HarvestTags.Contains(drop.tag) && (string.IsNullOrEmpty(drop.toolCategory) || HarvestTools.Contains(drop.toolCategory)) && !(random.RandomDouble > (double)drop.prob))
			{
				int num = ((drop.minCount == drop.maxCount) ? drop.maxCount : random.RandomRange(drop.minCount, drop.maxCount + 1));
				float value = EffectManager.GetValue((PassiveEffects)141, (ItemValue)null, (float)num, (EntityAlive)(object)val, (Recipe)null, FastTags<TagGroup.Global>.Parse(drop.tag), true, true, true, true, true, 1, true, false);
				num = (int)Math.Round((float)num * value);
				if (num > 0)
				{
					flag |= AddItemsToBag(bag, ItemClass.GetItem(drop.name, false), num);
				}
			}
		}
		return flag;
	}

	private bool AddItemsToBag(Bag bag, ItemValue iv, int count)
	{
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Expected Obj, but got Unknown
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_012f: Expected Obj, but got Unknown
		if (iv == null)
		{
			return false;
		}
		if (bag == null)
		{
			return false;
		}
		if (count <= 0)
		{
			return false;
		}
		bool flag = false;
		ItemStack[] slots = TractorCompat.GetSlots(bag);
		if (slots == null)
		{
			return false;
		}
		int addedType = TractorCompat.ItemType(iv);
		Vehicle vehicle = base.vehicle;
		object obj;
		if (vehicle == null)
		{
			obj = null;
		}
		else
		{
			EntityVehicle entity = vehicle.entity;
			obj = ((entity != null) ? ((Entity)entity).GetAttachedPlayerLocal() : null);
		}
		LocalPlayerUI uIForPlayer = LocalPlayerUI.GetUIForPlayer((EntityPlayerLocal)obj);
		XUiC_CollectedItemList val = ((uIForPlayer == null) ? null : uIForPlayer.xui?.CollectedItemList);
		LawnTractorCompanion.UseBagForItemCount = bag;
		int num = 0;
		while (count > 0 && num < slots.Length)
		{
			ItemStack slot = slots[num];
			ItemValue slotValue = TractorCompat.StackValue(slot);
			if (TractorCompat.ItemType(slotValue) == addedType)
			{
				int num2 = iv.ItemClass.Stacknumber.Value - TractorCompat.StackCount(slot);
				if (num2 > 0)
				{
					num2 = Utils.FastMin(num2, count);
					TractorCompat.SetStack(slot, slotValue, TractorCompat.StackCount(slot) + num2);
					if (val != null)
					{
						val.AddItemStack(new ItemStack(iv, num2), false);
					}
					flag = true;
					count -= num2;
				}
			}
			num++;
		}
		int num3 = 0;
		while (count > 0 && num3 < slots.Length)
		{
			if (slots[num3].IsEmpty())
			{
				int value = iv.ItemClass.Stacknumber.Value;
				if (value > 0)
				{
					value = Utils.FastMin(value, count);
					TractorCompat.SetStack(slots[num3], iv, value);
					if (val != null)
					{
						val.AddItemStack(new ItemStack(iv, value), false);
					}
					flag = true;
					count -= value;
				}
			}
			num3++;
		}
		LawnTractorCompanion.UseBagForItemCount = null;
		if (!flag)
		{
			return false;
		}
		TractorCompat.SetSlots(bag, slots);
		return true;
	}

	static VPMower()
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
	}
}
public class CopyTransform : MonoBehaviour
{
	public Vector3 Position = Vector3.zero;

	public Vector3 Scale = Vector3.one;

	public string Prefab;

	public string Path;

	public string Name;

	public bool Debug;

	private bool added;

	public void CopyNow()
	{
		if (added || string.IsNullOrEmpty(Prefab))
		{
			return;
		}
		added = true;
		GameObject asset = DataLoader.LoadAsset<GameObject>(Prefab);
		if (asset == null)
		{
			Log.Warning("OCB particle copy missing asset " + Prefab);
			return;
		}
		Transform source = asset.transform.Find(Path);
		if (source == null)
		{
			Log.Warning("OCB particle copy missing transform " + Path);
			return;
		}
		GameObject copy = Object.Instantiate(source.gameObject);
		copy.name = string.IsNullOrEmpty(Name) ? source.name : Name;
		copy.transform.SetParent(((Component)this).transform, false);
		copy.transform.localPosition = Position;
		copy.transform.localScale = Scale;
		copy.SetActive(true);
	}

	public CopyTransform()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
	}
}
