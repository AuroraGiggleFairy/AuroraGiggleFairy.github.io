using System;

namespace Discord.Interactions;

[AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
internal sealed class HideAttribute : Attribute
{
}
