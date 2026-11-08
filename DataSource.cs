namespace sek;

struct DataSource
{
	public IDisposable? container;
	public Stream stream;
	public string dataSourceDescription;
}
