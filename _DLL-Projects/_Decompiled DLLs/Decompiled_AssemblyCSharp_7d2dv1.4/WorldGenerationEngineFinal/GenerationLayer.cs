using System.Collections.Generic;

namespace WorldGenerationEngineFinal;

public class GenerationLayer
{
	public int x;

	public int y;

	public int Range;

	public List<TranslationData> children;

	public GenerationLayer(int _x, int _y, int _range)
	{
		x = _x;
		y = _y;
		Range = _range;
		children = new List<TranslationData>();
	}
}
