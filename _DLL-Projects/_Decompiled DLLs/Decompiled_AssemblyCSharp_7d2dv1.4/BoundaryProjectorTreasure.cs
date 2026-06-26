using System;

public class BoundaryProjectorTreasure : BoundaryProjector
{
	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool withinRadius;

	public bool WithinRadius
	{
		get
		{
			return withinRadius;
		}
		set
		{
			if (withinRadius != value)
			{
				withinRadius = value;
				HandleWithinRadiusChange();
			}
		}
	}

	public float CurrentRadius => ProjectorList[0].Projector.orthographicSize;

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void SetupProjectors()
	{
		SetAlpha(0, 1f);
		SetAutoRotate(0, autoRotate: true, 2f);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void HandleWithinRadiusChange()
	{
		SetGlow(0, withinRadius);
	}
}
