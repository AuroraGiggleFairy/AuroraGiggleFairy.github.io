using System;
using UnityEngine;

public class BlendshapeTestSceneTools : MonoBehaviour
{
	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Animator myAnim;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public AudioSource myAudio;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public int layerIndex;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public int maxLayers;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float turnRate = 200f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float targetLayerWeight;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float endLayerWeight;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public int currentAnim;

	public AudioClip[] audioClips;

	[PublicizedFrom(EAccessModifier.Private)]
	public void Start()
	{
		myAnim = GetComponent<Animator>();
		myAudio = GetComponent<AudioSource>();
		if (myAnim != null)
		{
			maxLayers = myAnim.layerCount;
			Debug.Log("Number of layers in controller is " + maxLayers);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Update()
	{
		if (Input.GetKeyUp(KeyCode.Space) && myAudio != null)
		{
			currentAnim++;
			if (currentAnim == maxLayers)
			{
				currentAnim = 0;
				for (int i = 1; i < maxLayers - 1; i++)
				{
					myAnim.SetLayerWeight(i, 0f);
				}
			}
			Debug.Log("Current Layer is: " + currentAnim);
			myAudio.clip = audioClips[currentAnim];
			myAudio.Play();
			myAnim.SetLayerWeight(currentAnim, 1f);
			myAnim.SetTrigger("RestartAnim");
		}
		if (Input.GetKey(KeyCode.A))
		{
			base.transform.Rotate(0f, turnRate * Time.deltaTime * -1f, 0f);
		}
		if (Input.GetKey(KeyCode.D))
		{
			base.transform.Rotate(0f, turnRate * Time.deltaTime, 0f);
		}
		Input.GetKeyUp(KeyCode.W);
		Input.GetKeyUp(KeyCode.S);
		Input.GetKeyUp(KeyCode.E);
	}
}
