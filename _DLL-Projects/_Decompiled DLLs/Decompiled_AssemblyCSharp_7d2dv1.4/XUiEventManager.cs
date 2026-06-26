public class XUiEventManager
{
	public delegate void XUiEvent_SkillExperienceAdded(ProgressionValue skill, int newXP);

	[PublicizedFrom(EAccessModifier.Private)]
	public static XUiEventManager instance;

	public static XUiEventManager Instance
	{
		get
		{
			if (instance == null)
			{
				instance = new XUiEventManager();
			}
			return instance;
		}
	}

	public event XUiEvent_SkillExperienceAdded OnSkillExperienceAdded;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiEventManager()
	{
	}

	public void SkillExperienceAdded(ProgressionValue skill, int newXP)
	{
		if (this.OnSkillExperienceAdded != null)
		{
			this.OnSkillExperienceAdded(skill, newXP);
		}
	}
}
