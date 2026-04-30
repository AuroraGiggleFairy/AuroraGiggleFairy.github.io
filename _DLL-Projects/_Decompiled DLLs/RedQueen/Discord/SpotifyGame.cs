using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Discord;

[DebuggerDisplay("{DebuggerDisplay,nq}")]
internal class SpotifyGame : Game
{
	public IReadOnlyCollection<string> Artists { get; internal set; }

	public string AlbumTitle { get; internal set; }

	public string TrackTitle { get; internal set; }

	public DateTimeOffset? StartedAt { get; internal set; }

	public DateTimeOffset? EndsAt { get; internal set; }

	public TimeSpan? Duration { get; internal set; }

	public TimeSpan? Elapsed
	{
		get
		{
			DateTimeOffset utcNow = DateTimeOffset.UtcNow;
			DateTimeOffset? startedAt = StartedAt;
			return utcNow - startedAt;
		}
	}

	public TimeSpan? Remaining => EndsAt - DateTimeOffset.UtcNow;

	public string TrackId { get; internal set; }

	public string SessionId { get; internal set; }

	public string AlbumArtUrl { get; internal set; }

	public string TrackUrl { get; internal set; }

	private string DebuggerDisplay => base.Name + " (Spotify)";

	internal SpotifyGame()
	{
	}

	public override string ToString()
	{
		return string.Format("{0} - {1} ({2})", string.Join(", ", Artists), TrackTitle, Duration);
	}
}
