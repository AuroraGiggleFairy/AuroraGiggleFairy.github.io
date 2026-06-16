using System;

namespace Platform.XBL.Save.MasterFileTable.Latest;

[Flags]
public enum NodeAttributes : uint
{
	None = 0u,
	Directory = 1u
}
