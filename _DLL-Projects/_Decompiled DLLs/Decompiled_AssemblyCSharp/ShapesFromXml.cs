using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using UnityEngine;

public static class ShapesFromXml
{
	public class ShapeCategory : IComparable<ShapeCategory>, IEquatable<ShapeCategory>, IComparable
	{
		public readonly string Name;

		public readonly string Icon;

		public readonly int Order;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly string localizationName;

		public string LocalizedName => Localization.Get(localizationName);

		public ShapeCategory(string _name, string _icon, int _order)
		{
			Name = _name;
			Icon = _icon;
			Order = _order;
			localizationName = "shapeCategory" + Name;
		}

		public bool Equals(ShapeCategory _other)
		{
			if (_other == null)
			{
				return false;
			}
			if (this == _other)
			{
				return true;
			}
			if (Name == _other.Name && Icon == _other.Icon)
			{
				return Order == _other.Order;
			}
			return false;
		}

		public override bool Equals(object _obj)
		{
			if (_obj == null)
			{
				return false;
			}
			if (this == _obj)
			{
				return true;
			}
			if (_obj.GetType() != GetType())
			{
				return false;
			}
			return Equals((ShapeCategory)_obj);
		}

		public override int GetHashCode()
		{
			return (((((Name != null) ? Name.GetHashCode() : 0) * 397) ^ ((Icon != null) ? Icon.GetHashCode() : 0)) * 397) ^ Order;
		}

		public int CompareTo(ShapeCategory _other)
		{
			if (this == _other)
			{
				return 0;
			}
			if (_other == null)
			{
				return 1;
			}
			int order = Order;
			return order.CompareTo(_other.Order);
		}

		public int CompareTo(object _obj)
		{
			if (_obj == null)
			{
				return 1;
			}
			if (this == _obj)
			{
				return 0;
			}
			if (!(_obj is ShapeCategory other))
			{
				throw new ArgumentException("Object must be of type ShapeCategory");
			}
			return CompareTo(other);
		}
	}

	public enum EDebugLevel
	{
		Off,
		Normal,
		Verbose
	}

	public struct TextureLabels(string suffix)
	{
		public readonly string Texture = "Texture" + suffix;

		public readonly string ImposterExchange = "ImposterExchange" + suffix;

		public readonly string ShapeAltTexture = "ShapeAltTexture" + suffix;

		public readonly string UseGlobalUV = "UseGlobalUV" + suffix;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static EDebugLevel debug;

	public static readonly TextureLabels[] TextureLabelsByChannel;

	public static readonly string VariantHelperName;

	[PublicizedFrom(EAccessModifier.Private)]
	public static Dictionary<string, XElement> shapes;

	public static readonly Dictionary<string, ShapeCategory> shapeCategories;

	public static EDebugLevel DebugLevel => debug;

	[PublicizedFrom(EAccessModifier.Private)]
	static ShapesFromXml()
	{
		debug = EDebugLevel.Off;
		TextureLabelsByChannel = new TextureLabels[1];
		VariantHelperName = "VariantHelper";
		shapeCategories = new CaseInsensitiveStringDictionary<ShapeCategory>();
		string launchArgument = GameUtils.GetLaunchArgument("debugshapes");
		if (launchArgument != null)
		{
			if (launchArgument == "verbose")
			{
				debug = EDebugLevel.Verbose;
			}
			else
			{
				debug = EDebugLevel.Normal;
			}
		}
		TextureLabelsByChannel[0] = new TextureLabels(string.Empty);
		for (int i = 1; i < 1; i++)
		{
			TextureLabelsByChannel[i] = new TextureLabels((i + 1).ToString());
		}
	}

	public static IEnumerator LoadShapes(XmlFile _xmlFile)
	{
		shapes = new CaseInsensitiveStringDictionary<XElement>();
		shapeCategories.Clear();
		BlockShapeNew.Cleanup();
		XElement root = _xmlFile.XmlDoc.Root;
		if (root == null || !root.HasElements)
		{
			throw new Exception("No element <shapes> found!");
		}
		int num = 1;
		foreach (XElement item in root.Elements("shape"))
		{
			string attribute = item.GetAttribute(XNames.name);
			SetProperty(item, Block.PropCreativeSort2, XNames.value, num++.ToString("0000"));
			shapes.Add(attribute, item);
		}
		foreach (XElement item2 in root.Elements("categories"))
		{
			ParseCategories(item2);
		}
		yield break;
	}

