namespace DynamicMusic;

public abstract class ContentPlayer : IPlayable
{
	[field: PublicizedFrom(EAccessModifier.Private)]
	public virtual float Volume { get; set; } = 1f;

	[field: PublicizedFrom(EAccessModifier.Private)]
	public virtual bool IsDone
	{
		get; [PublicizedFrom(EAccessModifier.Protected)]
		set;
	}

	[field: PublicizedFrom(EAccessModifier.Private)]
	public virtual bool IsPaused
	{
		get; [PublicizedFrom(EAccessModifier.Protected)]
		set;
	}

	[field: PublicizedFrom(EAccessModifier.Private)]
	public virtual bool IsPlaying
	{
		get; [PublicizedFrom(EAccessModifier.Protected)]
		set;
	}

	[field: PublicizedFrom(EAccessModifier.Private)]
	public virtual bool IsReady
	{
		get; [PublicizedFrom(EAccessModifier.Protected)]
		set;
	}

	public abstract void Init();

	public virtual void Play()
	{
		IsDone = false;
		IsPlaying = true;
		IsPaused = false;
	}

	public virtual void Pause()
	{
		IsPlaying = false;
		IsPaused = true;
	}

	public virtual void UnPause()
	{
		IsPlaying = true;
		IsPaused = false;
	}

	public virtual void Stop()
	{
		IsDone = true;
		IsPlaying = false;
		IsPaused = false;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public ContentPlayer()
	{
	}
}
