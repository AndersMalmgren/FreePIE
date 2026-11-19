using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace FreePIE.Core.Plugins.Ffb
{
    // Drivers report force feedback on their own threads, commands are queued and applied on the script thread
    // so that sinks never touch their devices concurrently with the script
    public class FfbSource : IFfbSource
    {
        private readonly ConcurrentQueue<Action> commands;
        private readonly List<IFfbSink> sinks;

        public FfbSource()
        {
            commands = new ConcurrentQueue<Action>();
            sinks = new List<IFfbSink>();
            State = new FfbState();
        }

        public FfbState State { get; private set; }

        public void AddSink(IFfbSink sink)
        {
            if (sinks.Contains(sink))
                return;

            sinks.Add(sink);
            State.ReplayTo(sink);
        }

        public void RemoveSink(IFfbSink sink)
        {
            if (sinks.Remove(sink))
                sink.Control(FfbControl.Reset);
        }

        public void UpdateEffect(int index, Action<FfbEffect> update)
        {
            commands.Enqueue(() =>
            {
                var effect = State.GetEffectCopy(index);
                update(effect);
                Dispatch(s => s.SetEffect(index, effect));
            });
        }

        public void StartEffect(int index, int loopCount, bool solo)
        {
            commands.Enqueue(() => Dispatch(s => s.StartEffect(index, loopCount, solo)));
        }

        public void StopEffect(int index)
        {
            commands.Enqueue(() => Dispatch(s => s.StopEffect(index)));
        }

        public void RemoveEffect(int index)
        {
            commands.Enqueue(() => Dispatch(s => s.RemoveEffect(index)));
        }

        public void SetGain(double gain)
        {
            commands.Enqueue(() => Dispatch(s => s.SetGain(gain)));
        }

        public void Control(FfbControl control)
        {
            commands.Enqueue(() => Dispatch(s => s.Control(control)));
        }

        public void ReportError(Exception exception)
        {
            commands.Enqueue(() => { throw new Exception("Failed to read force feedback: " + exception.Message, exception); });
        }

        public bool Pump()
        {
            var processed = false;

            Action command;
            while (commands.TryDequeue(out command))
            {
                command();
                processed = true;
            }

            return processed;
        }

        private void Dispatch(Action<IFfbSink> command)
        {
            command(State);
            sinks.ForEach(command);
        }
    }
}
