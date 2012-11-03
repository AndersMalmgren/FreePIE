using System.Threading;

namespace FreePIE.Core.Common
{
    public struct InterlockableBool
    {
        private volatile int value;

        public InterlockableBool(bool value = false)
        {
            this.value = value ? 1 : 0;
        }

        public bool CurrentValue { get { return value == 1; } }

        private static int Convert(bool val)
        {
            return val ? 1 : 0;
        }

        public static implicit operator bool(InterlockableBool value)
        {
            return Convert(value.value);
        }

        public static implicit operator InterlockableBool(bool value)
        {
            return new InterlockableBool(value);
        }

        private static bool Convert(int val)
        {
            return val != 0;
        }

        public bool CompareExchange(bool newValue, bool comparand)
        {
            var oldVal = Interlocked.CompareExchange(ref value, Convert(newValue), Convert(comparand));
            return Convert(oldVal);
        }
    }
}
