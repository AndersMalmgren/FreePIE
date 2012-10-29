using System;
using System.IO.MemoryMappedFiles;
using System.Runtime.InteropServices;
using System.Threading;
using FreePIE.Core.Plugins;
using FreePIE.Tests.Test;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FreePIE.Tests.Core.Plugins
{
    [TestClass]
    public class When_reading_freetrack_data : TestBase
    {
        private bool run = true;

        [TestInitialize]
        public void Context()
        {
            var reset = new AutoResetEvent(false);
            var count = 0;
            var rand = new Random();

            ThreadPool.QueueUserWorkItem(o =>
            {
                
                using (var memoryMappedFile = MemoryMappedFile.CreateOrOpen("FT_SharedMem", Marshal.SizeOf(typeof(FreeTrackData))))
                {
                    using (var accessor = memoryMappedFile.CreateViewAccessor())
                    {

                        while (run)
                        {
                            FreeTrackData model = new FreeTrackData();

                           
                            count++;
                            model.DataID = count;
                            model.Yaw = rand.Next(0, 5);
                            model.Pitch = rand.Next(5, 10);
                            model.Roll = rand.Next(10, 15);
                            model.X = rand.Next(15, 20);
                            model.Y = rand.Next(20, 25);
                            model.Z = rand.Next(25, 30);

                            accessor.Write(0, ref model);
                     
                            reset.Set();
                        }
                    }
                }
            });
            reset.WaitOne();
        }

        [TestMethod]
        public void It_should_read_all_values_from_free_track()
        {

            var plugin = Get<FreeTrackPlugin>();
            var global = plugin.CreateGlobal() as FreeTrackGlobal;
            plugin.Start();

            for (int i = 0; i < 10; i++)
            {
                Assert.IsTrue(global.yaw < 5);
                Assert.IsTrue(global.pitch < 10);
                Assert.IsTrue(global.roll < 15);
                Assert.IsTrue(global.x < 20);
                Assert.IsTrue(global.y < 25);
                Assert.IsTrue(global.z < 30);
            }

            plugin.Stop();
            run = false;
        }
    }
}
