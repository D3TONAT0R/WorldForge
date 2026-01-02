using System.ComponentModel;

namespace RegionViewer
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