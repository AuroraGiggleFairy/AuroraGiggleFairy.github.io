namespace DynamicMusic;

public interface IPlayable
{
	float Volume { get; set; }

	bool IsDone { get; }

	bool IsPaused { get; }

	bool IsPlaying { get; }

	bool IsReady { get; }

	void Init();

	void Play();

	void Pause();

	void UnPause();

	void Stop();
}
