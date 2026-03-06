using System.Xml.Linq;

public interface IMinEventAction
{
	bool CanExecute(MinEventTypes _eventType, MinEventParams _params);

	void Execute(MinEventParams _params);

	bool ParseXmlAttribute(XAttribute _attribute);

	void ParseXMLPostProcess();
}
