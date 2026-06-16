using System.Xml.Linq;

namespace CoverClippingTool;

public struct PropertyValue(XElement element, string value)
{
	public readonly string Value = value;

	public readonly XElement Element = element;

	public PropertyValue Clone()
	{
		return new PropertyValue(Element, Value);
	}
}
