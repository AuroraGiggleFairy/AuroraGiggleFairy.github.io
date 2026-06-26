using System.Collections.Generic;
using UnityEngine;

public class DismembermentManager
{
	public static class DamageKeys
	{
		public const string blade = "blade";

		public const string blunt = "blunt";

		public const string bullet = "bullet";

		public const string exlosive = "explosive";
	}

	public enum DamageTags
	{
		none,
		blade,
		blunt,
		any
	}

	public static class ParseKeys
	{
		public const string cType = "type";

		public const string cTarget = "target";

		public const string cAttachToParent = "atp";

		public const string cAlignToBone = "atb";

		public const string cDetach = "detach";

		public const string cMask = "mask";

		public const string cScaleOutLimb = "sol";

		public const string cSolTarget = "soltarget";

		public const string cSolScale = "solscale";

		public const string cInsertChildObj = "ico";

		public const string cInsertBoneObj = "ibo";
	}

	public const string DestroyedCapRoot = "pos";

	public const string DetachableRootName = "Detachable";

	public const string DetachableArmName = "HalfArm";

	public const string DetachableLegName = "HalfLeg";

	public const string AnimatorRootName = "Animator";

	public const string PhysicsRootName = "Physics";

	public const string ZombieSkinPrefix = "HD_";

	public const string RadiatedMatTarget = "LOD0";

	public const string ManagedLimbsParentName = "DismemberedLimbs";

	public const string MatPropRadiated = "_IsRadiated";

	public const string MatPropIrradiated = "_Irradiated";

	public const string MatPropLeftLowerLeg = "_LeftLowerLeg";

	public const string MatPropRightLowerLeg = "_RightLowerLeg";

	public const string MatPropLeftUpperLeg = "_LeftUpperLeg";

	public const string MatPropRightUpperLeg = "_RightUpperLeg";

	public const string cPrefabExt = ".prefab";

	public const string cAssetBundleSearchName = "Dismemberment";

	public const string cAssetBundleZombies = "@:Entities/Zombies/";

	public const string cAssetBundleFolder = "@:Entities/Zombies/Dismemberment/";

	public const int cAltMatOffset = 2;

	public const string cAssetBundleGibMats = "Gibs/Materials";

	public const string CAssetBundleDefaultGib = "gib_dismemberment";

	public const string CAssetBundleDefaultGibBlood = "gib_bloodcap";

	public const string CAssetBundleDefaultGibChunk = "ZombieGibs_caps";

	public static string[] DefaultBundleGibs = new string[3] { "gib_dismemberment", "gib_bloodcap", "ZombieGibs_caps" };

	public const string cAssetBundleTextures = "Zombies";

	public const string cMatExt = ".mat";

	public const string cDefaultTexExt = ".tga";

	public const string cDefaultMatBaseTex = "_ZombieColor";

	public const string cDefaultMatBaseTexAlt = "_Albedo";

	public const float cDefaultDetachLimbLifeTime = 10f;

	public const int cDefaultDetachLimbMax = 25;

	public const int cDefaultDetachLimbCleanupCount = 5;

	public const string cCensorGoreSearch = "_CGore";

	public static bool DebugLogEnabled;

	public static FastTags<TagGroup.Global> radiatedTag = FastTags<TagGroup.Global>.Parse("radiated");

	[PublicizedFrom(EAccessModifier.Private)]
	public List<DetachedDismembermentPart> parts = new List<DetachedDismembermentPart>();

	[PublicizedFrom(EAccessModifier.Private)]
	public Material zombieGibCapsMaterial;

	[PublicizedFrom(EAccessModifier.Private)]
	public Material zombieGibCapsMaterialRadiated;

	[PublicizedFrom(EAccessModifier.Private)]
	public const string cGibCapsMatPath = "@:Entities/Zombies/Gibs/Materials/ZombieGibs_caps.mat";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string cGibCapsMatRadPath = "@:Entities/Zombies/Gibs/Materials/ZombieGibs_caps_IsRadiated.mat";

	public const string cGlobalMatName = "(global)";

	public const string cLocalMatName = "(local)";

	public const string cInstanceMatName = "(Instance)";

	[PublicizedFrom(EAccessModifier.Private)]
	public static DismembermentManager instance;

