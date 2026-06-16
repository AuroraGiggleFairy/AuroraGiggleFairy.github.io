using System.Collections;
using MusicUtils.Enums;

namespace DynamicMusic;

public interface ILayerMixer
{
	SectionType Sect { get; set; }

	float this[int _idx] { get; }

	IEnumerator Load();

	void Unload();
}
