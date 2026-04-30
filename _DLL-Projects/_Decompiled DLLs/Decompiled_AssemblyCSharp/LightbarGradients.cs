using System;
using UnityEngine;

[Serializable]
[CreateAssetMenu(menuName = "Gradients/Lightbar Gradient Data")]
public class LightbarGradients : ScriptableObject
{
	public Gradient timeOfDayGradient;

	public Gradient dayGradient;

	public Gradient nightGradient;

	public Gradient cloudDayGradient;

	public Gradient cloudNightGradient;

	public Gradient bloodmoonGradient;

	public Color mainMenuColor;
}
