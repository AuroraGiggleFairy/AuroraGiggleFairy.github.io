using System.Collections.Generic;
using System.Text;
using Platform;
using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdSaveDataManagerInfo : ConsoleCmdAbstract
{
	public override bool AllowedInMainMenu => true;

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string[] getCommands()
	{
		return new string[1] { "sdminfo" };
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "SaveDataManager Information";
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getHelp()
	{
		return "sdminfo";
	}

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		StringBuilder stringBuilder = new StringBuilder();
		ISaveDataManager saveDataManager = SaveDataUtils.SaveDataManager;
		AppendSaveDataManagerInfo(stringBuilder, saveDataManager);
		IPlatformSaveGameProvider saveGameProvider = PlatformManager.MultiPlatform.SaveGameProvider;
		AppendSaveGameProviderInfo(stringBuilder, saveGameProvider);
		Log.Out(stringBuilder.TrimEnd().ToString());
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void AppendSaveDataManagerInfo(StringBuilder builder, ISaveDataManager saveDataManager)
	{
		if (saveDataManager == null)
		{
			builder.AppendLine("No ISaveDataManager available.");
			return;
		}
		builder.AppendLine("Save Data Manager Info:");
		builder.AppendLine($"\tWrite Mode: {saveDataManager.GetWriteMode()}");
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void AppendSaveGameProviderInfo(StringBuilder builder, IPlatformSaveGameProvider saveGameProvider)
	{
		if (saveGameProvider == null)
		{
			builder.AppendLine("No IPlatformSaveGameProvider available.");
			return;
		}
		builder.AppendLine("Save Game Provider Info:");
		builder.AppendLine($"\tStatus: {saveGameProvider.Status}");
		builder.AppendLine($"\tShould Backup: {saveGameProvider.ShouldBackup()}");
		builder.AppendLine($"\tShould Commit: {saveGameProvider.ShouldCommit()}");
		AppendSaveGameProviderSizeInfo(builder, saveGameProvider);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void AppendSaveGameProviderSizeInfo(StringBuilder builder, IPlatformSaveGameProvider saveGameProvider)
	{
		bool flag = saveGameProvider.ShouldLimitSize();
		builder.AppendLine($"\tShould Limit Size: {flag}");
		if (flag)
		{
			saveGameProvider.UpdateSizes();
			SaveDataSizes sizes = saveGameProvider.GetSizes();
			builder.AppendLine("\tUsed Size      : " + sizes.Used.FormatSize(includeOriginalBytes: true));
			builder.AppendLine("\tRemaining Size : " + sizes.Remaining.FormatSize(includeOriginalBytes: true));
			builder.AppendLine("\tTotal Size     : " + sizes.Total.FormatSize(includeOriginalBytes: true));
		}
	}
}
