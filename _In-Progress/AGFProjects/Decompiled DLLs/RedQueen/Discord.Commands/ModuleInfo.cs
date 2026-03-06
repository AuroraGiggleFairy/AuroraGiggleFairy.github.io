using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Discord.Commands.Builders;

namespace Discord.Commands;

internal class ModuleInfo
{
	public CommandService Service { get; }

	public string Name { get; }

	public string Summary { get; }

	public string Remarks { get; }

	public string Group { get; }

	public IReadOnlyList<string> Aliases { get; }

	public IReadOnlyList<CommandInfo> Commands { get; }

	public IReadOnlyList<PreconditionAttribute> Preconditions { get; }

	public IReadOnlyList<Attribute> Attributes { get; }

	public IReadOnlyList<ModuleInfo> Submodules { get; }

	public ModuleInfo Parent { get; }

	public bool IsSubmodule => Parent != null;

	internal ModuleInfo(ModuleBuilder builder, CommandService service, IServiceProvider services, ModuleInfo parent = null)
	{
		ModuleInfo info = this;
		Service = service;
		Name = builder.Name;
		Summary = builder.Summary;
		Remarks = builder.Remarks;
		Group = builder.Group;
		Parent = parent;
		Aliases = BuildAliases(builder, service).ToImmutableArray();
		Commands = builder.Commands.Select((CommandBuilder x) => x.Build(info, service)).ToImmutableArray();
		Preconditions = BuildPreconditions(builder).ToImmutableArray();
		Attributes = BuildAttributes(builder).ToImmutableArray();
		Submodules = BuildSubmodules(builder, service, services).ToImmutableArray();
	}

	private static IEnumerable<string> BuildAliases(ModuleBuilder builder, CommandService service)
	{
		List<string> list = builder.Aliases.ToList();
		Queue<ModuleBuilder> queue = new Queue<ModuleBuilder>();
		ModuleBuilder moduleBuilder = builder;
		while ((moduleBuilder = moduleBuilder.Parent) != null)
		{
			queue.Enqueue(moduleBuilder);
		}
		while (queue.Count > 0)
		{
			list = queue.Dequeue().Aliases.Permutate(list, delegate(string first, string second)
			{
				if (first == "")
				{
					return second;
				}
				return (second == "") ? first : (first + service._separatorChar + second);
			}).ToList();
		}
		return list;
	}

	private List<ModuleInfo> BuildSubmodules(ModuleBuilder parent, CommandService service, IServiceProvider services)
	{
		List<ModuleInfo> list = new List<ModuleInfo>();
		foreach (ModuleBuilder module in parent.Modules)
		{
			list.Add(module.Build(service, services, this));
		}
		return list;
	}

	private static List<PreconditionAttribute> BuildPreconditions(ModuleBuilder builder)
	{
		List<PreconditionAttribute> list = new List<PreconditionAttribute>();
		for (ModuleBuilder moduleBuilder = builder; moduleBuilder != null; moduleBuilder = moduleBuilder.Parent)
		{
			list.AddRange(moduleBuilder.Preconditions);
		}
		return list;
	}

	private static List<Attribute> BuildAttributes(ModuleBuilder builder)
	{
		List<Attribute> list = new List<Attribute>();
		for (ModuleBuilder moduleBuilder = builder; moduleBuilder != null; moduleBuilder = moduleBuilder.Parent)
		{
			list.AddRange(moduleBuilder.Attributes);
		}
		return list;
	}
}
