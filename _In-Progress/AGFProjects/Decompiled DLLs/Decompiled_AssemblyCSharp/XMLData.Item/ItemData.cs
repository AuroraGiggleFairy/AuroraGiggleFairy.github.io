using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using ICSharpCode.WpfDesign.XamlDom;
using UnityEngine.Scripting;
using XMLData.Exceptions;
using XMLData.Parsers;

namespace XMLData.Item;

[Preserve]
public class ItemData : IXMLParserBase, IXMLData
{
	public class DataItemArrayRepairTools
	{
		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<string>[] pRepairTools;

		public DataItem<string> this[int index]
		{
			get
			{
				if (index >= pRepairTools.Length)
				{
					throw new ArgumentOutOfRangeException("index", "index " + index + " greater/equal than array length " + pRepairTools.Length);
				}
				return pRepairTools[index];
			}
			set
			{
				if (index >= pRepairTools.Length)
				{
					throw new ArgumentOutOfRangeException("index", "index " + index + " greater/equal than array length " + pRepairTools.Length);
				}
				pRepairTools[index] = value;
			}
		}

		public int Length => pRepairTools.Length;

		public DataItemArrayRepairTools(int _size)
		{
			pRepairTools = new DataItem<string>[_size];
		}

		public DataItemArrayRepairTools(DataItem<string>[] _init)
		{
			pRepairTools = _init;
		}
	}

	public class DataItemArrayAction
	{
		[PublicizedFrom(EAccessModifier.Private)]
		public DataItem<ItemAction>[] pAction;

		public DataItem<ItemAction> this[int index]
		{
			get
			{
				if (index >= pAction.Length)
				{
					throw new ArgumentOutOfRangeException("index", "index " + index + " greater/equal than array length " + pAction.Length);
				}
				return pAction[index];
			}
			set
			{
				if (index >= pAction.Length)
				{
					throw new ArgumentOutOfRangeException("index", "index " + index + " greater/equal than array length " + pAction.Length);
				}
				pAction[index] = value;
			}
		}

		public int Length => pAction.Length;

		public DataItemArrayAction(int _size)
		{
			pAction = new DataItem<ItemAction>[_size];
		}

		public DataItemArrayAction(DataItem<ItemAction>[] _init)
		{
			pAction = _init;
		}
	}

	public static class Parser
	{
		public static int idOffset = 0;

		[PublicizedFrom(EAccessModifier.Private)]
		public static Dictionary<string, Range<int>> knownAttributesMultiplicity = new Dictionary<string, Range<int>>
		{
			{
				"Active",
				new Range<int>(_hasMin: true, 0, _hasMax: true, 1)
			},
			{
				"ImageEffectOnActive",
				new Range<int>(_hasMin: true, 0, _hasMax: true, 1)
			},
			{
				"Meshfile",
				new Range<int>(_hasMin: true, 0, _hasMax: true, 1)
			},
			{
				"Material",
				new Range<int>(_hasMin: true, 1, _hasMax: true, 1)
			},
			{
				"HoldType",
				new Range<int>(_hasMin: true, 0, _hasMax: true, 1)
			},
			{
				"Canhold",
				new Range<int>(_hasMin: true, 0, _hasMax: true, 1)
			},
			{
				"Stacknumber",
				new Range<int>(_hasMin: true, 1, _hasMax: true, 1)
			},
			{
				"Degradation",
				new Range<int>(_hasMin: true, 1, _hasMax: true, 1)
			},
			{
				"DegradationBreaksAfter",
				new Range<int>(_hasMin: true, 1, _hasMax: true, 1)
			},
			{
				"FuelValue",
				new Range<int>(_hasMin: true, 0, _hasMax: true, 1)
			},
			{
				"CritChance",
				new Range<int>(_hasMin: true, 1, _hasMax: true, 1)
			},
			{
				"Group",
				new Range<int>(_hasMin: true, 1, _hasMax: true, 1)
			},
			{
				"DamageEntityMin",
				new Range<int>(_hasMin: true, 0, _hasMax: true, 1)
			},
			{
				"DamageEntityMax",
				new Range<int>(_hasMin: true, 0, _hasMax: true, 1)
			},
			{
				"Smell",
				new Range<int>(_hasMin: true, 0, _hasMax: true, 1)
			},
			{
				"DropScale",
				new Range<int>(_hasMin: true, 0, _hasMax: true, 1)
			},
			{
				"CustomIcon",
				new Range<int>(_hasMin: true, 0, _hasMax: true, 1)
			},
			{
				"CustomIconTint",
				new Range<int>(_hasMin: true, 0, _hasMax: true, 1)
			},
			{
				"PartType",
				new Range<int>(_hasMin: true, 1, _hasMax: true, 1)
			},
			{
				"Weight",
				new Range<int>(_hasMin: true, 0, _hasMax: true, 1)
			},
			{
				"Candrop",
				new Range<int>(_hasMin: true, 0, _hasMax: true, 1)
			},
			{
				"UserHidden",
				new Range<int>(_hasMin: true, 1, _hasMax: true, 1)
			},
			{
				"LightSource",
				new Range<int>(_hasMin: true, 0, _hasMax: true, 1)
			},
			{
				"ThrowableDecoy",
				new Range<int>(_hasMin: true, 1, _hasMax: true, 1)
			},
			{
				"FuseTime",
				new Range<int>(_hasMin: true, 0, _hasMax: true, 1)
			},
			{
				"MoldTarget",
				new Range<int>(_hasMin: true, 0, _hasMax: true, 1)
			},
			{
				"EquipSlot",
				new Range<int>(_hasMin: true, 1, _hasMax: true, 1)
			},
			{
				"ActivateObject",
				new Range<int>(_hasMin: true, 0, _hasMax: true, 1)
			},
			{
				"RepairTools",
				new Range<int>(_hasMin: true, 0, _hasMax: false, 0)
			},
			{
				"RepairTime",
				new Range<int>(_hasMin: true, 0, _hasMax: true, 1)
			},
			{
				"RepairAmount",
				new Range<int>(_hasMin: true, 0, _hasMax: true, 1)
			},
			{
				"SoundDestroy",
				new Range<int>(_hasMin: true, 0, _hasMax: true, 1)
			},
			{
				"SoundIdle",
				new Range<int>(_hasMin: true, 0, _hasMax: true, 1)
			},
			{
				"SoundJammed",
				new Range<int>(_hasMin: true, 0, _hasMax: true, 1)
			},
			{
				"PartTypes",
				new Range<int>(_hasMin: true, 0, _hasMax: true, 1)
			},
			{
				"Action",
				new Range<int>(_hasMin: true, 0, _hasMax: true, 5)
			},
			{
				"Armor",
				new Range<int>(_hasMin: true, 0, _hasMax: true, 1)
			},
			{
				"Preview",
				new Range<int>(_hasMin: true, 0, _hasMax: true, 1)
			},
			{
				"Attributes",
				new Range<int>(_hasMin: true, 0, _hasMax: true, 1)
			},
			{
				"Explosion",
				new Range<int>(_hasMin: true, 0, _hasMax: true, 1)
			},
			{
				"UMA",
				new Range<int>(_hasMin: true, 0, _hasMax: true, 1)
			}
		};

