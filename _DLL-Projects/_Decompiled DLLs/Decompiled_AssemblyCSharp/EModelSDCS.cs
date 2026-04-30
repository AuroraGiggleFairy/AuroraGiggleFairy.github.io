using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class EModelSDCS : EModelPlayer
{
	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public EntityClass entityClass;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public EntityPlayer playerEntity;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public GameObject baseRig;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public SDCSUtils.TransformCatalog boneCatalog;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public GameObject baseRigFP;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public SDCSUtils.TransformCatalog boneCatalogFP;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Transform playerModelTransform;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Transform headT;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Transform headTransFP;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public UpdateLightOnPlayers updateLightScript;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Archetype archetype;

	public SDCSUtils.SlotData.HairMaskTypes HairMaskType;

	public SDCSUtils.SlotData.HairMaskTypes FacialHairMaskType;

	public List<Material> ClipMaterialsFP = new List<Material>();

	public bool isMale
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			if (archetype != null)
			{
				return archetype.Sex == "Male";
			}
			return true;
		}
	}

	public override Transform NeckTransform
	{
		get
		{
			if (!base.IsFPV)
			{
				return boneCatalog["Neck"];
			}
			return boneCatalogFP["Neck"];
		}
	}

	public Transform HeadTransformFP
	{
		get
		{
			if (headTransFP == null && boneCatalogFP.ContainsKey("Head"))
			{
				headTransFP = boneCatalogFP["Head"];
			}
			return headTransFP;
		}
	}

	public Archetype Archetype => archetype;

	public void Awake()
	{
		playerEntity = base.transform.GetComponent<EntityPlayerLocal>();
		if (playerEntity == null)
		{
			playerEntity = base.transform.GetComponent<EntityPlayer>();
		}
	}

	public override void Init(World _world, Entity _entity)
	{
		entity = _entity;
		entityClass = EntityClass.list[entity.entityClass];
		archetype = playerEntity.playerProfile.CreateTempArchetype();
		ragdollChance = entityClass.RagdollOnDeathChance;
		bHasRagdoll = entityClass.HasRagdoll;
		modelTransformParent = EModelBase.FindModel(base.transform);
		base.IsFPV = entity is EntityPlayerLocal;
		createModel(_world, entityClass);
		createAvatarController(EntityClass.list[entity.entityClass]);
		XUiM_PlayerEquipment.HandleRefreshEquipment += XUiM_PlayerEquipment_HandleRefreshEquipment;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnDestroy()
	{
		XUiM_PlayerEquipment.HandleRefreshEquipment -= XUiM_PlayerEquipment_HandleRefreshEquipment;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void XUiM_PlayerEquipment_HandleRefreshEquipment(XUiM_PlayerEquipment playerEquipment)
	{
		if (playerEquipment.Equipment == playerEntity.equipment)
		{
			UpdateEquipment();
		}
	}

	public void UpdateEquipment()
	{
		Animator animator = avatarController.GetAnimator();
		if ((bool)animator && (base.IsFPV || animator.enabled))
		{
			GenerateMeshes();
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void LateUpdate()
	{
		base.LateUpdate();
		if (!base.IsFPV || !(HeadTransformFP != null))
		{
			return;
		}
		foreach (Material item in ClipMaterialsFP)
		{
			item.SetVector("_ClipCenter", HeadTransformFP.position);
		}
	}

	public override void SwitchModelAndView(bool _bFPV, bool _isMale)
	{
		base.IsFPV = _bFPV;
		playerEntity.IsMale = isMale;
		GenerateMeshes();
		base.SwitchModelAndView(base.IsFPV, _isMale);
		meshTransform = modelTransform.FindInChildren("Spine1");
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void createModel(World _world, EntityClass _ec)
	{
		if (!(modelTransformParent == null))
		{
			if (playerModelTransform != null)
			{
				UnityEngine.Object.Destroy(playerModelTransform.gameObject);
			}
			_ec.mesh = null;
			playerModelTransform = GenerateMeshes();
			playerModelTransform.name = (modelName = "player_" + archetype.Sex + "Ragdoll");
			playerModelTransform.tag = "E_BP_Body";
			playerModelTransform.SetParent(modelTransformParent, worldPositionStays: false);
			playerModelTransform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
			playerModelTransform.gameObject.GetOrAddComponent<AnimationEventBridge>();
			updateLightScript = playerModelTransform.gameObject.GetOrAddComponent<UpdateLightOnPlayers>();
			updateLightScript.IsDynamicObject = true;
			if (entity is EntityAlive entityAlive)
			{
				entityAlive.ReassignEquipmentTransforms();
			}
			baseRig.transform.FindInChilds("Origin").tag = "E_BP_BipedRoot";
		}
	}

	public Transform GenerateMeshes()
	{
		SDCSUtils.CreateVizTP(archetype, ref baseRig, ref boneCatalog, playerEntity, base.IsFPV);
		if (playerEntity as EntityPlayerLocal != null)
		{
			SDCSUtils.CreateVizFP(archetype, ref baseRigFP, ref boneCatalogFP, playerEntity, base.IsFPV);
		}
		ClothSimInit();
		return baseRig.transform;
	}

	public override Transform GetHeadTransform()
	{
		if (headT == null && boneCatalog.ContainsKey("Head"))
		{
			headT = boneCatalog["Head"];
		}
		return headT;
	}

	public override Transform GetPelvisTransform()
	{
		if (bipedPelvisTransform == null && boneCatalog.ContainsKey("Hips"))
		{
			bipedPelvisTransform = boneCatalog["Hips"];
		}
		return bipedPelvisTransform;
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public void SetRace(string value)
	{
		archetype.Race = value;
		if (SDCSUtils.BasePartsExist(archetype))
		{
			SwitchModelAndView(base.IsFPV, isMale);
		}
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public void SetVariant(int value)
	{
		archetype.Variant = (byte)value;
		if (SDCSUtils.BasePartsExist(archetype))
		{
			SwitchModelAndView(base.IsFPV, isMale);
		}
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public void SetSex(bool value)
	{
		archetype.IsMale = value;
		if (SDCSUtils.BasePartsExist(archetype))
		{
			SwitchModelAndView(base.IsFPV, isMale);
		}
	}

	public override void SetVisible(bool _bVisible, bool _isKeepColliders = false)
	{
		bool flag = base.visible;
		if (_bVisible != flag)
		{
			SDCSUtils.SetVisible(baseRig, _bVisible);
		}
		base.SetVisible(_bVisible, _isKeepColliders);
	}
}
