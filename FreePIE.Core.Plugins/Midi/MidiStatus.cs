using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FreePIE.Core.Contracts;

namespace FreePIE.Core.Plugins.Midi
{
    [GlobalEnum]
    [Documentation("The kind of a MIDI message")]
    public enum MidiStatus
    {
        None = 0,
        [Documentation("A key was released, buffer is note and velocity")]
        NoteOff = 0x8,
        [Documentation("A key was pressed, buffer is note and velocity")]
        NoteOn = 0x9,
        [Documentation("Pressure on a key, buffer is note and pressure")]
        PolyphonicAftertouch = 0xA,
        [Documentation("A controller changed, buffer is controller and value")]
        Control = 0xB,
        [Documentation("The program changed, the first byte of buffer is the program")]
        ProgramChange = 0xC,
        [Documentation("Pressure on the channel, the first byte of buffer is the pressure")]
        ChannelAftertouch = 0xD,
        [Documentation("The pitch bend changed, buffer is the low and high 7 bits")]
        PitchBendChange = 0xE,
    }
}
