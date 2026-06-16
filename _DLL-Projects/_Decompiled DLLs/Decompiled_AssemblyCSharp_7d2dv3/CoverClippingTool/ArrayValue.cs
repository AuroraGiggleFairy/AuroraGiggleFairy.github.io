using System.Collections.Generic;
using System.Xml.Linq;

namespace CoverClippingTool;

public class ArrayValue
{
	public readonly List<Dictionary<string, PropertyValue>> Items;

	public readonly XElement Element;

	public ArrayValue(XElement element, List<Dictionary<string, PropertyValue>> items)
	{
		Element = element;
		Items = items;
	}

	public ArrayValue Clone()
	{
		ArrayValue arrayValue = new ArrayValue(Element, new List<Dictionary<string, PropertyValue>>());
		foreach (Dictionary<string, PropertyValue> item in Items)
		{
			Dictionary<string, PropertyValue> dictionary = new Dictionary<string, PropertyValue>();
			foreach (KeyValuePair<string, PropertyValue> item2 in item)
			{
				dictionary[item2.Key] = item2.Value.Clone();
			}
			arrayValue.Items.Add(dictionary);
		}
		return arrayValue;
	}
}
