namespace Audio;

public class NoiseData
{
	public float volume;

	public float time;

	public float heatMapStrength;

	public ulong heatMapTime;

	public float crouchMuffle;

	public NoiseData()
	{
		volume = 0f;
		time = 1f;
		heatMapStrength = 0f;
		heatMapTime = 100uL;
		crouchMuffle = 1f;
	}
}
