namespace Newtonsoft.Json;

internal interface IJsonLineInfo
{
	int LineNumber { get; }

	int LinePosition { get; }

	bool HasLineInfo();
}