	public static bool DebugShowArmRotations;

	public static bool DebugDismemberExplosions;

	public static bool DebugBulletTime;

	public static bool DebugBloodParticles;

	public static EnumBodyPartHit DebugBodyPartHit;

	public const string cXmlTag = "DismemberTag_";

	public const char cParamSplit = ';';

	public const char cRawSplit = '+';

	public const char cDataSplit = '=';

	public const char cCommaDel = ',';

	public static FastTags<TagGroup.Global> rangedTags = FastTags<TagGroup.Global>.Parse("ranged");

	[PublicizedFrom(EAccessModifier.Private)]
	public static FastTags<TagGroup.Global> shotgunTags = FastTags<TagGroup.Global>.Parse("shotgun");

	[PublicizedFrom(EAccessModifier.Private)]
	public static FastTags<TagGroup.Global> sledgeTags = FastTags<TagGroup.Global>.Parse("sledge");

	[PublicizedFrom(EAccessModifier.Private)]
	public static FastTags<TagGroup.Global> knifeTags = FastTags<TagGroup.Global>.Parse("knife");

	[PublicizedFrom(EAccessModifier.Private)]
	public const float MaxForce = 1.5f;

	public const string cLOD0 = "LOD0";

	public const string cLOD1 = "LOD1";

	public const string cLOD2 = "LOD2";

	public const string cDynamicGore = "DynamicGore";

	public const string cSubTagHeadAccessories = "HeadAccessories";

	public static readonly List<string> BluntCensors = new List<string> { "zombieLab", "zombieUtilityWorker" };

	public static readonly Dictionary<EnumBodyPartHit, string[]> BipedDismemberments = new Dictionary<EnumBodyPartHit, string[]>
	{
		{
			EnumBodyPartHit.Head,
			new string[2] { "DismemberTag_L_HeadGore", "HeadGore" }
		},
		{
			EnumBodyPartHit.LeftUpperLeg,
			new string[2] { "DismemberTag_L_LeftUpperLegGore", "UpperLegGore" }
		},
		{
			EnumBodyPartHit.LeftLowerLeg,
			new string[2] { "DismemberTag_L_LeftLowerLegGore", "LowerLegGore" }
		},
		{
			EnumBodyPartHit.RightUpperLeg,
			new string[2] { "DismemberTag_L_RightUpperLegGore", "UpperLegGore" }
		},
		{
			EnumBodyPartHit.RightLowerLeg,
			new string[2] { "DismemberTag_L_RightLowerLegGore", "LowerLegGore" }
		},
		{
			EnumBodyPartHit.LeftUpperArm,
			new string[2] { "DismemberTag_L_LeftUpperArmGore", "UpperArmGore" }
		},
		{
			EnumBodyPartHit.LeftLowerArm,
			new string[2] { "DismemberTag_L_LeftLowerArmGore", "LowerArmGore" }
		},
		{
			EnumBodyPartHit.RightUpperArm,
			new string[2] { "DismemberTag_L_RightUpperArmGore", "UpperArmGore" }
		},
		{
			EnumBodyPartHit.RightLowerArm,
			new string[2] { "DismemberTag_L_RightLowerArmGore", "LowerArmGore" }
		}
	};

	public static readonly Dictionary<EnumBodyPartHit, string[]> QuadrupedDismemberments = new Dictionary<EnumBodyPartHit, string[]>
	{
		{
			EnumBodyPartHit.Head,
			new string[2] { "DismemberTag_L_HeadGore", "HeadGore" }
		},
		{
			EnumBodyPartHit.LeftUpperLeg,
			new string[2] { "DismemberTag_L_LeftUpperLegGore", "UpperLegGore" }
		},
		{
			EnumBodyPartHit.LeftLowerLeg,
			new string[2] { "DismemberTag_L_LeftLowerLegGore", "LowerLegGore" }
		},
		{
			EnumBodyPartHit.RightUpperLeg,
			new string[2] { "DismemberTag_L_RightUpperLegGore", "UpperLegGore" }
		},
		{
			EnumBodyPartHit.RightLowerLeg,
			new string[2] { "DismemberTag_L_RightLowerLegGore", "LowerLegGore" }
		},
		{
			EnumBodyPartHit.LeftUpperArm,
			new string[2] { "DismemberTag_L_LeftUpperArmGore", "UpperArmGore" }
		},
		{
			EnumBodyPartHit.LeftLowerArm,
			new string[2] { "DismemberTag_L_LeftLowerArmGore", "LowerArmGore" }
		},
		{
			EnumBodyPartHit.RightUpperArm,
			new string[2] { "DismemberTag_L_RightUpperArmGore", "UpperArmGore" }
		},
		{
			EnumBodyPartHit.RightLowerArm,
			new string[2] { "DismemberTag_L_RightLowerArmGore", "LowerArmGore" }
		}
	};

