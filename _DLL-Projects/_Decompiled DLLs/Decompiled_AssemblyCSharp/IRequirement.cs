using System.Collections.Generic;
using System.Xml.Linq;

public interface IRequirement
{
	bool IsValid(MinEventParams _params);

	bool ParseXAttribute(XAttribute _attribute);

	void GetInfoStrings(ref List<string> list);

	string GetInfoString();

	string GetDescription();

	void SetDescription(string desc);
}
