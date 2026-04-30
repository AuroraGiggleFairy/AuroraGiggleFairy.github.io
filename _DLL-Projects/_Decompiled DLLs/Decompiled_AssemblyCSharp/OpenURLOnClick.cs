using UnityEngine;

public class OpenURLOnClick : MonoBehaviour
{
	[PublicizedFrom(EAccessModifier.Private)]
	public void OnClick()
	{
		UILabel component = GetComponent<UILabel>();
		if (component != null)
		{
			string urlAtPosition = component.GetUrlAtPosition(UICamera.lastWorldPosition);
			if (!string.IsNullOrEmpty(urlAtPosition))
			{
				Application.OpenURL(urlAtPosition);
			}
		}
	}
}