	[PublicizedFrom(EAccessModifier.Private)]
	public const string cDebugAxisPath = "@:Entities/Zombies/Gibs/Debug/debugAxisObj.prefab";

	public Material GibCapsMaterial
	{
		get
		{
			if (!zombieGibCapsMaterial)
			{
				Material material = DataLoader.LoadAsset<Material>("@:Entities/Zombies/Gibs/Materials/ZombieGibs_caps.mat");
				zombieGibCapsMaterial = Object.Instantiate(material);
				zombieGibCapsMaterial.name = material.name + "(global)";
				Log.Out("{0} material: {1}", zombieGibCapsMaterial ? "load" : "load failed", "@:Entities/Zombies/Gibs/Materials/ZombieGibs_caps.mat");
			}
			return zombieGibCapsMaterial;
		}
	}

	public Material GibCapsRadMaterial
	{
		get
		{
			if (!zombieGibCapsMaterialRadiated)
			{
				Material material = DataLoader.LoadAsset<Material>("@:Entities/Zombies/Gibs/Materials/ZombieGibs_caps_IsRadiated.mat");
				zombieGibCapsMaterialRadiated = Object.Instantiate(material);
				zombieGibCapsMaterialRadiated.name = material.name + "(global)";
				Log.Out("{0} material: {1}", zombieGibCapsMaterialRadiated ? "load" : "load failed", "@:Entities/Zombies/Gibs/Materials/ZombieGibs_caps_IsRadiated.mat");
			}
			return zombieGibCapsMaterialRadiated;
		}
	}

	public static DismembermentManager Instance => instance;

	public static string GetAssetBundlePath(string prefabPath)
	{
		return "@:Entities/Zombies/Dismemberment/" + prefabPath + ".prefab";
	}

	public static void SetShaderTexture(Material _mat, Texture _altColor)
	{
		if (_mat.HasTexture("_ZombieColor"))
		{
			_mat.SetTexture("_ZombieColor", _altColor);
		}
		if (_mat.HasTexture("_Albedo"))
		{
			_mat.SetTexture("_Albedo", _altColor);
		}
	}

	public static Texture GetShaderTexture(Material _mat)
	{
		if (_mat.HasTexture("_ZombieColor"))
		{
			return _mat.GetTexture("_ZombieColor");
		}
		if (_mat.HasTexture("_Albedo"))
		{
			return _mat.GetTexture("_Albedo");
		}
		return null;
	}

	public static void Init()
	{
		instance = new DismembermentManager();
	}

	public static void Cleanup()
	{
		if (instance != null)
		{
			List<DetachedDismembermentPart> list = instance.parts;
			for (int i = 0; i < list.Count; i++)
			{
				list[i].CleanupDetached();
			}
			list.Clear();
		}
	}

	public void AddPart(DetachedDismembermentPart part)
	{
		parts.Add(part);
	}

	public void Update()
	{
		for (int i = 0; i < parts.Count; i++)
		{
			DetachedDismembermentPart detachedDismembermentPart = parts[i];
			detachedDismembermentPart.Update();
			if (detachedDismembermentPart.ReadyForCleanup)
			{
				detachedDismembermentPart.CleanupDetached();
				parts.RemoveAt(i);
				i--;
			}
		}
	}

	public static float GetImpactForce(ItemClass ic, float strength)
	{
		if (ic != null)
		{
			if (ic.HasAnyTags(shotgunTags))
			{
				return 1.5f;
			}
			if (ic.HasAnyTags(sledgeTags))
			{
				return Mathf.Clamp(1f + Mathf.Abs(strength), 1f, 1.5f);
			}
			if (ic.HasAnyTags(knifeTags))
			{
				return Mathf.Abs(1f * strength) * 0.67f;
			}
		}
		return 1f;
	}

