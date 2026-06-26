using System.Xml;
using System.Xml.Linq;
using UnityEngine;

public struct UVRectTiling
{
	public static UVRectTiling Empty;

	public Rect uv;

	public int blockW;

	public int blockH;

	public bool bSwitchUV;

	public bool bGlobalUV;

	public Color color;

	public int index;

	public MaterialBlock material;

	public string textureName;

	public override string ToString()
	{
		return "x=\"" + uv.x.ToCultureInvariantString() + "\" y=\"" + uv.y.ToCultureInvariantString() + "\" w=\"" + uv.width.ToCultureInvariantString() + "\" h=\"" + uv.height.ToCultureInvariantString() + "\" blockw=\"" + blockW + "\" blockh=\"" + blockH + "\" color=\"" + color.r.ToCultureInvariantString() + "," + color.g.ToCultureInvariantString() + "," + color.b.ToCultureInvariantString() + "\" globaluv=\"" + bGlobalUV + "\" index=\"" + index + "\"";
	}

	public void ToXML(XmlElement _elem)
	{
		_elem.SetAttrib("x", uv.x.ToCultureInvariantString());
		_elem.SetAttrib("y", uv.y.ToCultureInvariantString());
		_elem.SetAttrib("w", uv.width.ToCultureInvariantString());
		_elem.SetAttrib("h", uv.height.ToCultureInvariantString());
		_elem.SetAttrib("blockw", blockW.ToString());
		_elem.SetAttrib("blockh", blockH.ToString());
		_elem.SetAttrib("color", color.r.ToCultureInvariantString() + "," + color.g.ToCultureInvariantString() + "," + color.b.ToCultureInvariantString());
		_elem.SetAttrib("globaluv", bGlobalUV.ToString());
		_elem.SetAttrib("index", index.ToString());
	}

	public void FromXML(XElement _element)
	{
		uv.x = StringParsers.ParseFloat(_element.GetAttribute("x"));
		uv.y = StringParsers.ParseFloat(_element.GetAttribute("y"));
		uv.width = StringParsers.ParseFloat(_element.GetAttribute("w"));
		uv.height = StringParsers.ParseFloat(_element.GetAttribute("h"));
		blockW = int.Parse(_element.GetAttribute("blockw"));
		blockH = int.Parse(_element.GetAttribute("blockh"));
		bSwitchUV = _element.HasAttribute("switchuv") && StringParsers.ParseBool(_element.GetAttribute("switchuv"));
		bGlobalUV = _element.HasAttribute("globaluv") && StringParsers.ParseBool(_element.GetAttribute("globaluv"));
		material = MaterialBlock.fromString(_element.GetAttribute("material"));
		textureName = _element.GetAttribute("texture");
		string[] array = _element.GetAttribute("color").Split(',');
		color = new Color(StringParsers.ParseFloat(array[0]), StringParsers.ParseFloat(array[1]), StringParsers.ParseFloat(array[2]));
		index = (_element.HasAttribute("index") ? int.Parse(_element.GetAttribute("index")) : 0);
	}
}
