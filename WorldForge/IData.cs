namespace WorldForge
{
	public interface IData
	{
		void Save(string worldSaveRoot, int id, GameVersion version);
	}
}