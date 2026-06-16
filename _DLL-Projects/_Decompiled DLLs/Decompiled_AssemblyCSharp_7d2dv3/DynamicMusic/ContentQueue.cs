using System.Collections.Generic;
using MusicUtils.Enums;
using UniLinq;
using UnityEngine.Scripting;

namespace DynamicMusic;

[Preserve]
public class ContentQueue
{
	[PublicizedFrom(EAccessModifier.Private)]
	public static GameRandom rng = GameRandomManager.Instance.CreateGameRandom();

	[PublicizedFrom(EAccessModifier.Private)]
	public SectionType section;

	[PublicizedFrom(EAccessModifier.Private)]
	public LayerType layer;

	[PublicizedFrom(EAccessModifier.Private)]
	public int count;

	[PublicizedFrom(EAccessModifier.Private)]
	public Queue<LayeredContent> queue = new Queue<LayeredContent>();

	[Preserve]
	public bool IsReady => true;

	public ContentQueue(SectionType _section, LayerType _layer)
	{
		section = _section;
		layer = _layer;
		count = (from e in Content.AllContent.OfType<LayeredContent>()
			where e.Section == section && e.Layer == layer
			select e).Count();
	}

	public LayeredContent Next()
	{
		if (queue.Count < count / 2)
		{
			(from e in Content.AllContent.OfType<LayeredContent>()
				where e.Section == section && e.Layer == layer && !queue.Contains(e)
				orderby rng.RandomInt
				select e).ToList().ForEach(queue.Enqueue);
		}
		return queue.Dequeue();
	}

	public void Clear()
	{
		queue.Clear();
	}
}
