namespace Discord;

internal class Modal : IMessageComponent
{
	public ComponentType Type => ComponentType.ModalSubmit;

	public string Title { get; set; }

	public string CustomId { get; set; }

	public ModalComponent Component { get; set; }

	internal Modal(string title, string customId, ModalComponent components)
	{
		Title = title;
		CustomId = customId;
		Component = components;
	}
}
