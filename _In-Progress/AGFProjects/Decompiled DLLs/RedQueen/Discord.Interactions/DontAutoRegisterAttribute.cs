using System;

namespace Discord.Interactions;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
internal class DontAutoRegisterAttribute : Attribute
{
}
