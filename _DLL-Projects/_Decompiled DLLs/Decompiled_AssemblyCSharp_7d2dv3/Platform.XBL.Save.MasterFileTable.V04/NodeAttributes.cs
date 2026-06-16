using System;

namespace Platform.XBL.Save.MasterFileTable.V04;

[Flags]
public enum NodeAttributes : uint
{
	None = 0u,
	Directory = 1u
}
