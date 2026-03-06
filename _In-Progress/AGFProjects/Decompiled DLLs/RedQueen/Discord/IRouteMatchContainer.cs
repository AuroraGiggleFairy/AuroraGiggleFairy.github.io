using System.Collections.Generic;

namespace Discord;

internal interface IRouteMatchContainer
{
	IEnumerable<IRouteSegmentMatch> SegmentMatches { get; }

	void SetSegmentMatches(IEnumerable<IRouteSegmentMatch> segmentMatches);
}
