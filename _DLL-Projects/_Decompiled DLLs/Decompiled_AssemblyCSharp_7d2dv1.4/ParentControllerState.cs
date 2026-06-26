public class ParentControllerState
{
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiController m_parentController;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly bool m_isVisible;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly bool m_isEscClosable;

	public ParentControllerState(XUiController parentController)
	{
		m_parentController = parentController;
		if (m_parentController != null)
		{
			m_isVisible = m_parentController.ViewComponent.IsVisible;
			m_isEscClosable = m_parentController.WindowGroup.isEscClosable;
		}
	}

	public void Hide()
	{
		if (m_parentController != null)
		{
			m_parentController.ViewComponent.IsVisible = false;
			m_parentController.WindowGroup.isEscClosable = false;
		}
	}

	public void Restore()
	{
		if (m_parentController != null)
		{
			m_parentController.ViewComponent.IsVisible = m_isVisible;
			m_parentController.WindowGroup.isEscClosable = m_isEscClosable;
		}
	}
}
