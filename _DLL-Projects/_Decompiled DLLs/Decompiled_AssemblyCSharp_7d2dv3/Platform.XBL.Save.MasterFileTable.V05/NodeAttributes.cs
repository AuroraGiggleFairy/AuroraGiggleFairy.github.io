using System;

namespace Platform.XBL.Save.MasterFileTable.V05;

[Flags]
public enum NodeAttributes : uint
{
	None = 0u,
	Directory = 1u
}
