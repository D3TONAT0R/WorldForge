using System.ComponentModel;
using System.Numerics;
using System.Windows.Forms;
using WorldForge.Coordinates;

namespace WorldForgeToolbox;

[DesignerCategory("Custom")]
public class MapView : Panel
{
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible), Category("Map Settings")]
	public int Zoom
	{
		get => zoom;
		set => zoom = Math.Clamp(value, MinZoom, MaxZoom);
	}
	private int zoom = 1;

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
	public int ZoomScale => 1 << (Zoom - 1);

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible), Category("Map Settings")]
	public int MinZoom { get; set; } = 1;

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible), Category("Map Settings")]
	public int MaxZoom { get; set; } = 8;

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible), Category("Map Settings")]
	public float UnitScale { get; set; } = 1;

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible), Category("Map Settings")]
	public bool AllowInteractions { get; set; } = true;

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible), Category("Map Settings")]
	public bool AllowPanning { get; set; } = true;

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible), Category("Map Settings")]
	public bool AllowZooming { get; set; } = true;

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
	public Vector2 Center { get; set; } = Vector2.Zero;

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
	public bool IsMouseDown { get; private set; }

	private Point lastMousePos;
	private Point mousePosition;

	public MapView()
	{
		DoubleBuffered = true;
		ResizeRedraw = true;
	}

	public void Repaint() => Invalidate();

	public Vector2 ScreenToMapPos(Point screenPos)
	{
		float x = screenPos.X - ClientRectangle.Width * 0.5f;
		float y = screenPos.Y - ClientRectangle.Height * 0.5f;
		x /= ZoomScale;
		y /= ZoomScale;
		x *= UnitScale;
		y *= UnitScale;
		x += Center.X;
		y += Center.Y;
		return new Vector2(x, y);
	}

	public Point MapToScreenPos(Vector2 pos)
	{
		pos -= Center;
		float x = pos.X / UnitScale;
		float y = pos.Y / UnitScale;
		x *= ZoomScale;
		y *= ZoomScale;
		x += ClientRectangle.Width * 0.5f;
		y += ClientRectangle.Height * 0.5f;
		return new Point((int)x, (int)y);
	}

	protected override void OnMouseWheel(MouseEventArgs e)
	{
		if (AllowInteractions && AllowZooming)
		{
			var lastZoom = Zoom;
			Zoom += Math.Clamp(e.Delta, -1, 1);
			var zoomDelta = Zoom - lastZoom;
			var centerPos = ScreenToMapPos(e.Location);
			var diff = centerPos - Center;
			if (zoomDelta > 0)
			{
				Center += diff * zoomDelta;
			}
			else if (zoomDelta < 0)
			{
				Center += diff * zoomDelta / 2f;
			}
			Repaint();
		}
		base.OnMouseWheel(e);
	}

	protected override void OnMouseDown(MouseEventArgs e)
	{
		IsMouseDown = true;
		lastMousePos = e.Location;
		base.OnMouseDown(e);
	}

	protected override void OnMouseUp(MouseEventArgs e)
	{
		IsMouseDown = false;
		base.OnMouseUp(e);
	}

	protected override void OnMouseMove(MouseEventArgs e)
	{
		mousePosition = e.Location;
		if(AllowInteractions && AllowPanning && IsMouseDown)
		{
			var moveDelta = new Vector2(e.Location.X - lastMousePos.X, e.Location.Y - lastMousePos.Y);
			Center -= moveDelta * 16f / ZoomScale;
			lastMousePos = e.Location;
		}
		base.OnMouseMove(e);
	}
}
