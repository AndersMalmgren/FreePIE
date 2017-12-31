using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScpDotNet
{
    internal class ScpCache
    {
        public const int SCP_RUMBLE_SIZE = 8;

        /// <summary>
        /// 28 bytes
        /// </summary>
        protected SCP_XINPUT_DATA m_xinputData = new SCP_XINPUT_DATA();
        protected byte[] m_Rumble = new byte[SCP_RUMBLE_SIZE];
        protected SCP_INPUT_REPORT m_Mapped = default(SCP_INPUT_REPORT);

        /// <summary>
        /// 28 bytes
        /// </summary>
        public SCP_XINPUT_DATA XinputData
        {
            set { m_xinputData = value; }
            get { return m_xinputData; }
        }

        public byte[] Rumble
        {
            get { return m_Rumble; }
        }

        public SCP_INPUT_REPORT Mapped
        {
            get { return m_Mapped; }
        }
    }
}
