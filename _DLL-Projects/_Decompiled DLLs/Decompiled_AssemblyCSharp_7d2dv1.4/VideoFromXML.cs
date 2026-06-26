using System.Collections;
using System.Xml.Linq;

public class VideoFromXML
{
	public static IEnumerator CreateVideos(XmlFile xmlFile)
	{
		XElement root = xmlFile.XmlDoc.Root;
		_ = root.HasElements;
		VideoManager.Init();
		Parse(root);
		yield break;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void Parse(XElement root)
	{
		foreach (XElement item2 in root.Elements("Video"))
		{
			VideoData videoData = new VideoData();
			videoData.name = item2.Attribute("name").Value;
			videoData.url = item2.Attribute("path").Value;
			videoData.defaultSubtitleDuration = float.Parse(item2.Attribute("defaultSubtitleDuration").Value);
			foreach (XElement item3 in item2.Elements("Subtitle"))
			{
				VideoSubtitle item = new VideoSubtitle
				{
					timestamp = double.Parse(item3.GetAttribute("timestamp")),
					subtitleId = item3.GetAttribute("id")
				};
				if (item3.HasAttribute("duration"))
				{
					item.duration = float.Parse(item3.GetAttribute("duration"));
				}
				else
				{
					item.duration = videoData.defaultSubtitleDuration;
				}
				videoData.subtitles.Add(item);
			}
			VideoManager.AddVideo(videoData);
		}
	}
}