	public static void Cleanup()
	{
		shapes.Clear();
		shapes = null;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void ParseCategories(XElement _parentElement)
	{
		foreach (XElement item in _parentElement.Elements(XNames.category))
		{
			string attribute = item.GetAttribute(XNames.name);
			string attribute2 = item.GetAttribute(XNames.icon);
			int order = int.Parse(item.GetAttribute(XNames.order));
			shapeCategories.Add(attribute, new ShapeCategory(attribute, attribute2, order));
		}
	}

	public static IEnumerator CreateShapeVariants(bool _bEditMode, XElement _elementBlock)
	{
		string blockBaseName = _elementBlock.GetAttribute(XNames.name);
		string allowedShapes = _elementBlock.GetAttribute(XNames.shapes);
		StringParsers.TryParseBool(_elementBlock.Attribute(XNames.hideHelperInCreative)?.Value ?? "false", out var hideHelperInCreative);
		if (debug != EDebugLevel.Off)
		{
			Log.Out("Creating block+shape combinations for base block " + blockBaseName);
			Console.WriteLine();
			Console.WriteLine();
			Console.WriteLine();
			Console.WriteLine();
		}
		List<string> shapeNames = new List<string>();
		bool isAll = allowedShapes.EqualsCaseInsensitive("All");
		MicroStopwatch msw = new MicroStopwatch(_bStart: true);
		foreach (KeyValuePair<string, XElement> shape in shapes)
		{
			if (isAll || shape.Value.GetAttribute(XNames.tag).EqualsCaseInsensitive(allowedShapes))
			{
				CreateShapeMaterialCombination(_bEditMode, _elementBlock, blockBaseName, shape);
				shapeNames.Add(shape.Key);
				if (msw.ElapsedMilliseconds > Constants.cMaxLoadTimePerFrameMillis)
				{
					yield return null;
					msw.ResetAndRestart();
				}
			}
		}
		if (shapeNames.Count > 0)
		{
			CreateMaterialHelper(_bEditMode, blockBaseName, shapeNames, hideHelperInCreative);
		}
		if (debug != EDebugLevel.Off)
		{
			Console.WriteLine();
			Console.WriteLine();
			Console.WriteLine();
			Console.WriteLine();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void CreateMaterialHelper(bool _bEditMode, string _blockBaseName, List<string> _shapeNames, bool _hideHelperInCreative)
	{
		string text = _blockBaseName + ":" + VariantHelperName;
		string text2 = _blockBaseName + ":" + _shapeNames[0];
		DynamicProperties dynamicProperties = BlocksFromXml.CreateProperties(text2);
		dynamicProperties.SetValue("Extends", text2);
		dynamicProperties.SetValue(Block.PropCreativeMode, (_hideHelperInCreative ? EnumCreativeMode.None : EnumCreativeMode.All).ToStringCached());
		dynamicProperties.SetValue(Block.PropCreativeSort2, "0000");
		dynamicProperties.SetValue(Block.PropDescriptionKey, "blockVariantHelperGroupDesc");
		dynamicProperties.SetValue(Block.PropItemTypeIcon, "all_blocks");
		dynamicProperties.SetValue("SelectAlternates", "true");
		string value = _blockBaseName + ":" + string.Join("," + _blockBaseName + ":", _shapeNames);
		dynamicProperties.SetValue(Block.PropPlaceAltBlockValue, value);
		dynamicProperties.SetValue(Block.PropAutoShape, EAutoShapeType.Helper.ToStringCached());
		if (debug != EDebugLevel.Off)
		{
			Console.WriteLine("{0}: {1}", text, dynamicProperties.PrettyPrint());
		}
		Block block = BlocksFromXml.CreateBlock(_bEditMode, text, dynamicProperties);
		BlocksFromXml.LoadExtendedItemDrops(block);
		BlocksFromXml.InitBlock(block);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void CreateShapeMaterialCombination(bool _bEditMode, XElement _elementBlock, string _blockBaseName, KeyValuePair<string, XElement> _shapeKvp)
	{
		string key = _shapeKvp.Key;
		string text = _blockBaseName + ":" + key;
		BlocksFromXml.ParseExtendedBlock(_elementBlock, out var extendedBlockName, out var excludedPropertiesList);
		if (extendedBlockName == null)
		{
			BlocksFromXml.ParseExtendedBlock(_shapeKvp.Value, out extendedBlockName, out excludedPropertiesList);
		}
		DynamicProperties dynamicProperties = BlocksFromXml.CreateProperties(extendedBlockName, excludedPropertiesList);
		BlocksFromXml.LoadProperties(dynamicProperties, _elementBlock);
		BlocksFromXml.LoadProperties(dynamicProperties, _shapeKvp.Value);
		if (dynamicProperties.Contains(Block.PropDowngradeBlock))
		{
			string innerText = dynamicProperties.Values[Block.PropDowngradeBlock];
			innerText = AppendShapeName(innerText, _blockBaseName, key);
			dynamicProperties.SetValue(Block.PropDowngradeBlock, innerText);
		}
		if (dynamicProperties.Contains(Block.PropUpgradeBlockClassToBlock))
		{
			string innerText2 = dynamicProperties.Values[Block.PropUpgradeBlockClassToBlock];
			innerText2 = AppendShapeName(innerText2, _blockBaseName, key);
			dynamicProperties.SetValue("UpgradeBlock", "ToBlock", innerText2);
		}
		if (dynamicProperties.Contains(Block.PropSiblingBlock))
		{
			string innerText3 = dynamicProperties.Values[Block.PropSiblingBlock];
			innerText3 = PrependBlockBaseName(innerText3, _blockBaseName);
			dynamicProperties.SetValue(Block.PropSiblingBlock, innerText3);
		}
		if (dynamicProperties.Contains("MirrorSibling"))
		{
			string innerText4 = dynamicProperties.Values["MirrorSibling"];
			innerText4 = PrependBlockBaseName(innerText4, _blockBaseName);
			dynamicProperties.SetValue("MirrorSibling", innerText4);
		}
		dynamicProperties.SetValue(Block.PropCreativeMode, EnumCreativeMode.Dev.ToStringCached());
		dynamicProperties.SetParam1(Block.PropCanPickup, _blockBaseName + ":" + VariantHelperName);
		FixCustomIcon(dynamicProperties, _blockBaseName, _shapeKvp.Key);
		FixImposterExchangeId(dynamicProperties, _blockBaseName, _shapeKvp.Key, 0);
		for (int i = 0; i < 1; i++)
		{
			FixTextureId(dynamicProperties, _blockBaseName, _shapeKvp.Key, i);
		}
		SetMaxDamage(dynamicProperties, _blockBaseName, _shapeKvp.Key);
		dynamicProperties.SetValue("AutoShape", EAutoShapeType.Shape.ToStringCached());
		if (debug != EDebugLevel.Off)
		{
			Console.WriteLine("{0}: {1}", text, dynamicProperties.PrettyPrint());
		}
		Block block = BlocksFromXml.CreateBlock(_bEditMode, text, dynamicProperties);
		BlocksFromXml.ParseItemDrops(block, _shapeKvp.Value, out var dropExtendsOff);
		if (block.itemsToDrop.Count == 0)
		{
			BlocksFromXml.ParseItemDrops(block, _elementBlock, out var dropExtendsOff2);
			if (!dropExtendsOff2 && !dropExtendsOff)
			{
				BlocksFromXml.LoadExtendedItemDrops(block);
			}
		}
		BlocksFromXml.InitBlock(block);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void SetMaxDamage(DynamicProperties _properties, string _blockBaseName, string _shapeName)
	{
		if (!_properties.Contains("MaterialHitpointMultiplier"))
		{
			return;
		}
		if (!_properties.Contains("Material"))
		{
			Log.Warning("Blocks: Shape " + _shapeName + " defines a 'MaterialHitpointMultiplier' but block template " + _blockBaseName + " does not define a 'Material'!");
			return;
		}
		float num = StringParsers.ParseFloat(_properties.GetString("MaterialHitpointMultiplier"));
		if (num != 1f)
		{
			string text = _properties.GetString("Material");
			MaterialBlock materialBlock = MaterialBlock.fromString(text);
			if (materialBlock == null)
			{
				Log.Error("Blocks: Block template " + _blockBaseName + " refers to an unknown Material '" + text + "'!");
			}
			else
			{
				int v = Mathf.RoundToInt(num * (float)materialBlock.MaxDamage);
				v = Utils.FastClamp(v, 1, 65535);
				_properties.SetValue(Block.PropMaxDamage, v.ToString());
				ScaleProperty(_properties, Block.PropUpgradeBlockClass, Block.PropUpgradeBlockItemCount, num);
				_properties.SetValue(Block.PropResourceScale, num.ToCultureInvariantString());
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void FixCustomIcon(DynamicProperties _properties, string _blockBaseName, string _shapeName)
	{
		if (!_properties.Contains(Block.PropCustomIcon))
		{
			if (!_properties.Contains("Model"))
			{
				Log.Warning("Blocks: Neither shape " + _shapeName + " nor the block template " + _blockBaseName + " define a 'CustomIcon' or 'Model'!");
			}
			else
			{
				string text = _properties.GetString("Model");
				_properties.SetValue(Block.PropCustomIcon, "shape" + text);
			}
		}
	}

	public static void SetProperty(XElement _element, string _propertyName, XName _attribName, string _value)
	{
		XElement xElement = (from e in _element.Elements(XNames.property)
			where e.GetAttribute(XNames.name) == _propertyName
			select e).FirstOrDefault();
		if (xElement == null)
		{
			xElement = new XElement(XNames.property, new XAttribute(XNames.name, _propertyName));
			_element.Add(xElement);
		}
		xElement.SetAttributeValue(_attribName, _value);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void ScaleProperty(DynamicProperties _properties, string _className, string _propertyName, float _scale)
	{
		if (!_properties.Contains(_className, _propertyName))
		{
			return;
		}
		int num = int.Parse(_properties.GetString(_className, _propertyName));
		if (num > 0)
		{
			num = (int)((float)num * _scale);
			if (num < 1)
			{
				num = 1;
			}
			_properties.SetValue(_className, _propertyName, num.ToString());
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void FixImposterExchangeId(DynamicProperties _properties, string _blockBaseName, string _shapeName, int channel)
	{
		string texture = TextureLabelsByChannel[channel].Texture;
		string imposterExchange = TextureLabelsByChannel[channel].ImposterExchange;
		if (_properties.Contains(imposterExchange))
		{
			if (!_properties.Contains(texture))
			{
				Log.Warning("Blocks: Shape " + _shapeName + " defines " + imposterExchange + " but block template " + _blockBaseName + " does not have a '" + texture + "' property!");
			}
			else
			{
				int iD = BlockTextureData.GetDataByTextureID(int.Parse(_properties.GetString(texture))).ID;
				_properties.SetParam1(imposterExchange, iD.ToString());
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void FixTextureId(DynamicProperties _properties, string _blockBaseName, string _shapeName, int channel)
	{
		string texture = TextureLabelsByChannel[channel].Texture;
		string shapeAltTexture = TextureLabelsByChannel[channel].ShapeAltTexture;
		if (!_properties.Contains(shapeAltTexture))
		{
			return;
		}
		if (!_properties.Contains(texture))
		{
			Log.Warning("Blocks: Shape " + _shapeName + " defines " + shapeAltTexture + " but block template " + _blockBaseName + " does not have a '" + texture + "' property!");
			return;
		}
		string[] array = _properties.GetString(shapeAltTexture).Split(',');
		string text = _properties.GetString(texture);
		for (int i = 0; i < array.Length; i++)
		{
			if (!int.TryParse(array[i], out var _))
			{
				array[i] = text;
			}
		}
		string value = string.Join(",", array);
		_properties.SetValue(texture, value);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static string AppendShapeName(string _innerText, string _blockBaseName, string _shapeName)
	{
		if (_innerText[0] == ':')
		{
			return _blockBaseName + _innerText;
		}
		if (!_innerText.Contains(":"))
		{
			return _innerText + ":" + _shapeName;
		}
		return _innerText;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static string PrependBlockBaseName(string _innerText, string _blockBaseName)
	{
		return _blockBaseName + ":" + _innerText;
	}
}
