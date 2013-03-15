namespace FreePIE.Core.Contracts
{
    public interface IPluginProperty
    {
        string Name { get; set; }
        string Caption { get; set; }
        object Value { get; set; }
        object DefaultValue { get; set; }
        IChoices Choices { get; }
        string HelpText { get; set; }
    }

    public interface IChoice
    {
        string Caption { get; set; }
        object Value { get; set; }
    }

    public interface IChoices
    {
        void Add(string caption, object value);
    }
}
