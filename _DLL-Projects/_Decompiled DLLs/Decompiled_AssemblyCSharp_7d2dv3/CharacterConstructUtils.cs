using System;
using System.Collections.Generic;
using System.Linq;
using GearVariants;
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
	public static readonly string[] shortHairNames = new string[5] { "buzzcut", "cornrows", "flattop_fro", "mohawk", "small_fro" };

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly List<SkinnedMeshRenderer> _smrBuf = new List<SkinnedMeshRenderer>(64);

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly List<HingeJoint> _hingeBuf = new List<HingeJoint>(32);

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly List<BlendConstraint> _bcBuf = new List<BlendConstraint>(32);

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
			return DataLoader.LoadAsset<RuntimeAnimatorController>("@:Entities/Player/Common/AnimControllers/MenuSDCS" + archetype.Sex + "Controller" + (archetype.IsMale ? ".controller" : ".overrideController"));
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
				if ((bool)sharedMaterials[i] && sharedMaterials[i].HasColor("_Tint"))
				{
					string name2 = sharedMaterials[i].name;
					Material material = null;
					if (name2.Contains("_Body"))
					{
						material = DataLoader.LoadAsset<Material>(baseBodyMatLoc);
					}
					else if (name2.Contains("_Head"))
					{
						material = DataLoader.LoadAsset<Material>(baseHeadMatLoc);
					}
					else if (name2.Contains("_Hand"))
					{
						material = DataLoader.LoadAsset<Material>(baseHandsMatLoc);
					}
					if (material != null && material.HasColor("_Tint"))
					{
						sharedMaterials[i] = new Material(sharedMaterials[i]);
						sharedMaterials[i].SetColor("_Tint", material.GetColor("_Tint"));
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

	[PublicizedFrom(EAccessModifier.Private)]
	public static HashSet<string> CollectRequiredNamesForSlot(Transform root, Transform slotSubRoot)
	{
		HashSet<string> hashSet = new HashSet<string>(StringComparer.Ordinal);
		GearBoneMap component = root.GetComponent<GearBoneMap>();
		if (component != null)
		{
			IReadOnlyList<Transform> partBones = component.GetPartBones(slotSubRoot.name);
			if (partBones != null && partBones.Count > 0)
			{
				foreach (Transform item in partBones)
				{
					if ((bool)item)
					{
						hashSet.Add(item.name);
					}
				}
			}
		}
		else
		{
			Debug.LogWarning("[SDCSUtils] No GearBoneMap found on root " + root.name + ", falling back to collecting all bones from SMRs under " + slotSubRoot.name + ".");
			_smrBuf.Clear();
			slotSubRoot.GetComponentsInChildren(includeInactive: true, _smrBuf);
			for (int i = 0; i < _smrBuf.Count; i++)
			{
				SkinnedMeshRenderer skinnedMeshRenderer = _smrBuf[i];
				Transform[] array = ((skinnedMeshRenderer != null) ? skinnedMeshRenderer.bones : null);
				if (array == null)
				{
					continue;
				}
				foreach (Transform transform in array)
				{
					if ((bool)transform)
					{
						hashSet.Add(transform.name);
					}
				}
			}
		}
		hashSet = BuildAllowedWithAncestors(root, hashSet);
		_hingeBuf.Clear();
		slotSubRoot.GetComponentsInChildren(includeInactive: true, _hingeBuf);
		for (int k = 0; k < _hingeBuf.Count; k++)
		{
			HingeJoint hingeJoint = _hingeBuf[k];
			if ((bool)hingeJoint)
			{
				hashSet.Add(hingeJoint.transform.name);
				if ((bool)hingeJoint.connectedBody)
				{
					hashSet.Add(hingeJoint.connectedBody.transform.name);
				}
			}
		}
		_bcBuf.Clear();
		root.GetComponentsInChildren(includeInactive: true, _bcBuf);
		for (int l = 0; l < _bcBuf.Count; l++)
		{
			BlendConstraint blendConstraint = _bcBuf[l];
			if (!blendConstraint)
			{
				continue;
			}
			Transform constrainedObject = blendConstraint.data.constrainedObject;
			Transform sourceObjectA = blendConstraint.data.sourceObjectA;
			Transform sourceObjectB = blendConstraint.data.sourceObjectB;
			if ((bool)constrainedObject && hashSet.Contains(constrainedObject.name))
			{
				if ((bool)sourceObjectA)
				{
					hashSet.Add(sourceObjectA.name);
				}
				if ((bool)sourceObjectB)
				{
					hashSet.Add(sourceObjectB.name);
				}
			}
		}
		return BuildAllowedWithAncestors(root, hashSet);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static Dictionary<string, Transform> MapSourceByName(Transform sourceOrigin)
	{
		Dictionary<string, Transform> map = new Dictionary<string, Transform>(StringComparer.Ordinal);
		Recurse(sourceOrigin);
		return map;
		[PublicizedFrom(EAccessModifier.Internal)]
		void Recurse(Transform t)
		{
			map[t.name] = t;
			for (int i = 0; i < t.childCount; i++)
			{
				Recurse(t.GetChild(i));
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static HashSet<string> BuildAllowedWithAncestors(Transform sourceOrigin, HashSet<string> allowedBones)
	{
		HashSet<string> hashSet = new HashSet<string>(allowedBones, StringComparer.Ordinal);
		Dictionary<string, Transform> dictionary = MapSourceByName(sourceOrigin);
		foreach (string allowedBone in allowedBones)
		{
			if (!dictionary.TryGetValue(allowedBone, out var value))
			{
				continue;
			}
			Transform parent = value.parent;
			while (parent != null)
			{
				hashSet.Add(parent.name);
				if (parent == sourceOrigin)
				{
					break;
				}
				parent = parent.parent;
			}
		}
		return hashSet;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void AddRequiredChildren(Transform sourceT, Transform targetT, SDCSUtils.TransformCatalog catalog, HashSet<string> allowedBones, AuxBoneTracker auxBoneTracker = null)
	{
		if (auxBoneTracker == null && catalog != null && catalog["Origin"] != null)
		{
			auxBoneTracker = catalog["Origin"].GetComponent<AuxBoneTracker>();
		}
		for (int i = 0; i < sourceT.childCount; i++)
		{
			Transform child = sourceT.GetChild(i);
			if (!allowedBones.Contains(child.name))
			{
				continue;
			}
			Transform transform = null;
			for (int j = 0; j < targetT.childCount; j++)
			{
				Transform child2 = targetT.GetChild(j);
				if (child2.name == child.name)
				{
					transform = child2;
					break;
				}
			}
			if (!transform)
			{
				transform = UnityEngine.Object.Instantiate(child.gameObject, targetT, worldPositionStays: true).transform;
				transform.name = child.name;
				transform.SetLocalPositionAndRotation(child.localPosition, child.localRotation);
				transform.localScale = child.localScale;
				if ((bool)auxBoneTracker && !auxBoneTracker.AuxBoneLookup.ContainsKey(transform.name))
				{
					auxBoneTracker.AuxBoneLookup.Add(transform.name, transform);
				}
			}
			if (!catalog.ContainsKey(transform.name))
			{
				catalog.Add(transform.name, transform);
			}
			TransferCharacterJoint(child, transform.gameObject, catalog);
			AddRequiredChildren(child, transform, catalog, allowedBones, auxBoneTracker);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void SetupRigConstraints(RigBuilder rigBuilder, Transform sourceRootT, Transform targetRootT, SDCSUtils.TransformCatalog catalog, HashSet<string> allowedBones)
	{
		Transform transform = sourceRootT.parent.Find("RigConstraints");
		if (!transform)
		{
			return;
		}
		string text = "RigConstraints_" + sourceRootT.name;
		Transform transform2 = targetRootT.Find(text);
		if (!transform2)
		{
			transform2 = new GameObject(text).transform;
			transform2.SetParent(targetRootT, worldPositionStays: false);
		}
		Rig rig = transform2.gameObject.GetOrAddComponent<Rig>();
		if (!rigBuilder.layers.Any([PublicizedFrom(EAccessModifier.Internal)] (RigLayer l) => l.rig == rig))
		{
			rigBuilder.layers.Add(new RigLayer(rig));
		}
		for (int num = transform2.childCount - 1; num >= 0; num--)
		{
			UnityEngine.Object.DestroyImmediate(transform2.GetChild(num).gameObject);
		}
		BlendConstraint[] componentsInChildren = transform.GetComponentsInChildren<BlendConstraint>(includeInactive: true);
		foreach (BlendConstraint blendConstraint in componentsInChildren)
		{
			string text2 = ((blendConstraint.data.constrainedObject != null) ? blendConstraint.data.constrainedObject.name : null);
			if (text2 != null && allowedBones.Contains(text2))
			{
				GameObject gameObject = UnityEngine.Object.Instantiate(blendConstraint.gameObject, transform2, worldPositionStays: false);
				gameObject.name = blendConstraint.name;
				BlendConstraint component = gameObject.GetComponent<BlendConstraint>();
				component.data.constrainedObject = ((text2 != null) ? Find(catalog, text2) : null);
				string text3 = ((blendConstraint.data.sourceObjectA != null) ? blendConstraint.data.sourceObjectA.name : null);
				string text4 = ((blendConstraint.data.sourceObjectB != null) ? blendConstraint.data.sourceObjectB.name : null);
				component.data.sourceObjectA = ((text3 != null) ? Find(catalog, text3) : null);
				component.data.sourceObjectB = ((text4 != null) ? Find(catalog, text4) : null);
			}
		}
	}

	public static void MatchRigs(Transform source, Transform target, SDCSUtils.TransformCatalog catalog, HashSet<string> allowedBones)
	{
		Transform transform = source.Find("Origin");
		Transform transform2 = target.Find("Origin");
		if ((bool)transform && (bool)transform2)
		{
			AuxBoneTracker auxBoneTracker = null;
			if (catalog != null && catalog["Origin"] != null)
			{
				auxBoneTracker = catalog["Origin"].GetComponent<AuxBoneTracker>();
			}
			AddRequiredChildren(transform, transform2, catalog, allowedBones, auxBoneTracker);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void TransferCharacterJoint(Transform source, GameObject newBone, SDCSUtils.TransformCatalog transformCatalog)
	{
		CharacterJoint component;
		CharacterJoint characterJoint;
		if ((component = source.GetComponent<CharacterJoint>()) != null && (characterJoint = newBone.AddMissingComponent<CharacterJoint>()) != null)
		{
			Transform transform = Find(transformCatalog, component.connectedBody.name);
			characterJoint.connectedBody = ((transform != null) ? transform.GetComponent<Rigidbody>() : null);
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
		setupEquipment(baseRigUI, boneCatalogUI, _archetype.Equipment, _ignoreDlcEntitlements: true);
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
			BoneRenderer[] componentsInChildren = _rigObj.GetComponentsInChildren<BoneRenderer>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].enabled = false;
			}
		}
		else
		{
			cleanupEquipment(_rigObj, _boneCatalog);
		}
		Animator component = _rigObj.GetComponent<Animator>();
		if ((bool)component && component.runtimeAnimatorController != animController)
		{
			component.runtimeAnimatorController = animController;
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
	public static void cleanupEquipment(GameObject _rigObj, SDCSUtils.TransformCatalog _boneCatalog)
	{
		SDCSUtils.SlotAllowedBonesCache.Clear();
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
		GameUtils.DestroyAllChildrenImmediatelyBut(_rigObj.transform, new List<string> { "Origin", "IKRig" });
		SanitizeRig(_rigObj, _boneCatalog);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void SanitizeRig(GameObject _rigObj, SDCSUtils.TransformCatalog _boneCatalog)
	{
		AuxBoneTracker auxBoneTracker = null;
		if (_boneCatalog != null && _boneCatalog["Origin"] != null)
		{
			auxBoneTracker = _boneCatalog["Origin"].GetComponent<AuxBoneTracker>();
		}
		if ((bool)auxBoneTracker)
		{
			foreach (string key in auxBoneTracker.AuxBoneLookup.Keys)
			{
				if (_boneCatalog.ContainsKey(key) && _boneCatalog[key] != null)
				{
					UnityEngine.Object.DestroyImmediate(_boneCatalog[key].gameObject);
					_boneCatalog.Remove(key);
				}
			}
			auxBoneTracker.AuxBoneLookup.Clear();
		}
		List<string> list = new List<string>();
		foreach (KeyValuePair<string, Transform> item in _boneCatalog)
		{
			if (item.Value == null)
			{
				list.Add(item.Key);
			}
		}
		foreach (string item2 in list)
		{
			_boneCatalog.Remove(item2);
		}
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
					orAddComponent.headLookAtTargetAngle = 75f;
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
		if (archetype.Equipment == null)
		{
			archetype.Equipment = new List<SDCSUtils.SlotData>();
			foreach (SDCSUtils.SlotData slotDatum in slotData)
			{
				archetype.Equipment.Add(slotDatum);
			}
		}
		foreach (SDCSUtils.SlotData slotDatum2 in slotData)
		{
			GameObject gameObject = null;
			if ("head".Equals(slotDatum2.PartName, StringComparison.OrdinalIgnoreCase) && slotDatum2.PrefabName != null && slotDatum2.PrefabName.Contains("HeadGearMorphMatrix", StringComparison.OrdinalIgnoreCase))
			{
				gameObject = setupHeadgearMorph(_rig, _boneCatalog, slotDatum2, _ignoreDlcEntitlements);
			}
			else
			{
				Transform transform2 = setupEquipmentSlot(_rig, _boneCatalog, slotDatum2, allGears, _ignoreDlcEntitlements);
				if ((bool)transform2)
				{
					gameObject = Stitch(transform2.gameObject, _rig, _boneCatalog);
					if (gameObject != null)
					{
						Morphable componentInChildren = gameObject.GetComponentInChildren<Morphable>();
						if ((bool)componentInChildren)
						{
							componentInChildren.MorphHeadgear(archetype, _ignoreDlcEntitlements);
						}
					}
				}
			}
			if (gameObject != null)
			{
				ColorSwatchApplicator componentInChildren2 = gameObject.GetComponentInChildren<ColorSwatchApplicator>();
				if (componentInChildren2 != null)
				{
					componentInChildren2.ApplyColorSwatch(archetype.HairColor);
				}
			}
		}
		setupEquipmentCommon(_rig, _boneCatalog, allGears);
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
		string targetBodyPath = "";
		for (int i = 0; i < archetype.Equipment.Count; i++)
		{
			SDCSUtils.SlotData slotData = archetype.Equipment[i];
			if (slotData.PartName.Equals("body", StringComparison.OrdinalIgnoreCase))
			{
				targetBodyPath = GetPathForSlotData(slotData);
				break;
			}
		}
		Transform clothingPartWithName = getClothingPartWithName(gameObject, getPartNameWithVariant(wornItem.PartName, pathForSlotData, targetBodyPath));
		if ((bool)clothingPartWithName)
		{
			HashSet<string> allowedBones = CollectRequiredNamesForSlot(gameObject.transform, clothingPartWithName);
			SDCSUtils.SlotAllowedBonesCache.Set(clothingPartWithName, allowedBones);
			if (!allGears.Contains(clothingPartWithName))
			{
				allGears.Add(clothingPartWithName);
			}
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
			MatchRigs(gameObject.transform, _rig.transform, _boneCatalog, allowedBones);
		}
		return clothingPartWithName;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static string getPartNameWithVariant(string partName, string sourceGearPath, string targetBodyPath)
	{
		if (archetype == null || archetype.Equipment == null)
		{
			return partName;
		}
		string text = string.Empty;
		if (GearVariantMatrixSO.Instance != null)
		{
			text = GearVariantMatrixSO.Instance.GetVariantOrEmpty(archetype.Sex, partName, sourceGearPath, targetBodyPath);
		}
		else
		{
			Log.Warning("SDCSUtils::getPartNameWithVariant: No GearVariantMatrixSO instance found!");
		}
		if (string.IsNullOrEmpty(text))
		{
			return partName;
		}
		return partName + "_" + text;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void setupEquipmentCommon(GameObject _rigObj, SDCSUtils.TransformCatalog _boneCatalog, List<Transform> allGears)
	{
		RigBuilder orAddComponent = _rigObj.GetOrAddComponent<RigBuilder>();
		orAddComponent.enabled = false;
		foreach (Transform allGear in allGears)
		{
			if (!SDCSUtils.SlotAllowedBonesCache.TryGet(allGear, out var allowedBones) || allowedBones == null || allowedBones.Count == 0)
			{
				Debug.LogWarning("[SDCS] No required leaves cached for slot '" + allGear.name + "'. Skipping constraints.");
			}
			else
			{
				SetupRigConstraints(orAddComponent, allGear, _rigObj.transform, _boneCatalog, allowedBones);
			}
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
		orAddComponent.enabled = true;
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
		if (string.IsNullOrEmpty(hairName) || hairName == "-1" || hairName == "none")
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
	public static GameObject setupHeadgearMorph(GameObject _rig, SDCSUtils.TransformCatalog _boneCatalog, SDCSUtils.SlotData _slotData, bool _ignoreDlcEntitlements)
	{
		string pathForSlotData = GetPathForSlotData(_slotData, _headgearShortHairMask: true);
		if (string.IsNullOrEmpty(pathForSlotData))
		{
			return null;
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
			return null;
		}
		if (!gameObject.gameObject.activeSelf)
		{
			gameObject.gameObject.SetActive(value: true);
		}
		DataLoader.LoadAsset<Material>(baseBodyMatLoc);
		GameObject result = Stitch(gameObject.gameObject, _rig, _boneCatalog);
		UnityEngine.Object.Destroy(gameObject);
		return result;
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
