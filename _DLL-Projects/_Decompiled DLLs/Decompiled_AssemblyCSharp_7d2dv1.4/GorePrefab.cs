using System;

public class GorePrefab : RootTransformRefEntity
{
	public string Sound;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool _restoreState;

	public bool restoreState
	{
		set
		{
			_restoreState = value;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void Start()
	{
		base.Start();
		if (!_restoreState && RootTransform != null && Sound != null && Sound != string.Empty)
		{
			RootTransform.GetComponent<Entity>().PlayOneShot(Sound);
		}
	}
}
