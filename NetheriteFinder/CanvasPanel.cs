using System.ComponentModel;

namespace NetheriteFinder
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