using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Animations.Rigging;

public static class CharacterConstructUtils
{
	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly List<Cloth> tempCloths = new List<Cloth>();

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly List<Material> tempMats = new List<Material>();

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly List<SkinnedMeshRenderer> tempSMRs = new List<SkinnedMeshRenderer>();

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly string[] shortHairNames = new string[7] { "buzzcut", "comb_over", "cornrows", "flattop_fro", "mohawk", "pixie_cut", "small_fro" };

	[PublicizedFrom(EAccessModifier.Private)]
	public const string ORIGIN = "Origin";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string RIGCON = "RigConstraints";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string IKRIG = "IKRig";

	public const string HEAD = "head";

	public const string EYES = "eyes";

	public const string BEARD = "beard";

	public const string HAIR = "hair";

	public const string BODY = "body";

	public const string HANDS = "hands";

	public const string FEET = "feet";

	public const string HELMET = "helmet";

	public const string TORSO = "torso";

	public const string GLOVES = "gloves";

	public const string BOOTS = "boots";

	public const string SEX_MARKER = "{sex}";

	public const string RACE_MARKER = "{race}";

	public const string VARIANT_MARKER = "{variant}";

	public const string HAIR_MARKER = "{hair}";

	[PublicizedFrom(EAccessModifier.Private)]
	public static Archetype archetype;

	[PublicizedFrom(EAccessModifier.Private)]
	public static string[] baseParts = new string[4] { "head", "body", "hands", "feet" };

