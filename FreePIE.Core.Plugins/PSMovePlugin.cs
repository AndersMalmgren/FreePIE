using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using FreePIE.Core.Contracts;
using FreePIE.Core.Plugins.Globals;
using FreePIE.Core.Plugins.PSMove;
using FreePIE.Core.Plugins.SensorFusion;
using FreePIE.Core.Plugins.Strategies;

namespace FreePIE.Core.Plugins
{
    [GlobalType(Type = typeof(PSMoveGlobal), IsIndexed = true)]
    public class PSMovePlugin : Plugin
    {
        private Dictionary<int, PSMoveGlobalHolder> globalHolders; 

        public override object CreateGlobal()
        {
            globalHolders = new Dictionary<int, PSMoveGlobalHolder>();
            return new GlobalIndexer<PSMoveGlobal>(id =>
            {
                globalHolders[id] = new PSMoveGlobalHolder();
                return globalHolders[id].Global;
            });
        }

        public override string FriendlyName { get { return "PS Move"; } }

        public override Action Start()
        {
            Api.init();
            Api.SubscribeMoveUpdate(MoveUpdateCallback, MoveKeyDownCallback, MoveKeyUpCallback, NavUpdateCallback, NavKeyDownCallback, NavKeyUpCallback);

            return null;
        }

        public override void DoBeforeNextExecute()
        {
            foreach (var global in globalHolders.Values)
                global.Update();
        }

        public override void Stop()
        {
            Api.unsubscribeMove();
        }

        private void MoveUpdateCallback(int id, Api.Vector3 position, Api.Quaternion orientation, int trigger)
        {
            Call(id, g => g.MoveUpdate(position, orientation, trigger));

            //UNCOMMENT if you want to see the position and orientation updates
            Console.WriteLine("UPDATE moveid: " + id +
                " position: " + position.x + " " + position.y + " " + position.z +
                " orientation:" + orientation.w + " " + orientation.x + " " + orientation.y + " " + orientation.z + " trigger: " + trigger);
        }

        private void MoveKeyUpCallback(int id, int keyCode)
        {
            Call(id, g => g.ButtonHandler.KeyUpUpdate(keyCode));

            Console.WriteLine("KEYUP moveid: " + id + " keyname: " + Enum.GetName(typeof(MoveButton), keyCode));
        }

        private void MoveKeyDownCallback(int id, int keyCode)
        {
            Call(id, g => g.ButtonHandler.KeyDownUpdate(keyCode));

            Console.WriteLine("KEYDOWN moveid: " + id + " keyname: " + Enum.GetName(typeof(MoveButton), keyCode));
        }

        private void NavUpdateCallback(int id, int trigger1, int trigger2, int stickX, int stickY)
        {
            Call(id, g => g.NavUpdate(trigger1, trigger2, stickX, stickY));

            //UNCOMMENT if you want to see the trigger and stick updates
            Console.WriteLine("UPDATE navid: " + id + " trigger1: " + trigger1 + " trigger2: " + trigger2 +
                " stickX: " + stickX + " stickY:" + stickY);
        }

        private void NavKeyUpCallback(int id, int keyCode)
        {
            Call(id, g => g.NavButtonHandler.KeyUpUpdate(keyCode));

            Console.WriteLine("NAV KEYUP navid: " + id + " keyname: " + Enum.GetName(typeof(MoveButton), keyCode));
        }

        private void NavKeyDownCallback(int id, int keyCode)
        {
            Call(id, g => g.NavButtonHandler.KeyDownUpdate(keyCode));

            Console.WriteLine("NAV KEYDOWN navid: " + id + " keyname: " + Enum.GetName(typeof(MoveButton), keyCode));
        }

        private void Call(int id, Action<PSMoveGlobalHolder> call)
        {
            if (globalHolders.ContainsKey(id))
                call(globalHolders[id]);
        }
    }

    public class MoveButtonHandler
    {
        private readonly Dictionary<MoveButton, bool> buttonState = new Dictionary<MoveButton, bool>();
        private readonly GetPressedStrategy<MoveButton> getPressedStrategy;

        public MoveButtonHandler()
        {
            getPressedStrategy = new GetPressedStrategy<MoveButton>(IsButtonDown);
        }

        public bool IsButtonDown(MoveButton button)
        {
            return buttonState.ContainsKey(button) && buttonState[button];
        }

        public bool IsButtonPressed(MoveButton button)
        {
            return getPressedStrategy.IsPressed(button);
        }

        public void KeyUpUpdate(int keyCode)
        {
            buttonState[(MoveButton)keyCode] = false;
        }

        public void KeyDownUpdate(int keyCode)
        {
            buttonState[(MoveButton)keyCode] = true;
        }
    }

    public class PSMoveGlobalHolder : IUpdatable
    {
        private readonly Quaternion quaternion = new Quaternion();
        private readonly MoveButtonHandler buttonHandler = new MoveButtonHandler();
        public MoveButtonHandler ButtonHandler { get { return buttonHandler; } }

        private readonly MoveButtonHandler navButtonHandler = new MoveButtonHandler();
        public MoveButtonHandler NavButtonHandler { get { return navButtonHandler; } }

        public PSMoveGlobalHolder()
        {
            Global = new PSMoveGlobal(this);
        }

        public void Update()
        {
            if (NewData)
                OnUpdate();
        }

        public bool NewData { get; set; }
        public PSMoveGlobal Global { get; private set; }
        public Action OnUpdate { get; set; }
        public bool GlobalHasUpdateListener { get; set; }

        public void MoveUpdate(Api.Vector3 position, Api.Quaternion q, int trigger)
        {
            Global.x = position.x;
            Global.y = position.y;
            Global.z = position.z;

            quaternion.Update(q.w, q.x, q.y, q.z);

            Global.yaw = quaternion.Yaw;
            Global.pitch = quaternion.Pitch;
            Global.roll = quaternion.Roll;

            Global.trigger = trigger;

            NewData = true;
        }

        public void NavUpdate(int trigger1, int trigger2, int stickX, int stickY)
        {
            Global.trigger1 = trigger1;
            Global.trigger2 = trigger2;
            Global.stickX = stickX;
            Global.stickY = stickY;
        }
    }

    [Global(Name = "psmove")]
    public class PSMoveGlobal : UpdateblePluginGlobal<PSMoveGlobalHolder>
    {
        public PSMoveGlobal(PSMoveGlobalHolder plugin) : base(plugin) { }

        public float x { get; internal set; }
        public float y { get; internal set; }
        public float z { get; internal set; }


        public double yaw { get; internal set; }
    
        public double pitch { get; internal set; }
        public double roll { get; internal set; }

        public int trigger { get; internal set; }
        public int trigger1 { get; internal set; }
        public int trigger2 { get; internal set; }
        public int stickX { get; internal set; }
        public int stickY { get; internal set; }

        public bool getDown(MoveButton button)
        {
            return plugin.ButtonHandler.IsButtonDown(button);
        }

        public bool getPressed(MoveButton button)
        {
            return plugin.ButtonHandler.IsButtonPressed(button);
        }

        public bool getNavDown(MoveButton button)
        {
            return plugin.ButtonHandler.IsButtonDown(button);
        }

        public bool getNavPressed(MoveButton button)
        {
            return plugin.ButtonHandler.IsButtonPressed(button);
        }
    }
}
