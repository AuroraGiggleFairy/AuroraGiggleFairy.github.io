using System.Collections.Generic;
using UnityEngine.Scripting;

namespace UAI;

[Preserve]
public class Context
{
	public EntityAlive Self;

	public World World;

	public ConsiderationData ConsiderationData;

	public ActionData ActionData;

	public float updateTimer;

	public List<string> AIPackages => Self.AIPackages;

	public Context(EntityAlive _self)
	{
		Self = _self;
		World = GameManager.Instance.World;
		ConsiderationData = new ConsiderationData();
		ActionData = default(ActionData);
	}
}
