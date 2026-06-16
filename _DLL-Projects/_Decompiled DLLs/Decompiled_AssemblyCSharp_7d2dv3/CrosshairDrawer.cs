using UnityEngine;

public class CrosshairDrawer : MonoBehaviour
{
	public bool draw;

	public float centerX;

	public float centerY;

	public float openAreaX;

	public float openAreaY;

	public float length;

	public float thickness;

	public float opacity;

	public Color color = Color.white;

	[PublicizedFrom(EAccessModifier.Protected)]
	public void OnGUI()
	{
		if (draw)
		{
			EntityPlayerLocal.DrawDynamicCrosshair(centerX, centerY, openAreaX, openAreaY, length, thickness, opacity, color);
		}
	}
}
