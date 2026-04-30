using System;

namespace Newtonsoft.Json;

[AttributeUsage(AttributeTargets.Constructor, AllowMultiple = false)]
internal sealed class JsonConstructorAttribute : Attribute
{
}
