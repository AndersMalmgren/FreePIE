using System;

namespace JonesCorp
{
    public class DebugEventArgs : EventArgs
    {
        protected DateTime m_Time = DateTime.Now;
        protected String m_Data = String.Empty;

        public DebugEventArgs(String Data)
        {
            m_Data = Data;
        }

        public DateTime Time
        {
            get { return m_Time; }
        }

        public String Data
        {
            get { return m_Data; }
        }
    }
}
