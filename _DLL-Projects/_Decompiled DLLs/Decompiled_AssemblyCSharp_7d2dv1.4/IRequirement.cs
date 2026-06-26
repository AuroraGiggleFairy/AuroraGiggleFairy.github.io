using System.Collections.Generic;
using System.Xml.Linq;

public interface IRequirement
{
	bool IsValid(MinEventParams _params);

	bool ParseXAttribute(XAttribute _attribute);

	void GetInfoStrings(ref List<string> list);

	string GetInfoString();

	void SetDescription(string desc);
}
