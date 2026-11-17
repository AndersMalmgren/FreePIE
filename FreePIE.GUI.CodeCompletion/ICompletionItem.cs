namespace FreePIE.GUI.CodeCompletion
{
    public interface ICompletionItem
    {
        string Name { get; }
        string Description { get; }
        string Kind { get; }
        void Insert();
    }
}
