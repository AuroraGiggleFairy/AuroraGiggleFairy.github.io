using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using UnityEngine;

public class BlocksFromXml
{
	public static IEnumerator CreateBlocks(XmlFile _xmlFile, bool _fillLookupTable, bool _bEditMode = false)
	{
		XElement root = _xmlFile.XmlDoc.Root;
		if (!root.HasElements)
		{
			throw new Exception("No element <blocks> found!");
		}
		if (root.HasAttribute("defaultDescriptionKey"))
		{
			Block.defaultBlockDescriptionKey = root.GetAttribute("defaultDescriptionKey");
		}
		MicroStopwatch msw = new MicroStopwatch(_bStart: true);
		int i = 0;
		int totalBlocks = root.Elements("block").Count();
		LocalPlayerUI ui = LocalPlayerUI.primaryUI;
		bool progressWindowOpen = (bool)ui && ui.windowManager.IsWindowOpen(XUiC_ProgressWindow.ID);
		foreach (XElement item in root.Elements("block"))
		{
			i++;
			if (item.HasAttribute(XNames.shapes))
			{
				yield return ShapesFromXml.CreateShapeVariants(_bEditMode, item);
			}
			else
			{
				ParseBlock(_bEditMode, item);
			}
			if (msw.ElapsedMilliseconds > Constants.cMaxLoadTimePerFrameMillis)
			{
				if (progressWindowOpen)
				{
					XUiC_ProgressWindow.SetText(ui, string.Format(Localization.Get("uiLoadLoadingXmlBlocks"), Math.Min(100.0, 105.0 * (double)i / (double)totalBlocks).ToString("0")));
				}
				yield return null;
				msw.ResetAndRestart();
			}
		}
		if (progressWindowOpen)
		{
			XUiC_ProgressWindow.SetText(ui, string.Format(Localization.Get("uiLoadLoadingXmlBlocks"), "100"));
			yield return null;
			XUiC_ProgressWindow.SetText(ui, Localization.Get("uiLoadLoadingXml"));
		}
		if (Application.isPlaying)
		{
			Resources.UnloadUnusedAssets();
		}
		ShapesFromXml.Cleanup();
	}

	public static void ParseBlock(bool _bEditMode, XElement elementBlock)
	{
		DynamicProperties properties = ParseProperties(elementBlock);
		string attribute = elementBlock.GetAttribute(XNames.name);
		Block block = CreateBlock(_bEditMode, attribute, properties);
		ParseItemDrops(block, elementBlock, out var dropExtendsOff);
		if (!dropExtendsOff)
		{
			LoadExtendedItemDrops(block);
		}
		InitBlock(block);
	}

	public static void ParseExtendedBlock(XElement elementBlock, out string extendedBlockName, out string excludedPropertiesList)
	{
		XElement xElement = (from e in elementBlock.Elements(XNames.property)
			where e.GetAttribute(XNames.name) == "Extends"
			select e)?.FirstOrDefault();
		if (xElement != null)
		{
			extendedBlockName = xElement.GetAttribute(XNames.value);
			excludedPropertiesList = xElement.GetAttribute(XNames.param1);
		}
		else
		{
			extendedBlockName = null;
			excludedPropertiesList = null;
		}
	}

	public static DynamicProperties ParseProperties(XElement elementBlock)
	{
		ParseExtendedBlock(elementBlock, out var extendedBlockName, out var excludedPropertiesList);
		DynamicProperties dynamicProperties = CreateProperties(extendedBlockName, excludedPropertiesList);
		LoadProperties(dynamicProperties, elementBlock);
		return dynamicProperties;
	}

	public static DynamicProperties CreateProperties(string extendedBlockName = null, string excludedPropertiesList = null)
	{
		DynamicProperties dynamicProperties = new DynamicProperties();
		if (extendedBlockName != null)
		{
			Block blockByName = Block.GetBlockByName(extendedBlockName);
			if (blockByName == null)
			{
				throw new Exception($"Could not find Extends block {extendedBlockName}");
			}
			HashSet<string> hashSet = new HashSet<string> { Block.PropCreativeMode };
			if (!string.IsNullOrEmpty(excludedPropertiesList))
			{
				string[] array = excludedPropertiesList.Split(',');
				foreach (string text in array)
				{
					hashSet.Add(text.Trim());
				}
			}
			dynamicProperties.CopyFrom(blockByName.Properties, hashSet);
		}
		return dynamicProperties;
	}

