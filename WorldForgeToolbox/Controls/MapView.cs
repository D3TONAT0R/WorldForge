using System.ComponentModel;
using System.Drawing;
using System.Numerics;
using System.Windows.Forms;
using WorldForge;
using WorldForge.Coordinates;
using Vector2 = System.Numerics.Vector2;

namespace WorldForgeToolbox;

[DesignerCategory("Custom")]
public class MapView : Panel
{
	public enum MarkerShape
	{
		Cross,
		Circle,
		Square,
		Diamond,
		TriangleUp,
		TriangleDown,
		Hexagon
	}

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

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible), Category("Map Settings")]
	public bool LabelShadow { get; set; } = true;

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
	public Vector2 Center { get; set; } = Vector2.Zero;

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
	public bool IsMouseDown { get; private set; }

	private Point lastMousePos;
	private Point mousePosition;

	private Font boldFont = new Font(SystemFonts.DefaultFont, FontStyle.Bold);
	private StringFormat markerLabelFormat = new StringFormat()
	{
		Alignment = StringAlignment.Near,
		LineAlignment = StringAlignment.Center
	};
	private StringFormat centeredLabelFormat = new StringFormat()
	{
		Alignment = StringAlignment.Center,
		LineAlignment = StringAlignment.Center
	};

	public MapView()
	{
		DoubleBuffered = true;
		ResizeRedraw = true;
	}

	public void Repaint() => Invalidate();

	#region Space conversions
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

	public Point MapToScreenPos(float x, float y) => MapToScreenPos(new Vector2(x, y));

	public Rectangle MapToScreenRectangle(Vector2 pos, Vector2 size)
	{
		var topLeft = MapToScreenPos(pos);
		size *= ZoomScale;
		size *= UnitScale;
		return new Rectangle(topLeft.X, topLeft.Y, (int)size.X, (int)size.Y);
	}

	public Rectangle MapToScreenRectangle(int x, int y, int width, int height) => MapToScreenRectangle(new Vector2(x, y), new Vector2(width, height));
	#endregion

	#region Drawing functions
	public void DrawGrid(Graphics g, Pen pen, float gridSize)
	{
		var topLeft = ScreenToMapPos(Point.Empty);
		var bottomRight = ScreenToMapPos(new Point(ClientRectangle.Width, ClientRectangle.Height));
		int startX = (int)(Math.Floor(topLeft.X / gridSize) * gridSize);
		int endX = (int)(Math.Ceiling(bottomRight.X / gridSize) * gridSize);
		int startY = (int)(Math.Floor(topLeft.Y / gridSize) * gridSize);
		int endY = (int)(Math.Ceiling(bottomRight.Y / gridSize) * gridSize);
		for (float x = startX; x <= endX; x += gridSize)
		{
			var p1 = MapToScreenPos(x, startY);
			var p2 = MapToScreenPos(x, endY);
			g.DrawLine(pen, p1, p2);
		}
		for (float y = startY; y <= endY; y += gridSize)
		{
			var p1 = MapToScreenPos(startX, y);
			var p2 = MapToScreenPos(endX, y);
			g.DrawLine(pen, p1, p2);
		}
	} 

	public void DrawHorizontalGuide(Graphics g, Pen p, float y)
	{
		var pt = MapToScreenPos(0, y);
		if(pt.Y >= 0 && pt.Y <= ClientRectangle.Height)
		{
			g.DrawLine(p, 0, pt.Y, ClientRectangle.Width, pt.Y);
		}
	}

	public void DrawVerticalGuide(Graphics g, Pen p, float x)
	{
		var pt = MapToScreenPos(x, 0);
		if (pt.X >= 0 && pt.X <= ClientRectangle.Width)
		{
			g.DrawLine(p, pt.X, 0, pt.X, ClientRectangle.Height);
		}
	}

	public void DrawRectangle(Graphics g, Pen pen, Vector2 pos, Vector2 size)
	{
		var rect = MapToScreenRectangle(pos, size);
		g.DrawRectangle(pen, rect);
	}

	public void DrawRectangle(Graphics g, Pen pen, int x, int y, int width, int height) => DrawRectangle(g, pen, new Vector2(x, y), new Vector2(width, height));

	public void FillRectangle(Graphics g, Brush brush, Vector2 pos, Vector2 size)
	{
		var rect = MapToScreenRectangle(pos, size);
		g.FillRectangle(brush, rect);
	}

	public void FillRectangle(Graphics g, Brush brush, int x, int y, int width, int height) => FillRectangle(g, brush, new Vector2(x, y), new Vector2(width, height));

	public void DrawCircle(Graphics g, Pen pen, Vector2 center, float radius)
	{
		var screenCenter = MapToScreenPos(center);
		float scaledRadius = radius * ZoomScale * UnitScale;
		g.DrawEllipse(pen, screenCenter.X - scaledRadius, screenCenter.Y - scaledRadius, scaledRadius * 2, scaledRadius * 2);
	}

	public void DrawCircle(Graphics g, Pen pen, float centerX, float centerY, float radius) => DrawCircle(g, pen, new Vector2(centerX, centerY), radius);

	public void FillCircle(Graphics g, Brush brush, Vector2 center, float radius)
	{
		var screenCenter = MapToScreenPos(center);
		float scaledRadius = radius * ZoomScale * UnitScale;
		g.FillEllipse(brush, screenCenter.X - scaledRadius, screenCenter.Y - scaledRadius, scaledRadius * 2, scaledRadius * 2);
	}

	public void FillCircle(Graphics g, Brush brush, float centerX, float centerY, float radius) => FillCircle(g, brush, new Vector2(centerX, centerY), radius);

	public void DrawMarker(Graphics g, Pen pen, Vector2 pos, MarkerShape shape, float size, string? label = null)
	{
		var screenPos = MapToScreenPos(pos);
		switch (shape)
		{
			case MarkerShape.Cross:
				g.DrawLine(pen, screenPos.X - size, screenPos.Y - size, screenPos.X + size, screenPos.Y + size);
				g.DrawLine(pen, screenPos.X - size, screenPos.Y + size, screenPos.X + size, screenPos.Y - size);
				break;
			case MarkerShape.Circle:
				g.DrawEllipse(pen, screenPos.X - size, screenPos.Y - size, size * 2, size * 2);
				break;
			case MarkerShape.Square:
				g.DrawRectangle(pen, screenPos.X - size, screenPos.Y - size, size * 2, size * 2);
				break;
			default:
				var points = GetMarkerPolyPoints(screenPos, shape, size);
				g.DrawPolygon(pen, points);
				break;
		}
		if (label != null) DrawString(g, label, pen.Brush, screenPos.X + size + 2, screenPos.Y, markerLabelFormat);
	}

	public void DrawMarker(Graphics g, Pen pen, float x, float y, MarkerShape shape, float size, string? label = null) => DrawMarker(g, pen, new Vector2(x, y), shape, size, label);

	public void FillMarker(Graphics g, Brush brush, Vector2 pos, MarkerShape shape, float size, string? label = null)
	{
		var screenPos = MapToScreenPos(pos);
		switch (shape)
		{
			case MarkerShape.Cross:
				var pen = new Pen(brush, size / 2);
				g.DrawLine(pen, screenPos.X - size, screenPos.Y - size, screenPos.X + size, screenPos.Y + size);
				g.DrawLine(pen, screenPos.X - size, screenPos.Y + size, screenPos.X + size, screenPos.Y - size);
				break;
			case MarkerShape.Circle:
				g.FillEllipse(brush, screenPos.X - size, screenPos.Y - size, size * 2, size * 2);
				break;
			case MarkerShape.Square:
				g.FillRectangle(brush, screenPos.X - size, screenPos.Y - size, size * 2, size * 2);
				break;
			default:
				var points = GetMarkerPolyPoints(screenPos, shape, size);
				g.FillPolygon(brush, points);
				break;
		}
		if (label != null) DrawString(g, label, brush, screenPos.X + size + 2, screenPos.Y, markerLabelFormat);
	}

	public void FillMarker(Graphics g, Brush brush, float x, float y, MarkerShape shape, float size) => FillMarker(g, brush, new Vector2(x, y), shape, size);

	private PointF[] GetMarkerPolyPoints(Point screenPos, MarkerShape shape, float size)
	{
		switch (shape)
		{
			case MarkerShape.Diamond:
				var diamondPoints = new PointF[]
				{
					new PointF(screenPos.X, screenPos.Y - size),
					new PointF(screenPos.X + size, screenPos.Y),
					new PointF(screenPos.X, screenPos.Y + size),
					new PointF(screenPos.X - size, screenPos.Y)
				};
				return diamondPoints;
			case MarkerShape.TriangleUp:
				var triangleUpPoints = new PointF[]
				{
					new PointF(screenPos.X, screenPos.Y - size),
					new PointF(screenPos.X + size, screenPos.Y + size),
					new PointF(screenPos.X - size, screenPos.Y + size)
				};
				return triangleUpPoints;
			case MarkerShape.TriangleDown:
				var triangleDownPoints = new PointF[]
				{
					new PointF(screenPos.X, screenPos.Y + size),
					new PointF(screenPos.X + size, screenPos.Y - size),
					new PointF(screenPos.X - size, screenPos.Y - size)
				};
				return triangleDownPoints;
			case MarkerShape.Hexagon:
				var hexagonPoints = new PointF[6];
				for (int i = 0; i < 6; i++)
				{
					float angle = (float)(Math.PI / 3 * i);
					hexagonPoints[i] = new PointF(
						screenPos.X + (float)Math.Cos(angle) * size,
						screenPos.Y + (float)Math.Sin(angle) * size);
				}
				return hexagonPoints;
			default:
				throw new ArgumentException("Shape does not have polygon points");
		}
	}

	public void DrawImage(Graphics g, Image img, Vector2 pos, Vector2 size)
	{
		var rect = MapToScreenRectangle(pos, size);
		g.DrawImage(img, rect);
	}

	public void DrawImage(Graphics g, Image? img, int x, int y, int width, int height) => DrawImage(g, img, new Vector2(x, y), new Vector2(width, height));

	public void DrawIcon(Graphics g, Image? icon, Pen pen, Vector2 pos, float size, bool border = false, string? label = null)
	{
		var screenPos = MapToScreenPos(pos);
		if(icon != null) g.DrawImage(icon, screenPos.X - size / 2, screenPos.Y - size / 2, size, size);
		if(icon == null || border) g.DrawRectangle(pen, screenPos.X - size / 2, screenPos.Y - size / 2, size, size);
		if (label != null) DrawString(g, label, pen.Brush, screenPos.X + size / 2 + 2, screenPos.Y, markerLabelFormat);
	}

	public void DrawIcon(Graphics g, Image? icon, Pen pen, float x, float y, float size, bool border = false, string? label = null) => DrawIcon(g, icon, pen, new Vector2(x, y), size, border, label);

	public void DrawLabel(Graphics g, string text, Brush brush, Vector2 pos)
	{
		var screenPos = MapToScreenPos(pos);
		DrawString(g, text, brush, screenPos.X, screenPos.Y, centeredLabelFormat);
	}

	private void DrawString(Graphics g, string text, Brush b, float x, float y, StringFormat format)
	{
		if (LabelShadow) g.DrawString(text, boldFont, Brushes.Black, x + 1, y + 1, format);
		g.DrawString(text, boldFont, b, x, y, format);
	}
	#endregion

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
			Center -= moveDelta * UnitScale / ZoomScale;
			lastMousePos = e.Location;
		}
		base.OnMouseMove(e);
	}
}
