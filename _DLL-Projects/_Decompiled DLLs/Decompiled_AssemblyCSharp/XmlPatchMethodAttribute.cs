using System;

[AttributeUsage(AttributeTargets.Method, Inherited = false)]
public class XmlPatchMethodAttribute : Attribute
{
	public readonly string PatchName;

	public readonly bool RequiresXpath = true;

	public XmlPatchMethodAttribute(string _patchName)
	{
		PatchName = _patchName;
	}

	public XmlPatchMethodAttribute(string _patchName, bool _requiresXpath)
	{
		PatchName = _patchName;
		RequiresXpath = _requiresXpath;
	}
}
