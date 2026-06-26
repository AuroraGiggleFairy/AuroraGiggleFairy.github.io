public class TagGroup
{
	public abstract class TagsGroupAbs
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		public TagsGroupAbs()
		{
		}
	}

	public class Global : TagsGroupAbs
	{
	}

	public class Poi : TagsGroupAbs
	{
	}
}
