using Newtonsoft.Json;

namespace Discord.API;

internal class TextInputComponent : IMessageComponent
{
	[JsonProperty("type")]
	public ComponentType Type { get; set; }

	[JsonProperty("style")]
	public TextInputStyle Style { get; set; }

	[JsonProperty("custom_id")]
	public string CustomId { get; set; }

	[JsonProperty("label")]
	public string Label { get; set; }

	[JsonProperty("placeholder")]
	public Optional<string> Placeholder { get; set; }

	[JsonProperty("min_length")]
	public Optional<int> MinLength { get; set; }

	[JsonProperty("max_length")]
	public Optional<int> MaxLength { get; set; }

	[JsonProperty("value")]
	public Optional<string> Value { get; set; }

	[JsonProperty("required")]
	public Optional<bool> Required { get; set; }

	public TextInputComponent()
	{
	}

	public TextInputComponent(Discord.TextInputComponent component)
	{
		Type = component.Type;
		Style = component.Style;
		CustomId = component.CustomId;
		Label = component.Label;
		Placeholder = component.Placeholder;
		MinLength = ((Optional<int>?)component.MinLength) ?? Optional<int>.Unspecified;
		MaxLength = ((Optional<int>?)component.MaxLength) ?? Optional<int>.Unspecified;
		Required = ((Optional<bool>?)component.Required) ?? Optional<bool>.Unspecified;
		string value = component.Value;
		Value = ((value != null) ? ((Optional<string>)value) : Optional<string>.Unspecified);
	}
}
