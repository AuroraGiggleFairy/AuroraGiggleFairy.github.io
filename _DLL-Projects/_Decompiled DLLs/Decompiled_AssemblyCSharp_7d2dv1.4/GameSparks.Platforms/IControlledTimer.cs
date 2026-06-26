namespace GameSparks.Platforms;

public interface IControlledTimer : IGameSparksTimer
{
	void Update(long ticks);
}
