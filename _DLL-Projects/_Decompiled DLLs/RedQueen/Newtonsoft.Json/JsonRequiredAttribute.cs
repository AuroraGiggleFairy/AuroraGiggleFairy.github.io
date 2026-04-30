using System;

namespace Newtonsoft.Json;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
internal sealed class JsonRequiredAttribute : Attribute
{
}
