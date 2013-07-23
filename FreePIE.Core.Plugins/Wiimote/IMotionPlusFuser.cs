using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AHRS;
using FreePIE.Core.Plugins.SensorFusion;

namespace FreePIE.Core.Plugins.Wiimote
{
    public interface IMotionPlusFuser
    {
        void HandleIMUData(double yawDown, double pitchLeft, double rollLeft, double accX, double accY, double accZ);
        EulerAngles FusedValues { get; }
    }

    public class SimpleIntegrationMotionPlusFuser : IMotionPlusFuser
    {
        private readonly Integrator integrator;

        public SimpleIntegrationMotionPlusFuser()
        {
            integrator = new Integrator(3);
            FusedValues = new EulerAngles(0, 0, 0);
        }

        public void HandleIMUData(double yawDown, double pitchLeft, double rollLeft, double accX, double accY, double accZ)
        {
            integrator.Update(new [] { yawDown, pitchLeft, rollLeft });
            FusedValues = new EulerAngles(integrator.Values[0], integrator.Values[1], integrator.Values[2]);
        }

        public EulerAngles FusedValues { get; private set; }
    }

    public class MahonyMotionPlusFuser : IMotionPlusFuser
    {
        private readonly SamplePeriodCounter motionPlusPeriodCounter;
        private MahonyAHRS mahonyAHRS;

        public MahonyMotionPlusFuser()
        {
            motionPlusPeriodCounter = new SamplePeriodCounter();
        }

        public void HandleIMUData(double yawDown, double pitchLeft, double rollLeft, double accX, double accY, double accZ)
        {
            if (EnsureMahonyReady())
                mahonyAHRS.Update((float)(rollLeft * (Math.PI / 180)), (float)(pitchLeft * (Math.PI / 180)), (float)(yawDown * (Math.PI / 180)), (float)accX, (float)accY, (float)accZ);
        }

        public EulerAngles FusedValues
        {
            get
            {
                if (mahonyAHRS == null)
                    return new EulerAngles(0, 0, 0);

                var calculator = new Quaternion();
                calculator.Update(mahonyAHRS.Quaternion[0], mahonyAHRS.Quaternion[1], mahonyAHRS.Quaternion[2],
                                 mahonyAHRS.Quaternion[3]);
                return new EulerAngles(calculator.Yaw*(180/Math.PI), calculator.Pitch*(180/Math.PI),
                                       calculator.Roll*(180/Math.PI));
            }
        }

        private bool EnsureMahonyReady()
        {
            if (mahonyAHRS != null)
                return true;

            if (motionPlusPeriodCounter.Update())
            {
                mahonyAHRS = new MahonyAHRS(motionPlusPeriodCounter.SamplePeriod);
                motionPlusPeriodCounter.Stop();
                return true;
            }

            return false;
        }
    }
}
