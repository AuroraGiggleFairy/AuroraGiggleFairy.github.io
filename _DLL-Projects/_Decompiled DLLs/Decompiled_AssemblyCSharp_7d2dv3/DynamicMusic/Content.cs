using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using MusicUtils.Enums;
using UnityEngine.Scripting;

namespace DynamicMusic;

[Preserve]
public abstract class Content
{
	public static Dictionary<SectionType, int> SamplesFor = new Dictionary<SectionType, int>();

	public static Dictionary<SectionType, string> SourcePathFor = new Dictionary<SectionType, string>();

	[PublicizedFrom(EAccessModifier.Protected)]
	public static GameRandom rng = GameRandomManager.Instance.CreateGameRandom();

	public static List<Content> AllContent = new List<Content>();

	[field: PublicizedFrom(EAccessModifier.Private)]
	public string Name
	{
		get; [PublicizedFrom(EAccessModifier.Protected)]
		set;
	}

	[field: PublicizedFrom(EAccessModifier.Private)]
	public SectionType Section
	{
		get; [PublicizedFrom(EAccessModifier.Protected)]
		set;
	}

	[field: PublicizedFrom(EAccessModifier.Private)]
	public virtual bool IsLoaded
	{
		get; [PublicizedFrom(EAccessModifier.Protected)]
		set;
	}

	public Content()
	{
		AllContent.Add(this);
	}

	[Preserve]
	public abstract IEnumerator Load();

	[Preserve]
	public abstract void Unload();

	[Preserve]
	public static Content CreateWrapper(string _type)
	{
		return (Content)Activator.CreateInstance(ReflectionHelpers.GetTypeWithPrefix("DynamicMusic.", _type));
	}

	public virtual void ParseFromXml(XElement _xmlNode)
	{
		Name = _xmlNode.GetAttribute("name");
	}
}
