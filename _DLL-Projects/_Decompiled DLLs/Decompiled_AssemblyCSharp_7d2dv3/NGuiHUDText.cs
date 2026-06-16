using System;
using System.Collections.Generic;
using UnityEngine;

public class NGuiHUDText : MonoBehaviour
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public class Entry
	{
		public float time;

		public float stay;

		public Vector3 intialOffset;

		public float curveOffset;

		public float val;

		public UILabel label;

		public UISprite sprite;

		public bool isLabel;

		public float movementStart => time + stay;
	}

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const string uiFontName = "ReferenceFont";

	[HideInInspector]
	[SerializeField]
	[PublicizedFrom(EAccessModifier.Private)]
	public INGUIFont font;

	public int fontSize = 20;

	public FontStyle fontStyle;

	public bool applyGradient;

	public Color gradientTop = Color.white;

	public Color gradienBottom = new Color(0.7f, 0.7f, 0.7f);

	public UILabel.Effect effect;

	public Color effectColor = Color.black;

	public bool verticalStack;

	public AnimationCurve offsetCurve = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(3f, 40f));

	public AnimationCurve alphaCurve = new AnimationCurve(new Keyframe(1f, 1f), new Keyframe(3f, 0f));

	public AnimationCurve scaleCurve = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(0.25f, 1f));

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public List<Entry> mList = new List<Entry>();

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public List<Entry> mUnused = new List<Entry>();

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public int counter;

	public bool isVisible => mList.Count != 0;

	public INGUIFont ambigiousFont
	{
		get
		{
			return font;
		}
		set
		{
			font = value;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static int Comparison(Entry a, Entry b)
	{
		if (a.movementStart < b.movementStart)
		{
			return -1;
		}
		if (a.movementStart > b.movementStart)
		{
			return 1;
		}
		return 0;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Entry Create()
	{
		if (mUnused.Count > 0)
		{
			Entry entry = mUnused[mUnused.Count - 1];
			mUnused.RemoveAt(mUnused.Count - 1);
			entry.time = Time.realtimeSinceStartup;
			entry.label.depth = NGUITools.CalculateNextDepth(base.gameObject);
			NGUITools.SetActive(entry.label.gameObject, state: true);
			entry.intialOffset = default(Vector3);
			entry.curveOffset = 0f;
			mList.Add(entry);
			return entry;
		}
		Entry entry2 = new Entry();
		entry2.time = Time.realtimeSinceStartup;
		entry2.label = base.gameObject.AddWidget<UILabel>();
		entry2.label.name = $"Entry_{counter}_Label";
		entry2.label.font = ambigiousFont;
		entry2.label.fontSize = fontSize;
		entry2.label.fontStyle = fontStyle;
		entry2.label.applyGradient = applyGradient;
		entry2.label.gradientTop = gradientTop;
		entry2.label.gradientBottom = gradienBottom;
		entry2.label.effectStyle = effect;
		entry2.label.effectColor = effectColor;
		entry2.label.overflowMethod = UILabel.Overflow.ResizeFreely;
		entry2.label.cachedTransform.localScale = new Vector3(0.001f, 0.001f, 0.001f);
		entry2.isLabel = true;
		entry2.sprite = base.gameObject.AddWidget<UISprite>();
		entry2.sprite.name = $"Entry_{counter}_Sprite";
		entry2.sprite.keepAspectRatio = UIWidget.AspectRatioSource.BasedOnHeight;
		mList.Add(entry2);
		counter++;
		return entry2;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Delete(Entry ent)
	{
		mList.Remove(ent);
		mUnused.Add(ent);
		NGUITools.SetActive(ent.label.gameObject, state: false);
	}

	public void Add(object obj, Color c, float stayDuration)
	{
		if (!base.enabled)
		{
			return;
		}
		float realtimeSinceStartup = Time.realtimeSinceStartup;
		bool flag = false;
		float num = 0f;
		if (obj is float)
		{
			flag = true;
			num = (float)obj;
		}
		else if (obj is int)
		{
			flag = true;
			num = (int)obj;
		}
		if (flag)
		{
			if (num == 0f)
			{
				return;
			}
			int num2 = mList.Count;
			while (num2 > 0)
			{
				Entry entry = mList[--num2];
				if (!(entry.time + 1f < realtimeSinceStartup) && entry.val != 0f)
				{
					if (entry.val < 0f && num < 0f)
					{
						entry.val += num;
						entry.label.text = Mathf.RoundToInt(entry.val).ToString();
						return;
					}
					if (entry.val > 0f && num > 0f)
					{
						entry.val += num;
						entry.label.text = "+" + Mathf.RoundToInt(entry.val);
						return;
					}
				}
			}
		}
		Entry entry2 = Create();
		entry2.stay = stayDuration;
		entry2.label.color = c;
		entry2.label.alpha = 0f;
		entry2.sprite.color = c;
		entry2.val = num;
		if (flag)
		{
			entry2.label.text = ((num < 0f) ? Mathf.RoundToInt(entry2.val).ToString() : ("+" + Mathf.RoundToInt(entry2.val)));
		}
		else
		{
			entry2.label.text = obj.ToString();
		}
		mList.Sort(Comparison);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void OnEnable()
	{
		if (ambigiousFont == null)
		{
			NGUIFont[] array = Resources.FindObjectsOfTypeAll<NGUIFont>();
			foreach (NGUIFont nGUIFont in array)
			{
				if (nGUIFont.name.EqualsCaseInsensitive("ReferenceFont"))
				{
					ambigiousFont = nGUIFont;
					break;
				}
			}
			if (ambigiousFont == null)
			{
				Log.Error("NGuiHUDText font not found");
			}
		}
		fontStyle = font.dynamicFontStyle;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void OnValidate()
	{
		INGUIFont iNGUIFont = ambigiousFont;
		if (iNGUIFont != null && iNGUIFont.isDynamic)
		{
			fontStyle = iNGUIFont.dynamicFontStyle;
			fontSize = iNGUIFont.defaultSize;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void OnDisable()
	{
		int num = mList.Count;
		while (num > 0)
		{
			Entry entry = mList[--num];
			if (entry.label != null)
			{
				entry.label.enabled = false;
			}
			else
			{
				mList.RemoveAt(num);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void Update()
	{
		float time = RealTime.time;
		Keyframe[] keys = offsetCurve.keys;
		Keyframe[] keys2 = alphaCurve.keys;
		Keyframe[] keys3 = scaleCurve.keys;
		float time2 = keys[^1].time;
		float time3 = keys2[^1].time;
		float num = Mathf.Max(keys3[^1].time, Mathf.Max(time2, time3));
		int num2 = mList.Count;
		while (num2 > 0)
		{
			Entry entry = mList[--num2];
			float num3 = time - entry.movementStart;
			entry.curveOffset = offsetCurve.Evaluate(num3);
			entry.label.alpha = alphaCurve.Evaluate(num3);
			float num4 = scaleCurve.Evaluate(time - entry.time);
			if (num4 < 0.001f)
			{
				num4 = 0.001f;
			}
			entry.label.cachedTransform.localScale = new Vector3(num4, num4, num4);
			if (num3 > num)
			{
				Delete(entry);
			}
			else
			{
				entry.label.enabled = true;
			}
		}
		float num5 = 0f;
		float num6 = 0f;
		for (int i = 0; i < mList.Count; i++)
		{
			Entry entry2 = mList[i];
			if (verticalStack)
			{
				num6 += (float)(entry2.isLabel ? entry2.label.height : entry2.sprite.height);
				num5 = Mathf.Max(num5, entry2.isLabel ? entry2.label.width : entry2.sprite.width);
			}
			else
			{
				num5 += (float)(entry2.isLabel ? entry2.label.width : entry2.sprite.width);
				num6 = Mathf.Max(num6, entry2.isLabel ? entry2.label.height : entry2.sprite.height);
			}
		}
		if (verticalStack)
		{
			float a = 0f;
			for (int j = 0; j < mList.Count; j++)
			{
				Entry entry3 = mList[j];
				a = Mathf.Max(a, entry3.curveOffset);
				if (entry3.isLabel)
				{
					entry3.label.cachedTransform.localPosition = new Vector3(0f, a, 0f) + entry3.intialOffset * num6;
					a += Mathf.Round(entry3.label.cachedTransform.localScale.y * (float)entry3.label.fontSize);
				}
				else
				{
					entry3.sprite.cachedTransform.localPosition = new Vector3(0f, a, 0f) + entry3.intialOffset * num6;
					a += Mathf.Round(entry3.sprite.cachedTransform.localScale.y * (float)entry3.sprite.height);
				}
			}
			return;
		}
		float num7 = 0f;
		for (int k = 0; k < mList.Count; k++)
		{
			Entry entry4 = mList[k];
			if (entry4.isLabel)
			{
				entry4.label.cachedTransform.localPosition = new Vector3(num7 + ((float)entry4.label.width - num5) / 2f, entry4.curveOffset, 0f) + entry4.intialOffset * num6;
				num7 += (float)entry4.label.width;
			}
			else
			{
				entry4.sprite.cachedTransform.localPosition = new Vector3(num7 + ((float)entry4.sprite.width - num5) / 2f, entry4.curveOffset, 0f) + entry4.intialOffset * num6;
				num7 += (float)entry4.sprite.width;
			}
		}
	}

	public void SetEntry(int _index, string _input, bool _isSprite, INGUIAtlas _spriteAtlas = null)
	{
		if (mList.Count > _index)
		{
			mList[_index].isLabel = !_isSprite;
			if (!_isSprite)
			{
				mList[_index].label.text = _input;
				mList[_index].sprite.atlas = null;
				mList[_index].sprite.spriteName = string.Empty;
			}
			else
			{
				mList[_index].label.text = string.Empty;
				mList[_index].sprite.atlas = (string.IsNullOrEmpty(_input) ? null : _spriteAtlas);
				mList[_index].sprite.spriteName = _input;
			}
		}
	}

	public void SetEntrySize(int _index, int _size)
	{
		mList[_index].label.fontSize = _size;
		mList[_index].sprite.height = _size;
	}

	public void SetEntryOffset(int _index, Vector3 _offset)
	{
		mList[_index].intialOffset = _offset;
	}

	public void SetEntryColor(int _index, Color _c)
	{
		mList[_index].label.color = _c;
		mList[_index].sprite.color = _c;
	}
}
