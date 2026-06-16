using Unity.XGamingRuntime;
using Unity.XGamingRuntime.Interop;

namespace Platform.XBL;

public class XblSandboxHelper
{
	[PublicizedFrom(EAccessModifier.Private)]
	public string sandboxId;

	public string SandboxId
	{
		get
		{
			string text = sandboxId;
			if (text == null)
			{
				Log.Error("[XBL] XblSandboxHelper SandboxId has not finished refreshing");
				return null;
			}
			return text;
		}
	}

	public void RefreshSandboxId()
	{
		sandboxId = null;
		ThreadManager.AddSingleTask(GetSandboxTask);
		[PublicizedFrom(EAccessModifier.Private)]
		void GetSandboxTask(ThreadManager.TaskInfo taskInfo)
		{
			string text;
			int hr = SDK.XSystemGetXboxLiveSandboxId(out text);
			XblHelpers.LogHR(hr, "XSystemGetXboxLiveSandboxId");
			if (Unity.XGamingRuntime.Interop.HR.SUCCEEDED(hr))
			{
				Log.Out("[XBL] retrieved sandbox id: " + text);
				sandboxId = text;
			}
		}
	}

	public static EMatchmakingGroup SandboxIdToMatchmakingGroup(string sandboxId)
	{
		switch (sandboxId)
		{
		case "CERT":
		case "CERT.DEBUG":
			return EMatchmakingGroup.CertQA;
		case "RETAIL":
			return EMatchmakingGroup.Retail;
		default:
			return EMatchmakingGroup.Dev;
		}
	}

	public static DLCEnvironmentFlags SandboxIdToDLCEnvironment(string sandboxId)
	{
		switch (sandboxId)
		{
		case "CERT":
		case "CERT.DEBUG":
			return DLCEnvironmentFlags.Cert;
		case "RETAIL":
			return DLCEnvironmentFlags.Retail;
		default:
			return DLCEnvironmentFlags.Dev;
		}
	}
}
