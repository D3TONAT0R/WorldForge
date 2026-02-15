using System.ComponentModel;

namespace WorldForgeToolbox
{
	[DesignerCategory("Custom")]
	public class CanvasPanel : Panel
	{
		public CanvasPanel()
		{
			DoubleBuffered = true;
			ResizeRedraw = true;
		}
	}
}