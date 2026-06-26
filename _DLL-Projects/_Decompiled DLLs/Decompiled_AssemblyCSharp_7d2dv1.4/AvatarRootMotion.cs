using System;
using UnityEngine;

public class AvatarRootMotion : MonoBehaviour
{
	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public AvatarController mainController;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Animator root;

	public void Init(AvatarController _mainController, Animator _root)
	{
		mainController = _mainController;
		root = _root;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void OnAnimatorMove()
	{
		if (mainController != null && root != null)
		{
			mainController.NotifyAnimatorMove(root);
		}
	}
}
