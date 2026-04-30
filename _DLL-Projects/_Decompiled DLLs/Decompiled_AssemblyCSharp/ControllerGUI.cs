using System;
using UnityEngine;

public class ControllerGUI : MonoBehaviour
{
	public Transform sceneLightObject;

	public float lightDirection = 0.25f;

	public float lightIntensity = 0.75f;

	public GameObject modeObjectE;

	public GameObject modeObjectF;

	public Transform targetObjectE;

	public Transform targetObjectF1;

	public Transform targetObjectF2;

	public bool autoDilate;

	public float lodLevel = 0.15f;

	public float parallaxAmt = 1f;

	public float pupilDilation = 0.5f;

	public float scleraSize;

	public float irisSize = 0.22f;

	public Color irisColor = new Color(1f, 1f, 1f, 1f);

	public Color scleraColor = new Color(1f, 1f, 1f, 1f);

	public int irisTexture;

	public Texture[] irisTextures;

	public Texture2D texTitle;

	public Texture2D texTD;

	public Texture2D texDiv1;

	public Texture2D texSlideA;

	public Texture2D texSlideB;

	public Texture2D texSlideD;

	public Transform lodLevel0;

	public Transform lodLevel1;

	public Transform lodLevel2;

	[HideInInspector]
	public string sceneMode = "figure";

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float currentLodLevel;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float doLodSwitch = -1f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 lodRot;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Light sceneLight;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Renderer targetRenderer;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Renderer targetRenderer1;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Renderer targetRenderer2;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float lightAngle;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float ambientFac;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float irisTextureF;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float irisTextureD;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Color colorGold = new Color(0.79f, 0.55f, 0.054f, 1f);

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Color colorGrey = new Color(0.333f, 0.3f, 0.278f, 1f);

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Color colorHighlight = new Color(0.99f, 0.75f, 0.074f, 1f);

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public EyeAdv_AutoDilation autoDilateObject;

