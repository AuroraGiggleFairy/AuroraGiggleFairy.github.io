using UnityEngine;

[ExecuteInEditMode]
public class ShaderGlobalsHelper : MonoBehaviour
{
	[Header("Electric Shock Shader Properties")]
	[SerializeField]
	[PublicizedFrom(EAccessModifier.Private)]
	public float electricShockIntensity = 1f;

	[SerializeField]
	[PublicizedFrom(EAccessModifier.Private)]
	public Color electricShockColor = new Color(0.5f, 0.8f, 1f, 1f);

	[SerializeField]
	[PublicizedFrom(EAccessModifier.Private)]
	public float electricShockSpeed = 5f;

	[SerializeField]
	[PublicizedFrom(EAccessModifier.Private)]
	public float electricShockScale = 2.5f;

	[SerializeField]
	[PublicizedFrom(EAccessModifier.Private)]
	public Texture electricShockTexture;

	[SerializeField]
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector4 electricShockTexture_ST;

	[SerializeField]
	[PublicizedFrom(EAccessModifier.Private)]
	public float electricShockTexturePanSpeed = 10f;

	[SerializeField]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool updateEveryFrame;

	[PublicizedFrom(EAccessModifier.Private)]
	public void Start()
	{
		Shader.SetGlobalFloat("_ElectricShockIntensity", electricShockIntensity);
		Shader.SetGlobalColor("_ElectricShockColor", electricShockColor);
		Shader.SetGlobalFloat("_ElectricShockSpeed", electricShockSpeed);
		Shader.SetGlobalFloat("_ElectricShockScale", electricShockScale);
		Shader.SetGlobalTexture("_ElectricShockTexture", electricShockTexture);
		Shader.SetGlobalVector("_ElectricShockTexture_ST", electricShockTexture_ST);
		Shader.SetGlobalFloat("_ElectricShockTexturePanSpeed", electricShockTexturePanSpeed);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Update()
	{
	}
}
