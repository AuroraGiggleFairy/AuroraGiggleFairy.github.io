using System;
using UnityEngine;

public class QuestGeneratorController : MonoBehaviour
{
	public enum GeneratorStates
	{
		OnNoQuest,
		Off,
		RebootState,
		EnteringOnState,
		On
	}

	public Light MainLight;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public GeneratorStates currentState;

	public GeneratorStates TestingCurrentState = GeneratorStates.Off;

	public GameObject OffState;

	public GameObject RebootState;

	public GameObject EnteringOnState;

	public GameObject OnState;

	public bool IsRunning => currentState == GeneratorStates.On;

	public void SetGeneratorState(GeneratorStates state, bool isInit)
	{
		if (state != currentState)
		{
			currentState = state;
			updateStateDisplay();
			if (!isInit)
			{
				PrefabInstance.RefreshTriggersInContainingPoi(base.transform.position + Origin.position);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateStateDisplay()
	{
		switch (currentState)
		{
		case GeneratorStates.OnNoQuest:
			OffState.SetActive(value: false);
			RebootState.SetActive(value: false);
			EnteringOnState.SetActive(value: false);
			OnState.SetActive(value: true);
			break;
		case GeneratorStates.Off:
			OffState.SetActive(value: true);
			RebootState.SetActive(value: false);
			EnteringOnState.SetActive(value: false);
			OnState.SetActive(value: false);
			break;
		case GeneratorStates.RebootState:
			OffState.SetActive(value: false);
			RebootState.SetActive(value: true);
			EnteringOnState.SetActive(value: false);
			OnState.SetActive(value: false);
			break;
		case GeneratorStates.EnteringOnState:
			OffState.SetActive(value: false);
			RebootState.SetActive(value: false);
			EnteringOnState.SetActive(value: true);
			OnState.SetActive(value: false);
			break;
		case GeneratorStates.On:
			OffState.SetActive(value: false);
			RebootState.SetActive(value: false);
			EnteringOnState.SetActive(value: false);
			OnState.SetActive(value: true);
			break;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Start()
	{
		updateStateDisplay();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Update()
	{
	}
}
