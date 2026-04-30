using System.Runtime.CompilerServices;

namespace Newtonsoft.Json.Converters;

[_003C464f7c58_002D4ec4_002D4694_002Da30c_002D0ded4d74fb4d_003ENullableContext(2)]
internal interface IXmlDeclaration : IXmlNode
{
	string Version { get; }

	string Encoding { get; set; }

	string Standalone { get; set; }
}
