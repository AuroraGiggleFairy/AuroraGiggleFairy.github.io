using System.Collections.Generic;

public class MaterialBlock
{
	public static MaterialBlock air;

	public static Dictionary<string, MaterialBlock> materials;

	public DynamicProperties Properties;

	public bool StabilitySupport = true;

	public DataItem<float> Hardness;

	public StepSound stepSound;

	public bool IsCollidable;

	public int LightOpacity;

	public int StabilityGlue = 6;

	public bool IsLiquid;

	public string DamageCategory;

	public string SurfaceCategory;

	public string ParticleCategory;

	public string ParticleDestroyCategory;

	public string ForgeCategory;

	public int FertileLevel;

	public bool IsPlant;

	public float MovementFactor = 1f;

	public float Friction = 1f;

	public DataItem<int> Mass;

	public int MaxDamage;

	public int MaxIncomingDamage = int.MaxValue;

	public float Experience = 1f;

	public string id;

	public FastTags<TagGroup.Global> IgnoreDamageFromTag = FastTags<TagGroup.Global>.none;

	public bool IsGroundCover;

	public bool CanDestroy = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public float explosionResistance;

	public float ExplosionResistance
	{
		get
		{
			return explosionResistance;
		}
		set
		{
			explosionResistance = Utils.FastClamp01(value);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	static MaterialBlock()
	{
		materials = new Dictionary<string, MaterialBlock>();
		air = new MaterialBlock();
	}

	public static void Cleanup()
	{
		materials = new Dictionary<string, MaterialBlock>();
	}

	public MaterialBlock()
	{
		Properties = new DynamicProperties();
	}

	public MaterialBlock(string _id)
		: this()
	{
		id = _id;
		IsCollidable = true;
		LightOpacity = 0;
		materials[_id] = this;
	}

	public static MaterialBlock fromString(string _name)
	{
		if (!materials.ContainsKey(_name))
		{
			return null;
		}
		return materials[_name];
	}

	public string GetLocalizedMaterialName()
	{
		return Localization.Get("material" + id);
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public bool CheckDamageIgnore(FastTags<TagGroup.Global> _tags, EntityAlive _entity)
	{
		if (!IgnoreDamageFromTag.IsEmpty && _tags.Test_AnySet(IgnoreDamageFromTag))
		{
			if (_entity != null)
			{
				_entity.PlayOneShot("keystone_impact_overlay");
			}
			return true;
		}
		return false;
	}
}