	public static bool HasDismemberedPart(EnumBodyPartHit part, bool isBiped = true)
	{
		if (isBiped)
		{
			return BipedDismemberments.ContainsKey(part);
		}
		return QuadrupedDismemberments.ContainsKey(part);
	}

	public static string[] GetDismemberedPart(EnumBodyPartHit part, bool isBiped = true)
	{
		string[] value = null;
		if (isBiped)
		{
			BipedDismemberments.TryGetValue(part, out value);
		}
		else
		{
			QuadrupedDismemberments.TryGetValue(part, out value);
		}
		return value;
	}

	public static EnumBodyPartHit GetBodyPartHit(uint bodyDamageFlag)
	{
		return bodyDamageFlag switch
		{
			1u => EnumBodyPartHit.Head, 
			2u => EnumBodyPartHit.LeftUpperArm, 
			4u => EnumBodyPartHit.LeftLowerArm, 
			8u => EnumBodyPartHit.RightUpperArm, 
			16u => EnumBodyPartHit.RightLowerArm, 
			32u => EnumBodyPartHit.LeftUpperLeg, 
			64u => EnumBodyPartHit.LeftLowerLeg, 
			128u => EnumBodyPartHit.RightUpperLeg, 
			256u => EnumBodyPartHit.RightLowerLeg, 
			_ => EnumBodyPartHit.None, 
		};
	}

	public static EnumBodyPartHit GetBodyPartHit(string _propKey)
	{
		if (_propKey.ContainsCaseInsensitive("L_HeadGore"))
		{
			return EnumBodyPartHit.Head;
		}
		if (_propKey.ContainsCaseInsensitive("L_LeftUpperArmGore"))
		{
			return EnumBodyPartHit.LeftUpperArm;
		}
		if (_propKey.ContainsCaseInsensitive("L_LeftLowerArmGore"))
		{
			return EnumBodyPartHit.LeftLowerArm;
		}
		if (_propKey.ContainsCaseInsensitive("L_RightUpperArmGore"))
		{
			return EnumBodyPartHit.RightUpperArm;
		}
		if (_propKey.ContainsCaseInsensitive("L_RightLowerArmGore"))
		{
			return EnumBodyPartHit.RightLowerArm;
		}
		if (_propKey.ContainsCaseInsensitive("L_LeftUpperLegGore"))
		{
			return EnumBodyPartHit.LeftUpperLeg;
		}
		if (_propKey.ContainsCaseInsensitive("L_LeftLowerLegGore"))
		{
			return EnumBodyPartHit.LeftLowerLeg;
		}
		if (_propKey.ContainsCaseInsensitive("L_RightUpperLegGore"))
		{
			return EnumBodyPartHit.RightUpperLeg;
		}
		if (_propKey.ContainsCaseInsensitive("L_RightLowerLegGore"))
		{
			return EnumBodyPartHit.RightLowerLeg;
		}
		return EnumBodyPartHit.None;
	}

