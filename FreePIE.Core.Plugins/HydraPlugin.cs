using System;
using System.Collections.Generic;
using System.Threading;
using FreePIE.Core.Contracts;
using FreePIE.Core.Plugins.Hydra;

namespace FreePIE.Core.Plugins
{
    [GlobalType(Type = typeof (HydraPluginGlobal), IsIndexed = true)]
    public class HydraPlugin : Plugin
    {
        private EmulatedData[] emulatedData;

        internal EmulatedData[] EmulatedData
        {
            get
            {
                isWriting = true;
                return emulatedData; 

            }
            set { emulatedData = value; }
        }

        private Sixense.ControllerData[] controller;
        internal Sixense.ControllerData[] Controller
        {
            get
            {
                isReading = true;
                return controller;
            }
            set { controller = value; }
        }

        private Sixense.ControllerAngles[] angles;
        internal Sixense.ControllerAngles[] Angles
        {
            get
            {
                isReading = true;
                return angles;
            }
            set { angles = value; }
        }

        private HydraSpoof hydraSpoof;
        private bool readInitlized;
        private bool isReading;
        private bool isWriting;

        public override object CreateGlobal()
        {

            var globals = new HydraPluginGlobal[2];
            globals[0] = new HydraPluginGlobal(0, this);
            globals[1] = new HydraPluginGlobal(1, this);
            return globals;
        }

        public override Action Start()
        {
            Controller = new Sixense.ControllerData[2];
            Controller[0] = new Sixense.ControllerData();
            Controller[1] = new Sixense.ControllerData();

            Angles = new Sixense.ControllerAngles[2];
            Angles[0] = new Sixense.ControllerAngles();
            Angles[1] = new Sixense.ControllerAngles();

            isReading = false;
            InitHydraWrite();
            return null;
        }

        private void InitHydraWrite()
        {
            hydraSpoof = new HydraSpoof(2);
            EmulatedData = new EmulatedData[2];
        }

        private void InitHydraRead()
        {
            if (Sixense.Init() == Sixense.SUCCESS)
            {

                int attempts = 0;
                int base_found = 0;
                while (base_found == 0 && attempts++ < 2)
                {
                    base_found = Sixense.IsBaseConnected(0);

                    if(base_found == 0)
                        Thread.Sleep(1000);
                }

                if (base_found == 0)
                {
                    Sixense.Exit();
                    throw new Exception("Hydra not attached");
                }

                Sixense.SetActiveBase(0);
            }
            else
                throw new Exception("Failed to initialize Hydra");
        }

        public override void Stop()
        {
            if(isReading && readInitlized)
                Sixense.Exit();
        }

        public override string FriendlyName
        {
            get { return "Razer Hydra"; }
        }

        public override bool GetProperty(int index, IPluginProperty property)
        {
            return false;
        }

        public override bool SetProperties(Dictionary<string, object> properties)
        {
            return true;
        }

        public override void DoBeforeNextExecute()
        {
            if (isWriting)
                hydraSpoof.Write(EmulatedData);

            if (isReading)
            {
                if (!readInitlized)
                {
                    InitHydraRead();
                    readInitlized = true;
                }

                //This method will be executed each iteration of the script
                for (int i = 0; i < 2; i++)
                {
                    var lastSequence = Controller[i].sequence_number;
                    Sixense.GetNewestData(i, out Controller[i]);

                    if (lastSequence == Controller[i].sequence_number)
                        continue;

                    //getEulerAngles() from sixense_math.cpp
                    float h, p, r;
                    float A, B;

                    B =  Controller[i].rot_mat_12;
                    p = (float) Math.Asin(B);
                    A = (float) Math.Cos(p);

                    if (Math.Abs(A) > 0.005f)
                    {
                        h = (float) Math.Atan2(- Controller[i].rot_mat_02 / A,  Controller[i].rot_mat_22 / A); // atan2( D, C )
                        r = (float) Math.Atan2(- Controller[i].rot_mat_10 / A,  Controller[i].rot_mat_11 / A); // atan2( F, E )
                    }
                    else
                    {
                        h = 0;
                        r = (float) Math.Atan2( Controller[i].rot_mat_21,  Controller[i].rot_mat_20); // atan2( F, E ) when B=0, D=1
                    }

                    Angles[i].yaw = -h;
                    Angles[i].pitch = p;
                    Angles[i].roll = -r;

                    OnUpdate();
                }
            }
        }
    }

