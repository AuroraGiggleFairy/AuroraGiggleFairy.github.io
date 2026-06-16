using System.Collections.Generic;

public class VideoData
{
	public string name;

	public string url;

	public float defaultSubtitleDuration;

	public List<VideoSubtitle> subtitles;

	public VideoData()
	{
		subtitles = new List<VideoSubtitle>();
	}
}
