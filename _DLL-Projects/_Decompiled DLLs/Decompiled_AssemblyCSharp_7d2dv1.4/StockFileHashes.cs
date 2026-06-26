using System.IO;
using System.Linq;

public class StockFileHashes
{
	[PublicizedFrom(EAccessModifier.Private)]
	public struct HashDef(string _filename, byte[] _hash)
	{
		public string filename = _filename;

		public byte[] hash = _hash;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static HashDef[] hashDefinitions = new HashDef[49]
	{
		new HashDef("Data/Config/archetypes.xml", new byte[16]
		{
			25, 51, 133, 174, 196, 148, 4, 198, 233, 159,
			173, 66, 98, 179, 201, 183
		}),
		new HashDef("Data/Config/biomes.xml", new byte[16]
		{
			233, 168, 34, 134, 171, 175, 132, 152, 133, 102,
			65, 203, 183, 92, 201, 56
		}),
		new HashDef("Data/Config/blockplaceholders.xml", new byte[16]
		{
			229, 97, 168, 78, 76, 85, 152, 24, 103, 84,
			12, 47, 163, 159, 217, 35
		}),
		new HashDef("Data/Config/blocks.xml", new byte[16]
		{
			28, 38, 192, 152, 239, 111, 102, 78, 131, 241,
			111, 174, 33, 157, 30, 56
		}),
		new HashDef("Data/Config/buffs.xml", new byte[16]
		{
			62, 219, 122, 112, 236, 173, 35, 60, 150, 202,
			196, 114, 164, 146, 73, 147
		}),
		new HashDef("Data/Config/challenges.xml", new byte[16]
		{
			130, 121, 237, 108, 135, 187, 112, 53, 142, 55,
			55, 123, 227, 151, 197, 79
		}),
		new HashDef("Data/Config/devicesgameprefs.xml", new byte[16]
		{
			116, 216, 164, 112, 212, 99, 189, 83, 67, 146,
			21, 153, 137, 190, 111, 105
		}),
		new HashDef("Data/Config/dialogs.xml", new byte[16]
		{
			35, 201, 149, 2, 241, 201, 196, 179, 84, 72,
			235, 78, 7, 182, 157, 9
		}),
		new HashDef("Data/Config/dmscontent.xml", new byte[16]
		{
			227, 228, 249, 130, 165, 104, 103, 226, 40, 122,
			227, 135, 209, 230, 224, 253
		}),
		new HashDef("Data/Config/entityclasses.xml", new byte[16]
		{
			26, 10, 82, 184, 96, 31, 113, 180, 216, 68,
			126, 88, 203, 235, 244, 80
		}),
		new HashDef("Data/Config/entitygroups.xml", new byte[16]
		{
			197, 68, 83, 231, 172, 193, 106, 221, 3, 63,
			62, 154, 9, 41, 25, 138
		}),
		new HashDef("Data/Config/events.xml", new byte[16]
		{
			182, 79, 153, 68, 89, 55, 110, 44, 144, 175,
			7, 100, 227, 247, 172, 249
		}),
		new HashDef("Data/Config/gameevents.xml", new byte[16]
		{
			39, 171, 114, 40, 124, 101, 190, 170, 98, 92,
			168, 255, 201, 122, 143, 25
		}),
		new HashDef("Data/Config/gamestages.xml", new byte[16]
		{
			24, 18, 11, 95, 92, 12, 211, 82, 184, 146,
			6, 163, 124, 88, 86, 69
		}),
		new HashDef("Data/Config/items.xml", new byte[16]
		{
			160, 2, 8, 41, 139, 196, 30, 127, 56, 151,
			214, 199, 111, 228, 39, 140
		}),
		new HashDef("Data/Config/item_modifiers.xml", new byte[16]
		{
			95, 242, 127, 114, 58, 68, 19, 198, 48, 138,
			180, 122, 153, 174, 26, 180
		}),
		new HashDef("Data/Config/loadingscreen.xml", new byte[16]
		{
			152, 75, 81, 54, 100, 18, 199, 72, 63, 121,
			192, 76, 70, 38, 222, 87
		}),
		new HashDef("Data/Config/loot.xml", new byte[16]
		{
			19, 35, 211, 146, 251, 191, 227, 149, 30, 47,
			26, 38, 121, 33, 249, 56
		}),
		new HashDef("Data/Config/materials.xml", new byte[16]
		{
			131, 72, 242, 29, 26, 222, 221, 51, 147, 71,
			102, 55, 140, 17, 153, 234
		}),
		new HashDef("Data/Config/misc.xml", new byte[16]
		{
			6, 30, 64, 210, 57, 24, 111, 218, 220, 196,
			79, 71, 82, 100, 147, 202
		}),
		new HashDef("Data/Config/music.xml", new byte[16]
		{
			217, 163, 53, 240, 52, 17, 162, 81, 253, 105,
			18, 110, 145, 228, 245, 66
		}),
		new HashDef("Data/Config/nav_objects.xml", new byte[16]
		{
			168, 62, 237, 11, 204, 81, 35, 31, 155, 12,
			81, 165, 107, 229, 172, 190
		}),
		new HashDef("Data/Config/npc.xml", new byte[16]
		{
			230, 87, 31, 138, 202, 206, 185, 148, 242, 155,
			157, 160, 248, 162, 39, 216
		}),
		new HashDef("Data/Config/painting.xml", new byte[16]
		{
			231, 210, 164, 172, 55, 227, 69, 43, 94, 172,
			110, 155, 66, 192, 149, 221
		}),
		new HashDef("Data/Config/physicsbodies.xml", new byte[16]
		{
			177, 133, 161, 23, 111, 175, 24, 168, 56, 192,
			212, 201, 206, 240, 191, 49
		}),
		new HashDef("Data/Config/progression.xml", new byte[16]
		{
			232, 50, 54, 14, 38, 233, 82, 32, 222, 108,
			231, 133, 218, 214, 69, 55
		}),
		new HashDef("Data/Config/qualityinfo.xml", new byte[16]
		{
			245, 42, 38, 133, 161, 89, 249, 251, 119, 10,
			19, 138, 15, 146, 239, 133
		}),
		new HashDef("Data/Config/quests.xml", new byte[16]
		{
			114, 168, 34, 42, 27, 234, 140, 188, 58, 71,
			202, 13, 60, 239, 135, 99
		}),
		new HashDef("Data/Config/recipes.xml", new byte[16]
		{
			174, 151, 2, 207, 80, 63, 115, 107, 219, 240,
			21, 179, 239, 239, 112, 12
		}),
		new HashDef("Data/Config/rwgmixer.xml", new byte[16]
		{
			6, 128, 38, 68, 91, 157, 142, 186, 114, 107,
			201, 205, 140, 34, 30, 232
		}),
		new HashDef("Data/Config/shapes.xml", new byte[16]
		{
			175, 69, 48, 250, 11, 156, 189, 114, 42, 237,
			113, 56, 95, 190, 128, 252
		}),
		new HashDef("Data/Config/sounds.xml", new byte[16]
		{
			222, 215, 174, 243, 90, 138, 227, 151, 181, 234,
			200, 141, 50, 13, 4, 146
		}),
		new HashDef("Data/Config/spawning.xml", new byte[16]
		{
			129, 80, 132, 43, 189, 243, 188, 49, 128, 55,
			157, 55, 73, 52, 156, 121
		}),
		new HashDef("Data/Config/subtitles.xml", new byte[16]
		{
			4, 56, 213, 81, 5, 204, 124, 203, 30, 73,
			149, 245, 157, 19, 225, 150
		}),
		new HashDef("Data/Config/traders.xml", new byte[16]
		{
			182, 177, 47, 46, 15, 192, 20, 109, 130, 168,
			53, 68, 61, 131, 3, 253
		}),
		new HashDef("Data/Config/twitch.xml", new byte[16]
		{
			43, 127, 117, 71, 21, 233, 78, 154, 232, 74,
			172, 36, 78, 148, 201, 229
		}),
		new HashDef("Data/Config/twitch_events.xml", new byte[16]
		{
			237, 183, 148, 133, 226, 153, 210, 134, 94, 227,
			141, 252, 133, 67, 69, 181
		}),
		new HashDef("Data/Config/ui_display.xml", new byte[16]
		{
			83, 173, 175, 239, 82, 51, 198, 129, 228, 31,
			33, 244, 166, 0, 124, 238
		}),
		new HashDef("Data/Config/utilityai.xml", new byte[16]
		{
			148, 66, 30, 155, 79, 240, 189, 89, 140, 199,
			156, 38, 31, 46, 220, 226
		}),
		new HashDef("Data/Config/vehicles.xml", new byte[16]
		{
			185, 57, 37, 203, 102, 122, 192, 239, 5, 150,
			210, 211, 143, 121, 108, 45
		}),
		new HashDef("Data/Config/videos.xml", new byte[16]
		{
			178, 87, 79, 101, 15, 121, 124, 227, 47, 212,
			249, 178, 188, 149, 13, 90
		}),
		new HashDef("Data/Config/weathersurvival.xml", new byte[16]
		{
			231, 248, 75, 252, 204, 81, 44, 189, 206, 14,
			186, 174, 39, 115, 216, 204
		}),
		new HashDef("Data/Config/worldglobal.xml", new byte[16]
		{
			32, 198, 191, 36, 139, 180, 31, 150, 164, 194,
			154, 121, 141, 160, 221, 130
		}),
		new HashDef("Data/Config/XUi/controls.xml", new byte[16]
		{
			207, 55, 195, 130, 86, 136, 123, 109, 167, 245,
			171, 72, 210, 253, 110, 51
		}),
		new HashDef("Data/Config/XUi/styles.xml", new byte[16]
		{
			85, 175, 136, 53, 223, 110, 185, 11, 201, 158,
			162, 237, 134, 74, 144, 186
		}),
		new HashDef("Data/Config/XUi/windows.xml", new byte[16]
		{
			170, 193, 39, 244, 248, 149, 254, 189, 53, 143,
			237, 244, 251, 160, 173, 196
		}),
		new HashDef("Data/Config/XUi/xui.xml", new byte[16]
		{
			38, 94, 70, 242, 245, 44, 44, 96, 181, 123,
			69, 244, 8, 121, 253, 221
		}),
		new HashDef("Data/Config/XUi_Common/controls.xml", new byte[16]
		{
			107, 19, 156, 117, 189, 156, 60, 123, 89, 183,
			158, 19, 144, 166, 42, 112
		}),
		new HashDef("Data/Config/XUi_Common/styles.xml", new byte[16]
		{
			246, 225, 86, 105, 127, 10, 172, 191, 106, 206,
			141, 254, 115, 145, 41, 159
		})
	};

	public static bool HasStockXMLs()
	{
		string applicationPath = GameIO.GetApplicationPath();
		bool result = true;
		HashDef[] array = hashDefinitions;
		for (int i = 0; i < array.Length; i++)
		{
			HashDef hashDef = array[i];
			if (File.Exists(applicationPath + "/" + hashDef.filename) && !IOUtils.CalcHashSync(applicationPath + "/" + hashDef.filename).SequenceEqual(hashDef.hash))
			{
				Log.Out("Wrong hash on " + hashDef.filename);
				result = false;
			}
		}
		return result;
	}
}