    [Global(Name = "hydra")]
    public class HydraPluginGlobal : UpdateblePluginGlobal<HydraPlugin>
    {
        private readonly int index;

        public HydraPluginGlobal(int index, HydraPlugin plugin) : base(plugin)
        {
            this.index = index;
        }

        public char side
        {
            get
            {
                switch (plugin.Controller[index].which_hand)
                {
                    case 1:
                        return 'L';
                    case 2:
                        return 'R';
                    default:
                        return '?';
                }
            }
            set { plugin.EmulatedData[index].WhichHand = value == 'L' ? (byte)1 : (byte)2; }
        }

        private void SetButtonState(int button, bool pressed)
        {
            if (pressed)
                plugin.EmulatedData[index].Buttons |= button;
            else
                plugin.EmulatedData[index].Buttons &= ~button;
        }

        public bool one
        {
            get { return (plugin.Controller[index].buttons & Sixense.BUTTON_1) != 0; }
            set { SetButtonState(Sixense.BUTTON_1, value); }
        }

        public bool two
        {
            get { return (plugin.Controller[index].buttons & Sixense.BUTTON_2) != 0; }
            set { SetButtonState(Sixense.BUTTON_2, value); }
        }

        public bool three
        {
            get { return (plugin.Controller[index].buttons & Sixense.BUTTON_3) != 0; }
            set { SetButtonState(Sixense.BUTTON_3, value); }
        }

        public bool four
        {
            get { return (plugin.Controller[index].buttons & Sixense.BUTTON_4) != 0; }
            set { SetButtonState(Sixense.BUTTON_4, value); }
        }

        public bool start
        {
            get { return (plugin.Controller[index].buttons & Sixense.BUTTON_START) != 0; }
            set { SetButtonState(Sixense.BUTTON_START, value); }
        }

        public bool bumper
        {
            get { return (plugin.Controller[index].buttons & Sixense.BUTTON_BUMPER) != 0; }
            set { SetButtonState(Sixense.BUTTON_BUMPER, value); }
        }

        public float trigger
        {
            get { return plugin.Controller[index].trigger; }
            set { plugin.EmulatedData[index].Trigger = value; }
        }

        public bool joybutton
        {
            get { return (plugin.Controller[index].buttons & Sixense.BUTTON_JOYSTICK) != 0; }
            set { SetButtonState(Sixense.BUTTON_JOYSTICK, value); }
        }

        public float joyx
        {
            get { return plugin.Controller[index].joystick_x; }
            set { plugin.EmulatedData[index].JoystickX = value; }
        }

        public float joyy
        {
            get { return plugin.Controller[index].joystick_y; }
            set { plugin.EmulatedData[index].JoystickY = value; }
        }

        public float x
        {
            get { return plugin.Controller[index].pos_x; }
            set { plugin.EmulatedData[index].X = value; }
        }

        public float y
        {
            get { return plugin.Controller[index].pos_y; }
            set { plugin.EmulatedData[index].Y = value; }
        }

        public float z
        {
            get { return plugin.Controller[index].pos_z; }
            set { plugin.EmulatedData[index].Z = value; }
        }

        public float yaw
        {
            get { return plugin.Angles[index].yaw; }
            set { plugin.EmulatedData[index].Yaw = value; }
        }

        public float pitch
        {
            get { return plugin.Angles[index].pitch; }
            set { plugin.EmulatedData[index].Pitch = value; }
        }

        public float roll
        {
            get { return plugin.Angles[index].roll; }
            set { plugin.EmulatedData[index].Roll = value; }
        }

        public float q0
        {
            get { return plugin.Controller[index].rot_quat0; }
        }

        public float q1
        {
            get { return plugin.Controller[index].rot_quat1; }
        }

        public float q2
        {
            get { return plugin.Controller[index].rot_quat2; }
        }

        public float q3
        {
            get { return plugin.Controller[index].rot_quat3; }
        }

        public bool enabled
        {
            get { return plugin.Controller[index].enabled == 1; }
            set { plugin.EmulatedData[index].Enabled = value ? 1 : 0; }
        }

        public bool isDocked
        {
            get { return plugin.Controller[index].is_docked == 1; }
            set { plugin.EmulatedData[index].IsDocked = (byte)(value ? 1 : 0); }
        }

        public void enableHemisphereTracking()
        {
            Sixense.AutoEnableHemisphereTracking(index);
        }
    }
}
