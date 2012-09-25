namespace FreePIE.Core.Plugins.TrackIR
{
    public class HeadPoseData
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }
        public float Yaw { get; set; }
        public float Pitch { get; set; }
        public float Roll { get; set; }

        protected bool Equals(HeadPoseData other)
        {
            return X.Equals(other.X) && Y.Equals(other.Y) && Z.Equals(other.Z) && Yaw.Equals(other.Yaw) && Pitch.Equals(other.Pitch) && Roll.Equals(other.Roll);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((HeadPoseData) obj);
        }

        public override string ToString()
        {
            return "yaw: " + Yaw + " pitch: " + Pitch + "roll: " + Roll + " x: " + X + " y: " + Y + " z: " + Z;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = X.GetHashCode();
                hashCode = (hashCode*397) ^ Y.GetHashCode();
                hashCode = (hashCode*397) ^ Z.GetHashCode();
                hashCode = (hashCode*397) ^ Yaw.GetHashCode();
                hashCode = (hashCode*397) ^ Pitch.GetHashCode();
                hashCode = (hashCode*397) ^ Roll.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(HeadPoseData left, HeadPoseData right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(HeadPoseData left, HeadPoseData right)
        {
            return !Equals(left, right);
        }

        public void CopyFrom(HeadPoseData data)
        {
            this.Yaw = data.Yaw;
            this.Pitch = data.Pitch;
            this.Roll = data.Roll;
            this.X = data.X;
            this.Y = data.Y;
            this.Z = data.Z;
        }
    }
}