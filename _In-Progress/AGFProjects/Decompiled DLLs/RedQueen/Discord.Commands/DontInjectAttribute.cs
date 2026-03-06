using System;

namespace Discord.Commands;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
internal class DontInjectAttribute : Attribute
{
}
