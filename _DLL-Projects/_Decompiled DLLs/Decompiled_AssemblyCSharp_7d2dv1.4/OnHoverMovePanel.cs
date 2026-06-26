using UnityEngine;

public class OnHoverMovePanel : MonoBehaviour
{
	[PublicizedFrom(EAccessModifier.Private)]
	public void Start()
	{
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Update()
	{
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void OnHover(bool isOver)
	{
		if (isOver)
		{
			base.transform.GetComponent<UIPanel>().depth = 1;
		}
		else
		{
			base.transform.GetComponent<UIPanel>().depth = 0;
		}
	}
}
