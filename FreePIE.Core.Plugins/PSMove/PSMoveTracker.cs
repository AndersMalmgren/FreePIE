using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FreePIE.Core.Plugins.PSMove
{
    public class PSMoveTracker
    {
        private IntPtr tracker;
        private IntPtr fusion;

        public PSMoveTracker() {
            // Initialize to "null"
            tracker = fusion = IntPtr.Zero;
        }

        public void Start() {
            // Create the tracker object
            tracker = PSMoveAPI.psmove_tracker_new();
            fusion = PSMoveAPI.psmove_fusion_new(tracker, 1, 1000);

            // Disable light bulb auto dimming effect
            // PSMoveAPI.psmove_tracker_set_dimming(tracker, 1);
        }

        public void Stop() {
            PSMoveAPI.psmove_fusion_free(fusion);
            PSMoveAPI.psmove_tracker_free(tracker);
        }

        public void UpdateImage()
        {
            if (tracker != IntPtr.Zero)
                PSMoveAPI.psmove_tracker_update_image(tracker);
        }

        public IntPtr TrackerHandle { get { return this.tracker; } }
        public IntPtr FusionHandle { get { return this.fusion; } }
    }

}
