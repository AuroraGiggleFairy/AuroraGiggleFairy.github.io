using System.Xml.Linq;
using UnityEngine.Scripting;

[Preserve]
public class BaseLootEntryRequirement
{
	public virtual void Init(XElement e)
	{
	}

	public virtual bool CheckRequirement(EntityPlayer player)
	{
		return true;
	}
}
