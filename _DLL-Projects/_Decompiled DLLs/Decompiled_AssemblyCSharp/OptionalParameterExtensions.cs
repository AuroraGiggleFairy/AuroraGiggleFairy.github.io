public static class OptionalParameterExtensions
{
	public static T OrIfNullThen<T>(this T element, T defaultValue) where T : class
	{
		if (element != null)
		{
			return element;
		}
		return defaultValue;
	}
}
