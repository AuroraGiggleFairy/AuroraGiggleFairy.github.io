namespace WorldGenerationEngineFinal;

public class DataMap<T>
{
	public T[,] data;

	public DataMap(int tileWidth, T defaultValue)
	{
		data = new T[tileWidth, tileWidth];
		for (int i = 0; i < data.GetLength(0); i++)
		{
			for (int j = 0; j < data.GetLength(1); j++)
			{
				data[i, j] = defaultValue;
			}
		}
	}
}