	public static DismemberedPartData GetPartData(EntityAlive _entity)
	{
		string[] dismemberedPart = GetDismemberedPart(_entity.bodyDamage.bodyPartHit);
		if (dismemberedPart != null)
		{
			string text = dismemberedPart[0];
			DynamicProperties properties = _entity.EntityClass.Properties;
			if (properties.Data.ContainsKey(text))
			{
				string[] array = properties.Values[text].Split(';');
				string a = array[0];
				string[] array2 = properties.Data[text].Split(';');
				string text2 = array2[0].Replace("target=", "");
				DismemberedPartData dismemberedPartData = ReadPart(array2);
				if (dismemberedPartData != null)
				{
					if (a.ContainsCaseInsensitive("linked"))
					{
						array = properties.Values[text2].Split(';');
						dismemberedPartData = ReadPart(properties.Data[text2].Split(';'));
						dismemberedPartData.isLinked = true;
					}
					dismemberedPartData.propertyKey = text2.Trim();
					dismemberedPartData.prefabPath = array[0].Trim();
					return dismemberedPartData;
				}
			}
		}
		return null;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static DismemberedPartData ReadPart(string[] data)
	{
		List<DismemberedPartData> list = new List<DismemberedPartData>();
		for (int i = 0; i < data.Length; i++)
		{
			DismemberedPartData dismemberedPartData = new DismemberedPartData();
			string text = data[i];
			if (text.Contains('+'.ToString()))
			{
				string[] array = text.Split('+');
				for (int j = 0; j < array.Length; j++)
				{
					readString(array[j], dismemberedPartData);
				}
				list.Add(dismemberedPartData);
			}
			else
			{
				readString(text, dismemberedPartData);
				list.Add(dismemberedPartData);
			}
		}
		if (list.Count > 0)
		{
			return list[0];
		}
		return null;
	}

	public static DismemberedPartData DismemberPart(uint bodyDamageFlag, EnumDamageTypes damageType, EntityAlive _entity, bool isBiped)
	{
		return DismemberPart(GetBodyPartHit(bodyDamageFlag), damageType, _entity, isBiped);
	}

	public static DismemberedPartData DismemberPart(EnumBodyPartHit partHit, EnumDamageTypes damageType, EntityAlive _entity, bool isBiped)
	{
		if (!HasDismemberedPart(partHit, isBiped))
		{
			return null;
		}
		DismemberedPartData dismemberedPartData = new DismemberedPartData();
		string[] dismemberedPart = GetDismemberedPart(partHit, isBiped);
		dismemberedPartData.propertyKey = dismemberedPart[0];
		dismemberedPartData.prefabPath = dismemberedPart[1];
		dismemberedPartData.damageTypeKey = getDamageTag(damageType, _entity.lastHitRanged);
		DynamicProperties properties = _entity.EntityClass.Properties;
		if (!properties.Contains(dismemberedPartData.propertyKey) || string.IsNullOrEmpty(properties.Values[dismemberedPartData.propertyKey]))
		{
			return dismemberedPartData;
		}
		if (properties.Data.ContainsKey(dismemberedPartData.propertyKey))
		{
			string[] array = properties.Values[dismemberedPartData.propertyKey].Split(';');
			string[] array2 = properties.Data[dismemberedPartData.propertyKey].Split(';');
			DismemberedPartData dismemberedPartData2 = GetRandomPart(array, array2, dismemberedPartData.damageTypeKey);
			if (array[0].ContainsCaseInsensitive("linked"))
			{
				string v = array2[0].Replace("target=", "");
				array = properties.Values[v].Split(';');
				array2 = properties.Data[v].Split(';');
				dismemberedPartData.isLinked = true;
				dismemberedPartData2 = GetRandomPart(array, array2, dismemberedPartData.damageTypeKey);
			}
			if (dismemberedPartData2 == null && dismemberedPartData.damageTypeKey == "blunt" && !dismemberedPartData.prefabPath.ContainsCaseInsensitive("blunt"))
			{
				DismemberedPartData randomPart = GetRandomPart(array, array2, "blade");
				if (randomPart != null && (randomPart.useMask || randomPart.scaleOutLimb))
				{
					dismemberedPartData2 = randomPart;
				}
			}
			if (dismemberedPartData2 != null)
			{
				if (!dismemberedPartData2.isLinked && dismemberedPartData2.Invalid)
				{
					return dismemberedPartData;
				}
				if (!string.IsNullOrEmpty(dismemberedPartData2.prefabPath))
				{
					dismemberedPartData.prefabPath = dismemberedPartData2.prefabPath;
				}
				dismemberedPartData.scale = dismemberedPartData2.scale;
				if (dismemberedPartData2.hasRotOffset)
				{
					dismemberedPartData.SetRot(dismemberedPartData2.rot);
				}
				dismemberedPartData.targetBone = dismemberedPartData2.targetBone;
				dismemberedPartData.attachToParent = dismemberedPartData2.attachToParent;
				dismemberedPartData.alignToBone = dismemberedPartData2.alignToBone;
				dismemberedPartData.particlePaths = dismemberedPartData2.particlePaths;
				dismemberedPartData.isDetachable = dismemberedPartData2.isDetachable;
				dismemberedPartData.snapToChild = dismemberedPartData2.snapToChild;
				dismemberedPartData.overrideAnimationState = dismemberedPartData2.overrideAnimationState;
				dismemberedPartData.offset = dismemberedPartData2.offset;
				dismemberedPartData.useMask = dismemberedPartData2.useMask;
				dismemberedPartData.maskOverride = dismemberedPartData2.maskOverride;
				dismemberedPartData.tscale = dismemberedPartData2.tscale;
				dismemberedPartData.scaleOutLimb = dismemberedPartData2.scaleOutLimb;
				dismemberedPartData.solTarget = dismemberedPartData2.solTarget;
				dismemberedPartData.solScale = dismemberedPartData2.solScale;
				dismemberedPartData.hasSolScale = dismemberedPartData2.hasSolScale;
				dismemberedPartData.childTargetObj = dismemberedPartData2.childTargetObj;
				dismemberedPartData.insertBoneObj = dismemberedPartData2.insertBoneObj;
			}
		}
		if (DebugLogEnabled)
		{
			Log.Out("DismembermentManager.DismemberPart - entity: {0} {1}", EntityClass.list[_entity.entityClass].entityClassName, dismemberedPartData.Log());
		}
		return dismemberedPartData;
	}

	public static void ActivateDetachable(Transform rootT, string targetPart)
	{
		Transform transform = rootT;
		Transform transform2 = transform.Find("Physics");
		if ((bool)transform2)
		{
			transform = transform2;
		}
		for (int i = 0; i < transform.childCount; i++)
		{
			Transform child = transform.GetChild(i);
			bool active = child.name.ContainsCaseInsensitive(targetPart);
			child.gameObject.SetActive(active);
		}
	}

	public static string getDamageTag(EnumDamageTypes _damageType, bool lastHitRanged)
	{
		if (_damageType == EnumDamageTypes.Piercing && lastHitRanged)
		{
			return "blunt";
		}
		switch (_damageType)
		{
		case EnumDamageTypes.Piercing:
		case EnumDamageTypes.Slashing:
			return "blade";
		case EnumDamageTypes.Bashing:
		case EnumDamageTypes.Crushing:
			return "blunt";
		case EnumDamageTypes.Heat:
			return "blunt";
		default:
			return null;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static DismemberedPartData GetRandomPart(string[] prefabs, string[] data, string tag)
	{
		List<DismemberedPartData> list = new List<DismemberedPartData>();
		for (int i = 0; i < data.Length; i++)
		{
			DismemberedPartData dismemberedPartData = new DismemberedPartData();
			if (i < prefabs.Length)
			{
				string prefabPath = prefabs[i].Trim();
				dismemberedPartData.prefabPath = prefabPath;
			}
			if (data[i].Contains('+'.ToString()))
			{
				string[] array = data[i].Split('+');
				for (int j = 0; j < array.Length; j++)
				{
					readString(array[j], dismemberedPartData);
				}
				if (!string.IsNullOrEmpty(dismemberedPartData.damageTypeKey) && dismemberedPartData.damageTypeKey == tag)
				{
					list.Add(dismemberedPartData);
				}
			}
			else
			{
				readString(data[i], dismemberedPartData);
				if (!string.IsNullOrEmpty(dismemberedPartData.damageTypeKey) && dismemberedPartData.damageTypeKey == tag)
				{
					list.Add(dismemberedPartData);
				}
			}
		}
		if (list.Count > 0)
		{
			int index = Random.Range(0, list.Count);
			return list[index];
		}
		return null;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void readString(string rawString, DismemberedPartData part)
	{
		string[] array = rawString.Split('=');
		string text = array[0].Trim();
		switch (text)
		{
		case "rot":
		{
			string[] array2 = array[1].Split(',');
			if (array2.Length == 3)
			{
				Vector3 zero = Vector3.zero;
				float.TryParse(array2[0], out zero.x);
				float.TryParse(array2[1], out zero.y);
				float.TryParse(array2[2], out zero.z);
				if (zero != Vector3.zero)
				{
					part.SetRot(zero);
				}
			}
			break;
		}
		case "scale":
		{
			string[] array6 = array[1].Split(',');
			float.TryParse(array6[0], out part.scale.x);
			float.TryParse(array6[1], out part.scale.y);
			float.TryParse(array6[2], out part.scale.z);
			break;
		}
		case "type":
			switch (array[1].Trim())
			{
			case "blunt":
			case "blade":
			case "bullet":
			case "explosive":
				part.damageTypeKey = array[1].Trim();
				break;
			}
			break;
		case "target":
			part.targetBone = array[1].Trim();
			break;
		case "atp":
			bool.TryParse(array[1], out part.attachToParent);
			break;
		case "atb":
			bool.TryParse(array[1], out part.alignToBone);
			break;
		case "particles":
			part.particlePaths = array[1].Split(',');
			break;
		case "detach":
			bool.TryParse(array[1], out part.isDetachable);
			break;
		case "stc":
			bool.TryParse(array[1], out part.snapToChild);
			break;
		case "oset":
		{
			string[] array5 = array[1].Split(',');
			float.TryParse(array5[0], out part.offset.x);
			float.TryParse(array5[1], out part.offset.y);
			float.TryParse(array5[2], out part.offset.z);
			break;
		}
		case "oas":
			bool.TryParse(array[1], out part.overrideAnimationState);
			break;
		case "mask":
			bool.TryParse(array[1], out part.useMask);
			break;
		case "movr":
			bool.TryParse(array[1], out part.maskOverride);
			break;
		case "tscale":
		{
			string[] array4 = array[1].Split(',');
			float.TryParse(array4[0], out part.tscale.x);
			float.TryParse(array4[1], out part.tscale.y);
			float.TryParse(array4[2], out part.tscale.z);
			break;
		}
		case "sol":
			bool.TryParse(array[1], out part.scaleOutLimb);
			break;
		case "soltarget":
			part.solTarget = array[1].Trim();
			break;
		case "solscale":
		{
			string[] array3 = array[1].Split(',');
			float.TryParse(array3[0], out part.solScale.x);
			float.TryParse(array3[1], out part.solScale.y);
			float.TryParse(array3[2], out part.solScale.z);
			part.hasSolScale = true;
			break;
		}
		case "ico":
			part.childTargetObj = array[1].Trim();
			break;
		case "ibo":
			part.insertBoneObj = array[1].Trim();
			break;
		default:
			part.Invalid = true;
			if (DebugLogEnabled)
			{
				Log.Out("DismembermentManager EntityClasses.xml unknown dismemberment data raw:{0} key:{1}", rawString, text);
			}
			break;
		}
	}

	public static void SpawnParticleEffect(ParticleEffect _pe, int _entityId = -1)
	{
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			if (!GameManager.IsDedicatedServer)
			{
				GameManager.Instance.SpawnParticleEffectClient(_pe, _entityId);
			}
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageParticleEffect>().Setup(_pe, _entityId), _onlyClientsAttachedToAnEntity: false, -1, _entityId);
		}
		else
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageParticleEffect>().Setup(_pe, _entityId));
		}
	}

