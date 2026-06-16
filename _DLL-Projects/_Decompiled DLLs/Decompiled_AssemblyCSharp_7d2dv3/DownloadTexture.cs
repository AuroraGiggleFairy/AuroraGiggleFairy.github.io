using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

[RequireComponent(typeof(UITexture))]
public class DownloadTexture : MonoBehaviour
{
	public string url = "http://www.yourwebsite.com/logo.png";

	public bool pixelPerfect = true;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Texture2D mTex;

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator Start()
	{
		UnityWebRequest www = UnityWebRequest.Get(url);
		yield return www.SendWebRequest();
		mTex = DownloadHandlerTexture.GetContent(www);
		if (mTex != null)
		{
			UITexture component = GetComponent<UITexture>();
			component.mainTexture = mTex;
			if (pixelPerfect)
			{
				component.MakePixelPerfect();
			}
		}
		www.Dispose();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnDestroy()
	{
		if (mTex != null)
		{
			UnityEngine.Object.Destroy(mTex);
		}
	}
}
