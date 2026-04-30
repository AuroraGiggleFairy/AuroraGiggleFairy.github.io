using System;

namespace Discord.Commands;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
internal class DontAutoLoadAttribute : Attribute
{
}