		public static ItemClass Parse(PositionXmlElement _elem, Dictionary<PositionXmlElement, DataItem<ItemClass>> _updateLater)
		{
			string pName = ParserUtils.ParseStringAttribute(_elem, "name", _mandatory: true);
			int pId = ParserUtils.ParseIntAttribute(_elem, "id", _mandatory: true) + idOffset;
			string text = (_elem.HasAttribute("class") ? _elem.GetAttribute("class") : "ItemClass");
			Type type = Type.GetType(typeof(Parser).Namespace + "." + text);
			if (type == null)
			{
				type = Type.GetType(text);
				if (type == null)
				{
					throw new InvalidValueException("Specified class \"" + text + "\" not found", _elem.LineNumber);
				}
			}
			ItemClass itemClass = (ItemClass)Activator.CreateInstance(type);
			Dictionary<string, int> dictionary = new Dictionary<string, int>();
			itemClass.pName = pName;
			itemClass.pId = pId;
			List<DataItem<string>> list = new List<DataItem<string>>();
			List<DataItem<ItemAction>> list2 = new List<DataItem<ItemAction>>();
			foreach (XmlNode childNode in _elem.ChildNodes)
			{
				switch (childNode.NodeType)
				{
				case XmlNodeType.Element:
				{
					PositionXmlElement positionXmlElement = (PositionXmlElement)childNode;
					if (knownAttributesMultiplicity.ContainsKey(positionXmlElement.Name))
					{
						switch (positionXmlElement.Name)
						{
						case "Active":
						{
							bool startValue10;
							try
							{
								startValue10 = boolParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true));
							}
							catch (Exception innerException4)
							{
								throw new InvalidValueException("Could not parse attribute \"" + positionXmlElement.Name + "\" value \"" + ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true) + "\"", positionXmlElement.LineNumber, innerException4);
							}
							DataItem<bool> active = new DataItem<bool>("Active", startValue10);
							itemClass.Active = active;
							break;
						}
						case "ImageEffectOnActive":
						{
							string startValue26;
							try
							{
								startValue26 = stringParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true));
							}
							catch (Exception innerException20)
							{
								throw new InvalidValueException("Could not parse attribute \"" + positionXmlElement.Name + "\" value \"" + ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true) + "\"", positionXmlElement.LineNumber, innerException20);
							}
							DataItem<string> pMeshfile = new DataItem<string>("ImageEffectOnActive", startValue26);
							itemClass.pMeshfile = pMeshfile;
							break;
						}
						case "Meshfile":
						{
							string startValue34;
							try
							{
								startValue34 = stringParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true));
							}
							catch (Exception innerException28)
							{
								throw new InvalidValueException("Could not parse attribute \"" + positionXmlElement.Name + "\" value \"" + ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true) + "\"", positionXmlElement.LineNumber, innerException28);
							}
							DataItem<string> pMeshfile2 = new DataItem<string>("Meshfile", startValue34);
							itemClass.pMeshfile = pMeshfile2;
							break;
						}
						case "Material":
						{
							string startValue18;
							try
							{
								startValue18 = stringParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true));
							}
							catch (Exception innerException12)
							{
								throw new InvalidValueException("Could not parse attribute \"" + positionXmlElement.Name + "\" value \"" + ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true) + "\"", positionXmlElement.LineNumber, innerException12);
							}
							DataItem<string> pMaterial = new DataItem<string>("Material", startValue18);
							itemClass.pMaterial = pMaterial;
							break;
						}
						case "HoldType":
						{
							int startValue38;
							try
							{
								startValue38 = intParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true));
							}
							catch (Exception innerException32)
							{
								throw new InvalidValueException("Could not parse attribute \"" + positionXmlElement.Name + "\" value \"" + ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true) + "\"", positionXmlElement.LineNumber, innerException32);
							}
							DataItem<int> pHoldType = new DataItem<int>("HoldType", startValue38);
							itemClass.pHoldType = pHoldType;
							break;
						}
						case "Canhold":
						{
							bool startValue30;
							try
							{
								startValue30 = boolParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true));
							}
							catch (Exception innerException24)
							{
								throw new InvalidValueException("Could not parse attribute \"" + positionXmlElement.Name + "\" value \"" + ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true) + "\"", positionXmlElement.LineNumber, innerException24);
							}
							DataItem<bool> pCanhold = new DataItem<bool>("Canhold", startValue30);
							itemClass.pCanhold = pCanhold;
							break;
						}
						case "Stacknumber":
						{
							int startValue22;
							try
							{
								startValue22 = intParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true));
							}
							catch (Exception innerException16)
							{
								throw new InvalidValueException("Could not parse attribute \"" + positionXmlElement.Name + "\" value \"" + ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true) + "\"", positionXmlElement.LineNumber, innerException16);
							}
							DataItem<int> pStacknumber = new DataItem<int>("Stacknumber", startValue22);
							itemClass.pStacknumber = pStacknumber;
							break;
						}
						case "Degradation":
						{
							int startValue14;
							try
							{
								startValue14 = intParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true));
							}
							catch (Exception innerException8)
							{
								throw new InvalidValueException("Could not parse attribute \"" + positionXmlElement.Name + "\" value \"" + ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true) + "\"", positionXmlElement.LineNumber, innerException8);
							}
							DataItem<int> pDegradation = new DataItem<int>("Degradation", startValue14);
							itemClass.pDegradation = pDegradation;
							break;
						}
						case "DegradationBreaksAfter":
						{
							bool startValue40;
							try
							{
								startValue40 = boolParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true));
							}
							catch (Exception innerException34)
							{
								throw new InvalidValueException("Could not parse attribute \"" + positionXmlElement.Name + "\" value \"" + ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true) + "\"", positionXmlElement.LineNumber, innerException34);
							}
							DataItem<bool> pDegradationBreaksAfter = new DataItem<bool>("DegradationBreaksAfter", startValue40);
							itemClass.pDegradationBreaksAfter = pDegradationBreaksAfter;
							break;
						}
						case "FuelValue":
						{
							int startValue36;
							try
							{
								startValue36 = intParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true));
							}
							catch (Exception innerException30)
							{
								throw new InvalidValueException("Could not parse attribute \"" + positionXmlElement.Name + "\" value \"" + ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true) + "\"", positionXmlElement.LineNumber, innerException30);
							}
							DataItem<int> pFuelValue = new DataItem<int>("FuelValue", startValue36);
							itemClass.pFuelValue = pFuelValue;
							break;
						}
						case "CritChance":
						{
							float startValue32;
							try
							{
								startValue32 = floatParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true));
							}
							catch (Exception innerException26)
							{
								throw new InvalidValueException("Could not parse attribute \"" + positionXmlElement.Name + "\" value \"" + ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true) + "\"", positionXmlElement.LineNumber, innerException26);
							}
							DataItem<float> pCritChance = new DataItem<float>("CritChance", startValue32);
							itemClass.pCritChance = pCritChance;
							break;
						}
						case "Group":
						{
							string startValue28;
							try
							{
								startValue28 = stringParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true));
							}
							catch (Exception innerException22)
							{
								throw new InvalidValueException("Could not parse attribute \"" + positionXmlElement.Name + "\" value \"" + ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true) + "\"", positionXmlElement.LineNumber, innerException22);
							}
							DataItem<string> pGroup = new DataItem<string>("Group", startValue28);
							itemClass.pGroup = pGroup;
							break;
						}
						case "DamageEntityMin":
						{
							int startValue24;
							try
							{
								startValue24 = intParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true));
							}
							catch (Exception innerException18)
							{
								throw new InvalidValueException("Could not parse attribute \"" + positionXmlElement.Name + "\" value \"" + ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true) + "\"", positionXmlElement.LineNumber, innerException18);
							}
							DataItem<int> pDamageEntityMin = new DataItem<int>("DamageEntityMin", startValue24);
							itemClass.pDamageEntityMin = pDamageEntityMin;
							break;
						}
						case "DamageEntityMax":
						{
							int startValue20;
							try
							{
								startValue20 = intParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true));
							}
							catch (Exception innerException14)
							{
								throw new InvalidValueException("Could not parse attribute \"" + positionXmlElement.Name + "\" value \"" + ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true) + "\"", positionXmlElement.LineNumber, innerException14);
							}
							DataItem<int> pDamageEntityMax = new DataItem<int>("DamageEntityMax", startValue20);
							itemClass.pDamageEntityMax = pDamageEntityMax;
							break;
						}
						case "Smell":
						{
							string startValue16;
							try
							{
								startValue16 = stringParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true));
							}
							catch (Exception innerException10)
							{
								throw new InvalidValueException("Could not parse attribute \"" + positionXmlElement.Name + "\" value \"" + ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true) + "\"", positionXmlElement.LineNumber, innerException10);
							}
							DataItem<string> pSmell = new DataItem<string>("Smell", startValue16);
							itemClass.pSmell = pSmell;
							break;
						}
						case "DropScale":
						{
							int startValue12;
							try
							{
								startValue12 = intParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true));
							}
							catch (Exception innerException6)
							{
								throw new InvalidValueException("Could not parse attribute \"" + positionXmlElement.Name + "\" value \"" + ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true) + "\"", positionXmlElement.LineNumber, innerException6);
							}
							DataItem<int> pDropScale = new DataItem<int>("DropScale", startValue12);
							itemClass.pDropScale = pDropScale;
							break;
						}
						case "CustomIcon":
						{
							string startValue8;
							try
							{
								startValue8 = stringParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true));
							}
							catch (Exception innerException2)
							{
								throw new InvalidValueException("Could not parse attribute \"" + positionXmlElement.Name + "\" value \"" + ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true) + "\"", positionXmlElement.LineNumber, innerException2);
							}
							DataItem<string> pCustomIcon = new DataItem<string>("CustomIcon", startValue8);
							itemClass.pCustomIcon = pCustomIcon;
							break;
						}
						case "CustomIconTint":
						{
							string startValue39;
							try
							{
								startValue39 = stringParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true));
							}
							catch (Exception innerException33)
							{
								throw new InvalidValueException("Could not parse attribute \"" + positionXmlElement.Name + "\" value \"" + ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true) + "\"", positionXmlElement.LineNumber, innerException33);
							}
							DataItem<string> pCustomIconTint = new DataItem<string>("CustomIconTint", startValue39);
							itemClass.pCustomIconTint = pCustomIconTint;
							break;
						}
						case "PartType":
						{
							EPartType startValue37;
							try
							{
								startValue37 = EnumParser.Parse<EPartType>(ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true));
							}
							catch (Exception innerException31)
							{
								throw new InvalidValueException("Could not parse attribute \"" + positionXmlElement.Name + "\" value \"" + ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true) + "\"", positionXmlElement.LineNumber, innerException31);
							}
							DataItem<EPartType> pPartType = new DataItem<EPartType>("PartType", startValue37);
							itemClass.pPartType = pPartType;
							break;
						}
						case "Weight":
						{
							int startValue35;
							try
							{
								startValue35 = intParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true));
							}
							catch (Exception innerException29)
							{
								throw new InvalidValueException("Could not parse attribute \"" + positionXmlElement.Name + "\" value \"" + ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true) + "\"", positionXmlElement.LineNumber, innerException29);
							}
							DataItem<int> pWeight = new DataItem<int>("Weight", startValue35);
							itemClass.pWeight = pWeight;
							break;
						}
						case "Candrop":
						{
							bool startValue33;
							try
							{
								startValue33 = boolParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true));
							}
							catch (Exception innerException27)
							{
								throw new InvalidValueException("Could not parse attribute \"" + positionXmlElement.Name + "\" value \"" + ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true) + "\"", positionXmlElement.LineNumber, innerException27);
							}
							DataItem<bool> pCandrop = new DataItem<bool>("Candrop", startValue33);
							itemClass.pCandrop = pCandrop;
							break;
						}
						case "UserHidden":
						{
							bool startValue31;
							try
							{
								startValue31 = boolParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true));
							}
							catch (Exception innerException25)
							{
								throw new InvalidValueException("Could not parse attribute \"" + positionXmlElement.Name + "\" value \"" + ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true) + "\"", positionXmlElement.LineNumber, innerException25);
							}
							DataItem<bool> pUserHidden = new DataItem<bool>("UserHidden", startValue31);
							itemClass.pUserHidden = pUserHidden;
							break;
						}
						case "LightSource":
						{
							string startValue29;
							try
							{
								startValue29 = stringParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true));
							}
							catch (Exception innerException23)
							{
								throw new InvalidValueException("Could not parse attribute \"" + positionXmlElement.Name + "\" value \"" + ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true) + "\"", positionXmlElement.LineNumber, innerException23);
							}
							DataItem<string> pLightSource = new DataItem<string>("LightSource", startValue29);
							itemClass.pLightSource = pLightSource;
							break;
						}
						case "ThrowableDecoy":
						{
							bool startValue27;
							try
							{
								startValue27 = boolParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true));
							}
							catch (Exception innerException21)
							{
								throw new InvalidValueException("Could not parse attribute \"" + positionXmlElement.Name + "\" value \"" + ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true) + "\"", positionXmlElement.LineNumber, innerException21);
							}
							DataItem<bool> pThrowableDecoy = new DataItem<bool>("ThrowableDecoy", startValue27);
							itemClass.pThrowableDecoy = pThrowableDecoy;
							break;
						}
						case "FuseTime":
						{
							float startValue25;
							try
							{
								startValue25 = floatParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true));
							}
							catch (Exception innerException19)
							{
								throw new InvalidValueException("Could not parse attribute \"" + positionXmlElement.Name + "\" value \"" + ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true) + "\"", positionXmlElement.LineNumber, innerException19);
							}
							DataItem<float> pFuseTime = new DataItem<float>("FuseTime", startValue25);
							itemClass.pFuseTime = pFuseTime;
							break;
						}
						case "MoldTarget":
						{
							ItemClass startValue23;
							try
							{
								startValue23 = null;
							}
							catch (Exception innerException17)
							{
								throw new InvalidValueException("Could not parse attribute \"" + positionXmlElement.Name + "\" value \"" + ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true) + "\"", positionXmlElement.LineNumber, innerException17);
							}
							DataItem<ItemClass> dataItem = new DataItem<ItemClass>("MoldTarget", startValue23);
							_updateLater.Add(positionXmlElement, dataItem);
							itemClass.pMoldTarget = dataItem;
							break;
						}
						case "EquipSlot":
						{
							EquipmentSlots startValue21;
							try
							{
								startValue21 = EnumParser.Parse<EquipmentSlots>(ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true));
							}
							catch (Exception innerException15)
							{
								throw new InvalidValueException("Could not parse attribute \"" + positionXmlElement.Name + "\" value \"" + ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true) + "\"", positionXmlElement.LineNumber, innerException15);
							}
							DataItem<EquipmentSlots> pEquipSlot = new DataItem<EquipmentSlots>("EquipSlot", startValue21);
							itemClass.pEquipSlot = pEquipSlot;
							break;
						}
						case "ActivateObject":
						{
							string startValue19;
							try
							{
								startValue19 = stringParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true));
							}
							catch (Exception innerException13)
							{
								throw new InvalidValueException("Could not parse attribute \"" + positionXmlElement.Name + "\" value \"" + ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true) + "\"", positionXmlElement.LineNumber, innerException13);
							}
							DataItem<string> pActivateObject = new DataItem<string>("ActivateObject", startValue19);
							itemClass.pActivateObject = pActivateObject;
							break;
						}
						case "RepairTools":
						{
							string startValue17;
							try
							{
								startValue17 = stringParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true));
							}
							catch (Exception innerException11)
							{
								throw new InvalidValueException("Could not parse attribute \"" + positionXmlElement.Name + "\" value \"" + ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true) + "\"", positionXmlElement.LineNumber, innerException11);
							}
							DataItem<string> item2 = new DataItem<string>("RepairTools", startValue17);
							list.Add(item2);
							break;
						}
						case "RepairTime":
						{
							float startValue15;
							try
							{
								startValue15 = floatParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true));
							}
							catch (Exception innerException9)
							{
								throw new InvalidValueException("Could not parse attribute \"" + positionXmlElement.Name + "\" value \"" + ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true) + "\"", positionXmlElement.LineNumber, innerException9);
							}
							DataItem<float> pRepairTime = new DataItem<float>("RepairTime", startValue15);
							itemClass.pRepairTime = pRepairTime;
							break;
						}
						case "RepairAmount":
						{
							int startValue13;
							try
							{
								startValue13 = intParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true));
							}
							catch (Exception innerException7)
							{
								throw new InvalidValueException("Could not parse attribute \"" + positionXmlElement.Name + "\" value \"" + ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true) + "\"", positionXmlElement.LineNumber, innerException7);
							}
							DataItem<int> pRepairAmount = new DataItem<int>("RepairAmount", startValue13);
							itemClass.pRepairAmount = pRepairAmount;
							break;
						}
						case "SoundDestroy":
						{
							string startValue11;
							try
							{
								startValue11 = stringParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true));
							}
							catch (Exception innerException5)
							{
								throw new InvalidValueException("Could not parse attribute \"" + positionXmlElement.Name + "\" value \"" + ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true) + "\"", positionXmlElement.LineNumber, innerException5);
							}
							DataItem<string> pSoundDestroy = new DataItem<string>("SoundDestroy", startValue11);
							itemClass.pSoundDestroy = pSoundDestroy;
							break;
						}
						case "SoundIdle":
						{
							string startValue9;
							try
							{
								startValue9 = stringParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true));
							}
							catch (Exception innerException3)
							{
								throw new InvalidValueException("Could not parse attribute \"" + positionXmlElement.Name + "\" value \"" + ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true) + "\"", positionXmlElement.LineNumber, innerException3);
							}
							DataItem<string> pSoundIdle = new DataItem<string>("SoundIdle", startValue9);
							itemClass.pSoundIdle = pSoundIdle;
							break;
						}
						case "SoundJammed":
						{
							string startValue7;
							try
							{
								startValue7 = stringParser.Parse(ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true));
							}
							catch (Exception innerException)
							{
								throw new InvalidValueException("Could not parse attribute \"" + positionXmlElement.Name + "\" value \"" + ParserUtils.ParseStringAttribute(positionXmlElement, "value", _mandatory: true) + "\"", positionXmlElement.LineNumber, innerException);
							}
							DataItem<string> pSoundJammed = new DataItem<string>("SoundJammed", startValue7);
							itemClass.pSoundJammed = pSoundJammed;
							break;
						}
						case "PartTypes":
						{
							PartsData startValue6 = PartsData.Parser.Parse(positionXmlElement, _updateLater);
							DataItem<PartsData> pPartTypes = new DataItem<PartsData>("PartTypes", startValue6);
							itemClass.pPartTypes = pPartTypes;
							break;
						}
						case "Action":
						{
							ItemAction startValue5 = ItemActionData.Parser.Parse(positionXmlElement, _updateLater);
							DataItem<ItemAction> item = new DataItem<ItemAction>("Action", startValue5);
							list2.Add(item);
							break;
						}
						case "Armor":
						{
							ArmorData startValue4 = ArmorData.Parser.Parse(positionXmlElement, _updateLater);
							DataItem<ArmorData> pArmor = new DataItem<ArmorData>("Armor", startValue4);
							itemClass.pArmor = pArmor;
							break;
						}
						case "Preview":
						{
							PreviewData startValue3 = PreviewData.Parser.Parse(positionXmlElement, _updateLater);
							DataItem<PreviewData> pPreview = new DataItem<PreviewData>("Preview", startValue3);
							itemClass.pPreview = pPreview;
							break;
						}
						case "Attributes":
						{
							AttributesData startValue2 = AttributesData.Parser.Parse(positionXmlElement, _updateLater);
							DataItem<AttributesData> pAttributes = new DataItem<AttributesData>("Attributes", startValue2);
							itemClass.pAttributes = pAttributes;
							break;
						}
						case "Explosion":
						{
							ExplosionData startValue = ExplosionData.Parser.Parse(positionXmlElement, _updateLater);
							DataItem<ExplosionData> pExplosion = new DataItem<ExplosionData>("Explosion", startValue);
							itemClass.pExplosion = pExplosion;
							break;
						}
						}
						if (!dictionary.ContainsKey(positionXmlElement.Name))
						{
							dictionary[positionXmlElement.Name] = 0;
						}
						dictionary[positionXmlElement.Name]++;
						break;
					}
					throw new UnexpectedElementException("Unknown element \"" + childNode.Name + "\" found while parsing Item", ((IXmlLineInfo)childNode).LineNumber);
				}
				default:
					throw new UnexpectedElementException("Unknown node \"" + childNode.NodeType.ToString() + "\" found while parsing Item", ((IXmlLineInfo)childNode).LineNumber);
				case XmlNodeType.Comment:
					break;
				}
			}
			if (!dictionary.ContainsKey("Stacknumber"))
			{
				int startValue41;
				try
				{
					startValue41 = intParser.Parse("64");
				}
				catch (Exception)
				{
					throw new InvalidValueException("Default value \"64\" for attribute \"Stacknumber\" could not be parsed", -1);
				}
				DataItem<int> pStacknumber2 = new DataItem<int>("Stacknumber", startValue41);
				itemClass.pStacknumber = pStacknumber2;
				dictionary["Stacknumber"] = 1;
			}
			if (!dictionary.ContainsKey("Degradation"))
			{
				int startValue42;
				try
				{
					startValue42 = intParser.Parse("0");
				}
				catch (Exception)
				{
					throw new InvalidValueException("Default value \"0\" for attribute \"Degradation\" could not be parsed", -1);
				}
				DataItem<int> pDegradation2 = new DataItem<int>("Degradation", startValue42);
				itemClass.pDegradation = pDegradation2;
				dictionary["Degradation"] = 1;
			}
			if (!dictionary.ContainsKey("DegradationBreaksAfter"))
			{
				bool startValue43;
				try
				{
					startValue43 = boolParser.Parse("true");
				}
				catch (Exception)
				{
					throw new InvalidValueException("Default value \"true\" for attribute \"DegradationBreaksAfter\" could not be parsed", -1);
				}
				DataItem<bool> pDegradationBreaksAfter2 = new DataItem<bool>("DegradationBreaksAfter", startValue43);
				itemClass.pDegradationBreaksAfter = pDegradationBreaksAfter2;
				dictionary["DegradationBreaksAfter"] = 1;
			}
			if (!dictionary.ContainsKey("CritChance"))
			{
				float startValue44;
				try
				{
					startValue44 = floatParser.Parse("0");
				}
				catch (Exception)
				{
					throw new InvalidValueException("Default value \"0\" for attribute \"CritChance\" could not be parsed", -1);
				}
				DataItem<float> pCritChance2 = new DataItem<float>("CritChance", startValue44);
				itemClass.pCritChance = pCritChance2;
				dictionary["CritChance"] = 1;
			}
			if (!dictionary.ContainsKey("Group"))
			{
				string startValue45;
				try
				{
					startValue45 = stringParser.Parse("Miscellaneous");
				}
				catch (Exception)
				{
					throw new InvalidValueException("Default value \"Miscellaneous\" for attribute \"Group\" could not be parsed", -1);
				}
				DataItem<string> pGroup2 = new DataItem<string>("Group", startValue45);
				itemClass.pGroup = pGroup2;
				dictionary["Group"] = 1;
			}
			if (!dictionary.ContainsKey("PartType"))
			{
				EPartType startValue46;
				try
				{
					startValue46 = EnumParser.Parse<EPartType>("None");
				}
				catch (Exception)
				{
					throw new InvalidValueException("Default value \"None\" for attribute \"PartType\" could not be parsed", -1);
				}
				DataItem<EPartType> pPartType2 = new DataItem<EPartType>("PartType", startValue46);
				itemClass.pPartType = pPartType2;
				dictionary["PartType"] = 1;
			}
			if (!dictionary.ContainsKey("UserHidden"))
			{
				bool startValue47;
				try
				{
					startValue47 = boolParser.Parse("false");
				}
				catch (Exception)
				{
					throw new InvalidValueException("Default value \"false\" for attribute \"UserHidden\" could not be parsed", -1);
				}
				DataItem<bool> pUserHidden2 = new DataItem<bool>("UserHidden", startValue47);
				itemClass.pUserHidden = pUserHidden2;
				dictionary["UserHidden"] = 1;
			}
			if (!dictionary.ContainsKey("ThrowableDecoy"))
			{
				bool startValue48;
				try
				{
					startValue48 = boolParser.Parse("false");
				}
				catch (Exception)
				{
					throw new InvalidValueException("Default value \"false\" for attribute \"ThrowableDecoy\" could not be parsed", -1);
				}
				DataItem<bool> pThrowableDecoy2 = new DataItem<bool>("ThrowableDecoy", startValue48);
				itemClass.pThrowableDecoy = pThrowableDecoy2;
				dictionary["ThrowableDecoy"] = 1;
			}
			if (!dictionary.ContainsKey("EquipSlot"))
			{
				EquipmentSlots startValue49;
				try
				{
					startValue49 = EnumParser.Parse<EquipmentSlots>("None");
				}
				catch (Exception)
				{
					throw new InvalidValueException("Default value \"None\" for attribute \"EquipSlot\" could not be parsed", -1);
				}
				DataItem<EquipmentSlots> pEquipSlot2 = new DataItem<EquipmentSlots>("EquipSlot", startValue49);
				itemClass.pEquipSlot = pEquipSlot2;
				dictionary["EquipSlot"] = 1;
			}
			foreach (KeyValuePair<string, Range<int>> item3 in knownAttributesMultiplicity)
			{
				int num = (dictionary.ContainsKey(item3.Key) ? dictionary[item3.Key] : 0);
				if ((item3.Value.hasMin && num < item3.Value.min) || (item3.Value.hasMax && num > item3.Value.max))
				{
					throw new IncorrectAttributeOccurrenceException("Element has incorrect number of \"" + item3.Key + "\" attribute instances, found " + num + ", expected " + item3.Value.ToString(), _elem.LineNumber);
				}
			}
			itemClass.pRepairTools = new DataItemArrayRepairTools(list.ToArray());
			itemClass.pAction = new DataItemArrayAction(list2.ToArray());
			return itemClass;
		}

		public static List<ItemClass> ParseXml(string _filename, string _content, bool _clearFirst = true, bool _validateOnly = false)
		{
			PositionXmlDocument positionXmlDocument = new PositionXmlDocument();
			if (_clearFirst && !_validateOnly)
			{
				Clear();
			}
			try
			{
				using Stream input = new MemoryStream(Encoding.UTF8.GetBytes(_content ?? ""));
				positionXmlDocument.Load(XmlReader.Create(input));
			}
			catch (XmlException e)
			{
				Log.Error("Failed parsing " + _filename + ":");
				Log.Exception(e);
				return null;
			}
			XmlElement documentElement = positionXmlDocument.DocumentElement;
			List<ItemClass> list = new List<ItemClass>();
			Dictionary<PositionXmlElement, DataItem<ItemClass>> updateLater = new Dictionary<PositionXmlElement, DataItem<ItemClass>>();
			foreach (XmlNode childNode in documentElement.ChildNodes)
			{
				switch (childNode.NodeType)
				{
				case XmlNodeType.Element:
					if (childNode.Name == "Item")
					{
						ItemClass itemClass = Parse((PositionXmlElement)childNode, updateLater);
						if (itemClass != null)
						{
							list.Add(itemClass);
						}
					}
					else
					{
						Log.Error($"Unknown element found: {childNode.Name} (file {_filename}, line {((IXmlLineInfo)childNode).LineNumber})");
					}
					break;
				default:
					Log.Error("Unexpected XML node: " + childNode.NodeType.ToString() + " at line " + ((IXmlLineInfo)childNode).LineNumber);
					break;
				case XmlNodeType.Comment:
					break;
				}
			}
			if (!_validateOnly)
			{
				FillLists(_filename, list, _clearFirst);
				UpdateXmlRefs(_filename, updateLater);
			}
			return list;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public static void Clear()
		{
			pElementMap.Clear();
			pElementIndexed = null;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public static void FillLists(string _filename, List<ItemClass> _entries, bool _clearFirst)
		{
			int num = -1;
			HashSet<int> hashSet = new HashSet<int>();
			foreach (ItemClass _entry in _entries)
			{
				if (_entry.Id > num)
				{
					num = _entry.Id;
				}
				if (hashSet.Contains(_entry.Id))
				{
					Log.Error($"Duplicate index: {_entry.Id} in {_filename}");
				}
				hashSet.Add(_entry.Id);
			}
			if (!_clearFirst && num >= pElementIndexed.Length)
			{
				ItemClass[] array = new ItemClass[num + 1];
				Array.Copy(pElementIndexed, array, pElementIndexed.Length);
				pElementIndexed = array;
			}
			else if (_clearFirst)
			{
				pElementIndexed = new ItemClass[num + 1];
			}
			foreach (ItemClass _entry2 in _entries)
			{
				if (pElementIndexed[_entry2.Id] != null)
				{
					Log.Warning($"Overwriting existing element index: {_entry2.Id} in {_filename}");
				}
				pElementIndexed[_entry2.Id] = _entry2;
				if (pElementMap.ContainsKey(_entry2.Name))
				{
					Log.Warning($"Overwriting existing element name: {_entry2.Name} in {_filename}");
				}
				pElementMap[_entry2.Name] = _entry2;
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public static void UpdateXmlRefs(string _filename, Dictionary<PositionXmlElement, DataItem<ItemClass>> _updateLater)
		{
			foreach (KeyValuePair<PositionXmlElement, DataItem<ItemClass>> item in _updateLater)
			{
				string text = ParserUtils.ParseStringAttribute(item.Key, "value", _mandatory: true);
				if (!pElementMap.ContainsKey(text))
				{
					throw new InvalidValueException("Element with name \"" + text + "\" for attribute \"" + item.Value.Name + "\" not found (referencing an XML entry by name which is not defined)", item.Key.LineNumber);
				}
				item.Value.Value = pElementMap[text];
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static Dictionary<string, ItemClass> pElementMap = new Dictionary<string, ItemClass>();

	[PublicizedFrom(EAccessModifier.Private)]
	public static ItemClass[] pElementIndexed;

	[PublicizedFrom(EAccessModifier.Protected)]
	public string pName;

	[PublicizedFrom(EAccessModifier.Protected)]
	public int pId;

	[PublicizedFrom(EAccessModifier.Private)]
	public DataItem<bool> pAlwaysActive;

	[PublicizedFrom(EAccessModifier.Private)]
	public DataItem<bool> pActive;

	[PublicizedFrom(EAccessModifier.Private)]
	public DataItem<string> pPlaySoundOnActive;

	[PublicizedFrom(EAccessModifier.Private)]
	public DataItem<string> pImageEffectOnActive;

	[PublicizedFrom(EAccessModifier.Private)]
	public DataItem<string> pMeshfile;

	[PublicizedFrom(EAccessModifier.Private)]
	public DataItem<string> pMaterial;

	[PublicizedFrom(EAccessModifier.Private)]
	public DataItem<int> pHoldType;

	[PublicizedFrom(EAccessModifier.Private)]
	public DataItem<bool> pCanhold;

	[PublicizedFrom(EAccessModifier.Private)]
	public DataItem<int> pStacknumber;

	[PublicizedFrom(EAccessModifier.Private)]
	public DataItem<int> pDegradation;

	[PublicizedFrom(EAccessModifier.Private)]
	public DataItem<bool> pDegradationBreaksAfter;

	[PublicizedFrom(EAccessModifier.Private)]
	public DataItem<int> pFuelValue;

	[PublicizedFrom(EAccessModifier.Private)]
	public DataItem<float> pCritChance;

	[PublicizedFrom(EAccessModifier.Private)]
	public DataItem<string> pGroup;

	[PublicizedFrom(EAccessModifier.Private)]
	public DataItem<int> pDamageEntityMin;

	[PublicizedFrom(EAccessModifier.Private)]
	public DataItem<int> pDamageEntityMax;

	[PublicizedFrom(EAccessModifier.Private)]
	public DataItem<string> pSmell;

	[PublicizedFrom(EAccessModifier.Private)]
	public DataItem<int> pDropScale;

	[PublicizedFrom(EAccessModifier.Private)]
	public DataItem<string> pCustomIcon;

	[PublicizedFrom(EAccessModifier.Private)]
	public DataItem<string> pCustomIconTint;

	[PublicizedFrom(EAccessModifier.Private)]
	public DataItem<EPartType> pPartType;

	[PublicizedFrom(EAccessModifier.Private)]
	public DataItem<int> pWeight;

	[PublicizedFrom(EAccessModifier.Private)]
	public DataItem<bool> pCandrop;

	[PublicizedFrom(EAccessModifier.Private)]
	public DataItem<bool> pUserHidden;

	[PublicizedFrom(EAccessModifier.Private)]
	public DataItem<string> pLightSource;

	[PublicizedFrom(EAccessModifier.Private)]
	public DataItem<bool> pThrowableDecoy;

	[PublicizedFrom(EAccessModifier.Private)]
	public DataItem<float> pFuseTime;

	[PublicizedFrom(EAccessModifier.Private)]
	public DataItem<ItemClass> pMoldTarget;

	[PublicizedFrom(EAccessModifier.Private)]
	public DataItem<EquipmentSlots> pEquipSlot;

	[PublicizedFrom(EAccessModifier.Private)]
	public DataItem<string> pActivateObject;

	[PublicizedFrom(EAccessModifier.Private)]
	public DataItemArrayRepairTools pRepairTools;

	[PublicizedFrom(EAccessModifier.Private)]
	public DataItem<float> pRepairTime;

	[PublicizedFrom(EAccessModifier.Private)]
	public DataItem<int> pRepairAmount;

	[PublicizedFrom(EAccessModifier.Private)]
	public DataItem<string> pSoundDestroy;

	[PublicizedFrom(EAccessModifier.Private)]
	public DataItem<string> pSoundIdle;

	[PublicizedFrom(EAccessModifier.Private)]
	public DataItem<string> pSoundJammed;

	[PublicizedFrom(EAccessModifier.Private)]
	public DataItem<PartsData> pPartTypes;

	[PublicizedFrom(EAccessModifier.Private)]
	public DataItemArrayAction pAction;

	[PublicizedFrom(EAccessModifier.Private)]
	public DataItem<ArmorData> pArmor;

	[PublicizedFrom(EAccessModifier.Private)]
	public DataItem<PreviewData> pPreview;

	[PublicizedFrom(EAccessModifier.Private)]
	public DataItem<AttributesData> pAttributes;

	[PublicizedFrom(EAccessModifier.Private)]
	public DataItem<ExplosionData> pExplosion;

	[PublicizedFrom(EAccessModifier.Private)]
	public DataItem<UMAData> pUMA;

	public string Name => pName;

	public int Id => pId;

	public DataItem<bool> Active
	{
		get
		{
			return pActive;
		}
		set
		{
			pActive = value;
		}
	}

	public DataItem<bool> AlwaysActive
	{
		get
		{
			return pAlwaysActive;
		}
		set
		{
			pAlwaysActive = value;
		}
	}

	public DataItem<int> FuelValue
	{
		get
		{
			return pFuelValue;
		}
		set
		{
			pFuelValue = value;
		}
	}

	public DataItem<string> ImageEffectOnActive
	{
		get
		{
			return pImageEffectOnActive;
		}
		set
		{
			pImageEffectOnActive = value;
		}
	}

	public DataItem<PartsData> PartTypes
	{
		get
		{
			return pPartTypes;
		}
		set
		{
			pPartTypes = value;
		}
	}

	public DataItem<string> PlaySoundOnActive
	{
		get
		{
			return pPlaySoundOnActive;
		}
		set
		{
			pPlaySoundOnActive = value;
		}
	}

	public DataItem<int> Weight
	{
		get
		{
			return pWeight;
		}
		set
		{
			pWeight = value;
		}
	}

	public List<IDataItem> GetDisplayValues(bool _recursive = true)
	{
		List<IDataItem> list = new List<IDataItem>();
		if (_recursive && pPartTypes != null)
		{
			list.AddRange(pPartTypes.Value.GetDisplayValues());
		}
		for (int i = 0; i < pAction.Length; i++)
		{
			if (_recursive && pAction[i] != null)
			{
				list.AddRange(pAction[i].Value.GetDisplayValues());
			}
		}
		if (_recursive && pArmor != null)
		{
			list.AddRange(pArmor.Value.GetDisplayValues());
		}
		if (_recursive && pPreview != null)
		{
			list.AddRange(pPreview.Value.GetDisplayValues());
		}
		if (_recursive && pAttributes != null)
		{
			list.AddRange(pAttributes.Value.GetDisplayValues());
		}
		if (_recursive && pExplosion != null)
		{
			list.AddRange(pExplosion.Value.GetDisplayValues());
		}
		if (_recursive && pUMA != null)
		{
			list.AddRange(pUMA.Value.GetDisplayValues());
		}
		return list;
	}
}
