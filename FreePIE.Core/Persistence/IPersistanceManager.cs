namespace FreePIE.Core.Persistence
{
    public interface IPersistanceManager
    {
        bool Load();
        void Save();
    }
}