	[PublicizedFrom(EAccessModifier.Private)]
	public void Start()
	{
		sceneMode = "eye";
		lodLevel0.gameObject.SetActive(value: true);
		lodLevel1.gameObject.SetActive(value: false);
		lodLevel2.gameObject.SetActive(value: false);
		if (sceneLightObject != null)
		{
			sceneLight = sceneLightObject.GetComponent<Light>();
		}
		if (targetObjectE != null)
		{
			targetRenderer = targetObjectE.transform.GetComponent<Renderer>();
			autoDilateObject = targetObjectE.gameObject.GetComponent<EyeAdv_AutoDilation>();
		}
		if (targetObjectE != null)
		{
			targetRenderer = targetObjectE.transform.GetComponent<Renderer>();
		}
		if (targetObjectF1 != null)
		{
			targetRenderer1 = targetObjectF1.transform.GetComponent<Renderer>();
		}
		if (targetObjectF2 != null)
		{
			targetRenderer2 = targetObjectF2.transform.GetComponent<Renderer>();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Update()
	{
		lightIntensity = Mathf.Clamp(lightIntensity, 0f, 1f);
		lightDirection = Mathf.Clamp(lightDirection, 0f, 1f);
		sceneLightObject.transform.eulerAngles = new Vector3(sceneLightObject.transform.eulerAngles.x, Mathf.Lerp(0f, 359f, lightDirection), sceneLightObject.transform.eulerAngles.z);
		sceneLight.intensity = lightIntensity;
		if (autoDilateObject != null)
		{
			autoDilateObject.enableAutoDilation = autoDilate;
		}
		irisSize = Mathf.Clamp(irisSize, 0f, 1f);
		parallaxAmt = Mathf.Clamp(parallaxAmt, 0f, 1f);
		scleraSize = Mathf.Clamp(scleraSize, 0f, 1f);
		irisTextureF = Mathf.Clamp(Mathf.FloorToInt(irisTextureF), 0, irisTextures.Length - 1);
		irisTextureD = irisTextureF / (float)(irisTextures.Length - 1);
		irisTexture = Mathf.Clamp(Mathf.FloorToInt(irisTextureF), 0, irisTextures.Length - 1);
		if (targetRenderer != null)
		{
			targetRenderer.material.SetFloat("_irisSize", Mathf.Lerp(1.5f, 5f, irisSize));
			targetRenderer.material.SetFloat("_parallax", Mathf.Lerp(0f, 0.05f, parallaxAmt));
			if (!autoDilate)
			{
				targetRenderer.material.SetFloat("_pupilSize", pupilDilation);
			}
			targetRenderer.material.SetFloat("_scleraSize", Mathf.Lerp(1.1f, 2.2f, scleraSize));
			targetRenderer.material.SetColor("_irisColor", irisColor);
			targetRenderer.material.SetColor("_scleraColor", scleraColor);
			targetRenderer.material.SetTexture("_IrisColorTex", irisTextures[irisTexture]);
		}
		if (targetRenderer1 != null)
		{
			targetRenderer1.material.CopyPropertiesFromMaterial(targetRenderer.material);
		}
		if (targetRenderer2 != null)
		{
			targetRenderer2.material.CopyPropertiesFromMaterial(targetRenderer.material);
		}
		if (currentLodLevel == lodLevel)
		{
			return;
		}
		doLodSwitch = -1f;
		if (lodLevel < 0.31f && currentLodLevel > 0.31f)
		{
			doLodSwitch = 0f;
		}
		if (lodLevel > 0.7f && currentLodLevel > 0.7f)
		{
			doLodSwitch = 2f;
		}
		if (lodLevel > 0.31f && lodLevel < 0.7f && (currentLodLevel < 0.31f || currentLodLevel > 0.7f))
		{
			doLodSwitch = 1f;
		}
		currentLodLevel = lodLevel;
		if (doLodSwitch >= 0f)
		{
			if (doLodSwitch == 0f && lodLevel0 != null)
			{
				lodLevel0.gameObject.SetActive(value: true);
				lodLevel1.gameObject.SetActive(value: false);
				lodLevel2.gameObject.SetActive(value: false);
				targetObjectF1 = lodLevel0;
			}
			if (doLodSwitch == 1f && lodLevel1 != null)
			{
				lodLevel0.gameObject.SetActive(value: false);
				lodLevel1.gameObject.SetActive(value: true);
				lodLevel2.gameObject.SetActive(value: false);
				targetObjectF1 = lodLevel1;
			}
			if (doLodSwitch == 2f && lodLevel2 != null)
			{
				lodLevel0.gameObject.SetActive(value: false);
				lodLevel1.gameObject.SetActive(value: false);
				lodLevel2.gameObject.SetActive(value: true);
				targetObjectF1 = lodLevel2;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnGUI()
	{
		GUI.color = new Color(1f, 1f, 1f, 1f);
		if (texTitle != null)
		{
			GUI.Label(new Rect(25f, 25f, texTitle.width, texTitle.height), texTitle);
		}
		if (texTD != null)
		{
			GUI.Label(new Rect(800f, 45f, texTD.width * 2, texTD.height * 2), texTD);
		}
		GUI.color = new Color(1f, 1f, 1f, 1f);
		if (texDiv1 != null)
		{
			GUI.Label(new Rect(150f, 130f, texDiv1.width, texDiv1.height), texDiv1);
		}
		GUI.color = colorGold;
		GUI.Label(new Rect(35f, 128f, 180f, 20f), "EYEBALL VIEW");
		GUI.color = colorGrey;
		GUI.Label(new Rect(160f, 128f, 280f, 20f), "CHARACTER VIEW (not included)");
		if (Event.current.type == EventType.MouseUp && new Rect(35f, 128f, 100f, 20f).Contains(Event.current.mousePosition))
		{
			sceneMode = "eye";
		}
		if (Event.current.type == EventType.MouseUp && new Rect(160f, 128f, 100f, 20f).Contains(Event.current.mousePosition))
		{
			sceneMode = "figure";
		}
		GenerateSlider("EYE LOD LEVEL", 35, 185, showPercent: false, "lodLevel", 293);
		GUI.color = new Color(1f, 1f, 1f, 1f);
		if (texDiv1 != null)
		{
			GUI.Label(new Rect(130f, 217f, texDiv1.width, texDiv1.height), texDiv1);
		}
		if (texDiv1 != null)
		{
			GUI.Label(new Rect(240f, 217f, texDiv1.width, texDiv1.height), texDiv1);
		}
		GUI.color = colorGold;
		if (lodLevel > 0.32f)
		{
			GUI.color = colorGrey;
		}
		if (new Rect(60f, 215f, 40f, 20f).Contains(Event.current.mousePosition))
		{
			GUI.color = colorHighlight;
		}
		GUI.Label(new Rect(60f, 215f, 40f, 20f), "LOD 0");
		if (Event.current.type == EventType.MouseUp && new Rect(60f, 215f, 100f, 20f).Contains(Event.current.mousePosition))
		{
			lodLevel = 0f;
		}
		GUI.color = colorGold;
		if (lodLevel < 0.32f || lodLevel > 0.7f)
		{
			GUI.color = colorGrey;
		}
		if (new Rect(165f, 215f, 50f, 20f).Contains(Event.current.mousePosition))
		{
			GUI.color = colorHighlight;
		}
		GUI.Label(new Rect(165f, 215f, 50f, 20f), "LOD 1");
		if (Event.current.type == EventType.MouseUp && new Rect(165f, 215f, 100f, 20f).Contains(Event.current.mousePosition))
		{
			lodLevel = 0.5f;
		}
		GUI.color = colorGold;
		if (lodLevel < 0.7f)
		{
			GUI.color = colorGrey;
		}
		if (new Rect(270f, 215f, 50f, 20f).Contains(Event.current.mousePosition))
		{
			GUI.color = colorHighlight;
		}
		GUI.Label(new Rect(270f, 215f, 50f, 20f), "LOD 2");
		if (Event.current.type == EventType.MouseUp && new Rect(270f, 215f, 100f, 20f).Contains(Event.current.mousePosition))
		{
			lodLevel = 1f;
		}
		GenerateSlider("PUPIL DILATION", 35, 248, showPercent: true, "pupilDilation", 293);
		GUI.color = new Color(1f, 1f, 1f, 1f);
		if (texDiv1 != null)
		{
			GUI.Label(new Rect(272f, 280f, texDiv1.width, texDiv1.height), texDiv1);
		}
		GUI.color = colorGold;
		if (!autoDilate)
		{
			GUI.color = colorGrey;
		}
		if (new Rect(240f, 278f, 40f, 20f).Contains(Event.current.mousePosition))
		{
			GUI.color = colorHighlight;
		}
		GUI.Label(new Rect(240f, 278f, 40f, 20f), "auto");
		GUI.color = colorGold;
		if (autoDilate)
		{
			GUI.color = colorGrey;
		}
		if (new Rect(280f, 278f, 40f, 20f).Contains(Event.current.mousePosition))
		{
			GUI.color = colorHighlight;
		}
		GUI.Label(new Rect(280f, 278f, 50f, 20f), "manual");
		if (Event.current.type == EventType.MouseUp && new Rect(240f, 278f, 40f, 20f).Contains(Event.current.mousePosition))
		{
			autoDilate = true;
		}
		if (Event.current.type == EventType.MouseUp && new Rect(280f, 278f, 50f, 20f).Contains(Event.current.mousePosition))
		{
			autoDilate = false;
		}
		GenerateSlider("SCLERA SIZE", 35, 310, showPercent: true, "scleraSize", 293);
		GenerateSlider("IRIS SIZE", 35, 350, showPercent: true, "irisSize", 293);
		GenerateSlider("IRIS TEXTURE", 35, 390, showPercent: false, "irisTexture", 293);
		GUI.color = new Color(1f, 1f, 1f, 1f);
		for (int i = 0; i < irisTextures.Length; i++)
		{
			if (texDiv1 != null)
			{
				GUI.Label(new Rect(36 + i * 22, 416f, texDiv1.width, texDiv1.height), texDiv1);
			}
		}
		GenerateSlider("IRIS PARALLAX", 35, 440, showPercent: true, "irisParallax", 293);
		GUI.color = colorGold;
		GUI.Label(new Rect(35f, 510f, 180f, 20f), "IRIS COLOR");
		GUI.color = colorGrey;
		GUI.Label(new Rect(35f, 525f, 20f, 20f), "r");
		GUI.Label(new Rect(35f, 538f, 20f, 20f), "g");
		GUI.Label(new Rect(35f, 551f, 20f, 20f), "b");
		GUI.Label(new Rect(35f, 564f, 20f, 20f), "a");
		GenerateSlider("", 50, 512, showPercent: false, "irisColorR", 278);
		GenerateSlider("", 50, 525, showPercent: false, "irisColorG", 278);
		GenerateSlider("", 50, 538, showPercent: false, "irisColorB", 278);
		GenerateSlider("", 50, 550, showPercent: false, "irisColorA", 278);
		GUI.color = colorGold;
		GUI.Label(new Rect(35f, 590f, 180f, 20f), "SCLERA COLOR");
		GUI.color = colorGrey;
		GUI.Label(new Rect(35f, 605f, 20f, 20f), "r");
		GUI.Label(new Rect(35f, 618f, 20f, 20f), "g");
		GUI.Label(new Rect(35f, 631f, 20f, 20f), "b");
		GUI.Label(new Rect(35f, 644f, 20f, 20f), "a");
		GenerateSlider("", 50, 592, showPercent: false, "scleraColorR", 278);
		GenerateSlider("", 50, 605, showPercent: false, "scleraColorG", 278);
		GenerateSlider("", 50, 618, showPercent: false, "scleraColorB", 278);
		GenerateSlider("", 50, 630, showPercent: false, "scleraColorA", 278);
		GUI.color = colorGold;
		GUI.Label(new Rect(35f, 730f, 150f, 20f), "LIGHT DIRECTION");
		GenerateSlider("", 160, 716, showPercent: false, "lightDir", 820);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void GenerateSlider(string title, int sX, int sY, bool showPercent, string funcName, int sWidth)
	{
		GUI.color = colorGold;
		if (title != "")
		{
			GUI.Label(new Rect(sX, sY, 180f, 20f), title);
		}
		if (funcName == "lightDir" && showPercent)
		{
			GUI.Label(new Rect(sX + (sWidth - 28), sY, 80f, 20f), Mathf.CeilToInt(lightDirection * 100f) + "%");
		}
		if (funcName == "lodLevel" && showPercent)
		{
			GUI.Label(new Rect(sX + (sWidth - 28), sY, 80f, 20f), Mathf.CeilToInt(100f - lodLevel * 100f) + "%");
		}
		if (funcName == "pupilDilation" && showPercent)
		{
			GUI.Label(new Rect(sX + (sWidth - 28), sY, 80f, 20f), Mathf.CeilToInt(pupilDilation * 100f) + "%");
		}
		if (funcName == "scleraSize" && showPercent)
		{
			GUI.Label(new Rect(sX + (sWidth - 28), sY, 80f, 20f), Mathf.CeilToInt(scleraSize * 100f) + "%");
		}
		if (funcName == "irisSize" && showPercent)
		{
			GUI.Label(new Rect(sX + (sWidth - 28), sY, 80f, 20f), Mathf.CeilToInt(irisSize * 100f) + "%");
		}
		if (funcName == "irisTexture" && showPercent)
		{
			GUI.Label(new Rect(sX + (sWidth - 28), sY, 80f, 20f), Mathf.CeilToInt(irisTextureD * 100f) + "%");
		}
		if (funcName == "irisParallax" && showPercent)
		{
			GUI.Label(new Rect(sX + (sWidth - 28), sY, 80f, 20f), Mathf.CeilToInt(parallaxAmt * 100f) + "%");
		}
		if (funcName == "irisColorR" && showPercent)
		{
			GUI.Label(new Rect(sX + (sWidth - 28), sY, 80f, 20f), Mathf.CeilToInt(irisColor.r * 100f) + "%");
		}
		if (funcName == "irisColorG" && showPercent)
		{
			GUI.Label(new Rect(sX + (sWidth - 28), sY, 80f, 20f), Mathf.CeilToInt(irisColor.g * 100f) + "%");
		}
		if (funcName == "irisColorB" && showPercent)
		{
			GUI.Label(new Rect(sX + (sWidth - 28), sY, 80f, 20f), Mathf.CeilToInt(irisColor.b * 100f) + "%");
		}
		if (funcName == "irisColorA" && showPercent)
		{
			GUI.Label(new Rect(sX + (sWidth - 28), sY, 80f, 20f), Mathf.CeilToInt(irisColor.a * 100f) + "%");
		}
		if (funcName == "scleraColorR" && showPercent)
		{
			GUI.Label(new Rect(sX + (sWidth - 28), sY, 80f, 20f), Mathf.CeilToInt(scleraColor.r * 100f) + "%");
		}
		if (funcName == "scleraColorG" && showPercent)
		{
			GUI.Label(new Rect(sX + (sWidth - 28), sY, 80f, 20f), Mathf.CeilToInt(scleraColor.g * 100f) + "%");
		}
		if (funcName == "scleraColorB" && showPercent)
		{
			GUI.Label(new Rect(sX + (sWidth - 28), sY, 80f, 20f), Mathf.CeilToInt(scleraColor.b * 100f) + "%");
		}
		if (funcName == "scleraColorA" && showPercent)
		{
			GUI.Label(new Rect(sX + (sWidth - 28), sY, 80f, 20f), Mathf.CeilToInt(scleraColor.a * 100f) + "%");
		}
		GUI.color = new Color(1f, 1f, 1f, 1f);
		if (texSlideB != null)
		{
			GUI.DrawTextureWithTexCoords(new Rect(sX, sY + 22, sWidth + 2, 7f), texSlideB, new Rect(sX, sY + 22, sWidth + 2, 7f), alphaBlend: true);
		}
		if (funcName == "lightDir" && texSlideA != null)
		{
			GUI.DrawTextureWithTexCoords(new Rect(sX + 1, sY + 23, Mathf.Lerp(1f, sWidth, lightDirection), 5f), texSlideA, new Rect(sX + 1, sY + 23, sWidth, 5f), alphaBlend: true);
		}
		if (funcName == "lodLevel" && texSlideA != null)
		{
			GUI.DrawTextureWithTexCoords(new Rect(sX + 1, sY + 23, Mathf.Lerp(1f, sWidth, lodLevel), 5f), texSlideA, new Rect(sX + 1, sY + 23, sWidth, 5f), alphaBlend: true);
		}
		if (funcName == "pupilDilation" && texSlideA != null)
		{
			GUI.DrawTextureWithTexCoords(new Rect(sX + 1, sY + 23, Mathf.Lerp(1f, sWidth, pupilDilation), 5f), texSlideA, new Rect(sX + 1, sY + 23, sWidth, 5f), alphaBlend: true);
		}
		if (funcName == "scleraSize" && texSlideA != null)
		{
			GUI.DrawTextureWithTexCoords(new Rect(sX + 1, sY + 23, Mathf.Lerp(1f, sWidth, scleraSize), 5f), texSlideA, new Rect(sX + 1, sY + 23, sWidth, 5f), alphaBlend: true);
		}
		if (funcName == "irisSize" && texSlideA != null)
		{
			GUI.DrawTextureWithTexCoords(new Rect(sX + 1, sY + 23, Mathf.Lerp(1f, sWidth, irisSize), 5f), texSlideA, new Rect(sX + 1, sY + 23, sWidth, 5f), alphaBlend: true);
		}
		if (funcName == "irisTexture" && texSlideA != null)
		{
			GUI.DrawTextureWithTexCoords(new Rect(sX + 1, sY + 23, Mathf.Lerp(1f, sWidth, irisTextureD), 5f), texSlideA, new Rect(sX + 1, sY + 23, sWidth, 5f), alphaBlend: true);
		}
		if (funcName == "irisParallax" && texSlideA != null)
		{
			GUI.DrawTextureWithTexCoords(new Rect(sX + 1, sY + 23, Mathf.Lerp(1f, sWidth, parallaxAmt), 5f), texSlideA, new Rect(sX + 1, sY + 23, sWidth, 5f), alphaBlend: true);
		}
		GUI.color = new Color(irisColor.r, irisColor.g, irisColor.b, irisColor.a);
		if (funcName == "irisColorR" && texSlideD != null)
		{
			GUI.DrawTextureWithTexCoords(new Rect(sX + 1, sY + 23, Mathf.Lerp(1f, sWidth, irisColor.r), 5f), texSlideD, new Rect(sX + 1, sY + 23, sWidth, 5f), alphaBlend: true);
		}
		if (funcName == "irisColorG" && texSlideD != null)
		{
			GUI.DrawTextureWithTexCoords(new Rect(sX + 1, sY + 23, Mathf.Lerp(1f, sWidth, irisColor.g), 5f), texSlideD, new Rect(sX + 1, sY + 23, sWidth, 5f), alphaBlend: true);
		}
		if (funcName == "irisColorB" && texSlideD != null)
		{
			GUI.DrawTextureWithTexCoords(new Rect(sX + 1, sY + 23, Mathf.Lerp(1f, sWidth, irisColor.b), 5f), texSlideD, new Rect(sX + 1, sY + 23, sWidth, 5f), alphaBlend: true);
		}
		GUI.color = colorGrey * 2f;
		if (funcName == "irisColorA" && texSlideD != null)
		{
			GUI.DrawTextureWithTexCoords(new Rect(sX + 1, sY + 23, Mathf.Lerp(1f, sWidth, irisColor.a), 5f), texSlideD, new Rect(sX + 1, sY + 23, sWidth, 5f), alphaBlend: true);
		}
		GUI.color = new Color(scleraColor.r, scleraColor.g, scleraColor.b, scleraColor.a);
		if (funcName == "scleraColorR" && texSlideD != null)
		{
			GUI.DrawTextureWithTexCoords(new Rect(sX + 1, sY + 23, Mathf.Lerp(1f, sWidth, scleraColor.r), 5f), texSlideD, new Rect(sX + 1, sY + 23, sWidth, 5f), alphaBlend: true);
		}
		if (funcName == "scleraColorG" && texSlideD != null)
		{
			GUI.DrawTextureWithTexCoords(new Rect(sX + 1, sY + 23, Mathf.Lerp(1f, sWidth, scleraColor.g), 5f), texSlideD, new Rect(sX + 1, sY + 23, sWidth, 5f), alphaBlend: true);
		}
		if (funcName == "scleraColorB" && texSlideD != null)
		{
			GUI.DrawTextureWithTexCoords(new Rect(sX + 1, sY + 23, Mathf.Lerp(1f, sWidth, scleraColor.b), 5f), texSlideD, new Rect(sX + 1, sY + 23, sWidth, 5f), alphaBlend: true);
		}
		GUI.color = colorGrey * 2f;
		if (funcName == "scleraColorA" && texSlideD != null)
		{
			GUI.DrawTextureWithTexCoords(new Rect(sX + 1, sY + 23, Mathf.Lerp(1f, sWidth, scleraColor.a), 5f), texSlideD, new Rect(sX + 1, sY + 23, sWidth, 5f), alphaBlend: true);
		}
		GUI.color = new Color(1f, 1f, 1f, 0f);
		if (funcName == "lightDir")
		{
			lightDirection = GUI.HorizontalSlider(new Rect(sX - 4, sY + 19, sWidth + 17, 10f), lightDirection, 0f, 1f);
		}
		if (funcName == "lodLevel")
		{
			lodLevel = GUI.HorizontalSlider(new Rect(sX - 4, sY + 19, sWidth + 17, 10f), lodLevel, 0f, 1f);
		}
		if (funcName == "pupilDilation")
		{
			pupilDilation = GUI.HorizontalSlider(new Rect(sX - 4, sY + 19, sWidth + 17, 10f), pupilDilation, 0f, 1f);
		}
		if (funcName == "scleraSize")
		{
			scleraSize = GUI.HorizontalSlider(new Rect(sX - 4, sY + 19, sWidth + 17, 10f), scleraSize, 0f, 1f);
		}
		if (funcName == "irisSize")
		{
			irisSize = GUI.HorizontalSlider(new Rect(sX - 4, sY + 19, sWidth + 17, 10f), irisSize, 0f, 1f);
		}
		if (funcName == "irisTexture")
		{
			irisTextureF = GUI.HorizontalSlider(new Rect(sX - 4, sY + 19, sWidth + 17, 10f), irisTextureF, 0f, (float)irisTextures.Length - 1f);
		}
		if (funcName == "irisParallax")
		{
			parallaxAmt = GUI.HorizontalSlider(new Rect(sX - 4, sY + 19, sWidth + 17, 10f), parallaxAmt, 0f, 1f);
		}
		if (funcName == "irisColorR")
		{
			irisColor.r = GUI.HorizontalSlider(new Rect(sX - 4, sY + 19, sWidth + 17, 10f), irisColor.r, 0f, 1f);
		}
		if (funcName == "irisColorG")
		{
			irisColor.g = GUI.HorizontalSlider(new Rect(sX - 4, sY + 19, sWidth + 17, 10f), irisColor.g, 0f, 1f);
		}
		if (funcName == "irisColorB")
		{
			irisColor.b = GUI.HorizontalSlider(new Rect(sX - 4, sY + 19, sWidth + 17, 10f), irisColor.b, 0f, 1f);
		}
		if (funcName == "irisColorA")
		{
			irisColor.a = GUI.HorizontalSlider(new Rect(sX - 4, sY + 19, sWidth + 17, 10f), irisColor.a, 0f, 1f);
		}
		if (funcName == "scleraColorR")
		{
			scleraColor.r = GUI.HorizontalSlider(new Rect(sX - 4, sY + 19, sWidth + 17, 10f), scleraColor.r, 0f, 1f);
		}
		if (funcName == "scleraColorG")
		{
			scleraColor.g = GUI.HorizontalSlider(new Rect(sX - 4, sY + 19, sWidth + 17, 10f), scleraColor.g, 0f, 1f);
		}
		if (funcName == "scleraColorB")
		{
			scleraColor.b = GUI.HorizontalSlider(new Rect(sX - 4, sY + 19, sWidth + 17, 10f), scleraColor.b, 0f, 1f);
		}
		if (funcName == "scleraColorA")
		{
			scleraColor.a = GUI.HorizontalSlider(new Rect(sX - 4, sY + 19, sWidth + 17, 10f), scleraColor.a, 0f, 1f);
		}
	}
}