	public static void LoadProperties(DynamicProperties properties, XElement elementBlock)
	{
		foreach (XElement item in elementBlock.Elements(XNames.property))
		{
			properties.Add(item);
		}
	}

	public static Block CreateBlock(bool _bEditMode, string blockName, DynamicProperties properties)
	{
		Block block;
		if (properties.Values.ContainsKey("Class"))
		{
			string text = properties.Values["Class"];
			Type typeWithPrefix = ReflectionHelpers.GetTypeWithPrefix("Block", text);
			if (typeWithPrefix == null || (block = Activator.CreateInstance(typeWithPrefix) as Block) == null)
			{
				throw new Exception("Class '" + text + "' not found on block " + blockName + "!");
			}
		}
		else
		{
			block = new Block();
		}
		block.Properties = properties;
		block.SetBlockName(blockName);
		block.ResourceScale = 1f;
		properties.ParseFloat(Block.PropResourceScale, ref block.ResourceScale);
		BlockPlacement blockPlacementHelper = BlockPlacement.None;
		if (properties.Values.ContainsKey("Place"))
		{
			string text2 = properties.Values["Place"];
			try
			{
				blockPlacementHelper = (BlockPlacement)Activator.CreateInstance(ReflectionHelpers.GetTypeWithPrefix("BlockPlacement", text2));
			}
			catch (Exception)
			{
				throw new Exception("No block placement class '" + text2 + "' found on block " + blockName + "!");
			}
		}
		block.BlockPlacementHelper = blockPlacementHelper;
		string text3 = properties.Values["Material"];
		block.blockMaterial = MaterialBlock.fromString(text3);
		if (text3 == null || text3.Length == 0)
		{
			throw new Exception("Block with name=" + blockName + " has no material defined");
		}
		if (block.blockMaterial == null)
		{
			throw new Exception("Block with name=" + blockName + " references a not existing material '" + text3 + "'");
		}
		BlockShape shape;
		if (properties.Values.ContainsKey("Shape"))
		{
			string text4 = properties.Values["Shape"];
			try
			{
				shape = (BlockShape)Activator.CreateInstance(ReflectionHelpers.GetTypeWithPrefix("BlockShape", text4));
			}
			catch (Exception)
			{
				throw new Exception("Shape class '" + text4 + "' not found for block " + blockName);
			}
		}
		else
		{
			shape = new BlockShapeNew();
			block.Properties.Values["Model"] = "@:Shapes/Cube.fbx";
		}
		block.shape = shape;
		if (properties.Values.ContainsKey("ShapeMinBB"))
		{
			Vector3 minAABB = StringParsers.ParseVector3(properties.Values["ShapeMinBB"]);
			block.shape.SetMinAABB(minAABB);
		}
		if (properties.Values.ContainsKey("Mesh"))
		{
			block.MeshIndex = byte.MaxValue;
			string text5 = properties.Values["Mesh"];
			for (int i = 0; i < MeshDescription.meshes.Length; i++)
			{
				if (text5.Equals(MeshDescription.meshes[i].Name))
				{
					block.MeshIndex = (byte)i;
					break;
				}
			}
			if (block.MeshIndex == byte.MaxValue)
			{
				throw new Exception("Unknown mesh attribute '" + text5 + "' on block " + blockName);
			}
		}
		if (!_bEditMode && properties.Values.ContainsKey("Stacknumber"))
		{
			block.Stacknumber = int.Parse(properties.Values["Stacknumber"]);
		}
		else
		{
			block.Stacknumber = 500;
		}
		if (properties.Values.ContainsKey("Light"))
		{
			block.SetLightValue(StringParsers.ParseFloat(properties.Values["Light"]));
		}
		if (properties.Values.ContainsKey("MovementFactor"))
		{
			block.MovementFactor = StringParsers.ParseFloat(properties.Values["MovementFactor"]);
		}
		else
		{
			block.MovementFactor = block.blockMaterial.MovementFactor;
		}
		if (block.MovementFactor <= 0f)
		{
			block.MovementFactor = 1f;
		}
		block.IsCheckCollideWithEntity |= block.MovementFactor != 1f;
		if (properties.Values.ContainsKey("EconomicValue"))
		{
			block.EconomicValue = StringParsers.ParseFloat(properties.Values["EconomicValue"]);
		}
		if (properties.Values.ContainsKey("Collide"))
		{
			string a = properties.Values["Collide"];
			block.BlockingType = 0;
			if (a.ContainsCaseInsensitive("sight"))
			{
				block.BlockingType |= 1;
			}
			if (a.ContainsCaseInsensitive("movement"))
			{
				block.BlockingType |= 2;
			}
			if (a.ContainsCaseInsensitive("bullet"))
			{
				block.BlockingType |= 4;
			}
			if (a.ContainsCaseInsensitive("rocket"))
			{
				block.BlockingType |= 8;
			}
			if (a.ContainsCaseInsensitive("arrow"))
			{
				block.BlockingType |= 32;
			}
			if (a.ContainsCaseInsensitive("melee"))
			{
				block.BlockingType |= 16;
			}
		}
		else
		{
			block.BlockingType = (block.blockMaterial.IsCollidable ? 255 : 0);
		}
		if (properties.Values.TryGetValue("Path", out var _value))
		{
			if (_value.EqualsCaseInsensitive("solid"))
			{
				block.PathType = 1;
			}
			if (_value.EqualsCaseInsensitive("scan"))
			{
				block.PathType = -1;
			}
		}
		else if (properties.Values.TryGetValue("Model", out _value) && (_value.EqualsCaseInsensitive("@:Shapes/cube.fbx") || _value.EqualsCaseInsensitive("@:Shapes/cube_glass.fbx") || _value.EqualsCaseInsensitive("@:Shapes/cube_frame.fbx")))
		{
			block.PathType = 1;
		}
		if (properties.Values.TryGetValue("WaterFlow", out var _value2))
		{
			block.WaterFlowMask = StringParsers.ParseWaterFlowMask(_value2);
		}
		if (properties.Values.TryGetValue("WaterClipPlane", out var _value3))
		{
			block.WaterClipPlane = StringParsers.ParsePlane(_value3);
			block.WaterClipEnabled = true;
		}
		else
		{
			block.WaterClipEnabled = false;
		}
		for (int j = 0; j < 1; j++)
		{
			string texture = ShapesFromXml.TextureLabelsByChannel[j].Texture;
			string text6 = properties.GetString(texture);
			if (text6.Length <= 0)
			{
				continue;
			}
			try
			{
				if (text6.Contains(","))
				{
					string[] texIds = text6.Split(',');
					block.SetSideTextureId(texIds, j);
				}
				else
				{
					int textureId = int.Parse(text6);
					block.SetSideTextureId(textureId, j);
				}
			}
			catch (Exception)
			{
				throw new Exception("Error parsing \"" + texture + "\" texture id '" + text6 + "' in block with name=" + blockName);
			}
		}
		properties.ParseInt("TerrainIndex", ref block.TerrainTAIndex);
		if (properties.Values.ContainsKey("BlockTag"))
		{
			block.BlockTag = EnumUtils.Parse<BlockTags>(properties.Values["BlockTag"]);
		}
		if (properties.Values.ContainsKey("StabilitySupport"))
		{
			block.StabilitySupport = properties.GetBool("StabilitySupport");
		}
		else
		{
			block.StabilitySupport = block.blockMaterial.StabilitySupport;
		}
		if (properties.Values.ContainsKey("StabilityFull"))
		{
			block.StabilityFull = properties.GetBool("StabilityFull");
		}
		if (properties.Values.ContainsKey("StabilityIgnore"))
		{
			block.StabilityIgnore = properties.GetBool("StabilityIgnore");
		}
		if (properties.Values.ContainsKey("Density"))
		{
			block.Density = (sbyte)(properties.GetFloat("Density") * (float)(block.shape.IsTerrain() ? MarchingCubes.DensityTerrain : MarchingCubes.DensityAir));
		}
		else
		{
			block.Density = (block.shape.IsTerrain() ? MarchingCubes.DensityTerrain : MarchingCubes.DensityAir);
		}
		DynamicProperties dynamicProperties = properties.Classes["RepairItems"];
		if (dynamicProperties != null)
		{
			block.RepairItems = new List<Block.SItemNameCount>();
			foreach (KeyValuePair<string, string> item3 in dynamicProperties.Values.Dict)
			{
				Block.SItemNameCount item = new Block.SItemNameCount
				{
					ItemName = item3.Key,
					Count = int.Parse(dynamicProperties.Values[item3.Key])
				};
				block.RepairItems.Add(item);
			}
		}
		DynamicProperties dynamicProperties2 = properties.Classes["RepairItemsMeshDamage"];
		if (dynamicProperties2 != null)
		{
			block.RepairItemsMeshDamage = new List<Block.SItemNameCount>();
			Block.SItemNameCount item2 = default(Block.SItemNameCount);
			foreach (KeyValuePair<string, string> item4 in dynamicProperties2.Values.Dict)
			{
				item2.ItemName = item4.Key;
				item2.Count = int.Parse(dynamicProperties2.Values[item4.Key]);
				block.RepairItemsMeshDamage.Add(item2);
			}
		}
		if (properties.Values.ContainsKey("RestrictSubmergedPlacement"))
		{
			block.bRestrictSubmergedPlacement = properties.GetBool("RestrictSubmergedPlacement");
		}
		return block;
	}

