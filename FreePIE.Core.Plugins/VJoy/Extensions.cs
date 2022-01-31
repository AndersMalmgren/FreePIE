using System.Text;

namespace FreePIE.Core.Plugins.VJoy
{
    public static class Extensions
    {
        public static string ToHexString(this byte[] data)
        {
            StringBuilder sb = new StringBuilder();
            foreach (byte b in data)
                sb.AppendFormat("{0:X2}", b);
            return sb.ToString();
        }

        /*
        public static unsafe byte[] BytePtrToArray(byte* b, int length)
        {
            byte[] buffer = new byte[length];
            return ;
        }*/
    }
}
