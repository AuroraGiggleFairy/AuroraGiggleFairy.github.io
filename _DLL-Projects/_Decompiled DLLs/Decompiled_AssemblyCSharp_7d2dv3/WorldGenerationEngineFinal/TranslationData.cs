namespace WorldGenerationEngineFinal;

public struct TranslationData
{
	public int x;

	public int y;

	public float scale;

	public int rotation;

	public TranslationData(int _x, int _y, float _randomScaleMin = 0.5f, float _randomScaleMax = 1.5f)
	{
		x = _x;
		y = _y;
		Rand instance = Rand.Instance;
		scale = instance.Range(_randomScaleMin, _randomScaleMax);
		rotation = instance.Angle();
	}

	public TranslationData(int _x, int _y, float _scale, int _rotation)
	{
		x = _x;
		y = _y;
		scale = _scale;
		rotation = _rotation;
	}
}
