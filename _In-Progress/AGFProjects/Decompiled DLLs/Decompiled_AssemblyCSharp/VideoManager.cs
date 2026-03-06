using System.Collections.Generic;

public class VideoManager
{
	public static Dictionary<string, VideoData> videos;

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool initialized;

	public static void Init()
	{
		initialized = true;
		videos = new Dictionary<string, VideoData>();
	}

	public static void AddVideo(VideoData data)
	{
		if (!initialized)
		{
			Init();
		}
		if (!videos.ContainsKey(data.name))
		{
			videos.Add(data.name, data);
		}
		else
		{
			videos[data.name] = data;
		}
	}

	public static VideoData GetVideoData(string id)
	{
		if (videos.TryGetValue(id, out var value))
		{
			return value;
		}
		return null;
	}
}
