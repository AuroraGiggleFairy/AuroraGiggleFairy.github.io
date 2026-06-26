namespace DynamicMusic;

public interface IThreatLevelTracker : ICleanable
{
	IThreatLevel ThreatLevel { get; }

	void Update(EntityPlayerLocal _player);
}
