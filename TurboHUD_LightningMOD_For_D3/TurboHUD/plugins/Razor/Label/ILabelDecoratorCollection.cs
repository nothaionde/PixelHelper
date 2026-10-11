namespace Turbo.Plugins.Razor.Label
{
	public interface ILabelDecoratorCollection : ILabelDecorator
	{
		System.Collections.Generic.List<ILabelDecorator> Labels { get; set; }
		ILabelDecorator HoveredLabel { get; }
	}
}