	public static void ParseItemDrops(Block block, XElement elementBlock, out bool dropExtendsOff)
	{
		dropExtendsOff = false;
		foreach (XElement item in elementBlock.Elements())
		{
			if (item.Name == XNames.dropextendsoff)
			{
				dropExtendsOff = true;
			}
			else if (item.Name == XNames.drop)
			{
				XElement xElement = item;
				string attribute = xElement.GetAttribute(XNames.name);
				int _minCount = 1;
				int _maxCount = 1;
				if (xElement.HasAttribute(XNames.count))
				{
					StringParsers.ParseMinMaxCount(xElement.GetAttribute(XNames.count), out _minCount, out _maxCount);
				}
				float optionalValue = 1f;
				DynamicProperties.ParseFloat(xElement, "prob", ref optionalValue);
				optionalValue *= block.ResourceScale;
				EnumDropEvent eEvent = EnumDropEvent.Destroy;
				if (xElement.HasAttribute(XNames.event_))
				{
					eEvent = EnumUtils.Parse<EnumDropEvent>(xElement.GetAttribute(XNames.event_));
				}
				float optionalValue2 = 0f;
				DynamicProperties.ParseFloat(xElement, "stick_chance", ref optionalValue2);
				string toolCategory = null;
				if (xElement.HasAttribute(XNames.tool_category))
				{
					toolCategory = xElement.GetAttribute(XNames.tool_category);
				}
				string attribute2 = xElement.GetAttribute(XNames.tag);
				block.AddDroppedId(eEvent, attribute, _minCount, _maxCount, optionalValue, block.ResourceScale, optionalValue2, toolCategory, attribute2);
			}
		}
	}

