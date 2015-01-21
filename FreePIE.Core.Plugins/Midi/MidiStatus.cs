using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FreePIE.Core.Contracts;

namespace FreePIE.Core.Plugins.Midi
{
    [GlobalEnum]
    public enum MidiStatus
    {
        None = 0,
        NoteOff = 0x8,
        NoteOn = 0x9,
        PolyphonicAftertouch = 0xA,
        Control = 0xB,
        ProgramChange = 0xC,
        ChannelAftertouch = 0xD,
        PitchBendChange = 0xE,
    }
}
