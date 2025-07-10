using System.IO;

namespace WorldForge
{
	public interface IResourceLoader
	{
		Stream GetResourceAsStream(string fileName);

		string GetResourceAsText(string fileName);
	}
}