	public static void LoadExtendedItemDrops(Block block)
	{
		if (block.Properties.Values.ContainsKey("Extends"))
		{
			Block blockByName = Block.GetBlockByName(block.Properties.Values["Extends"]);
			block.CopyDroppedFrom(blockByName);
		}
	}

	public static void InitBlock(Block block)
	{
		block.shape.Init(block);
		block.Init();
	}

	public static bool FindExternalModels(XmlFile _xmlFile, string _meshName, Dictionary<string, string> _referencedModels)
	{
		try
		{
			XElement root = _xmlFile.XmlDoc.Root;
			if (!root.HasElements)
			{
				throw new Exception("No element <blocks> found!");
			}
			HashSet<string> hashSet = new HashSet<string>();
			foreach (XElement item2 in root.Elements(XNames.block))
			{
				string attribute = item2.GetAttribute(XNames.name);
				DynamicProperties dynamicProperties = new DynamicProperties();
				foreach (XElement item3 in item2.Elements(XNames.property))
				{
					dynamicProperties.Add(item3);
				}
				bool flag = false;
				if (dynamicProperties.Values.ContainsKey("Extends"))
				{
					string item = dynamicProperties.Values["Extends"];
					flag = hashSet.Contains(item);
				}
				bool flag2 = dynamicProperties.Values.ContainsKey("Shape") && dynamicProperties.Values["Shape"].StartsWith("Ext3dModel");
				bool flag3 = dynamicProperties.Values.ContainsKey("Shape") && !dynamicProperties.Values["Shape"].StartsWith("Ext3dModel");
				if (!((flag && !flag3) || flag2))
				{
					continue;
				}
				string text = "opaque";
				if (dynamicProperties.Values.ContainsKey("Mesh"))
				{
					text = dynamicProperties.Values["Mesh"];
				}
				if (flag || text.Equals(_meshName))
				{
					string text2 = dynamicProperties.Values["Model"];
					if (text2 != null && !_referencedModels.ContainsKey(text2))
					{
						_referencedModels.Add(text2, dynamicProperties.Params1["Model"]);
					}
					hashSet.Add(attribute);
				}
			}
		}
		catch (Exception ex)
		{
			Log.Error("Loading and parsing '" + _xmlFile.Filename + "' (" + ex.Message + ")");
			Log.Error("Loading of blocks aborted due to errors!");
			Log.Error(ex.StackTrace);
			return false;
		}
		return true;
	}

