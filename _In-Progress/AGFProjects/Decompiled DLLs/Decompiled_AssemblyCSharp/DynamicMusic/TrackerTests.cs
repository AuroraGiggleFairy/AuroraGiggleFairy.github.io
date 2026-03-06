using System.Collections;
using System.Collections.Generic;
using DynamicMusic.Factories;
using MusicUtils.Enums;
using UnityEngine;

namespace DynamicMusic;

public static class TrackerTests
{
	public static bool continueTest = false;

	public static bool isFinished = false;

	[PublicizedFrom(EAccessModifier.Private)]
	public static List<SectionType> sections = new List<SectionType>
	{
		SectionType.Exploration,
		SectionType.Suspense,
		SectionType.Combat
	};

	public static void Run(int num)
	{
		switch (num)
		{
		case 0:
			GameManager.Instance.StartCoroutine(MusicTimeTrackerTest());
			break;
		case 1:
			GameManager.Instance.StartCoroutine(DayTimeTrackerTest());
			break;
		case 2:
			GameManager.Instance.StartCoroutine(PlayerTrackerTest());
			break;
		case 3:
			GameManager.Instance.StartCoroutine(SelectorTest());
			break;
		case 4:
			GameManager.Instance.StartCoroutine(ConductorTest());
			break;
		case 5:
			GameManager.Instance.StartCoroutine(RealTimeConductorTest());
			break;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static IEnumerator MusicTimeTrackerTest()
	{
		isFinished = false;
		IMultiNotifiableFilter musicTimeTracker = Factory.CreateMusicTimeTracker();
		for (int i = 0; i < 2; i++)
		{
			musicTimeTracker.Notify(MusicActionType.Play);
			yield return new WaitForSeconds(30f);
			musicTimeTracker.Notify(MusicActionType.Pause);
			yield return new WaitForSeconds(30f);
			musicTimeTracker.Notify(MusicActionType.UnPause);
			yield return new WaitForSeconds(30f);
			musicTimeTracker.Notify(MusicActionType.Stop);
			yield return new WaitForSeconds(30f);
		}
		musicTimeTracker.Notify();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static IEnumerator DayTimeTrackerTest()
	{
		isFinished = false;
		List<SectionType> sectionTypes = new List<SectionType> { SectionType.Exploration };
		DayTimeTracker dtt = Factory.CreateDayTimeTracker();
		do
		{
			yield return new WaitUntil([PublicizedFrom(EAccessModifier.Internal)] () => continueTest);
			continueTest = false;
			dtt.Filter(sectionTypes);
		}
		while (!isFinished);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static IEnumerator PlayerTrackerTest()
	{
		isFinished = false;
		IFilter<SectionType> playerTracker = Factory.CreatePlayerTracker();
		while (!isFinished)
		{
			List<SectionType> list = new List<SectionType>(sections);
			playerTracker.Filter(list);
			yield return new WaitUntil([PublicizedFrom(EAccessModifier.Internal)] () => continueTest);
			continueTest = false;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static IEnumerator SelectorTest()
	{
		isFinished = false;
		ISectionSelector sectionSelector = Factory.CreateSectionSelector();
		while (!isFinished)
		{
			SectionType sectionType = sectionSelector.Select();
			Log.Out($"Selected Section: {sectionType}");
			yield return new WaitUntil([PublicizedFrom(EAccessModifier.Internal)] () => continueTest);
			continueTest = false;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static IEnumerator ConductorTest()
	{
		isFinished = false;
		Conductor c = Factory.CreateConductor();
		while (!isFinished)
		{
			yield return new WaitUntil([PublicizedFrom(EAccessModifier.Internal)] () => continueTest);
			c.Update();
			continueTest = false;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static IEnumerator RealTimeConductorTest()
	{
		isFinished = false;
		Conductor c = Factory.CreateConductor();
		while (!isFinished)
		{
			c.Update();
			yield return null;
		}
	}
}
