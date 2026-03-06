using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class AIDirectorMarkerManagementComponent : AIDirectorComponent
{
	[PublicizedFrom(EAccessModifier.Private)]
	public List<IAIDirectorMarker> markers = new List<IAIDirectorMarker>(256);

	public override void Tick(double _dt)
	{
		base.Tick(_dt);
		TickMarkers(_dt);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void TickMarkers(double _dt)
	{
		for (int num = markers.Count - 1; num >= 0; num--)
		{
			IAIDirectorMarker iAIDirectorMarker = markers[num];
			iAIDirectorMarker.Tick(_dt);
			if (iAIDirectorMarker.TimeToLive <= 0f || (iAIDirectorMarker.Player != null && iAIDirectorMarker.Player.IsDead()))
			{
				markers.RemoveAt(num);
				iAIDirectorMarker.Release();
			}
		}
	}

	public IAIDirectorMarker FindBestMarker(Vector3 _pos, ref double _inOutIntensity)
	{
		IAIDirectorMarker result = null;
		int num = -1;
		for (int num2 = markers.Count - 1; num2 >= 0; num2--)
		{
			IAIDirectorMarker iAIDirectorMarker = markers[num2];
			if (iAIDirectorMarker.TimeToLive > 0f)
			{
				double num3 = iAIDirectorMarker.IntensityForPosition(_pos);
				if (num3 > 0.0 && iAIDirectorMarker.Priority > num)
				{
					num = iAIDirectorMarker.Priority;
					result = iAIDirectorMarker;
					_inOutIntensity = num3;
				}
			}
		}
		return result;
	}
}
