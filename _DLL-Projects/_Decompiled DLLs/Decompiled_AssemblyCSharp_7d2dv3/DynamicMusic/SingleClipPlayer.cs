using System.Collections;
using UniLinq;
using UnityEngine;

namespace DynamicMusic;

public abstract class SingleClipPlayer : Section, ISection, IPlayable, IFadeable, ICleanable
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public SingleClip clip;

	public override void Init()
	{
		base.Init();
		LoadRoutine = GameManager.Instance.StartCoroutine(InitializationCoroutine());
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public SingleClip GetSingleClip()
	{
		return Content.AllContent.OfType<SingleClip>().First([PublicizedFrom(EAccessModifier.Private)] (SingleClip c) => c.Section == base.Sect);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual IEnumerator InitializationCoroutine()
	{
		IsReady = false;
		clip = GetSingleClip();
		if (clip == null)
		{
			Log.Warning("content could not be cast as an object of type 'SingleClip'");
		}
		else
		{
			yield return clip.Load();
			GameObject gameObject = DataLoader.LoadAsset<GameObject>(Content.SourcePathFor[base.Sect]);
			src = gameObject.GetComponent<AudioSource>();
			src = Object.Instantiate(src);
			src.name = base.Sect.ToString();
			src.transform.SetParent(Section.parent.transform);
			src.clip = clip.Clip;
			SingleClipPlayer singleClipPlayer = this;
			SingleClipPlayer singleClipPlayer2 = this;
			bool isReady = true;
			singleClipPlayer2.IsInitialized = true;
			singleClipPlayer.IsReady = isReady;
		}
		LoadRoutine = null;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public SingleClipPlayer()
	{
	}
}