	public static string baseBodyLoc
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return "@:Entities/Player/" + archetype.Sex + "/Common/Meshes/player" + archetype.Sex + ".fbx";
		}
	}

	public static string baseHeadLoc
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return "@:Entities/Player/" + archetype.Sex + "/Heads/" + archetype.Race + "/" + archetype.Variant.ToString("00") + "/Meshes/player" + archetype.Sex + archetype.Race + archetype.Variant.ToString("00") + ".fbx";
		}
	}

	public static string baseHairLoc
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return "@:Entities/Player/" + archetype.Sex + "/Hair/" + archetype.Hair + "/HairMorphMatrix/" + archetype.Race + archetype.Variant.ToString("00");
		}
	}

	public static string baseMustacheLoc
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return "@:Entities/Player/" + archetype.Sex + "/FacialHair/Mustache/" + archetype.MustacheName + "/HairMorphMatrix/" + archetype.Race + archetype.Variant.ToString("00");
		}
	}

	public static string baseChopsLoc
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return "@:Entities/Player/" + archetype.Sex + "/FacialHair/Chops/" + archetype.ChopsName + "/HairMorphMatrix/" + archetype.Race + archetype.Variant.ToString("00");
		}
	}

	public static string baseBeardLoc
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return "@:Entities/Player/" + archetype.Sex + "/FacialHair/Beard/" + archetype.BeardName + "/HairMorphMatrix/" + archetype.Race + archetype.Variant.ToString("00");
		}
	}

	public static string baseHairColorLoc
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return "@:Entities/Player/Common/HairColorSwatches";
		}
	}

	public static string baseEyeColorMatLoc
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return "@:Entities/Player/Common/Eyes/Materials/" + archetype.EyeColorName + ".mat";
		}
	}

	public static string baseBodyMatLoc
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return "@:Entities/Player/" + archetype.Sex + "/Heads/" + archetype.Race + "/" + archetype.Variant.ToString("00") + "/Materials/player" + archetype.Sex + archetype.Race + archetype.Variant.ToString("00") + "_Body.mat";
		}
	}

	public static string baseHeadMatLoc
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return "@:Entities/Player/" + archetype.Sex + "/Heads/" + archetype.Race + "/" + archetype.Variant.ToString("00") + "/Materials/player" + archetype.Sex + archetype.Race + archetype.Variant.ToString("00") + "_Head.mat";
		}
	}

	public static string baseHandsMatLoc
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return "@:Entities/Player/" + archetype.Sex + "/Heads/" + archetype.Race + "/" + archetype.Variant.ToString("00") + "/Materials/player" + archetype.Sex + archetype.Race + archetype.Variant.ToString("00") + "_Hand.mat";
		}
	}

	public static string baseRigPrefab
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return "@:Entities/Player/Common/BaseRigs/baseRigPrefab.prefab";
		}
	}

	public static RuntimeAnimatorController UIAnimController
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return DataLoader.LoadAsset<RuntimeAnimatorController>("@:Entities/Player/Common/AnimControllers/MenuSDCSController.controller");
		}
	}

	public static GameObject Stitch(GameObject sourceObj, GameObject parentObj, SDCSUtils.TransformCatalog boneCatalog, Material eyeMat = null)
	{
		GameObject gameObject = UnityEngine.Object.Instantiate(sourceObj, parentObj.transform);
		gameObject.name = sourceObj.name;
		gameObject.GetComponentsInChildren(tempSMRs);
		foreach (SkinnedMeshRenderer tempSMR in tempSMRs)
		{
			string name = tempSMR.gameObject.name;
			tempSMR.bones = TranslateTransforms(tempSMR.bones, boneCatalog);
			tempSMR.rootBone = Find(boneCatalog, tempSMR.rootBone.name);
			tempSMR.updateWhenOffscreen = true;
			Material[] sharedMaterials = tempSMR.sharedMaterials;
			for (int i = 0; i < sharedMaterials.Length; i++)
			{
				Material material = sharedMaterials[i];
				if ((bool)material)
				{
					string name2 = material.name;
					if (name2.Contains("_Body"))
					{
						Material material2 = DataLoader.LoadAsset<Material>(baseBodyMatLoc);
						sharedMaterials[i] = (material2 ? material2 : sharedMaterials[i]);
					}
					else if (name2.Contains("_Head"))
					{
						Material material3 = DataLoader.LoadAsset<Material>(baseHeadMatLoc);
						sharedMaterials[i] = (material3 ? material3 : sharedMaterials[i]);
					}
					else if (name2.Contains("_Hand"))
					{
						Material material4 = DataLoader.LoadAsset<Material>(baseHandsMatLoc);
						sharedMaterials[i] = (material4 ? material4 : sharedMaterials[i]);
					}
				}
			}
			if (name == "eyes" && (bool)eyeMat)
			{
				sharedMaterials[0] = eyeMat;
			}
			tempSMR.sharedMaterials = sharedMaterials;
		}
		tempSMRs.Clear();
		Transform transform = boneCatalog["Hips"];
		gameObject.GetComponentsInChildren(tempCloths);
		foreach (Cloth tempCloth in tempCloths)
		{
			tempCloth.capsuleColliders = transform.GetComponentsInChildren<CapsuleCollider>();
		}
		tempCloths.Clear();
		return gameObject;
	}

	public static void MatchRigs(SDCSUtils.SlotData wornItem, Transform source, Transform target, SDCSUtils.TransformCatalog transformCatalog)
	{
		Transform transform = source.Find("Origin");
		Transform transform2 = target.Find("Origin");
		if ((bool)transform && (bool)transform2)
		{
			AddMissingChildren(wornItem, transform, transform2, transformCatalog);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void AddMissingChildren(SDCSUtils.SlotData wornItem, Transform sourceT, Transform targetT, SDCSUtils.TransformCatalog transformCatalog)
	{
		if (!sourceT || !targetT)
		{
			return;
		}
		foreach (Transform item in sourceT)
		{
			string name = item.name;
			Transform transform2 = null;
			bool flag = false;
			foreach (Transform item2 in targetT)
			{
				string name2 = item2.name;
				if (name2 == name)
				{
					transform2 = item2;
					flag = true;
					if (!transformCatalog.ContainsKey(name2))
					{
						transformCatalog.Add(name2, transform2);
					}
					break;
				}
			}
			if (!flag)
			{
				transform2 = UnityEngine.Object.Instantiate(item.gameObject).transform;
				transform2.SetParent(targetT, worldPositionStays: false);
				transform2.name = name;
				transformCatalog[name] = transform2;
			}
			if (!flag)
			{
				transform2.SetLocalPositionAndRotation(item.localPosition, item.localRotation);
				transform2.localScale = item.localScale;
			}
			TransferCharacterJoint(item, transform2.gameObject, transformCatalog);
			AddMissingChildren(wornItem, item, transform2, transformCatalog);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void SetupRigConstraints(RigBuilder rigBuilder, Transform sourceRootT, Transform targetRootT, SDCSUtils.TransformCatalog transformCatalog)
	{
		if (!sourceRootT.GetComponent<RigBuilder>())
		{
			return;
		}
		Transform transform = sourceRootT.Find("RigConstraints");
		if (!transform)
		{
			return;
		}
		string text = transform.name + "_" + transform.parent.name;
		Transform transform2 = targetRootT.Find(text);
		if (!transform2)
		{
			transform2 = UnityEngine.Object.Instantiate(transform, targetRootT);
			transform2.name = text;
			transform2.SetLocalPositionAndRotation(transform.localPosition, transform.localRotation);
			transform2.localScale = transform.localScale;
		}
		Rig component = transform2.GetComponent<Rig>();
		if (!component)
		{
			return;
		}
		rigBuilder.layers.Add(new RigLayer(component));
		BlendConstraint[] componentsInChildren = transform.GetComponentsInChildren<BlendConstraint>();
		BlendConstraint[] componentsInChildren2 = transform2.GetComponentsInChildren<BlendConstraint>();
		foreach (BlendConstraint blendConstraint in componentsInChildren2)
		{
			string name = blendConstraint.name;
			BlendConstraint[] array = componentsInChildren;
			foreach (BlendConstraint blendConstraint2 in array)
			{
				if (blendConstraint2.name == name)
				{
					blendConstraint.data.constrainedObject = Find(transformCatalog, blendConstraint2.data.constrainedObject.name);
					blendConstraint.data.sourceObjectA = Find(transformCatalog, blendConstraint2.data.sourceObjectA.name);
					blendConstraint.data.sourceObjectB = Find(transformCatalog, blendConstraint2.data.sourceObjectB.name);
					break;
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void TransferCharacterJoint(Transform source, GameObject newBone, SDCSUtils.TransformCatalog transformCatalog)
	{
		CharacterJoint component;
		CharacterJoint characterJoint;
		if ((component = source.GetComponent<CharacterJoint>()) != null && (characterJoint = newBone.AddMissingComponent<CharacterJoint>()) != null)
		{
			characterJoint.connectedBody = Find(transformCatalog, component.connectedBody.name)?.GetComponent<Rigidbody>();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static Transform[] TranslateTransforms(Transform[] transforms, SDCSUtils.TransformCatalog transformCatalog)
	{
		for (int i = 0; i < transforms.Length; i++)
		{
			Transform transform = transforms[i];
			if ((bool)transform)
			{
				transforms[i] = Find(transformCatalog, transform.name);
			}
			else
			{
				Log.Error("Null transform in bone list");
			}
		}
		return transforms;
	}

	public static TValue Find<TKey, TValue>(Dictionary<TKey, TValue> source, TKey key)
	{
		source.TryGetValue(key, out var value);
		return value;
	}

	public static void CreateViz(Archetype _archetype, ref GameObject baseRigUI, ref SDCSUtils.TransformCatalog boneCatalogUI)
	{
		DestroyViz(baseRigUI);
		archetype = _archetype;
		setupRig(ref baseRigUI, ref boneCatalogUI, baseRigPrefab, null, UIAnimController);
		setupBase(baseRigUI, boneCatalogUI, baseParts);
		setupHairObjects(baseRigUI, boneCatalogUI, _archetype.Hair, _archetype.MustacheName, _archetype.ChopsName, _archetype.BeardName);
		setupEquipment(baseRigUI, boneCatalogUI, _archetype.Equipment, _ignoreDlcEntitlements: false);
		Transform transform = baseRigUI.transform.Find("IKRig");
		if (transform != null)
		{
			transform.GetComponent<Rig>().weight = 0f;
		}
		HingeJoint[] componentsInChildren = baseRigUI.GetComponentsInChildren<HingeJoint>();
		foreach (HingeJoint hingeJoint in componentsInChildren)
		{
			if (hingeJoint.connectedBody == null)
			{
				Log.Warning("SDCSUtils::CreateVizUI: No connected body for " + hingeJoint.transform.name + "'s HingeJoint! Disabling for UI until this is solved.");
				hingeJoint.gameObject.SetActive(value: false);
			}
		}
	}

	public static void DestroyViz(GameObject _baseRigUI, bool _keepRig = false)
	{
		if (!_baseRigUI)
		{
			return;
		}
		Transform transform = _baseRigUI.transform;
		for (int i = 0; i < transform.childCount; i++)
		{
			Transform child = transform.GetChild(i);
			if (!(child.name != "Origin"))
			{
				continue;
			}
			child.GetComponentsInChildren(includeInactive: true, tempSMRs);
			foreach (SkinnedMeshRenderer tempSMR in tempSMRs)
			{
				Mesh sharedMesh = tempSMR.sharedMesh;
				if (MeshMorph.IsInstance(sharedMesh))
				{
					UnityEngine.Object.Destroy(sharedMesh);
				}
				tempSMR.GetSharedMaterials(tempMats);
				Utils.CleanupMaterials(tempMats);
				tempMats.Clear();
			}
		}
		if (!_keepRig)
		{
			UnityEngine.Object.DestroyImmediate(_baseRigUI);
		}
	}

	public static void SetVisible(GameObject _baseRigUI, bool _visible)
	{
		if (!_baseRigUI)
		{
			return;
		}
		Transform transform = _baseRigUI.transform;
		for (int i = 0; i < transform.childCount; i++)
		{
			Transform child = transform.GetChild(i);
			if (!(child.name != "Origin"))
			{
				continue;
			}
			child.GetComponentsInChildren(includeInactive: true, tempSMRs);
			foreach (SkinnedMeshRenderer tempSMR in tempSMRs)
			{
				tempSMR.gameObject.SetActive(_visible);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void setupRig(ref GameObject _rigObj, ref SDCSUtils.TransformCatalog _boneCatalog, string prefabLocation, Transform parent, RuntimeAnimatorController animController)
	{
		if (!_rigObj)
		{
			_rigObj = UnityEngine.Object.Instantiate(DataLoader.LoadAsset<GameObject>(prefabLocation), parent);
			_boneCatalog = new SDCSUtils.TransformCatalog(_rigObj.transform);
			Animator component = _rigObj.GetComponent<Animator>();
			if ((bool)component && component.runtimeAnimatorController != animController)
			{
				component.runtimeAnimatorController = animController;
			}
			BoneRenderer[] componentsInChildren = _rigObj.GetComponentsInChildren<BoneRenderer>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].enabled = false;
			}
		}
		else
		{
			cleanupEquipment(_rigObj);
		}
		if (!archetype.IsMale)
		{
			CapsuleCollider orAddComponent = _boneCatalog["Hips"].gameObject.GetOrAddComponent<CapsuleCollider>();
			orAddComponent.center = new Vector3(0f, 0f, -0.03f);
			orAddComponent.radius = 0.15f;
			orAddComponent.height = 0.375f;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void cleanupEquipment(GameObject _rigObj)
	{
		RigBuilder component = _rigObj.GetComponent<RigBuilder>();
		if ((bool)component)
		{
			List<RigLayer> layers = component.layers;
			for (int num = layers.Count - 1; num >= 0; num--)
			{
				if (layers[num].name != "IKRig")
				{
					layers.RemoveAt(num);
				}
			}
			component.Clear();
		}
		Animator component2 = _rigObj.GetComponent<Animator>();
		if ((bool)component2)
		{
			component2.UnbindAllStreamHandles();
		}
		GameUtils.DestroyAllChildrenBut(_rigObj.transform, new List<string> { "Origin", "IKRig" });
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void setupBase(GameObject _rig, SDCSUtils.TransformCatalog _boneCatalog, string[] baseParts)
	{
		foreach (string text in baseParts)
		{
			GameObject gameObject = ((text == "head") ? DataLoader.LoadAsset<GameObject>(baseHeadLoc) : ((!(text == "hands")) ? DataLoader.LoadAsset<GameObject>(baseBodyLoc) : DataLoader.LoadAsset<GameObject>(baseBodyLoc)));
			if (gameObject == null)
			{
				break;
			}
			GameObject bodyPartContainingName;
			if (!((bodyPartContainingName = getBodyPartContainingName(gameObject.transform, text)) == null))
			{
				bodyPartContainingName.name = text;
				Stitch(bodyPartContainingName, _rig, _boneCatalog, DataLoader.LoadAsset<Material>(baseEyeColorMatLoc));
				if (text == "head")
				{
					Transform transform = _rig.transform;
					Transform transform2 = gameObject.transform;
					CharacterGazeController orAddComponent = transform.FindRecursive("Head").gameObject.GetOrAddComponent<CharacterGazeController>();
					orAddComponent.rootTransform = transform.FindRecursive("Origin");
					orAddComponent.neckTransform = _boneCatalog["Neck"];
					orAddComponent.headTransform = _boneCatalog["Head"];
					orAddComponent.leftEyeTransform = _boneCatalog["LeftEye"];
					orAddComponent.rightEyeTransform = _boneCatalog["RightEye"];
					orAddComponent.eyeSkinnedMeshRenderer = transform.FindRecursive("eyes").GetComponent<SkinnedMeshRenderer>();
					orAddComponent.leftEyeLocalPosition = transform2.FindInChildren("LeftEye").localPosition;
					orAddComponent.rightEyeLocalPosition = transform2.FindInChildren("RightEye").localPosition;
					orAddComponent.eyeLookAtTargetAngle = 35f;
					orAddComponent.eyeRotationSpeed = 30f;
					orAddComponent.twitchSpeed = 25f;
					orAddComponent.headLookAtTargetAngle = 45f;
					orAddComponent.headRotationSpeed = 7f;
					orAddComponent.maxLookAtDistance = 5f;
					EyeLidController orAddComponent2 = transform.FindRecursive("Head").gameObject.GetOrAddComponent<EyeLidController>();
					orAddComponent2.leftTopTransform = _boneCatalog["LeftEyelidTop"];
					orAddComponent2.leftBottomTransform = _boneCatalog["LeftEyelidBot"];
					orAddComponent2.rightTopTransform = _boneCatalog["RightEyelidTop"];
					orAddComponent2.rightBottomTransform = _boneCatalog["RightEyelidBot"];
					orAddComponent2.leftTopLocalPosition = transform2.FindInChildren("LeftEyelidTop").localPosition;
					orAddComponent2.leftBottomLocalPosition = transform2.FindInChildren("LeftEyelidBot").localPosition;
					orAddComponent2.leftTopRotation = transform2.FindInChildren("LeftEyelidTop").localRotation;
					orAddComponent2.leftBottomRotation = transform2.FindInChildren("LeftEyelidBot").localRotation;
					orAddComponent2.rightTopLocalPosition = transform2.FindInChildren("RightEyelidTop").localPosition;
					orAddComponent2.rightBottomLocalPosition = transform2.FindInChildren("RightEyelidBot").localPosition;
					orAddComponent2.rightTopRotation = transform2.FindInChildren("RightEyelidTop").localRotation;
					orAddComponent2.rightBottomRotation = transform2.FindInChildren("RightEyelidBot").localRotation;
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void setupEquipment(GameObject _rig, SDCSUtils.TransformCatalog _boneCatalog, List<SDCSUtils.SlotData> slotData, bool _ignoreDlcEntitlements)
	{
		if (slotData == null)
		{
			return;
		}
		List<Transform> allGears = new List<Transform>();
		Transform transform = _rig.transform.Find("Origin");
		if ((bool)transform)
		{
			List<Transform> list = findStartsWith(transform, "RigConstraints");
			int count = list.Count;
			for (int i = 0; i < count; i++)
			{
				UnityEngine.Object.DestroyImmediate(list[i].gameObject);
			}
		}
		foreach (SDCSUtils.SlotData slotDatum in slotData)
		{
			if ("head".Equals(slotDatum.PartName, StringComparison.OrdinalIgnoreCase) && slotDatum.PrefabName != null && slotDatum.PrefabName.Contains("HeadGearMorphMatrix", StringComparison.OrdinalIgnoreCase))
			{
				setupHeadgearMorph(_rig, _boneCatalog, slotDatum, _ignoreDlcEntitlements);
				continue;
			}
			Transform transform2 = setupEquipmentSlot(_rig, _boneCatalog, slotDatum, allGears, _ignoreDlcEntitlements);
			if ((bool)transform2)
			{
				Morphable componentInChildren = Stitch(transform2.gameObject, _rig, _boneCatalog).GetComponentInChildren<Morphable>();
				if ((bool)componentInChildren)
				{
					componentInChildren.MorphHeadgear(archetype, _ignoreDlcEntitlements);
				}
			}
		}
		List<RigBuilder> rbs = new List<RigBuilder>();
		setupEquipmentCommon(_rig, _boneCatalog, allGears, rbs);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static Transform setupEquipmentSlot(GameObject _rig, SDCSUtils.TransformCatalog _boneCatalog, SDCSUtils.SlotData wornItem, List<Transform> allGears, bool _ignoreDlcEntitlements)
	{
		string pathForSlotData = GetPathForSlotData(wornItem, _headgearShortHairMask: true);
		if (string.IsNullOrEmpty(pathForSlotData))
		{
			return null;
		}
		GameObject gameObject = DataLoader.LoadAsset<GameObject>(pathForSlotData, _ignoreDlcEntitlements);
		if (gameObject == null && wornItem.PartName == "head")
		{
			pathForSlotData = GetPathForSlotData(wornItem);
			gameObject = DataLoader.LoadAsset<GameObject>(pathForSlotData, _ignoreDlcEntitlements);
		}
		if (!gameObject)
		{
			Log.Warning("SDCSUtils::" + pathForSlotData + " not found for item " + wornItem.PrefabName + "!");
			return null;
		}
		MatchRigs(wornItem, gameObject.transform, _rig.transform, _boneCatalog);
		if (!allGears.Contains(gameObject.transform))
		{
			allGears.Add(gameObject.transform);
		}
		Transform clothingPartWithName = getClothingPartWithName(gameObject, parseSexedLocation(wornItem.PartName, archetype.Sex));
		if ((bool)clothingPartWithName)
		{
			string baseToTurnOff = wornItem.BaseToTurnOff;
			if (baseToTurnOff != null && baseToTurnOff.Length > 0)
			{
				string[] array = wornItem.BaseToTurnOff.Split(',');
				foreach (string name in array)
				{
					Transform transform = _rig.transform.FindInChildren(name);
					if ((bool)transform)
					{
						UnityEngine.Object.Destroy(transform.gameObject);
					}
				}
			}
			if (!clothingPartWithName.gameObject.activeSelf)
			{
				clothingPartWithName.gameObject.SetActive(value: true);
			}
		}
		return clothingPartWithName;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void setupEquipmentCommon(GameObject _rigObj, SDCSUtils.TransformCatalog _boneCatalog, List<Transform> allGears, List<RigBuilder> rbs)
	{
		RigBuilder rigBuilder = _rigObj.GetComponent<RigBuilder>();
		if (!rigBuilder)
		{
			rigBuilder = _rigObj.AddComponent<RigBuilder>();
		}
		rigBuilder.enabled = false;
		rbs.Add(rigBuilder);
		foreach (Transform allGear in allGears)
		{
			SetupRigConstraints(rigBuilder, allGear, _rigObj.transform, _boneCatalog);
		}
		HingeJoint[] componentsInChildren = _rigObj.GetComponentsInChildren<HingeJoint>();
		foreach (HingeJoint hingeJoint in componentsInChildren)
		{
			if (hingeJoint.connectedBody != null && _boneCatalog.ContainsKey(hingeJoint.connectedBody.transform.name))
			{
				hingeJoint.connectedBody = _boneCatalog[hingeJoint.connectedBody.transform.name].GetComponent<Rigidbody>();
			}
			hingeJoint.autoConfigureConnectedAnchor = true;
		}
		rigBuilder.enabled = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void setupHairObjects(GameObject _rig, SDCSUtils.TransformCatalog _boneCatalog, string hairName, string mustacheName, string chopsName, string beardName)
	{
		HairColorSwatch hairColorSwatch = null;
		if (!string.IsNullOrEmpty(archetype.HairColor))
		{
			ScriptableObject scriptableObject = DataLoader.LoadAsset<ScriptableObject>(baseHairColorLoc + "/" + archetype.HairColor + ".asset");
			if (!(scriptableObject == null))
			{
				hairColorSwatch = scriptableObject as HairColorSwatch;
			}
		}
		bool flag = false;
		bool flag2 = true;
		if (archetype.Equipment != null && archetype.Equipment.Count > 0)
		{
			foreach (SDCSUtils.SlotData item in archetype.Equipment)
			{
				if (item.PartName == "head")
				{
					flag = item.HairMaskType == SDCSUtils.SlotData.HairMaskTypes.Hat;
					flag2 = item.FacialHairMaskType != SDCSUtils.SlotData.HairMaskTypes.None;
				}
			}
		}
		if (!string.IsNullOrEmpty(hairName))
		{
			if (flag)
			{
				setupHair(_rig, _boneCatalog, baseHairLoc + "/hair_" + hairName + "_hat.asset", hairName);
			}
			else
			{
				setupHair(_rig, _boneCatalog, baseHairLoc + "/hair_" + hairName + ".asset", hairName);
			}
		}
		if (flag2)
		{
			if (!string.IsNullOrEmpty(mustacheName))
			{
				setupHair(_rig, _boneCatalog, baseMustacheLoc + "/hair_facial_mustache" + mustacheName + ".asset", mustacheName);
			}
			if (!string.IsNullOrEmpty(chopsName))
			{
				setupHair(_rig, _boneCatalog, baseChopsLoc + "/hair_facial_sideburns" + chopsName + ".asset", chopsName);
			}
			if (!string.IsNullOrEmpty(beardName))
			{
				setupHair(_rig, _boneCatalog, baseBeardLoc + "/hair_facial_beard" + beardName + ".asset", beardName);
			}
		}
		if (hairColorSwatch != null)
		{
			ApplySwatchToGameObject(_rig, hairColorSwatch);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void ApplySwatchToGameObject(GameObject targetGameObject, HairColorSwatch hairSwatch)
	{
		if (targetGameObject != null)
		{
			Renderer[] componentsInChildren = targetGameObject.GetComponentsInChildren<Renderer>(includeInactive: true);
			foreach (Renderer renderer in componentsInChildren)
			{
				Material[] array = ((!Application.isPlaying) ? renderer.sharedMaterials : renderer.materials);
				Material[] array2 = array;
				foreach (Material material in array2)
				{
					if (material.shader.name == "Game/SDCS/Hair" && !material.name.Contains("lashes"))
					{
						hairSwatch.ApplyToMaterial(material);
					}
				}
			}
		}
		else
		{
			Debug.LogWarning("No target GameObject selected.");
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void setupHair(GameObject _rig, SDCSUtils.TransformCatalog _boneCatalog, string path, string hairName)
	{
		if (string.IsNullOrEmpty(hairName))
		{
			return;
		}
		GameObject gameObject = DataLoader.LoadAsset<MeshMorph>(path)?.GetMorphedSkinnedMesh();
		if (gameObject == null)
		{
			Log.Warning("SDCSUtils::" + path + " not found for hair " + hairName + "!");
		}
		else
		{
			MatchRigs(null, gameObject.transform, _rig.transform, _boneCatalog);
			if (!gameObject.gameObject.activeSelf)
			{
				gameObject.gameObject.SetActive(value: true);
			}
			Stitch(gameObject.gameObject, _rig, _boneCatalog);
			UnityEngine.Object.Destroy(gameObject);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static string GetPathForSlotData(SDCSUtils.SlotData _slotData, bool _headgearShortHairMask = false)
	{
		if (_slotData == null)
		{
			return null;
		}
		if (string.IsNullOrEmpty(_slotData.PartName))
		{
			return null;
		}
		if (_slotData.PartName.Equals("head", StringComparison.OrdinalIgnoreCase))
		{
			string text = _slotData.PrefabName;
			if (text.Contains("{sex}"))
			{
				text = text.Replace("{sex}", archetype.Sex);
			}
			if (text.Contains("{race}"))
			{
				text = text.Replace("{race}", archetype.Race);
			}
			if (text.Contains("{variant}"))
			{
				text = text.Replace("{variant}", archetype.Variant.ToString("00"));
			}
			if (text.Contains("{hair}"))
			{
				text = ((!_headgearShortHairMask || (!string.IsNullOrEmpty(archetype.Hair) && !shortHairNames.ContainsCaseInsensitive(archetype.Hair))) ? text.Replace("{hair}", "") : text.Replace("{hair}", "Bald"));
			}
			return text;
		}
		if (string.IsNullOrEmpty(_slotData.PrefabName))
		{
			return null;
		}
		return parseSexedLocation(_slotData.PrefabName, archetype.Sex);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void setupHeadgearMorph(GameObject _rig, SDCSUtils.TransformCatalog _boneCatalog, SDCSUtils.SlotData _slotData, bool _ignoreDlcEntitlements)
	{
		string pathForSlotData = GetPathForSlotData(_slotData, _headgearShortHairMask: true);
		if (string.IsNullOrEmpty(pathForSlotData))
		{
			return;
		}
		MeshMorph meshMorph = DataLoader.LoadAsset<MeshMorph>(pathForSlotData, _ignoreDlcEntitlements);
		if (meshMorph == null)
		{
			pathForSlotData = GetPathForSlotData(_slotData);
			meshMorph = DataLoader.LoadAsset<MeshMorph>(pathForSlotData, _ignoreDlcEntitlements);
		}
		GameObject gameObject = meshMorph?.GetMorphedSkinnedMesh();
		if (gameObject == null)
		{
			Log.Warning("SDCSUtils::" + pathForSlotData + " not found for headgear " + _slotData.PrefabName + "!");
			return;
		}
		MatchRigs(null, gameObject.transform, _rig.transform, _boneCatalog);
		if (!gameObject.gameObject.activeSelf)
		{
			gameObject.gameObject.SetActive(value: true);
		}
		DataLoader.LoadAsset<Material>(baseBodyMatLoc);
		Stitch(gameObject.gameObject, _rig, _boneCatalog);
		UnityEngine.Object.Destroy(gameObject);
	}

	public static bool BasePartsExist(Archetype _archetype)
	{
		archetype = _archetype;
		if (!DataLoader.LoadAsset<GameObject>(baseBodyLoc))
		{
			Log.Error("base body not found at " + baseBodyLoc);
			return false;
		}
		if (!DataLoader.LoadAsset<GameObject>(baseHeadLoc))
		{
			Log.Error("base head not found at " + baseHeadLoc);
			return false;
		}
		if (!DataLoader.LoadAsset<Material>(baseBodyMatLoc))
		{
			Log.Error("body material not found at " + baseBodyMatLoc);
			return false;
		}
		if (!DataLoader.LoadAsset<Material>(baseHeadMatLoc))
		{
			Log.Error("head material not found at " + baseHeadMatLoc);
			return false;
		}
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static List<Transform> findStartsWith(Transform parent, string key)
	{
		List<Transform> list = new List<Transform>();
		foreach (Transform item in parent)
		{
			if (item.name.StartsWith(key))
			{
				list.Add(item);
			}
		}
		return list;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static GameObject getBodyPartContainingName(Transform parent, string name)
	{
		foreach (Transform item in parent.transform)
		{
			if (item.name.ToLower().Contains(name))
			{
				return item.gameObject;
			}
		}
		return null;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static Transform getClothingPartWithName(GameObject clothingPrefab, string partName)
	{
		foreach (Transform item in clothingPrefab.transform)
		{
			if (item.name.ToLower() == partName.ToLower())
			{
				return item;
			}
		}
		return null;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static string parseSexedLocation(string sexedLocation, string sex)
	{
		return sexedLocation.Replace("{sex}", sex);
	}
}
