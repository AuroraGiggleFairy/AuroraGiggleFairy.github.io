using System;

namespace Discord.Commands;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
internal sealed class NamedArgumentTypeAttribute : Attribute
{
}
