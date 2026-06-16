using UnityEngine;
using UnityEngine.UI;

public class GUILocalizedLabel : MonoBehaviour
{
	[SerializeField]
	[PublicizedFrom(EAccessModifier.Private)]
	public string localizationKey;

	[PublicizedFrom(EAccessModifier.Private)]
	public void Awake()
	{
		Text component = GetComponent<Text>();
		if ((bool)component)
		{
			component.text = Localization.Get(localizationKey);
		}
	}
}