	public static void AddDebugArmObjects(Transform partT, Transform parentT)
	{
		if (!partT || !parentT || !partT.name.ContainsCaseInsensitive("arm"))
		{
			return;
		}
		GameObject gameObject = DataLoader.LoadAsset<GameObject>("@:Entities/Zombies/Gibs/Debug/debugAxisObj.prefab");
		if ((bool)gameObject)
		{
			GameObject gameObject2 = Object.Instantiate(gameObject);
			gameObject2.transform.SetParent(parentT);
			gameObject2.transform.localPosition = Vector3.zero;
			gameObject2.transform.localRotation = Quaternion.identity;
			if (partT.name.ContainsCaseInsensitive("right"))
			{
				gameObject2.transform.localRotation = Quaternion.Euler(180f, 0f, 0f);
			}
			gameObject2.transform.localScale = Vector3.one * 0.5f;
			Transform transform = parentT.FindRecursive("rot");
			if ((bool)transform)
			{
				GameObject gameObject3 = Object.Instantiate(gameObject);
				gameObject3.transform.SetParent(transform);
				gameObject3.transform.localPosition = Vector3.zero;
				gameObject3.transform.localRotation = Quaternion.identity;
				gameObject3.transform.localScale = Vector3.one * 0.33f;
				gameObject3.GetComponentInChildren<MeshRenderer>().material.color = Color.yellow;
			}
		}
	}
}
