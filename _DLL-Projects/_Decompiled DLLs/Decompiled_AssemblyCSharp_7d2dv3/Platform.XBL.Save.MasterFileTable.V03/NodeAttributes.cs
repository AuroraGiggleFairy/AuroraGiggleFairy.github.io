using System;

namespace Platform.XBL.Save.MasterFileTable.V03;

[Flags]
public enum NodeAttributes : uint
{
	None = 0u,
	Directory = 1u
}
