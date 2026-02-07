namespace WorldForgeToolbox;

public class Render
{
	public readonly Bitmap bitmap;

	public bool renderComplete = false;

	private Render(Bitmap bitmap)
	{
		this.bitmap = bitmap;
	}

	public static Render CreateNew(int width, int height, Action<Render, CancellationToken> renderer, Action<Render, CancellationToken>? completedCallback, CancellationToken token)
	{
		var r = new Render(new Bitmap(width, height));
		Task.Run(() =>
		{
			try
			{
				renderer(r, token);
			}
			finally
			{
				r.renderComplete = true;
				completedCallback?.Invoke(r, token);
			}
		}, token);
		return r;
	}

	public static Render CreateCompleted(Bitmap bitmap)
	{
		var r = new Render(bitmap);
		r.renderComplete = true;
		return r;
	}
}