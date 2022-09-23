using System;
using System.Collections.Generic;
using System.Diagnostics;
using JonesCorp.Data;
using JonesCorp.Utility;
//using log4net;

namespace JonesCorp
{
    public class ScpDriver : WinUsbDevice, IScpDriver
    {
        //private static readonly ILog _log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        //public event EventHandler<DebugEventArgs> Debug;
        private int _offset;

        
        
        public const int SCP_BUS_WIDTH = 4;
        public const int SCP_REPORT_SIZE = 28;
        public const int SCP_RUMBLE_SIZE = 8;

        public const string GUID_DEVINTERFACE_SCPVBUS = "{F679F562-3164-42CE-A4DB-E7DDBE723909}";

        private readonly object _plugLock = new object();

        private readonly List<int> _plugged;

        public ScpDriver():base(GUID_DEVINTERFACE_SCPVBUS)
        {
            _plugged = new List<int>();
            
        }


        

        private bool _isPaused;
        

        public bool PauseToggle()
        {
            if (_isPaused)
                return Resume();
            else
                return Suspend();
        }

        public bool Resume()
        {
            Console.WriteLine("Resume not implemented");
            return Start();
            //return true;
        }

        public bool Suspend()
        {
            //throw new NotImplementedException();
            Console.WriteLine("Suspend not implemented");
            return Stop();

            //return true;
        }

        protected override bool OnOpen(int instance)
        {
            _offset = instance * SCP_BUS_WIDTH;

            return true;
            
        }



        



        /// <summary>
        /// unplugs any controllers
        /// </summary>
        /// <returns></returns>
        protected override bool OnStop()
        {
            bool retval = false;
            var items = new Queue<int>();

            lock (_plugged)
            {
                foreach (int controllerNumber in _plugged)
                    items.Enqueue(controllerNumber - _offset);
            }

            int itemsToUnplug = items.Count;
            Console.WriteLine($"Ready to unplug {itemsToUnplug} virtual controllers");

            int unplugged = 0;
            while (items.Count > 0)
            {
                if (Unplug(items.Dequeue()))
                    unplugged++;
            }

            if (unplugged == itemsToUnplug)
            {
                retval = true;
            }
            else
            {
                Console.WriteLine("Unplugged {0} of {1} virtual controllers", unplugged, itemsToUnplug);
            }


            return retval;
        }

       
        protected override bool OnClose()
        {
            return true;
        }

        

        #region SCP Driver IO

        /// <summary>
        /// Plug the virtual controller In
        /// </summary>
        /// <param name="playerNumber"></param>
        /// <returns></returns>
        public bool Plugin(int playerNumber)
        {
            bool retVal = false;

            if (playerNumber < 1 || playerNumber > SCP_BUS_WIDTH)
            {
                Console.WriteLine($"Playernumber must be between 1 and {SCP_BUS_WIDTH}.\r\n{playerNumber} was passed");
                return false;
            }
            

            playerNumber += _offset;

            

            if (CurrentState != ConnectionState.Disconnected)
            {
                lock (_plugLock)
                {
                    if (!_plugged.Contains(playerNumber))
                    {
                        int bytesTransfered = 0;
                        byte[] buffer = new byte[16];

                        buffer[0] = 0x10;
                        buffer[1] = 0x00;
                        buffer[2] = 0x00;
                        buffer[3] = 0x00;

                        playerNumber.WriteBytes(buffer, 4);
                        
                        int errorcode;
                        
                        if (DeviceIoControl(IOCTL_BUSENUM.PLUGIN_HARDWARE, buffer, ref bytesTransfered, out errorcode))
                        {
                            _plugged.Add(playerNumber);
                            

                            retVal = true;
                            Console.WriteLine($"-- Bus Plugin : controllerNumber {playerNumber}");
                        }
                        else
                        {
                            if (errorcode > 0)// && retry == 0)
                            {
                                Console.WriteLine($"!!!ERROR: Plugin(controllerNumber:{playerNumber}) => DeviceIoControl - Code: {errorcode}");
                                retVal = true;// false;  setting to false may cause problems
                            }
                            else
                            {
                                Console.WriteLine("Controller {0} already plugged",playerNumber);
                                retVal = true;
                            }

                            
                            _plugged.Add(playerNumber);
                            

                        }
                    }
                    else retVal = true;
                }
            }

            if(_plugged.Count > 0)
                CurrentState = ConnectionState.Started;

            return retVal;
        }

        /// <summary>
        /// Unplug the virtual controller In
        /// </summary>
        /// <param name="playerNumber">the id of the controller int from 1 - 4</param>
        /// <returns></returns>
        public bool Unplug(int playerNumber)
        {
            bool retval = false;
            playerNumber += _offset;
            if (playerNumber < 1 || playerNumber > SCP_BUS_WIDTH)
            {
                Console.WriteLine($"playernumber MultilineStringConverter be between 1 and {SCP_BUS_WIDTH}. {playerNumber} was passed");
                return false;
            }

            if (CurrentState != ConnectionState.Disconnected)
            {
                lock (_plugLock)
                {
                    if (_plugged.Contains(playerNumber))
                    {
                        int bytesTransfered = 0;
                        byte[] buffer = new byte[16];

                        buffer[0] = 0x10;
                        buffer[1] = 0x00;
                        buffer[2] = 0x00;
                        buffer[3] = 0x00;
                        playerNumber.WriteBytes(buffer, 4);

                        int errorcode;

                        if (DeviceIoControl(IOCTL_BUSENUM.UNPLUG_HARDWARE, buffer, ref bytesTransfered, out errorcode))
                        {
                            retval = true;
                            _plugged.Remove(playerNumber);

                            Console.WriteLine("-- Bus Unplug : controllerNumber {0}", playerNumber);
                            
                        }
                        else
                        {
                            Console.WriteLine("!!!ERROR: Unplug(controllerNumber:{0}) => DeviceIoControl - Code: {1}", playerNumber, errorcode);
                            
                        }


                    }
                    else
                        retval = true;
                }
            }

            if (_plugged.Count == 0)
                CurrentState = ConnectionState.Opened;

            return retval;
        }



        /// <summary>
        /// Send the xbox report to the device driver. see offset enum for data locations <see cref="SCP_REPORT.DsOffset"/>
        /// </summary>
        /// <param name="xinput">28 xbox report</param>
        /// <param name="rumbleOutput">2 byte output buffer to fill with rumble data</param>
        /// <returns>0 if no error </returns>
        public int Report(SCP_XINPUT_DATA xinput, byte[] rumbleOutput)
        {

            int errorCode = 0;

            if (CurrentState == ConnectionState.Started)
            {
                //xinput.ControllerNumber = (xinput.ControllerNumber == 2) ? 1 : 2;
                int bytesTransfered=0;

                DeviceIoControl(IOCTL_BUSENUM.REPORT_HARDWARE, xinput.GetBytes(), rumbleOutput,ref bytesTransfered, out errorCode);

                //_cache[xinput.ControllerNumber - 1].XinputData = xinput;
                
            }


            return errorCode;
        }

        #endregion
      
        
    }
}
