public class ParentControllerState
{
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiController[] m_parentControllers;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly bool[] m_isVisible;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly bool[] m_isEscClosable;

	public ParentControllerState(XUiController parentController)
	{
		if (parentController == null)
		{
			return;
		}
		if (parentController.ViewComponent == null)
		{
			m_parentControllers = new XUiController[parentController.Children.Count];
			for (int i = 0; i < parentController.Children.Count; i++)
			{
				m_parentControllers[i] = parentController.Children[i];
			}
		}
		else
		{
			m_parentControllers = new XUiController[1] { parentController };
		}
		m_isVisible = new bool[m_parentControllers.Length];
		m_isEscClosable = new bool[m_parentControllers.Length];
		for (int j = 0; j < m_parentControllers.Length; j++)
		{
			m_isVisible[j] = m_parentControllers[j].ViewComponent.IsVisible;
			m_isEscClosable[j] = m_parentControllers[j].WindowGroup.isEscClosable;
		}
	}

	public void Hide()
	{
		if (m_parentControllers != null)
		{
			for (int i = 0; i < m_parentControllers.Length; i++)
			{
				m_parentControllers[i].ViewComponent.IsVisible = false;
				m_parentControllers[i].WindowGroup.isEscClosable = false;
			}
		}
	}

	public void Restore()
	{
		if (m_parentControllers != null)
		{
			for (int i = 0; i < m_parentControllers.Length; i++)
			{
				m_parentControllers[i].ViewComponent.IsVisible = m_isVisible[i];
				m_parentControllers[i].WindowGroup.isEscClosable = m_isEscClosable[i];
			}
		}
	}
}
