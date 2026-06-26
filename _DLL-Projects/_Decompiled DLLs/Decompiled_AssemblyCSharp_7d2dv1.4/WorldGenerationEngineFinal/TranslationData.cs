namespace WorldGenerationEngineFinal;

public class TranslationData
{
	public int x;

	public int y;

	public float scale;

	public int rotation;

	public TranslationData(int _x, int _y, float _randomScaleMin = 0.5f, float _randomScaleMax = 1.5f, int _rotation = -1)
	{
		x = _x;
		y = _y;
		scale = Rand.Instance.Range(_randomScaleMin, _randomScaleMax);
		rotation = _rotation;
		if (_rotation < 0)
		{
			rotation = Rand.Instance.Range(0, 360);
		}
	}

	public TranslationData(int _x, int _y, float _scale, int _rotation)
	{
		x = _x;
		y = _y;
		scale = _scale;
		rotation = _rotation;
	}
}
