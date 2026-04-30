using System;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace ModAPIInspector;

internal class Program
{
	[_003C500f4e69_002Df634_002D47aa_002D86a0_002D49f1de6b9701_003ENullableContext(1)]
	private static void Main(string[] args)
	{
		try
		{
			Assembly assembly = Assembly.LoadFrom("/home/josh/.local/share/Steam/steamapps/common/7 Days to Die Dedicated Server/7DaysToDieServer_Data/Managed/Assembly-CSharp.dll");
			Console.WriteLine("=== Looking for GameMessage Events and GMSG ===");
			Type type = assembly.GetTypes().FirstOrDefault((Type t) => t.Name == "ModEvents");
			if (type != null)
			{
				Console.WriteLine("Found ModEvents: " + type.FullName);
				FieldInfo field = type.GetField("GameMessage", BindingFlags.Static | BindingFlags.Public);
				if (field != null)
				{
					Console.WriteLine("\n*** FOUND GameMessage EVENT ***");
					Console.WriteLine("GameMessage Field: " + field.Name + " - Type: " + field.FieldType.Name);
					Console.WriteLine("Full Type: " + field.FieldType.FullName);
				}
				Type nestedType = type.GetNestedType("SGameMessageData");
				if (nestedType != null)
				{
					Console.WriteLine("\n*** FOUND SGameMessageData ***");
					Console.WriteLine("SGameMessageData: " + nestedType.FullName);
					FieldInfo[] fields = nestedType.GetFields(BindingFlags.Instance | BindingFlags.Public);
					Console.WriteLine("\nSGameMessageData Fields:");
					FieldInfo[] array = fields;
					foreach (FieldInfo fieldInfo in array)
					{
						Console.WriteLine("  " + fieldInfo.Name + ": " + fieldInfo.FieldType.Name + " (" + fieldInfo.FieldType.FullName + ")");
					}
					PropertyInfo[] properties = nestedType.GetProperties(BindingFlags.Instance | BindingFlags.Public);
					Console.WriteLine("\nSGameMessageData Properties:");
					PropertyInfo[] array2 = properties;
					foreach (PropertyInfo propertyInfo in array2)
					{
						Console.WriteLine("  " + propertyInfo.Name + ": " + propertyInfo.PropertyType.Name + " (" + propertyInfo.PropertyType.FullName + ")");
					}
				}
			}
			Console.WriteLine("\n=== Looking for GMSG-related Types ===");
			foreach (Type item in from t in (from t in assembly.GetTypes()
					where t.Name.ToLower().Contains("gmsg") || t.Name.ToLower().Contains("gamemessage") || t.FullName.ToLower().Contains("gmsg") || t.FullName.ToLower().Contains("gamemessage")
					select t).ToArray()
				orderby t.Name
				select t)
			{
				Console.WriteLine("GMSG-related type: " + item.FullName);
				if (!item.IsEnum)
				{
					continue;
				}
				Console.WriteLine("  Enum values:");
				foreach (object value in Enum.GetValues(item))
				{
					Console.WriteLine($"    {value} = {(int)value}");
				}
			}
			Console.WriteLine("\n=== Looking for Player Death Message Handlers ===");
			foreach (Type item2 in (from t in assembly.GetTypes()
				where t.GetMethods().Any((MethodInfo m) => m.Name.ToLower().Contains("death") && m.Name.ToLower().Contains("message")) || t.GetMethods().Any((MethodInfo m) => m.Name.ToLower().Contains("player") && m.Name.ToLower().Contains("died")) || t.GetMethods().Any((MethodInfo m) => m.Name.ToLower().Contains("gmsg"))
				select t).ToArray().Take(10))
			{
				Console.WriteLine("Type with death/gmsg methods: " + item2.FullName);
				foreach (MethodInfo item3 in (from m in item2.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
					where m.Name.ToLower().Contains("death") || m.Name.ToLower().Contains("died") || m.Name.ToLower().Contains("gmsg")
					select m).Take(5))
				{
					Console.WriteLine("  Method: " + item3.Name + " - Returns: " + item3.ReturnType.Name);
				}
			}
		}
		catch (Exception ex)
		{
			Console.WriteLine("Error: " + ex.Message);
			Console.WriteLine("Stack trace: " + ex.StackTrace);
		}
		Console.WriteLine("\nPress any key to exit...");
		Console.ReadKey();
	}
}
