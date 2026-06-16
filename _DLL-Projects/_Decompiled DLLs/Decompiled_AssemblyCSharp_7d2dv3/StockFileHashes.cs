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
	public static HashDef[] hashDefinitions = new HashDef[52]
	{
		new HashDef("Data/Config/archetypes.xml", new byte[16]
		{
			129, 149, 214, 213, 160, 126, 29, 253, 240, 10,
			8, 204, 99, 252, 199, 182
		}),
		new HashDef("Data/Config/biomes.xml", new byte[16]
		{
			194, 85, 165, 41, 188, 8, 172, 220, 66, 3,
			125, 67, 22, 213, 77, 66
		}),
		new HashDef("Data/Config/blockplaceholders.xml", new byte[16]
		{
			139, 41, 168, 124, 142, 8, 20, 230, 200, 231,
			138, 138, 190, 208, 183, 207
		}),
		new HashDef("Data/Config/blocks.xml", new byte[16]
		{
			66, 147, 98, 114, 23, 21, 69, 109, 111, 165,
			73, 19, 169, 110, 188, 110
		}),
		new HashDef("Data/Config/buffs.xml", new byte[16]
		{
			184, 168, 156, 235, 66, 114, 212, 3, 73, 20,
			145, 196, 106, 161, 113, 81
		}),
		new HashDef("Data/Config/challenges.xml", new byte[16]
		{
			164, 121, 16, 114, 79, 92, 158, 194, 62, 179,
			17, 221, 162, 77, 151, 49
		}),
		new HashDef("Data/Config/devicesgameprefs.xml", new byte[16]
		{
			196, 158, 114, 144, 153, 61, 251, 22, 157, 95,
			34, 159, 164, 137, 247, 189
		}),
		new HashDef("Data/Config/dialogs.xml", new byte[16]
		{
			0, 167, 89, 52, 168, 98, 141, 228, 52, 90,
			90, 208, 141, 207, 11, 43
		}),
		new HashDef("Data/Config/dmscontent.xml", new byte[16]
		{
			146, 234, 236, 139, 101, 159, 66, 55, 76, 140,
			14, 56, 174, 18, 22, 93
		}),
		new HashDef("Data/Config/entityclasses.xml", new byte[16]
		{
			179, 158, 225, 151, 71, 197, 178, 10, 87, 154,
			129, 22, 58, 148, 186, 231
		}),
		new HashDef("Data/Config/entitygroups.xml", new byte[16]
		{
			228, 23, 205, 64, 89, 33, 72, 141, 140, 161,
			237, 50, 97, 84, 220, 83
		}),
		new HashDef("Data/Config/events.xml", new byte[16]
		{
			69, 102, 250, 190, 70, 178, 36, 89, 123, 60,
			95, 204, 74, 75, 181, 17
		}),
		new HashDef("Data/Config/gameevents.xml", new byte[16]
		{
			117, 99, 59, 253, 250, 129, 34, 103, 61, 176,
			236, 116, 134, 204, 192, 14
		}),
		new HashDef("Data/Config/gamestages.xml", new byte[16]
		{
			76, 83, 146, 37, 153, 255, 57, 173, 175, 213,
			208, 241, 75, 81, 85, 10
		}),
		new HashDef("Data/Config/items.xml", new byte[16]
		{
			28, 184, 122, 218, 215, 217, 193, 71, 197, 81,
			96, 147, 58, 170, 233, 86
		}),
		new HashDef("Data/Config/item_modifiers.xml", new byte[16]
		{
			77, 166, 140, 180, 120, 218, 34, 13, 180, 236,
			242, 124, 102, 106, 24, 97
		}),
		new HashDef("Data/Config/loadingscreen.xml", new byte[16]
		{
			118, 242, 214, 84, 52, 125, 37, 176, 246, 97,
			93, 35, 202, 234, 92, 228
		}),
		new HashDef("Data/Config/loot.xml", new byte[16]
		{
			162, 203, 145, 81, 110, 192, 230, 8, 4, 201,
			198, 27, 116, 210, 166, 172
		}),
		new HashDef("Data/Config/materials.xml", new byte[16]
		{
			128, 141, 183, 99, 200, 240, 79, 65, 81, 66,
			166, 176, 178, 225, 15, 178
		}),
		new HashDef("Data/Config/misc.xml", new byte[16]
		{
			221, 223, 126, 194, 10, 67, 126, 120, 75, 188,
			233, 72, 150, 146, 41, 136
		}),
		new HashDef("Data/Config/music.xml", new byte[16]
		{
			81, 85, 5, 120, 112, 228, 213, 50, 7, 73,
			121, 96, 243, 210, 139, 204
		}),
		new HashDef("Data/Config/nav_objects.xml", new byte[16]
		{
			181, 226, 70, 45, 89, 206, 134, 249, 183, 2,
			53, 181, 134, 213, 105, 81
		}),
		new HashDef("Data/Config/npc.xml", new byte[16]
		{
			39, 115, 86, 80, 152, 202, 155, 64, 250, 199,
			252, 24, 201, 139, 196, 27
		}),
		new HashDef("Data/Config/painting.xml", new byte[16]
		{
			149, 174, 31, 175, 31, 238, 140, 122, 184, 224,
			212, 82, 77, 56, 99, 251
		}),
		new HashDef("Data/Config/physicsbodies.xml", new byte[16]
		{
			123, 240, 13, 225, 25, 233, 87, 207, 179, 189,
			37, 15, 152, 11, 70, 169
		}),
		new HashDef("Data/Config/progression.xml", new byte[16]
		{
			61, 76, 153, 223, 40, 221, 206, 61, 225, 112,
			180, 93, 100, 134, 24, 204
		}),
		new HashDef("Data/Config/qualityinfo.xml", new byte[16]
		{
			97, 120, 145, 33, 103, 203, 2, 168, 192, 21,
			25, 195, 68, 215, 251, 211
		}),
		new HashDef("Data/Config/quests.xml", new byte[16]
		{
			7, 49, 38, 15, 122, 237, 115, 58, 113, 170,
			31, 255, 126, 202, 47, 215
		}),
		new HashDef("Data/Config/recipes.xml", new byte[16]
		{
			169, 141, 177, 67, 123, 39, 121, 5, 5, 173,
			8, 191, 123, 11, 196, 232
		}),
		new HashDef("Data/Config/rwgmixer.xml", new byte[16]
		{
			163, 248, 8, 41, 103, 28, 207, 98, 108, 216,
			40, 12, 157, 113, 13, 145
		}),
		new HashDef("Data/Config/sandbox_overrides.xml", new byte[16]
		{
			21, 211, 79, 241, 118, 82, 75, 217, 111, 77,
			136, 74, 233, 17, 164, 104
		}),
		new HashDef("Data/Config/shapes.xml", new byte[16]
		{
			217, 222, 243, 93, 36, 150, 57, 2, 1, 225,
			157, 135, 116, 167, 185, 90
		}),
		new HashDef("Data/Config/signs.xml", new byte[16]
		{
			241, 151, 215, 160, 187, 129, 79, 33, 145, 140,
			116, 42, 155, 195, 126, 16
		}),
		new HashDef("Data/Config/sounds.xml", new byte[16]
		{
			204, 82, 121, 247, 205, 177, 195, 156, 43, 135,
			6, 47, 251, 186, 205, 87
		}),
		new HashDef("Data/Config/spawning.xml", new byte[16]
		{
			215, 31, 45, 45, 178, 189, 31, 230, 82, 222,
			125, 217, 126, 39, 107, 11
		}),
		new HashDef("Data/Config/subtitles.xml", new byte[16]
		{
			43, 226, 138, 234, 5, 182, 101, 56, 134, 12,
			29, 180, 158, 99, 28, 188
		}),
		new HashDef("Data/Config/taskboards.xml", new byte[16]
		{
			131, 187, 131, 8, 200, 45, 210, 129, 88, 165,
			145, 184, 111, 53, 79, 82
		}),
		new HashDef("Data/Config/traders.xml", new byte[16]
		{
			250, 162, 130, 34, 15, 101, 207, 175, 218, 72,
			167, 62, 139, 169, 208, 215
		}),
		new HashDef("Data/Config/twitch.xml", new byte[16]
		{
			9, 194, 94, 52, 73, 50, 131, 142, 214, 23,
			72, 19, 248, 143, 128, 123
		}),
		new HashDef("Data/Config/twitch_events.xml", new byte[16]
		{
			2, 109, 178, 40, 65, 165, 27, 17, 153, 161,
			161, 105, 193, 80, 223, 163
		}),
		new HashDef("Data/Config/ui_display.xml", new byte[16]
		{
			38, 125, 237, 77, 161, 159, 39, 134, 83, 21,
			78, 240, 100, 244, 50, 217
		}),
		new HashDef("Data/Config/utilityai.xml", new byte[16]
		{
			214, 167, 177, 22, 147, 156, 88, 129, 207, 113,
			61, 241, 70, 38, 155, 75
		}),
		new HashDef("Data/Config/vehicles.xml", new byte[16]
		{
			125, 77, 117, 176, 226, 193, 196, 149, 111, 49,
			151, 203, 244, 123, 54, 31
		}),
		new HashDef("Data/Config/videos.xml", new byte[16]
		{
			111, 28, 117, 204, 29, 196, 6, 21, 182, 82,
			6, 72, 67, 229, 163, 234
		}),
		new HashDef("Data/Config/weathersurvival.xml", new byte[16]
		{
			151, 245, 70, 52, 118, 190, 215, 193, 5, 166,
			162, 78, 110, 62, 129, 210
		}),
		new HashDef("Data/Config/worldglobal.xml", new byte[16]
		{
			238, 79, 96, 194, 24, 77, 208, 166, 155, 226,
			190, 215, 20, 216, 134, 75
		}),
		new HashDef("Data/Config/XUi_Common/styles.xml", new byte[16]
		{
			130, 202, 20, 37, 23, 191, 103, 87, 101, 144,
			119, 127, 203, 40, 14, 115
		}),
		new HashDef("Data/Config/XUi_Common/templates.xml", new byte[16]
		{
			128, 195, 103, 154, 82, 177, 206, 62, 246, 149,
			79, 42, 5, 162, 16, 17
		}),
		new HashDef("Data/Config/XUi_InGame/styles.xml", new byte[16]
		{
			85, 175, 136, 53, 223, 110, 185, 11, 201, 158,
			162, 237, 134, 74, 144, 186
		}),
		new HashDef("Data/Config/XUi_InGame/templates.xml", new byte[16]
		{
			49, 207, 6, 210, 141, 97, 100, 61, 172, 64,
			88, 201, 168, 99, 29, 26
		}),
		new HashDef("Data/Config/XUi_InGame/windows.xml", new byte[16]
		{
			25, 18, 67, 56, 177, 158, 235, 205, 211, 53,
			193, 146, 74, 127, 52, 194
		}),
		new HashDef("Data/Config/XUi_InGame/xui.xml", new byte[16]
		{
			25, 28, 222, 39, 204, 241, 217, 21, 66, 9,
			173, 201, 188, 243, 3, 32
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
