using System.Collections.Generic;
using UnityEngine;

public class GameGraphManager
{
	public class Graph
	{
		public delegate bool Callback(ref float value);

		public string name;

		public Callback callback;

		public string cvarName;

		public PassiveEffects passiveEffect;

		public string statName;

		public Vector2 pos;

		[PublicizedFrom(EAccessModifier.Private)]
		public GameGraphManager manager;

		[PublicizedFrom(EAccessModifier.Private)]
		public int count;

		[PublicizedFrom(EAccessModifier.Private)]
		public float maxValue;

		[PublicizedFrom(EAccessModifier.Private)]
		public float markerValue;

		[PublicizedFrom(EAccessModifier.Private)]
		public float[] values;

		[PublicizedFrom(EAccessModifier.Private)]
		public int index;

		public Graph(GameGraphManager _manager, string _name, int _count, float _maxValue, float _markerValue)
		{
			manager = _manager;
			name = _name;
			count = _count;
			count = Mathf.Clamp(count, 1, 4096);
			maxValue = _maxValue;
			markerValue = _markerValue;
			values = new float[count];
		}

		public void AddValue(float value)
		{
			index = (index + 1) % count;
			values[index] = value;
		}

		public void Draw()
		{
			Texture whiteTex = GameGraphManager.whiteTex;
			float width = (float)count * 2f + 2f;
			int graphHeight = manager.graphHeight;
			GUI.color = Color.white;
			GUI.DrawTexture(new Rect(pos.x, pos.y, width, graphHeight + 2), whiteTex, ScaleMode.StretchToFill, alphaBlend: false, 0f, new Color(0f, 0f, 0f, 0.9f), 0f, 0f);
			int num = index + 1;
			for (int i = 0; i < count; i++)
			{
				float num2 = values[num % count];
				num2 /= maxValue;
				if (num2 > 1f)
				{
					num2 = 1f;
				}
				float num3 = (float)graphHeight * num2;
				num3 = (int)(num3 + 0.5f);
				GUI.DrawTexture(color: new Color(1f, num2, num2 * 0.6f + 0.4f), position: new Rect(pos.x + 1f + (float)i * 2f, pos.y + 1f + (float)graphHeight - num3, 2f, num3), image: whiteTex, scaleMode: ScaleMode.StretchToFill, alphaBlend: false, imageAspect: 0f, borderWidth: 0f, borderRadius: 0f);
				num++;
			}
			if (markerValue > 0f)
			{
				GUI.DrawTexture(new Rect(pos.x, pos.y + (float)graphHeight - markerValue / maxValue * (float)graphHeight, width, 1f), whiteTex, ScaleMode.StretchToFill, alphaBlend: true, 0f, new Color(1f, 1f, 0f, 0.6f), 0f, 0f);
			}
			GUI.color = new Color(0.6f, 0.6f, 1f);
			GUI.Label(new Rect(pos.x + 1f, pos.y + 1f, 256f, 256f), $"{name} {values[index]}");
		}

		public void UpdateValues()
		{
			if (GameManager.Instance.World == null)
			{
				return;
			}
			EntityPlayerLocal player = manager.player;
			if (callback != null)
			{
				float value = values[index];
				if (callback(ref value))
				{
					AddValue(value);
				}
			}
			else if (!string.IsNullOrEmpty(cvarName))
			{
				float cVar = player.GetCVar(cvarName);
				if (cVar != values[index])
				{
					AddValue(cVar);
				}
			}
			else if (passiveEffect != PassiveEffects.None)
			{
				float value2 = EffectManager.GetValue(passiveEffect, null, 0f, player);
				if (value2 != values[index])
				{
					AddValue(value2);
				}
			}
			else if (!string.IsNullOrEmpty(statName))
			{
				float num = 0f;
				switch (statName)
				{
				case "health":
					num = player.PlayerStats.Health.Value;
					break;
				case "stamina":
					num = player.PlayerStats.Stamina.Value;
					break;
				case "coretemp":
					num = player.PlayerStats.CoreTemp;
					break;
				case "water":
					num = player.PlayerStats.Water.Value;
					break;
				}
				if (num != values[index])
				{
					AddValue(num);
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static Texture2D whiteTex;

	[PublicizedFrom(EAccessModifier.Private)]
	public EntityPlayerLocal player;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<Graph> graphs = new List<Graph>();

	[PublicizedFrom(EAccessModifier.Private)]
	public int graphHeight = 100;

	public static GameGraphManager Create(EntityPlayerLocal player)
	{
		GameGraphManager gameGraphManager = new GameGraphManager();
		gameGraphManager.player = player;
		gameGraphManager.Init();
		return gameGraphManager;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Init()
	{
		if (!whiteTex)
		{
			whiteTex = new Texture2D(1, 1);
			whiteTex.FillTexture(Color.white, _apply: true);
		}
	}

	public void Destroy()
	{
		if ((bool)whiteTex)
		{
			Object.Destroy(whiteTex);
		}
	}

	public void Add(string name, Graph.Callback callback, int sampleCount, float maxValue, float markerValue = 0f)
	{
		Graph graph = FindGraph(name);
		if (graph != null)
		{
			graphs.Remove(graph);
		}
		if (sampleCount > 0)
		{
			Graph graph2 = new Graph(this, name, sampleCount, maxValue, markerValue);
			graphs.Add(graph2);
			graph2.callback = callback;
		}
	}

	public void AddCVar(string name, int count, string cvarName, float maxValue, float markerValue = 0f)
	{
		Graph graph = FindGraph(name);
		if (graph != null)
		{
			graphs.Remove(graph);
		}
		if (count > 0)
		{
			Graph graph2 = new Graph(this, name, count, maxValue, markerValue);
			graphs.Add(graph2);
			graph2.cvarName = cvarName;
		}
	}

	public void AddPassiveEffect(string name, int count, PassiveEffects passiveEffect, float maxValue, float markerValue = 0f)
	{
		Graph graph = FindGraph(name);
		if (graph != null)
		{
			graphs.Remove(graph);
		}
		if (count > 0)
		{
			Graph graph2 = new Graph(this, name, count, maxValue, markerValue);
			graphs.Add(graph2);
			graph2.passiveEffect = passiveEffect;
		}
	}

	public void AddStat(string name, int count, string statName, float maxValue, float markerValue = 0f)
	{
		Graph graph = FindGraph(name);
		if (graph != null)
		{
			graphs.Remove(graph);
		}
		if (count > 0)
		{
			Graph graph2 = new Graph(this, name, count, maxValue, markerValue);
			graphs.Add(graph2);
			graph2.statName = statName.ToLower();
		}
	}

	public void RemoveAll()
	{
		graphs.Clear();
	}

	public Graph FindGraph(string name)
	{
		for (int i = 0; i < graphs.Count; i++)
		{
			Graph graph = graphs[i];
			if (graph.name == name)
			{
				return graph;
			}
		}
		return null;
	}

	public void Draw()
	{
		bool flag = Event.current.type == EventType.Repaint;
		float num = 1f;
		for (int i = 0; i < graphs.Count; i++)
		{
			Graph graph = graphs[i];
			if (flag)
			{
				graph.UpdateValues();
			}
			graph.pos.x = 2f;
			graph.pos.y = num;
			graph.Draw();
			num += (float)(graphHeight + 2);
		}
	}

	public void SetHeight(int _height)
	{
		graphHeight = _height;
		graphHeight = Mathf.Clamp(graphHeight, 1, 2100);
	}
}
