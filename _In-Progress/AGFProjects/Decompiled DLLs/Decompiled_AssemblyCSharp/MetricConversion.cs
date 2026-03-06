public static class MetricConversion
{
	public const double nanoToMilli = 1E-06;

	public const int kilobyte = 1024;

	public const int megabyte = 1048576;

	public const int gigabyte = 1073741824;

	public const double bytesToKilobyte = 0.0009765625;

	public const double bytesToMegabyte = 9.5367431640625E-07;

	public const double bytesToGigabyte = 9.313225746154785E-10;

	public static string ToShortestBytesString(long bytes)
	{
		if (bytes < 1024)
		{
			return $"{bytes} B";
		}
		if (bytes < 1048576)
		{
			return $"{0.0009765625 * (double)bytes:F2} KB";
		}
		if (bytes < 1073741824)
		{
			return $"{9.5367431640625E-07 * (double)bytes:F2} MB";
		}
		return $"{9.313225746154785E-10 * (double)bytes:F2} GB";
	}
}
