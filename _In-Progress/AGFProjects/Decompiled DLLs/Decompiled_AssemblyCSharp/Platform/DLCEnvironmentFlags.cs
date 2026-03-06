using System;

namespace Platform;

[Flags]
public enum DLCEnvironmentFlags
{
	None = 0,
	Dev = 1,
	Cert = 2,
	Retail = 4
}
