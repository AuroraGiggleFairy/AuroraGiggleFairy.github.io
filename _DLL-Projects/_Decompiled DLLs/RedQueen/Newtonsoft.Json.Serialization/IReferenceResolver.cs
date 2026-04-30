using System.Runtime.CompilerServices;

namespace Newtonsoft.Json.Serialization;

[_003C464f7c58_002D4ec4_002D4694_002Da30c_002D0ded4d74fb4d_003ENullableContext(1)]
internal interface IReferenceResolver
{
	object ResolveReference(object context, string reference);

	string GetReference(object context, object value);

	bool IsReferenced(object context, object value);

	void AddReference(object context, string reference, object value);
}
