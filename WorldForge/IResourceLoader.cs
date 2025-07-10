using System.IO;

namespace WorldForge
{
	public interface IResourceLoader
	{
		IBitmap GetResourceBitmap(string fileName);

		string GetResourceAsText(string fileName);
	}
}