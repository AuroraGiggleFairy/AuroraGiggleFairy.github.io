using System.Collections;
using System.Xml.Linq;
using MusicUtils.Enums;
using UnityEngine;
using UnityEngine.Scripting;

namespace DynamicMusic;

[Preserve]
public class SingleClip : Content
{
	[PublicizedFrom(EAccessModifier.Private)]
	public string path;

	[field: PublicizedFrom(EAccessModifier.Private)]
	public AudioClip Clip
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	public override void Unload()
	{
		Clip.UnloadAudioData();
		IsLoaded = false;
	}

	public override IEnumerator Load()
	{
		if (!IsLoaded)
		{
			LoadManager.AssetRequestTask<AudioClip> requestTask = LoadManager.LoadAsset<AudioClip>(path);
			yield return new WaitUntil([PublicizedFrom(EAccessModifier.Internal)] () => requestTask.IsDone);
			Clip = requestTask.Asset;
		}
		IsLoaded = true;
	}

	public override void ParseFromXml(XElement _xmlNode)
	{
		base.ParseFromXml(_xmlNode);
		base.Section = EnumUtils.Parse<SectionType>(_xmlNode.GetAttribute("section"));
		path = _xmlNode.GetAttribute("path");
	}
}
