using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FreePIE.Core.Contracts;
using FreePIE.Core.Plugins.Globals;
using FreePIE.Core.Plugins.Strategies;
using FreePIE.Core.Plugins.VJoy;

namespace FreePIE.Core.Plugins
{
    [GlobalEnum]
    public enum VJoyPov
    {
        Up = 0,
        Right = 1,
        Down = 2,
        Left = 3,
        Nil = 4
    }

    [GlobalType(Type = typeof(VJoyGlobal), IsIndexed = true)]
    public class VJoyPlugin : Plugin
    {
        private List<VJoyGlobalHolder> holders; 

        public override object CreateGlobal()
        {
            holders = new List<VJoyGlobalHolder>();

            return new GlobalIndexer<VJoyGlobal>(Create);
        }

        private VJoyGlobal Create(int index)
        {
            var holder = new VJoyGlobalHolder(index);
            holders.Add(holder);
            return holder.Global;
        }

        public override string FriendlyName
        {
            get { return "VJoy"; }
        }

        public override Action Start()
        {
            if(!Api.Initialize())
                throw new Exception("Could not connect to VJoy driver");

            return null;
        }

        public override void Stop()
        {
            Api.Dispose();
        }

        public override void DoBeforeNextExecute()
        {
            foreach (var holder in holders)
            {
                holder.SendPressed();
                if(!Api.Update(holder.Index, holder.State))
                    throw new Exception("VJoy driver did not respond correctly");
            }
        }
    }

    public class VJoyGlobalHolder
    {
        private readonly SetPressedStrategy setPressedStrategy;

        public VJoyGlobalHolder(int index)
        {
            Index = index;
            Center();
            Global = new VJoyGlobal(this);
            setPressedStrategy = new SetPressedStrategy(OnPress, OnRelease);
        }

        private void Center()
        {
            var centerPov = (ushort) VJoyPov.Nil;
            SetState(s => { s.POV = (ushort)((centerPov << 12) | (centerPov << 8) | (centerPov << 4) | centerPov); return s; }); 
        }

        public void SetPressed(int button)
        {
            setPressedStrategy.Add(button);
        }

        public void SetButton(int button, bool pressed)
        {
            SetState(s =>
                {

                    if (pressed)
                        s.Buttons |= (uint) (1 << button);
                    else
                        s.Buttons &= (uint) ~(1 << button);

                    return s;
                });
        }

        public void SetPov(int index, VJoyPov pov)
        {
            SetState(s =>
                {
                    s.POV &= (ushort)~((int)0xf << ((3 - index) * 4));
                    s.POV |= (ushort)((int)pov << ((3 - index) * 4));
                    return s;
                });
        }

        public void SendPressed()
        {
            setPressedStrategy.Do();
        }

        private void OnPress(int button)
        {
            SetButton(button, true);
        }

        private void OnRelease(int button)
        {
            SetButton(button, false);
        }

        public void SetState(Func<VJoyState, VJoyState> setState)
        {
            State = setState(State);
        }

        public VJoyGlobal Global { get; private set; }
        public VJoyState State { get; set; }
        public int Index { get; set; }
    }

    [Global(Name = "vJoy")]
    public class VJoyGlobal
    {
        private readonly VJoyGlobalHolder holder;

        public VJoyGlobal(VJoyGlobalHolder holder)
        {
            this.holder = holder;
        }

        public short x
        {
            get { return holder.State.XAxis; }
            set { holder.SetState(s => { s.XAxis = value; return s; }); }
        }

        public short y
        {
            get { return holder.State.YAxis; }
            set { holder.SetState(s => { s.YAxis = value; return s; }); }
        }

        public short z
        {
            get { return holder.State.ZAxis; }
            set { holder.SetState(s => { s.ZAxis = value; return s; }); }
        }

        public short xRotation
        {
            get { return holder.State.XRotation; }
            set { holder.SetState(s => { s.XRotation = value; return s; }); }
        }

        public short yRotation
        {
            get { return holder.State.YRotation; }
            set { holder.SetState(s => { s.YRotation = value; return s; }); }
        }

        public short zRotation
        {
            get { return holder.State.ZRotation; }
            set { holder.SetState(s => { s.ZRotation = value; return s; }); }
        }

        public short slider
        {
            get { return holder.State.Slider; }
            set { holder.SetState(s => { s.Slider = value; return s; }); }
        }

        public short dial
        {
            get { return holder.State.Dial; }
            set { holder.SetState(s => { s.Dial = value; return s; }); }
        }

        public void setButton(int button, bool pressed)
        {
            holder.SetButton(button, pressed);
        }

        public void setPressed(int button)
        {
            holder.SetPressed(button);
        }

        public void setPov(int index, VJoyPov pov)
        {
            holder.SetPov(index, pov);
        }
    }
}
