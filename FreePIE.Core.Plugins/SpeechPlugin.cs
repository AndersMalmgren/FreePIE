﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Speech.Recognition;
using System.Speech.Synthesis;
using FreePIE.Core.Contracts;

namespace FreePIE.Core.Plugins
{
    [GlobalType(Type = typeof (SpeechGlobal))]
    public class SpeechPlugin : Plugin
    {
        private SpeechSynthesizer synth;
        private Prompt prompt;

        private SpeechRecognitionEngine recognitionEngine;
        private Dictionary<string, RecognitionInfo> recognizerResults;
        private string[] voices;


        public override object CreateGlobal()
        {
            return new SpeechGlobal(this);
        }

        public override void Stop()
        {
            if (synth != null)
                synth.Dispose();

            if (recognitionEngine != null)
            {
                recognitionEngine.RecognizeAsyncStop();
                recognitionEngine.UnloadAllGrammars();
                recognitionEngine.Dispose();
            }
        }

        public void SelectVoice(string name)
        {
            EnsureSynthesizer();
            synth.SelectVoice(name);
        }

        public void Say(string text)
        {
            EnsureSynthesizer();

            if (prompt != null)
                synth.SpeakAsyncCancel(prompt);

            prompt = synth.SpeakAsync(text);
            Speaking = true;
        }

        public bool Said(string text, float confidence)
        {
            if(confidence < 0.0 || confidence > 1.0) throw new ArgumentException("Confidence has to be between 0.0 and 1.0");

            var init = EnsureRecognizer();

            if (!recognizerResults.ContainsKey(text))
            {
                var builder = new GrammarBuilder(text);
                recognitionEngine.LoadGrammarAsync(new Grammar(builder));
                recognizerResults[text] = new RecognitionInfo(confidence);
            }

            if (init)
            {
                recognitionEngine.SetInputToDefaultAudioDevice();
                recognitionEngine.RecognizeAsync(RecognizeMode.Multiple);
            }
            var info = recognizerResults[text];
            var result = info.Result;
            info.Result = false;

            return result;
        }

        public string[] Voices
        {
            get
            {
                EnsureSynthesizer();
                return voices;
            }
        }

        private bool EnsureRecognizer()
        {
            var result = recognitionEngine == null;

            if (recognitionEngine == null)
            {
                recognitionEngine = new SpeechRecognitionEngine();
                recognizerResults = new Dictionary<string, RecognitionInfo>();

                recognitionEngine.SpeechRecognized += (s, e) =>
                {
                    var info = recognizerResults[e.Result.Text];

                    if (e.Result.Confidence >= info.Confidence)
                        info.Result = true;
                };

            }

            return result;
        }

        public bool Speaking { get; private set; }

        private void EnsureSynthesizer()
        {
            if (synth == null)
            {
                synth = new SpeechSynthesizer();
                synth.SetOutputToDefaultAudioDevice();
                synth.SpeakCompleted += (s, e) => Speaking = false;
                voices = synth.GetInstalledVoices().Where(v => v.Enabled).Select(v => v.VoiceInfo.Name).ToArray();
            }
        }

        public override string FriendlyName
        {
            get { return "Speech"; }
        }

        private class RecognitionInfo
        {
            public bool Result { get; set; }
            public float Confidence { get; private set; }

            public RecognitionInfo(float confidence)
            {
                Confidence = confidence;
            }
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

        public bool speaking { get { return plugin.Speaking; } }

        public bool said(string text)
        {
            return plugin.Said(text, 0.9f);
        }

        public bool said(string text, float confidence)
        {
            return plugin.Said(text, confidence);
        }

        public void selectVoice(string name)
        {
            plugin.SelectVoice(name);
        }

        public string[] voices => plugin.Voices;
    }
}