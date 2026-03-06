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

		public const string cDetach = "detach";

		public const string cMask = "mask";

		public const string cScaleOutLimb = "sol";

		public const string cSolTarget = "soltarget";

		public const string cSolScale = "solscale";

		public const string cInsertChildObj = "ico";

		public const string cInsertBoneObj = "ibo";

		public const string cAddScalePoint = "asp";

		public const string cMaskScaleBlend = "msb";

		public const string cSetFixedValues = "sfv";
	}

	public const string cClassName = "DismembermentManager";

	public const string cManagedLimbsParentName = "DismemberedLimbs";

	public static bool DebugLogEnabled;

	public static bool DebugShowArmRotations;

	public static bool DebugDismemberExplosions;

	public static bool DebugBulletTime;

	public static bool DebugBloodParticles;

	public static bool DebugDontCreateParts;

	public static EnumBodyPartHit DebugBodyPartHit;

	public static bool DebugUseLegacy;

	public static bool DebugExplosiveCleanup;

	public const string DestroyedCapRoot = "pos";

	public const string PhysicsRootName = "Physics";

	public const string DetachableRootName = "Detachable";

	public const string cSubTagHeadAccessories = "HeadAccessories";

	public const string DetachableArmName = "HalfArm";

	public const string DetachableLegName = "HalfLeg";

	public const string ZombieSkinPrefix = "HD_";

	public const string MatPropRadiated = "_IsRadiated";

	public const string MatPropIrradiated = "_Irradiated";

	public const string MatPropFade = "_Fade";

	public static readonly FastTags<TagGroup.Global> radiatedTag = FastTags<TagGroup.Global>.Parse("radiated");

	public static readonly FastTags<TagGroup.Global> radOrChargedTag = FastTags<TagGroup.Global>.Parse("radiated,charged");

	public const string cCensorGoreSearch = "_CGore";

	public static readonly List<string> BluntCensors = new List<string> { "zombieLab", "zombieUtilityWorker" };

	public const string cAssetBundleZombies = "@:Entities/Zombies/";

	public const string cAssetBundleSearchName = "Dismemberment";

	public const string cAssetBundleFolder = "@:Entities/Zombies/";

	public const string cPrefabExt = ".prefab";

	public const string cLOD0 = "LOD0";

	public const string cLOD1 = "LOD1";

	public const string cLOD2 = "LOD2";

	public const string MatPropLeftLowerLeg = "_LeftLowerLeg";

	public const string MatPropRightLowerLeg = "_RightLowerLeg";

	public const string MatPropLeftUpperLeg = "_LeftUpperLeg";

	public const string MatPropRightUpperLeg = "_RightUpperLeg";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string cAssetBundleGibMats = "Common/Gibs/Materials";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string CAssetBundleDefaultGib = "gib_dismemberment";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string CAssetBundleDefaultGibBlood = "gib_bloodcap";

	public const string CAssetBundleDefaultGibChunk = "ZombieGibs_caps";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string cDismemberMatXmlProp = "DismemberMaterial";

	public static string[] DefaultBundleGibs = new string[3] { "gib_dismemberment", "gib_bloodcap", "ZombieGibs_caps" };

	public const string cGlobalMatName = "(global)";

	public const string cLocalMatName = "(local)";

	public const string cInstanceMatName = "(Instance)";

	public const string cMatExt = ".mat";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string cGibCapsMatPath = "@:Entities/Zombies/Common/Gibs/Materials/ZombieGibs_caps.mat";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string cGibCapsMatRadPath = "@:Entities/Zombies/Common/Gibs/Materials/ZombieGibs_caps_IsRadiated.mat";

	[PublicizedFrom(EAccessModifier.Private)]
	public Material zombieGibCapsMaterial;

	[PublicizedFrom(EAccessModifier.Private)]
	public Material zombieGibCapsMaterialRadiated;

	[PublicizedFrom(EAccessModifier.Private)]
	public const string cDefaultMatBaseTex = "_ZombieColor";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string cDefaultMatBaseTexAlt = "_Albedo";

	public const float cDefaultDetachLimbLifeTime = 10f;

	public const int cDefaultDetachLimbMax = 25;

	public const int cDefaultDetachLimbCleanupCount = 5;

	public const int cMaxLimbsFromExplosiveDeath = 3;

	public List<DismemberedPart> parts = new List<DismemberedPart>();

	[PublicizedFrom(EAccessModifier.Private)]
	public static DismembermentManager instance;

	public const string cDynamicGore = "DynamicGore";

	public static FastTags<TagGroup.Global> rangedTags = FastTags<TagGroup.Global>.Parse("ranged");

	public static FastTags<TagGroup.Global> launcherTags = FastTags<TagGroup.Global>.Parse("launcher");

	[PublicizedFrom(EAccessModifier.Private)]
	public static FastTags<TagGroup.Global> shotgunTags = FastTags<TagGroup.Global>.Parse("shotgun");

	[PublicizedFrom(EAccessModifier.Private)]
	public static FastTags<TagGroup.Global> sledgeTags = FastTags<TagGroup.Global>.Parse("sledge");

	[PublicizedFrom(EAccessModifier.Private)]
	public static FastTags<TagGroup.Global> knifeTags = FastTags<TagGroup.Global>.Parse("knife");

	[PublicizedFrom(EAccessModifier.Private)]
	public const float MaxForce = 1.5f;

	public const string cXmlTag = "DismemberTag_";

	public const char cParamSplit = ';';

	public const char cRawSplit = '+';

	public const char cDataSplit = '=';

	public const char cCommaDel = ',';

	public static readonly Dictionary<EnumBodyPartHit, string[]> BipedDismemberments = new Dictionary<EnumBodyPartHit, string[]>
	{
		{
			EnumBodyPartHit.Head,
			new string[2] { "DismemberTag_L_HeadGore", "Common/Dismemberment/HeadGore" }
		},
		{
			EnumBodyPartHit.LeftUpperLeg,
			new string[2] { "DismemberTag_L_LeftUpperLegGore", "Common/Dismemberment/UpperLegGore" }
		},
		{
			EnumBodyPartHit.LeftLowerLeg,
			new string[2] { "DismemberTag_L_LeftLowerLegGore", "Common/Dismemberment/LowerLegGore" }
		},
		{
			EnumBodyPartHit.RightUpperLeg,
			new string[2] { "DismemberTag_L_RightUpperLegGore", "Common/Dismemberment/UpperLegGore" }
		},
		{
			EnumBodyPartHit.RightLowerLeg,
			new string[2] { "DismemberTag_L_RightLowerLegGore", "Common/Dismemberment/LowerLegGore" }
		},
		{
			EnumBodyPartHit.LeftUpperArm,
			new string[2] { "DismemberTag_L_LeftUpperArmGore", "Common/Dismemberment/UpperArmGore" }
		},
		{
			EnumBodyPartHit.LeftLowerArm,
			new string[2] { "DismemberTag_L_LeftLowerArmGore", "Common/Dismemberment/LowerArmGore" }
		},
		{
			EnumBodyPartHit.RightUpperArm,
			new string[2] { "DismemberTag_L_RightUpperArmGore", "Common/Dismemberment/UpperArmGore" }
		},
		{
			EnumBodyPartHit.RightLowerArm,
			new string[2] { "DismemberTag_L_RightLowerArmGore", "Common/Dismemberment/LowerArmGore" }
		}
	};

	public static readonly Dictionary<EnumBodyPartHit, string[]> QuadrupedDismemberments = new Dictionary<EnumBodyPartHit, string[]>
	{
		{
			EnumBodyPartHit.Head,
			new string[2] { "DismemberTag_L_HeadGore", "Common/Dismemberment/HeadGore" }
		},
		{
			EnumBodyPartHit.LeftUpperLeg,
			new string[2] { "DismemberTag_L_LeftUpperLegGore", "Common/Dismemberment/UpperLegGore" }
		},
		{
			EnumBodyPartHit.LeftLowerLeg,
			new string[2] { "DismemberTag_L_LeftLowerLegGore", "Common/Dismemberment/LowerLegGore" }
		},
		{
			EnumBodyPartHit.RightUpperLeg,
			new string[2] { "DismemberTag_L_RightUpperLegGore", "Common/Dismemberment/UpperLegGore" }
		},
		{
			EnumBodyPartHit.RightLowerLeg,
			new string[2] { "DismemberTag_L_RightLowerLegGore", "Common/Dismemberment/LowerLegGore" }
		},
		{
			EnumBodyPartHit.LeftUpperArm,
			new string[2] { "DismemberTag_L_LeftUpperArmGore", "Common/Dismemberment/UpperArmGore" }
		},
		{
			EnumBodyPartHit.LeftLowerArm,
			new string[2] { "DismemberTag_L_LeftLowerArmGore", "Common/Dismemberment/LowerArmGore" }
		},
		{
			EnumBodyPartHit.RightUpperArm,
			new string[2] { "DismemberTag_L_RightUpperArmGore", "Common/Dismemberment/UpperArmGore" }
		},
		{
			EnumBodyPartHit.RightLowerArm,
			new string[2] { "DismemberTag_L_RightLowerArmGore", "Common/Dismemberment/LowerArmGore" }
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
				Material material = DataLoader.LoadAsset<Material>("@:Entities/Zombies/Common/Gibs/Materials/ZombieGibs_caps.mat");
				zombieGibCapsMaterial = Object.Instantiate(material);
				zombieGibCapsMaterial.name = material.name + "(global)";
				if (DebugLogEnabled)
				{
					Log.Out("{0} material: {1}", zombieGibCapsMaterial ? "load" : "load failed", "@:Entities/Zombies/Common/Gibs/Materials/ZombieGibs_caps.mat");
				}
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
				Material material = DataLoader.LoadAsset<Material>("@:Entities/Zombies/Common/Gibs/Materials/ZombieGibs_caps_IsRadiated.mat");
				zombieGibCapsMaterialRadiated = Object.Instantiate(material);
				zombieGibCapsMaterialRadiated.name = material.name + "(global)";
				if (DebugLogEnabled)
				{
					Log.Out("{0} material: {1}", zombieGibCapsMaterialRadiated ? "load" : "load failed", "@:Entities/Zombies/Common/Gibs/Materials/ZombieGibs_caps_IsRadiated.mat");
				}
			}
			return zombieGibCapsMaterialRadiated;
		}
	}

	public static DismembermentManager Instance => instance;

	public static string GetAssetBundlePath(string prefabPath)
	{
		return "@:Entities/Zombies/" + prefabPath + ".prefab";
	}

	public static bool IsDefaultGib(string matName)
	{
		for (int i = 0; i < DefaultBundleGibs.Length; i++)
		{
			if (DefaultBundleGibs[i] == matName)
			{
				return true;
			}
		}
		return false;
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

	public static void Init()
	{
		instance = new DismembermentManager();
		if (DebugLogEnabled)
		{
			Log.Out("DismembermentManager Init");
		}
	}

	public static void Cleanup()
	{
		if (instance != null)
		{
			List<DismemberedPart> list = instance.parts;
			for (int i = 0; i < list.Count; i++)
			{
				list[i].CleanupDetached();
			}
			list.Clear();
		}
	}

	public void AddPart(DismemberedPart part)
	{
		parts.Add(part);
	}

	public void Update()
	{
		for (int i = 0; i < parts.Count; i++)
		{
			DismemberedPart dismemberedPart = parts[i];
			dismemberedPart.Update();
			if (dismemberedPart.ReadyForCleanup)
			{
				dismemberedPart.CleanupDetached();
				parts.RemoveAt(i);
				i--;
			}
		}
		if (parts.Count > 25)
		{
			int num = parts.Count - 25;
			for (int j = 0; j < num; j++)
			{
				parts[j].ReadyForCleanup = true;
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

	public static string GetDamageTag(EnumDamageTypes _damageType, bool lastHitRanged)
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

	public static DismemberedPartData DismemberPart(uint bodyDamageFlag, EnumDamageTypes damageType, EntityAlive _entity, bool isBiped, bool useLegacy = false)
	{
		return dismemberPart(GetBodyPartHit(bodyDamageFlag), damageType, _entity, isBiped, useLegacy);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static DismemberedPartData dismemberPart(EnumBodyPartHit partHit, EnumDamageTypes damageType, EntityAlive _entity, bool isBiped, bool useLegacy = false)
	{
		if (!hasDismemberedPart(partHit, isBiped))
		{
			return null;
		}
		DismemberedPartData dismemberedPartData = new DismemberedPartData();
		string[] dismemberedPart = getDismemberedPart(partHit, isBiped);
		dismemberedPartData.propertyKey = dismemberedPart[0];
		dismemberedPartData.prefabPath = dismemberedPart[1];
		dismemberedPartData.damageTypeKey = GetDamageTag(damageType, _entity.lastHitRanged);
		DynamicProperties properties = _entity.EntityClass.Properties;
		if (useLegacy || !properties.Contains(dismemberedPartData.propertyKey) || string.IsNullOrEmpty(properties.Values[dismemberedPartData.propertyKey]))
		{
			return dismemberedPartData;
		}
		if (properties.Data.ContainsKey(dismemberedPartData.propertyKey))
		{
			string[] array = properties.Values[dismemberedPartData.propertyKey].Split(';');
			string[] array2 = properties.Data[dismemberedPartData.propertyKey].Split(';');
			if (array[0].ContainsCaseInsensitive("linked"))
			{
				string v = array2[0].Replace("target=", "");
				array = properties.Values[v].Split(';');
				array2 = properties.Data[v].Split(';');
				dismemberedPartData.isLinked = true;
			}
			DismemberedPartData dismemberedPartData2 = readRandomPart(array, array2, dismemberedPartData.damageTypeKey);
			if (dismemberedPartData2 == null && dismemberedPartData.damageTypeKey == "blunt" && !dismemberedPartData.prefabPath.ContainsCaseInsensitive("blunt"))
			{
				DismemberedPartData dismemberedPartData3 = readRandomPart(array, array2, "blade");
				if (dismemberedPartData3 != null && (dismemberedPartData3.useMask || dismemberedPartData3.scaleOutLimb))
				{
					dismemberedPartData2 = dismemberedPartData3;
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
				dismemberedPartData.particlePaths = dismemberedPartData2.particlePaths;
				dismemberedPartData.isDetachable = dismemberedPartData2.isDetachable;
				dismemberedPartData.offset = dismemberedPartData2.offset;
				dismemberedPartData.useMask = dismemberedPartData2.useMask;
				dismemberedPartData.scaleOutLimb = dismemberedPartData2.scaleOutLimb;
				dismemberedPartData.solTarget = dismemberedPartData2.solTarget;
				dismemberedPartData.solScale = dismemberedPartData2.solScale;
				dismemberedPartData.hasSolScale = dismemberedPartData2.hasSolScale;
				dismemberedPartData.childTargetObj = dismemberedPartData2.childTargetObj;
				dismemberedPartData.insertBoneObj = dismemberedPartData2.insertBoneObj;
				dismemberedPartData.addScalePoint = dismemberedPartData2.addScalePoint;
				dismemberedPartData.maskScaleBlend = dismemberedPartData2.maskScaleBlend;
				dismemberedPartData.setFixedValues = dismemberedPartData2.setFixedValues;
				if (properties.Contains("DismemberMaterial"))
				{
					dismemberedPartData.dismemberMatPath = properties.Values["DismemberMaterial"];
				}
			}
		}
		if (DebugLogEnabled)
		{
			Log.Out("[{0}.DismemberPart] - entityClass: {1}{2}", "DismembermentManager", EntityClass.list[_entity.entityClass].entityClassName, dismemberedPartData.Log());
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

	public static DismemberedPartData GetPartData(EntityAlive _entity)
	{
		string[] dismemberedPart = getDismemberedPart(_entity.bodyDamage.bodyPartHit);
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
				bool flag = a.ContainsCaseInsensitive("linked");
				if (flag)
				{
					array = properties.Values[text2].Split(';');
					array2 = properties.Data[text2].Split(';');
				}
				DismemberedPartData dismemberedPartData = readPart(array2);
				if (dismemberedPartData != null)
				{
					dismemberedPartData.propertyKey = text2.Trim();
					dismemberedPartData.prefabPath = array[0].Trim();
					dismemberedPartData.isLinked = flag;
					return dismemberedPartData;
				}
			}
		}
		return null;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool hasDismemberedPart(EnumBodyPartHit part, bool isBiped = true)
	{
		if (isBiped)
		{
			return BipedDismemberments.ContainsKey(part);
		}
		return QuadrupedDismemberments.ContainsKey(part);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static string[] getDismemberedPart(EnumBodyPartHit part, bool isBiped = true)
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

	[PublicizedFrom(EAccessModifier.Private)]
	public static void readString(string rawString, DismemberedPartData data)
	{
		string[] array = rawString.Split('=');
		string text = array[0].Trim();
		switch (text)
		{
		case "pos":
		{
			string[] array2 = array[1].Split(',');
			float.TryParse(array2[0], out data.pos.x);
			float.TryParse(array2[1], out data.pos.y);
			float.TryParse(array2[2], out data.pos.z);
			break;
		}
		case "rot":
		{
			string[] array6 = array[1].Split(',');
			if (array6.Length == 3)
			{
				Vector3 zero = Vector3.zero;
				float.TryParse(array6[0], out zero.x);
				float.TryParse(array6[1], out zero.y);
				float.TryParse(array6[2], out zero.z);
				if (zero != Vector3.zero)
				{
					data.SetRot(zero);
				}
			}
			break;
		}
		case "scale":
		{
			string[] array5 = array[1].Split(',');
			float.TryParse(array5[0], out data.scale.x);
			float.TryParse(array5[1], out data.scale.y);
			float.TryParse(array5[2], out data.scale.z);
			break;
		}
		case "type":
			switch (array[1].Trim())
			{
			case "blunt":
			case "blade":
			case "bullet":
			case "explosive":
				data.damageTypeKey = array[1].Trim();
				break;
			}
			break;
		case "target":
			data.targetBone = array[1].Trim();
			break;
		case "atp":
			bool.TryParse(array[1], out data.attachToParent);
			break;
		case "particles":
			data.particlePaths = array[1].Split(',');
			break;
		case "detach":
			bool.TryParse(array[1], out data.isDetachable);
			break;
		case "oset":
		{
			string[] array4 = array[1].Split(',');
			float.TryParse(array4[0], out data.offset.x);
			float.TryParse(array4[1], out data.offset.y);
			float.TryParse(array4[2], out data.offset.z);
			break;
		}
		case "mask":
			bool.TryParse(array[1], out data.useMask);
			break;
		case "sol":
			bool.TryParse(array[1], out data.scaleOutLimb);
			break;
		case "soltarget":
			data.solTarget = array[1].Trim();
			break;
		case "solscale":
		{
			string[] array3 = array[1].Split(',');
			float.TryParse(array3[0], out data.solScale.x);
			float.TryParse(array3[1], out data.solScale.y);
			float.TryParse(array3[2], out data.solScale.z);
			data.hasSolScale = true;
			break;
		}
		case "ico":
			data.childTargetObj = array[1].Trim();
			break;
		case "ibo":
			data.insertBoneObj = array[1].Trim();
			break;
		case "asp":
			data.addScalePoint = array[1].Trim();
			break;
		case "msb":
			data.maskScaleBlend = array[1].Trim();
			break;
		case "sfv":
			data.setFixedValues = array[1].Trim();
			break;
		default:
			data.Invalid = true;
			if (DebugLogEnabled)
			{
				Log.Warning("[{0}.readString] entityclasses.xml unknown key:{1} in raw:{2}", "DismembermentManager", text, rawString);
			}
			break;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static DismemberedPartData readRandomPart(string[] prefabs, string[] data, string tag)
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
	public static DismemberedPartData readPart(string[] data)
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

	public static void SpawnParticleEffect(ParticleEffect _pe, int _entityId = -1)
	{
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			if (!GameManager.IsDedicatedServer)
			{
				GameManager.Instance.SpawnParticleEffectClient(_pe, _entityId, _forceCreation: false, _worldSpawn: true);
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
