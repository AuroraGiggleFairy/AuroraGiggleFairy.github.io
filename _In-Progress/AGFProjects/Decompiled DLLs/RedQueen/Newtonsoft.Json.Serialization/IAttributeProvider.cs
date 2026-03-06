using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Newtonsoft.Json.Serialization;

[_003C464f7c58_002D4ec4_002D4694_002Da30c_002D0ded4d74fb4d_003ENullableContext(1)]
internal interface IAttributeProvider
{
	IList<Attribute> GetAttributes(bool inherit);

	IList<Attribute> GetAttributes(Type attributeType, bool inherit);
}
