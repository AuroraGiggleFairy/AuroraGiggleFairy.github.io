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
			57, 186, 106, 3, 79, 73, 100, 32, 12, 53,
			149, 32, 29, 108, 250, 167
		}),
		new HashDef("Data/Config/biomes.xml", new byte[16]
		{
			179, 37, 63, 74, 192, 44, 65, 220, 12, 143,
			44, 143, 133, 116, 154, 40
		}),
		new HashDef("Data/Config/blockplaceholders.xml", new byte[16]
		{
			106, 30, 16, 146, 83, 121, 56, 35, 31, 47,
			98, 38, 184, 198, 109, 6
		}),
		new HashDef("Data/Config/blocks.xml", new byte[16]
		{
			22, 115, 42, 213, 242, 66, 4, 168, 114, 226,
			134, 198, 17, 94, 207, 135
		}),
		new HashDef("Data/Config/buffs.xml", new byte[16]
		{
			221, 144, 30, 97, 149, 249, 48, 188, 77, 214,
			155, 168, 244, 81, 38, 197
		}),
		new HashDef("Data/Config/challenges.xml", new byte[16]
		{
			29, 38, 84, 203, 166, 14, 96, 191, 180, 102,
			18, 182, 217, 83, 61, 199
		}),
		new HashDef("Data/Config/devicesgameprefs.xml", new byte[16]
		{
			116, 216, 164, 112, 212, 99, 189, 83, 67, 146,
			21, 153, 137, 190, 111, 105
		}),
		new HashDef("Data/Config/dialogs.xml", new byte[16]
		{
			24, 192, 100, 111, 189, 235, 206, 230, 200, 158,
			190, 35, 197, 123, 158, 113
		}),
		new HashDef("Data/Config/dmscontent.xml", new byte[16]
		{
			153, 174, 129, 219, 147, 241, 54, 229, 162, 237,
			90, 211, 95, 57, 151, 122
		}),
		new HashDef("Data/Config/entityclasses.xml", new byte[16]
		{
			41, 81, 95, 82, 63, 61, 122, 97, 219, 90,
			146, 3, 47, 4, 63, 180
		}),
		new HashDef("Data/Config/entitygroups.xml", new byte[16]
		{
			169, 5, 57, 83, 97, 102, 5, 220, 234, 3,
			25, 149, 246, 196, 231, 30
		}),
		new HashDef("Data/Config/events.xml", new byte[16]
		{
			17, 171, 87, 135, 18, 54, 222, 208, 101, 200,
			28, 221, 235, 153, 101, 78
		}),
		new HashDef("Data/Config/gameevents.xml", new byte[16]
		{
			147, 62, 119, 81, 180, 130, 132, 122, 61, 47,
			197, 191, 10, 136, 38, 119
		}),
		new HashDef("Data/Config/gamestages.xml", new byte[16]
		{
			191, 179, 5, 228, 34, 124, 98, 212, 7, 35,
			163, 14, 181, 182, 66, 237
		}),
		new HashDef("Data/Config/items.xml", new byte[16]
		{
			77, 43, 77, 26, 247, 245, 12, 50, 209, 57,
			13, 58, 14, 33, 188, 26
		}),
		new HashDef("Data/Config/item_modifiers.xml", new byte[16]
		{
			166, 71, 4, 137, 143, 225, 203, 98, 27, 224,
			161, 203, 204, 242, 20, 56
		}),
		new HashDef("Data/Config/loadingscreen.xml", new byte[16]
		{
			43, 232, 150, 144, 119, 63, 131, 68, 40, 149,
			24, 227, 48, 57, 135, 5
		}),
		new HashDef("Data/Config/loot.xml", new byte[16]
		{
			149, 113, 161, 42, 8, 143, 56, 170, 246, 91,
			8, 214, 81, 38, 181, 247
		}),
		new HashDef("Data/Config/materials.xml", new byte[16]
		{
			3, 73, 109, 207, 4, 144, 165, 178, 19, 99,
			249, 155, 203, 37, 89, 9
		}),
		new HashDef("Data/Config/misc.xml", new byte[16]
		{
			63, 202, 62, 14, 245, 43, 175, 116, 41, 40,
			32, 121, 175, 63, 78, 21
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
			154, 157, 88, 37, 56, 188, 74, 147, 78, 249,
			17, 60, 1, 234, 53, 133
		}),
		new HashDef("Data/Config/progression.xml", new byte[16]
		{
			234, 153, 50, 37, 211, 134, 44, 194, 180, 109,
			150, 36, 58, 106, 111, 225
		}),
		new HashDef("Data/Config/qualityinfo.xml", new byte[16]
		{
			245, 42, 38, 133, 161, 89, 249, 251, 119, 10,
			19, 138, 15, 146, 239, 133
		}),
		new HashDef("Data/Config/quests.xml", new byte[16]
		{
			103, 56, 25, 247, 228, 5, 84, 103, 94, 99,
			143, 150, 70, 120, 187, 71
		}),
		new HashDef("Data/Config/recipes.xml", new byte[16]
		{
			171, 202, 171, 131, 53, 139, 68, 14, 26, 74,
			182, 210, 23, 190, 16, 29
		}),
		new HashDef("Data/Config/rwgmixer.xml", new byte[16]
		{
			53, 246, 218, 126, 233, 19, 57, 16, 252, 98,
			255, 70, 60, 26, 9, 168
		}),
		new HashDef("Data/Config/shapes.xml", new byte[16]
		{
			218, 18, 107, 204, 133, 37, 79, 96, 157, 103,
			228, 55, 144, 198, 234, 121
		}),
		new HashDef("Data/Config/sounds.xml", new byte[16]
		{
			45, 42, 38, 84, 12, 22, 232, 10, 173, 134,
			10, 106, 34, 153, 76, 177
		}),
		new HashDef("Data/Config/spawning.xml", new byte[16]
		{
			100, 178, 46, 216, 97, 45, 54, 130, 207, 28,
			56, 97, 195, 53, 97, 59
		}),
		new HashDef("Data/Config/subtitles.xml", new byte[16]
		{
			206, 165, 216, 217, 253, 57, 165, 0, 46, 237,
			175, 49, 186, 11, 114, 5
		}),
		new HashDef("Data/Config/traders.xml", new byte[16]
		{
			154, 216, 117, 210, 219, 240, 85, 14, 175, 58,
			115, 149, 4, 90, 205, 44
		}),
		new HashDef("Data/Config/twitch.xml", new byte[16]
		{
			99, 200, 81, 52, 101, 139, 32, 23, 82, 81,
			196, 214, 135, 38, 3, 222
		}),
		new HashDef("Data/Config/twitch_events.xml", new byte[16]
		{
			237, 183, 148, 133, 226, 153, 210, 134, 94, 227,
			141, 252, 133, 67, 69, 181
		}),
		new HashDef("Data/Config/ui_display.xml", new byte[16]
		{
			39, 58, 121, 204, 65, 116, 125, 254, 253, 101,
			221, 189, 51, 123, 238, 222
		}),
		new HashDef("Data/Config/utilityai.xml", new byte[16]
		{
			148, 66, 30, 155, 79, 240, 189, 89, 140, 199,
			156, 38, 31, 46, 220, 226
		}),
		new HashDef("Data/Config/vehicles.xml", new byte[16]
		{
			217, 50, 2, 141, 91, 20, 41, 164, 78, 150,
			224, 29, 107, 218, 88, 49
		}),
		new HashDef("Data/Config/videos.xml", new byte[16]
		{
			178, 87, 79, 101, 15, 121, 124, 227, 47, 212,
			249, 178, 188, 149, 13, 90
		}),
		new HashDef("Data/Config/weathersurvival.xml", new byte[16]
		{
			225, 141, 239, 191, 136, 42, 57, 110, 191, 53,
			140, 74, 145, 172, 40, 161
		}),
		new HashDef("Data/Config/worldglobal.xml", new byte[16]
		{
			32, 198, 191, 36, 139, 180, 31, 150, 164, 194,
			154, 121, 141, 160, 221, 130
		}),
		new HashDef("Data/Config/XUi/controls.xml", new byte[16]
		{
			195, 182, 156, 85, 132, 136, 246, 84, 156, 37,
			75, 250, 140, 45, 54, 140
		}),
		new HashDef("Data/Config/XUi/styles.xml", new byte[16]
		{
			85, 175, 136, 53, 223, 110, 185, 11, 201, 158,
			162, 237, 134, 74, 144, 186
		}),
		new HashDef("Data/Config/XUi/windows.xml", new byte[16]
		{
			31, 129, 193, 78, 165, 164, 50, 2, 152, 31,
			113, 69, 5, 151, 247, 12
		}),
		new HashDef("Data/Config/XUi/xui.xml", new byte[16]
		{
			215, 59, 198, 41, 190, 127, 17, 220, 192, 136,
			57, 5, 142, 248, 159, 249
		}),
		new HashDef("Data/Config/XUi_Common/controls.xml", new byte[16]
		{
			24, 142, 162, 218, 202, 134, 120, 62, 141, 213,
			55, 116, 132, 230, 48, 6
		}),
		new HashDef("Data/Config/XUi_Common/styles.xml", new byte[16]
		{
			138, 62, 228, 8, 175, 130, 134, 14, 196, 21,
			168, 61, 60, 252, 157, 143
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