	public static HashSet<int> GetTextureIdsForMesh(XmlFile _xmlFile, string _meshName)
	{
		HashSet<int> hashSet = new HashSet<int>();
		try
		{
			XElement root = _xmlFile.XmlDoc.Root;
			if (!root.HasElements)
			{
				throw new Exception("No element <blocks> found!");
			}
			Dictionary<string, DynamicProperties> dictionary = new Dictionary<string, DynamicProperties>();
			foreach (XElement item in root.Elements(XNames.block))
			{
				DynamicProperties dynamicProperties = new DynamicProperties();
				foreach (XElement item2 in item.Elements(XNames.property))
				{
					dynamicProperties.Add(item2);
				}
				if (dynamicProperties.Values.ContainsKey("Extends"))
				{
					string text = dynamicProperties.Values["Extends"];
					if (!dictionary.ContainsKey(text))
					{
						Log.Error($"Extends references not existing block {text}");
					}
					else
					{
						DynamicProperties dynamicProperties2 = new DynamicProperties();
						dynamicProperties2.CopyFrom(dictionary[text]);
						dynamicProperties2.CopyFrom(dynamicProperties);
						dynamicProperties = dynamicProperties2;
					}
				}
				string attribute = item.GetAttribute(XNames.name);
				try
				{
					dictionary.Add(attribute, dynamicProperties);
				}
				catch (Exception)
				{
					throw new Exception("Duplicate block with name " + attribute);
				}
			}
			foreach (XElement item3 in root.Elements(XNames.block))
			{
				DynamicProperties dynamicProperties3 = dictionary[item3.GetAttribute(XNames.name)];
				string text2 = "opaque";
				if (dynamicProperties3.Values.ContainsKey("Mesh"))
				{
					text2 = dynamicProperties3.Values["Mesh"];
				}
				if (!text2.Equals(_meshName) || (dynamicProperties3.Values.ContainsKey("Shape") && (dynamicProperties3.Values["Shape"].Equals("ModelEntity") || dynamicProperties3.Values["Shape"].Equals("DistantDeco"))))
				{
					continue;
				}
				for (int i = 0; i < 1; i++)
				{
					if (!dynamicProperties3.Values.TryGetValue(ShapesFromXml.TextureLabelsByChannel[i].Texture, out var _value))
					{
						continue;
					}
					try
					{
						int result;
						if (_value.Contains(","))
						{
							string[] array = _value.Split(new char[1] { ',' });
							for (int j = 0; j < array.Length; j++)
							{
								hashSet.Add(int.Parse(array[j]));
							}
						}
						else if (int.TryParse(_value, out result))
						{
							hashSet.Add(result);
						}
					}
					catch (Exception)
					{
						throw new Exception("Error parsing texture id '" + _value + "' on layer '" + (i + 1) + "' in block " + item3.GetAttribute(XNames.name));
					}
				}
			}
		}
		catch (Exception ex3)
		{
			Log.Error("Loading and parsing '" + _xmlFile.Filename + "' (" + ex3.Message + ")");
			Log.Error("Loading of blocks aborted due to errors!");
			Log.Error(ex3.StackTrace);
		}
		return hashSet;
	}
}
