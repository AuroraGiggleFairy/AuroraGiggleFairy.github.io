using System;
using System.Collections;
using System.Xml.Linq;
using UnityEngine;

public class MaterialsFromXml
{
	public static IEnumerator CreateMaterials(XmlFile _xmlFile)
	{
		XElement root = _xmlFile.XmlDoc.Root;
		if (!root.HasElements)
		{
			throw new Exception("No element <materials> found!");
		}
		foreach (XElement item in root.Elements("material"))
		{
			string attribute = item.GetAttribute("id");
			if (attribute == null || attribute.Length == 0)
			{
				throw new Exception("Material defined without a tag 'id'");
			}
			MaterialBlock materialBlock = new MaterialBlock(attribute);
			foreach (XElement item2 in item.Elements())
			{
				materialBlock.Properties.Add(item2);
			}
			if (materialBlock.Properties.Values.ContainsKey("damage_category"))
			{
				materialBlock.DamageCategory = materialBlock.Properties.Values["damage_category"];
			}
			if (materialBlock.Properties.Values.ContainsKey("surface_category"))
			{
				materialBlock.SurfaceCategory = materialBlock.Properties.Values["surface_category"];
			}
			if (materialBlock.Properties.Values.ContainsKey("particle_category"))
			{
				materialBlock.ParticleCategory = materialBlock.Properties.Values["particle_category"];
			}
			if (materialBlock.Properties.Values.ContainsKey("particle_destroy_category"))
			{
				materialBlock.ParticleDestroyCategory = materialBlock.Properties.Values["particle_destroy_category"];
			}
			if (materialBlock.Properties.Values.ContainsKey("forge_category"))
			{
				materialBlock.ForgeCategory = materialBlock.Properties.Values["forge_category"];
			}
			if (materialBlock.Properties.Values.ContainsKey("liquid"))
			{
				materialBlock.IsLiquid = materialBlock.Properties.GetBool("liquid");
			}
			if (materialBlock.Properties.Values.ContainsKey("Hardness"))
			{
				materialBlock.Hardness = new DataItem<float>(materialBlock.Properties.GetFloat("Hardness"));
			}
			else
			{
				materialBlock.Hardness = new DataItem<float>();
			}
			float optionalValue = 0f;
			materialBlock.Properties.ParseFloat("explosionresistance", ref optionalValue);
			materialBlock.ExplosionResistance = optionalValue;
			if (materialBlock.Properties.Values.ContainsKey("collidable"))
			{
				materialBlock.IsCollidable = materialBlock.Properties.GetBool("collidable");
			}
			if (materialBlock.Properties.Values.ContainsKey("lightopacity"))
			{
				materialBlock.LightOpacity = materialBlock.Properties.GetInt("lightopacity");
			}
			if (materialBlock.Properties.Values.ContainsKey("fertile_level"))
			{
				materialBlock.FertileLevel = materialBlock.Properties.GetInt("fertile_level");
			}
			if (materialBlock.Properties.Values.ContainsKey("plant"))
			{
				materialBlock.IsPlant = materialBlock.Properties.GetBool("plant");
			}
			if (materialBlock.Properties.Values.ContainsKey("movement_factor"))
			{
				materialBlock.MovementFactor = materialBlock.Properties.GetFloat("movement_factor");
			}
			if (materialBlock.Properties.Values.ContainsKey("friction"))
			{
				materialBlock.Friction = Mathf.Clamp(materialBlock.Properties.GetFloat("friction"), 0.01f, 1f);
			}
			else
			{
				materialBlock.Friction = 0.454f;
			}
			if (materialBlock.Properties.Values.ContainsKey("fertile_level"))
			{
				materialBlock.FertileLevel = materialBlock.Properties.GetInt("fertile_level");
			}
			if (materialBlock.Properties.Values.ContainsKey("ground_cover"))
			{
				materialBlock.IsGroundCover = materialBlock.Properties.GetBool("ground_cover");
			}
			if (materialBlock.Properties.Values.ContainsKey("stability_glue"))
			{
				materialBlock.StabilityGlue = materialBlock.Properties.GetInt("stability_glue");
			}
			if (materialBlock.Properties.Values.ContainsKey("Mass"))
			{
				materialBlock.Mass = new DataItem<int>(materialBlock.Properties.GetInt("Mass"));
			}
			else
			{
				materialBlock.Mass = new DataItem<int>();
				materialBlock.Mass.Value = 2;
			}
			if (materialBlock.Properties.Values.ContainsKey("stepsound"))
			{
				string text = materialBlock.Properties.Values["stepsound"];
				materialBlock.stepSound = StepSound.FromString(text);
				if (materialBlock.stepSound == null)
				{
					throw new Exception("Stepsound with name '" + text + "' not found");
				}
			}
			if (materialBlock.Properties.Values.ContainsKey("StabilitySupport"))
			{
				materialBlock.StabilitySupport = materialBlock.Properties.GetBool("StabilitySupport");
			}
			if (materialBlock.Properties.Values.ContainsKey("MaxDamage"))
			{
				materialBlock.MaxDamage = materialBlock.Properties.GetInt("MaxDamage");
			}
			else
			{
				materialBlock.MaxDamage = 100;
			}
			if (materialBlock.Properties.Values.ContainsKey("MaxIncomingDamage"))
			{
				materialBlock.MaxIncomingDamage = materialBlock.Properties.GetInt("MaxIncomingDamage");
			}
			else
			{
				materialBlock.MaxIncomingDamage = int.MaxValue;
			}
			if (materialBlock.Properties.Values.ContainsKey("IgnoreDamageFromTag"))
			{
				materialBlock.IgnoreDamageFromTag = FastTags<TagGroup.Global>.Parse(materialBlock.Properties.Values["IgnoreDamageFromTag"]);
			}
			else
			{
				materialBlock.IgnoreDamageFromTag = FastTags<TagGroup.Global>.none;
			}
			if (materialBlock.Properties.Values.ContainsKey("Experience"))
			{
				materialBlock.Experience = materialBlock.Properties.GetFloat("Experience");
			}
			materialBlock.Properties.ParseBool("CanDestroy", ref materialBlock.CanDestroy);
		}
		yield break;
	}
}
