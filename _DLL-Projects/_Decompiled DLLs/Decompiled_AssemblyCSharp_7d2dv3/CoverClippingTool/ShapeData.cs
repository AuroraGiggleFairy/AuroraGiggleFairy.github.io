using System;
using System.Collections.Generic;

namespace CoverClippingTool;

public class ShapeData
{
	public readonly string Name;

	public ShapeData Extends;

	public Dictionary<string, PropertyValue> Properties = new Dictionary<string, PropertyValue>(StringComparer.OrdinalIgnoreCase);

	public Dictionary<string, ArrayValue> Arrays = new Dictionary<string, ArrayValue>(StringComparer.OrdinalIgnoreCase);

	public ShapeData(string name)
	{
		Name = name;
	}

	public ShapeData Clone(string name)
	{
		ShapeData shapeData = new ShapeData(name);
		foreach (KeyValuePair<string, PropertyValue> property in Properties)
		{
			shapeData.Properties[property.Key] = property.Value.Clone();
		}
		foreach (KeyValuePair<string, ArrayValue> array in Arrays)
		{
			shapeData.Arrays[array.Key] = array.Value.Clone();
		}
		return shapeData;
	}
}
