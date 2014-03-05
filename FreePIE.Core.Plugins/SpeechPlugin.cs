using System;
using System.Collections.Generic;
using System.Speech.Recognition;
using System.Speech.Synthesis;
using FreePIE.Core.Contracts;

namespace FreePIE.Core.Plugins
{
    [GlobalType(Type = typeof(SpeechGlobal))]
    public class SpeechPlugin : Plugin
    {
        private SpeechSynthesizer synth;
        private Prompt prompt;

        private SpeechRecognizer recognizer;
        private Dictionary<string, bool> recognizerResults; 
        

        public override object CreateGlobal()
        {
            return new SpeechGlobal(this);
        }
        
        public override void Stop()
        {
            if(synth != null)
                synth.Dispose();
            
            if (recognizer != null)
                recognizer.Dispose();
        }

        public void Say(string text)
        {
            EnsureSynthesizer();

            if(prompt != null)
                synth.SpeakAsyncCancel(prompt);

            prompt = synth.SpeakAsync(text);
        }

        public bool Said(string text)
        {
            EnsureRecognizer();

            if (!recognizerResults.ContainsKey(text))
            {
                var builder = new GrammarBuilder(text);
                recognizer.LoadGrammar(new Grammar(builder));
                recognizerResults[text] = false;
            }

            var result = recognizerResults[text];
            recognizerResults[text] = false;
            
            return result;
        }

        private void EnsureRecognizer()
        {
            if (recognizer == null)
            {
                recognizer = new SpeechRecognizer();
                recognizerResults = new Dictionary<string, bool>();

                recognizer.SpeechRecognized += (s, e) => recognizerResults[e.Result.Text] = true;
            }
        }

        private void EnsureSynthesizer()
        {
            if (synth == null)
            {
                synth = new SpeechSynthesizer();
                synth.SetOutputToDefaultAudioDevice();
            }
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

        public bool said(string text)
        {
            return plugin.Said(text);
        }
    }
}
