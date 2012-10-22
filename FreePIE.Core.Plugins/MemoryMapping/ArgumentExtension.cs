namespace FreePIE.Core.Plugins.MemoryMapping
{
    public static class ArgumentExtension
    {
        public static string Quote(this string input)
        {
            return "\"" + input + "\"";
        }
    }
}