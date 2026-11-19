using System.Collections.Generic;
using System.Globalization;
using FreePIE.Core.Plugins.Ffb;

namespace FreePIE.Tests.Core.Plugins.Ffb
{
    public class RecordingFfbSink : IFfbSink
    {
        public RecordingFfbSink()
        {
            Calls = new List<string>();
            Effects = new Dictionary<int, FfbEffect>();
        }

        public List<string> Calls { get; private set; }
        public Dictionary<int, FfbEffect> Effects { get; private set; }

        public void SetEffect(int index, FfbEffect effect)
        {
            Effects[index] = effect.Clone();
            Calls.Add(string.Format("set {0} {1}", index, effect.type));
        }

        public void StartEffect(int index, int loopCount, bool solo)
        {
            Calls.Add(string.Format("start {0} {1}{2}", index, loopCount, solo ? " solo" : ""));
        }

        public void StopEffect(int index)
        {
            Calls.Add(string.Format("stop {0}", index));
        }

        public void RemoveEffect(int index)
        {
            Calls.Add(string.Format("remove {0}", index));
        }

        public void SetGain(double gain)
        {
            Calls.Add(string.Format(CultureInfo.InvariantCulture, "gain {0}", gain));
        }

        public void Control(FfbControl control)
        {
            Calls.Add(string.Format("control {0}", control));
        }
    }
}
