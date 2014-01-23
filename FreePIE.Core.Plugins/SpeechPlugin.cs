using System;
using System.Speech.Synthesis;
using FreePIE.Core.Contracts;

namespace FreePIE.Core.Plugins
{
    [GlobalType(Type = typeof(SpeechGlobal))]
    public class SpeechPlugin : Plugin
    {
        private SpeechSynthesizer synth;
        private Prompt prompt;

        public override object CreateGlobal()
        {
            return new SpeechGlobal(this);
        }

        public override Action Start()
        {
            synth = new SpeechSynthesizer();
            synth.SetOutputToDefaultAudioDevice();

            return null;
        }

        public override void Stop()
        {
            synth.Dispose();
        }

        public void Say(string text)
        {
            if(prompt != null)
                synth.SpeakAsyncCancel(prompt);

            prompt = synth.SpeakAsync(text);
        }

        public override string FriendlyName
        {
            get { return "Speech"; }
        }
    }

    [Global(Name = "speech")]
    public class SpeechGlobal
    {
        private readonly SpeechPlugin plugin;

        public SpeechGlobal(SpeechPlugin plugin)
        {
            this.plugin = plugin;
        } 

        public void say(string text)
        {
            plugin.Say(text);   
        }   
    }
}
