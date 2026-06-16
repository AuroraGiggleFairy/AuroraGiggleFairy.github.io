using System.Collections;
using Unity.XGamingRuntime;
using Unity.XGamingRuntime.Interop;

namespace Platform.XBL;

public class PrivilegeState
{
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUserHandle m_userHandle;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUserPrivilege m_privilege;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool m_has;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUserPrivilegeDenyReason m_denyReason;

	public bool Has => m_has;

	public XUserPrivilegeDenyReason DenyReason => m_denyReason;

	public PrivilegeState(XUserHandle userHandle, XUserPrivilege privilege)
	{
		m_userHandle = userHandle;
		m_privilege = privilege;
		m_has = false;
		m_denyReason = XUserPrivilegeDenyReason.Unknown;
	}

	public void ResolveSilent()
	{
		int hr = SDK.XUserCheckPrivilege(m_userHandle, XUserPrivilegeOptions.None, m_privilege, out m_has, out m_denyReason);
		XblHelpers.LogHR(hr, string.Format("{0} checked privilege '{1}' = {2} ({3})", "XUserCheckPrivilege", m_privilege, m_has, m_denyReason));
		if (!Unity.XGamingRuntime.Interop.HR.SUCCEEDED(hr))
		{
			m_has = false;
			m_denyReason = XUserPrivilegeDenyReason.Unknown;
		}
	}

	public IEnumerator ResolveWithPrompt(CoroutineCancellationToken _cancellationToken = null)
	{
		ResolveSilent();
		if (m_has)
		{
			yield break;
		}
		bool uiOpen = true;
		SDK.XUserResolvePrivilegeWithUiAsync(m_userHandle, XUserPrivilegeOptions.None, m_privilege, [PublicizedFrom(EAccessModifier.Internal)] (int hr) =>
		{
			CoroutineCancellationToken coroutineCancellationToken = _cancellationToken;
			if (coroutineCancellationToken != null && coroutineCancellationToken.IsCancelled())
			{
				return;
			}
			try
			{
				XblHelpers.LogHR(hr, "XUserResolvePrivilegeWithUiCompleted");
			}
			finally
			{
				uiOpen = false;
			}
		});
		while (uiOpen)
		{
			if (_cancellationToken?.IsCancelled() ?? false)
			{
				yield break;
			}
			yield return null;
		}
		ResolveSilent();
	}
}
