using System;
using Platform;
using UnityEngine;
using UnityEngine.UI;

public class GUIButtonPrompt : MonoBehaviour
{
	[SerializeField]
	[PublicizedFrom(EAccessModifier.Private)]
	public Sprite XBSprite;

	[SerializeField]
	[PublicizedFrom(EAccessModifier.Private)]
	public Sprite PSSprite;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Image image;

	[PublicizedFrom(EAccessModifier.Private)]
	public void Awake()
	{
		image = GetComponent<Image>();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnEnable()
	{
		RefreshIcon();
	}

	public void RefreshIcon()
	{
		PlayerInputManager.InputStyle inputStyle = PlayerInputManager.InputStyleFromSelectedIconStyle();
		image.sprite = ((inputStyle == PlayerInputManager.InputStyle.PS4) ? PSSprite : XBSprite);
	}
}
