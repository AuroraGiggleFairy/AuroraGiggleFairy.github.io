using UnityEngine;

public class CharacterClickHandler : MonoBehaviour
{
	public CharacterConstruct parentScript;

	public void HandleClick()
	{
		parentScript.OnCharacterClicked(base.gameObject);
	}
}
