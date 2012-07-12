namespace FreePIE.GUI.CodeCompletion
{
    public interface ICompletionItem
    {
        string Name { get; }
        string Description { get; }
        void Insert();
    }
}
