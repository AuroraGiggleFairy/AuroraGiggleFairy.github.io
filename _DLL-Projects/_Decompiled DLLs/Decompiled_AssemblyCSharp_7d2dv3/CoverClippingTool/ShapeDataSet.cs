using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace CoverClippingTool;

public class ShapeDataSet
{
	public readonly DataSource Source;

	public readonly Dictionary<string, XElement> Data = new Dictionary<string, XElement>(StringComparer.OrdinalIgnoreCase);

	public ShapeDataSet(DataSource source)
	{
		Source = source;
	}
}
