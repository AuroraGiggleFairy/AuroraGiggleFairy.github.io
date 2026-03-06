using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class NetPackageMetrics
{
	[PublicizedFrom(EAccessModifier.Private)]
	public static NetPackageMetrics _instance;

	[PublicizedFrom(EAccessModifier.Private)]
	public float updateLength = 5f;

	[PublicizedFrom(EAccessModifier.Private)]
	public float lastUpdateTime;

	[PublicizedFrom(EAccessModifier.Private)]
	public string lastStatsOutput = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<string, int> receivedNetPackageCounts;

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<string, List<int>> receivedNetPackageSizes;

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<string, int> sentNetPackageCounts;

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<string, List<int>> sentNetPackageSizes;

	[PublicizedFrom(EAccessModifier.Private)]
	public int playerCount;

	[PublicizedFrom(EAccessModifier.Private)]
	public int entityAliveCount;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool active;

	public string clientCSV = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public static int tick;

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<string, List<PackagesSentInfoEntry>> packagesSent;

	public bool includeRelPosRot = true;

	public static NetPackageMetrics Instance => _instance;

	public NetPackageMetrics()
	{
		_instance = this;
		lastUpdateTime = Time.time - updateLength;
		receivedNetPackageCounts = new Dictionary<string, int>();
		receivedNetPackageSizes = new Dictionary<string, List<int>>();
		sentNetPackageCounts = new Dictionary<string, int>();
		sentNetPackageSizes = new Dictionary<string, List<int>>();
		packagesSent = new Dictionary<string, List<PackagesSentInfoEntry>>();
	}

	public void ResetStats()
	{
		SingletonMonoBehaviour<ConnectionManager>.Instance.ResetNetworkStatistics();
		receivedNetPackageCounts.Clear();
		sentNetPackageCounts.Clear();
		sentNetPackageSizes = new Dictionary<string, List<int>>();
		receivedNetPackageSizes = new Dictionary<string, List<int>>();
		packagesSent = new Dictionary<string, List<PackagesSentInfoEntry>>();
		tick = 0;
		Log.Out("Network stats reset");
	}

	public void SetUpdateLength(float length)
	{
		Log.Out("Setting network stat length to " + length + " seconds");
		updateLength = length;
		lastUpdateTime = Time.time;
	}

	public void RestartTimer()
	{
		lastUpdateTime = Time.time;
	}

	public void RecordForPeriod(float length)
	{
		active = true;
		ResetStats();
		SetUpdateLength(length);
		RestartTimer();
		SingletonMonoBehaviour<ConnectionManager>.Instance.EnableNetworkStatistics();
		entityAliveCount = Object.FindObjectsOfType<EntityAlive>().Length;
		playerCount = Object.FindObjectsOfType<EntityPlayer>().Length;
	}

	public void CopyToClipboard()
	{
		TextEditor textEditor = new TextEditor();
		textEditor.text = lastStatsOutput;
		textEditor.SelectAll();
		textEditor.Copy();
	}

	public void CopyToCSV(bool includeDetails = false)
	{
		TextEditor textEditor = new TextEditor();
		textEditor.text = ProduceCSV(includeDetails);
		textEditor.SelectAll();
		textEditor.Copy();
	}

	public void RegisterReceivedPackage(string packageType, int length)
	{
		if (active)
		{
			if (receivedNetPackageCounts.ContainsKey(packageType))
			{
				receivedNetPackageCounts[packageType]++;
			}
			else
			{
				receivedNetPackageCounts[packageType] = 1;
				receivedNetPackageSizes[packageType] = new List<int>();
			}
			receivedNetPackageSizes[packageType].Add(length);
		}
	}

	public void RegisterSentPackage(string packageType, int length)
	{
		if (active)
		{
			if (sentNetPackageCounts.ContainsKey(packageType))
			{
				sentNetPackageCounts[packageType]++;
			}
			else
			{
				sentNetPackageCounts[packageType] = 1;
				sentNetPackageSizes[packageType] = new List<int>();
			}
			sentNetPackageSizes[packageType].Add(length);
		}
	}

	public void RegisterPackagesSent(List<NetPackageInfo> packages, int count, long uncompressedSize, long compressedSize, float timeStamp, string client)
	{
		if (active)
		{
			PackagesSentInfoEntry item = new PackagesSentInfoEntry
			{
				packages = new List<NetPackageInfo>(packages),
				count = count,
				bCompressed = (compressedSize != -1),
				uncompressedSize = uncompressedSize,
				compressedSize = ((compressedSize == -1) ? uncompressedSize : compressedSize),
				timestamp = timeStamp,
				client = client
			};
			if (!packagesSent.ContainsKey(client))
			{
				packagesSent[client] = new List<PackagesSentInfoEntry>();
			}
			packagesSent[client].Add(item);
		}
	}

	public string ProduceCSV(bool includeDetails = false)
	{
		string text = "";
		int num = 0;
		int num2 = 0;
		text += "NET METRICS";
		text = text + "\nPrioritization Enabled: " + GameManager.enableNetworkdPrioritization;
		EntityPlayerLocal primaryPlayer = GameManager.Instance.World.GetPrimaryPlayer();
		if ((bool)primaryPlayer)
		{
			text = text + "\nPlayer name: " + primaryPlayer.EntityName;
		}
		text += "\n\nSent NetPackages:";
		text += "\npackage type,count,total length, average length";
		foreach (KeyValuePair<string, int> sentNetPackageCount in sentNetPackageCounts)
		{
			num2 += sentNetPackageCount.Value;
			float num3 = 0f;
			foreach (int item in sentNetPackageSizes[sentNetPackageCount.Key])
			{
				num3 += (float)item;
			}
			float num4 = num3 / (float)sentNetPackageCount.Value;
			text = text + "\n" + sentNetPackageCount.Key + "," + sentNetPackageCount.Value + "," + num3 + "," + num4;
		}
		text += "\n\nReceived NetPackages:";
		text += "\npackage type,count,total length, average length";
		foreach (KeyValuePair<string, int> receivedNetPackageCount in receivedNetPackageCounts)
		{
			num += receivedNetPackageCount.Value;
			float num3 = 0f;
			foreach (int item2 in receivedNetPackageSizes[receivedNetPackageCount.Key])
			{
				num3 += (float)item2;
			}
			float num4 = num3 / (float)receivedNetPackageCount.Value;
			text = text + "\n" + receivedNetPackageCount.Key + "," + receivedNetPackageCount.Value + "," + num3 + "," + num4;
		}
		text += "\n\n\nTotals";
		text = text + "\nPackages Sent: " + num2;
		text = text + "\nPackages Received: " + num;
		text = text + "\n\nPlayers: " + playerCount;
		text = text + "\n\nEntityAlive Count: " + entityAliveCount;
		foreach (KeyValuePair<string, List<PackagesSentInfoEntry>> item3 in packagesSent)
		{
			new List<PackagesSentInfoEntry>();
			text = text + "\n Packages sent for client " + item3.Key + "\n\n";
			text += "\nmilliseconds,count,uncompressed size, compressed size";
			foreach (PackagesSentInfoEntry item4 in item3.Value)
			{
				if (includeRelPosRot)
				{
					text = text + "\n" + item4.timestamp + "," + item4.count + "," + item4.uncompressedSize + "," + item4.compressedSize;
				}
				else
				{
					int num5 = 0;
					int num6 = 0;
					foreach (NetPackageInfo package in item4.packages)
					{
						if (!(package.netPackageType == "NetPackageEntityRelPosAndRot"))
						{
							num5++;
							num6 += package.length;
						}
					}
					text = text + "\n" + item4.timestamp + "," + num5 + "," + num6;
				}
				if (!includeDetails)
				{
					continue;
				}
				text += "\n,package type,package length";
				foreach (NetPackageInfo package2 in item4.packages)
				{
					if (includeRelPosRot || !(package2.netPackageType == "NetPackageEntityRelPosAndRot"))
					{
						text = text + "\n," + package2.netPackageType + "," + package2.length;
					}
				}
			}
		}
		return text;
	}

	public void Update()
	{
		if (!active || !(Time.time - lastUpdateTime >= updateLength))
		{
			return;
		}
		lastStatsOutput = SingletonMonoBehaviour<ConnectionManager>.Instance.PrintNetworkStatistics();
		int num = 0;
		int num2 = 0;
		lastStatsOutput += "NET METRICS";
		EntityPlayerLocal primaryPlayer = GameManager.Instance.World.GetPrimaryPlayer();
		if ((bool)primaryPlayer)
		{
			lastStatsOutput = lastStatsOutput + "\nPlayer name: " + primaryPlayer.EntityName;
		}
		lastStatsOutput += "\n\nSent NetPackages:";
		foreach (KeyValuePair<string, int> sentNetPackageCount in sentNetPackageCounts)
		{
			num2 += sentNetPackageCount.Value;
			float num3 = 0f;
			foreach (int item in sentNetPackageSizes[sentNetPackageCount.Key])
			{
				num3 += (float)item;
			}
			float num4 = num3 / (float)sentNetPackageCount.Value;
			lastStatsOutput = lastStatsOutput + "\n" + sentNetPackageCount.Key + ": Count:" + sentNetPackageCount.Value + " Total Size: " + num3 + " Avg Size: " + num4;
		}
		lastStatsOutput += "\n\nReceived NetPackages:";
		foreach (KeyValuePair<string, int> receivedNetPackageCount in receivedNetPackageCounts)
		{
			num += receivedNetPackageCount.Value;
			float num3 = 0f;
			foreach (int item2 in receivedNetPackageSizes[receivedNetPackageCount.Key])
			{
				num3 += (float)item2;
			}
			float num4 = num3 / (float)receivedNetPackageCount.Value;
			lastStatsOutput = lastStatsOutput + "\n" + receivedNetPackageCount.Key + ": Count:" + receivedNetPackageCount.Value + " Total Size: " + num3 + " Avg Size: " + num4;
		}
		lastStatsOutput += "\n\n\nTotals";
		lastStatsOutput = lastStatsOutput + "\nPackages Sent: " + num2;
		lastStatsOutput = lastStatsOutput + "\nPackages Received: " + num;
		lastStatsOutput = lastStatsOutput + "\n\nPlayers: " + playerCount;
		lastStatsOutput = lastStatsOutput + "\n\nEntityAlive Count: " + entityAliveCount;
		Log.Out(lastStatsOutput);
		string csv = ProduceCSV();
		lastUpdateTime = Time.time;
		active = false;
		if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			NetPackageNetMetrics package = NetPackageNetMetrics.SetupClient(lastStatsOutput, csv);
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(package);
		}
	}

	public void OutputPackageSentDetails(string client, float timestamp)
	{
		if (packagesSent.TryGetValue(client, out var value))
		{
			PackagesSentInfoEntry packagesSentInfoEntry = value.Find([PublicizedFrom(EAccessModifier.Internal)] (PackagesSentInfoEntry x) => x.timestamp == timestamp);
			if (packagesSentInfoEntry != null)
			{
				string text = "Packages for " + client + " at timestamp " + timestamp + "\n\n";
				text += GetOutputPacakageSentDetails(packagesSentInfoEntry);
				Log.Out(text);
				TextEditor textEditor = new TextEditor();
				textEditor.text = text;
				textEditor.SelectAll();
				textEditor.Copy();
				Log.Out("Copied to clipboard");
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public string GetOutputPacakageSentDetails(PackagesSentInfoEntry entry)
	{
		string text = "\nPackage Type, Length";
		foreach (NetPackageInfo package in entry.packages)
		{
			text = text + "\n" + package.netPackageType + "," + package.length;
		}
		return text;
	}

	public void AppendClientCSV(string csv)
	{
		clientCSV = clientCSV + "\n\n" + csv;
	}
}
