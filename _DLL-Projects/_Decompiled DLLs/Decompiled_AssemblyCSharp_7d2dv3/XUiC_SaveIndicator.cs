using System;
using System.Diagnostics;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_SaveIndicator : XUiController
{
	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly TimeSpan TailDuration = TimeSpan.FromSeconds(2.0);

	[PublicizedFrom(EAccessModifier.Private)]
	public ISaveDataManager m_saveDataManager;

	[PublicizedFrom(EAccessModifier.Private)]
	public Stopwatch m_tailTimer;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool m_commitInProgress;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool shouldShow;

	public override void Init()
	{
		base.Init();
		m_tailTimer = new Stopwatch();
		m_saveDataManager = SaveDataUtils.SaveDataManager;
		m_saveDataManager.CommitStarted += OnCommitStarted;
		m_saveDataManager.CommitFinished += OnCommitFinished;
		xui.SaveIndicator = this;
	}

	public override void Cleanup()
	{
		base.Cleanup();
		if (m_saveDataManager != null)
		{
			m_saveDataManager.CommitStarted -= OnCommitStarted;
			m_saveDataManager.CommitFinished -= OnCommitFinished;
			m_saveDataManager = null;
		}
		xui.SaveIndicator = null;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnCommitStarted()
	{
		m_commitInProgress = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnCommitFinished()
	{
		m_tailTimer.Restart();
		m_commitInProgress = false;
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (!m_commitInProgress && m_tailTimer.IsRunning && m_tailTimer.Elapsed >= TailDuration)
		{
			m_tailTimer.Stop();
		}
		shouldShow = m_commitInProgress || m_tailTimer.IsRunning;
		RefreshBindings();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool GetBindingValueInternal(ref string _value, string _bindingName)
	{
		if (_bindingName == "isSaving")
		{
			_value = shouldShow.ToString();
			return true;
		}
		return base.GetBindingValueInternal(ref _value, _bindingName);
	}
}
