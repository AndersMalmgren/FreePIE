namespace FreePIE.Core.Persistence
{
    public interface IPaths
    {
        string Data { get; }
        string Application { get; }
        string GetDataPath(string filename);
        string GetApplicationPath(string filename);
    }
}