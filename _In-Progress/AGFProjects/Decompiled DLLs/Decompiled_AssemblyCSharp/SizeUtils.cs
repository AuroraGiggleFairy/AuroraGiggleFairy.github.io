using System;

public static class SizeUtils
{
	public static string FormatSize(this int sizeBytes, bool includeOriginalBytes = false)
	{
		if (Math.Abs(sizeBytes) < 1024)
		{
			return $"{sizeBytes} B";
		}
		double num = (double)sizeBytes / 1024.0;
		string arg = "kB";
		if (Math.Abs(num) >= 1024.0)
		{
			num /= 1024.0;
			arg = "MB";
		}
		if (Math.Abs(num) >= 1024.0)
		{
			num /= 1024.0;
			arg = "GB";
		}
		if (!includeOriginalBytes)
		{
			return $"{num:F3} {arg}";
		}
		return $"{num:F3} {arg} ({sizeBytes} B)";
	}

	public static string FormatSize(this uint sizeBytes, bool includeOriginalBytes = false)
	{
		if (sizeBytes < 1024)
		{
			return $"{sizeBytes} B";
		}
		double num = (double)sizeBytes / 1024.0;
		string arg = "kB";
		if (num >= 1024.0)
		{
			num /= 1024.0;
			arg = "MB";
		}
		if (num >= 1024.0)
		{
			num /= 1024.0;
			arg = "GB";
		}
		if (!includeOriginalBytes)
		{
			return $"{num:F3} {arg}";
		}
		return $"{num:F3} {arg} ({sizeBytes} B)";
	}

	public static string FormatSize(this long sizeBytes, bool includeOriginalBytes = false)
	{
		if (Math.Abs(sizeBytes) < 1024)
		{
			return $"{sizeBytes} B";
		}
		double num = (double)sizeBytes / 1024.0;
		string arg = "kB";
		if (Math.Abs(num) >= 1024.0)
		{
			num /= 1024.0;
			arg = "MB";
		}
		if (Math.Abs(num) >= 1024.0)
		{
			num /= 1024.0;
			arg = "GB";
		}
		if (Math.Abs(num) >= 1024.0)
		{
			num /= 1024.0;
			arg = "TB";
		}
		if (Math.Abs(num) >= 1024.0)
		{
			num /= 1024.0;
			arg = "PB";
		}
		if (Math.Abs(num) >= 1024.0)
		{
			num /= 1024.0;
			arg = "EB";
		}
		if (!includeOriginalBytes)
		{
			return $"{num:F3} {arg}";
		}
		return $"{num:F3} {arg} ({sizeBytes} B)";
	}

	public static string FormatSize(this ulong sizeBytes, bool includeOriginalBytes = false)
	{
		if (sizeBytes < 1024)
		{
			return $"{sizeBytes} B";
		}
		double num = (double)sizeBytes / 1024.0;
		string arg = "kB";
		if (num >= 1024.0)
		{
			num /= 1024.0;
			arg = "MB";
		}
		if (num >= 1024.0)
		{
			num /= 1024.0;
			arg = "GB";
		}
		if (num >= 1024.0)
		{
			num /= 1024.0;
			arg = "TB";
		}
		if (num >= 1024.0)
		{
			num /= 1024.0;
			arg = "PB";
		}
		if (num >= 1024.0)
		{
			num /= 1024.0;
			arg = "EB";
		}
		if (!includeOriginalBytes)
		{
			return $"{num:F3} {arg}";
		}
		return $"{num:F3} {arg} ({sizeBytes} B)";
	}
}
