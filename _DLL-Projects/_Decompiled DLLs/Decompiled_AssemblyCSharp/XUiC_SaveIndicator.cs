using System;
using System.Diagnostics;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_SaveIndicator : XUiController
{
	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly TimeSpan TailDuration = TimeSpan.FromSeconds(2.0);

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Window m_window;

	[PublicizedFrom(EAccessModifier.Private)]
	public ISaveDataManager m_saveDataManager;

	[PublicizedFrom(EAccessModifier.Private)]
	public Stopwatch m_tailTimer;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool m_commitInProgress;

	public string ID = "";

	public override void Init()
	{
		base.Init();
		m_window = (XUiV_Window)base.ViewComponent;
		m_tailTimer = new Stopwatch();
		m_saveDataManager = SaveDataUtils.SaveDataManager;
		m_saveDataManager.CommitStarted += OnCommitStarted;
		m_saveDataManager.CommitFinished += OnCommitFinished;
		ID = base.WindowGroup.ID;
		base.xui.saveIndicator = this;
		m_window.TargetAlpha = 0.0015f;
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
		m_window = null;
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
		bool flag = m_commitInProgress || m_tailTimer.IsRunning;
		m_window.TargetAlpha = (flag ? 1f : 0.0015f);
	}
}
