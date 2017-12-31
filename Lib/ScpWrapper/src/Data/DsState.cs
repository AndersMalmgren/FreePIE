namespace JonesCorp.Data
{
    public enum ConnectionState : byte
    {
        Disconnected = 0x01,
        Opened = 0x02,
        Started = 0x03
    };
}