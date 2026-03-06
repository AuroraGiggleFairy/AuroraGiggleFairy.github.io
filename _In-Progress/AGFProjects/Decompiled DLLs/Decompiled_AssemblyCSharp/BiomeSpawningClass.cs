public class BiomeSpawningClass
{
	public static DictionarySave<string, BiomeSpawnEntityGroupList> list = new DictionarySave<string, BiomeSpawnEntityGroupList>();

	public static void Cleanup()
	{
		list.Clear();
	}
}
