namespace FreePIE.Core.Common
{
    public class Range
    {
        public Range(int start, int numberOfElements)
        {
            this.Start = start;
            this.NumberOfElements = numberOfElements;
        }

        public int Start { get; private set; }
        public int NumberOfElements { get; private set; }
    }
}
