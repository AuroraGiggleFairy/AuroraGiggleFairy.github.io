using System.Collections.Generic;

public class EAIItemTask : EAIBase
{
	public string ItemKey;

	public override void Init(EntityAlive _theEntity)
	{
		base.Init(_theEntity);
	}

	public override void SetData(Dictionary<string, string> data)
	{
		base.SetData(data);
	}

	public override bool CanExecute()
	{
		return !string.IsNullOrEmpty(ItemKey);
	}

	public override bool Continue()
	{
		return base.Continue();
	}

	public override void Update()
	{
		base.Update();
	}

	public override void Reset()
	{
		base.Reset();
	}
}
