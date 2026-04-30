public static class Submission
{
	[PublicizedFrom(EAccessModifier.Private)]
	public static bool isSubmissionChecked;

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool isSubmission;

	public static bool Enabled
	{
		get
		{
			if (!isSubmissionChecked)
			{
				string[] commandLineArgs = GameStartupHelper.GetCommandLineArgs();
				for (int i = 0; i < commandLineArgs.Length; i++)
				{
					if (commandLineArgs[i].EqualsCaseInsensitive(Constants.cArgSubmissionBuild))
					{
						Log.Out("Submission Enabled by argument");
						isSubmission = true;
					}
				}
				isSubmissionChecked = true;
			}
			return isSubmission;
		}
	}
}
