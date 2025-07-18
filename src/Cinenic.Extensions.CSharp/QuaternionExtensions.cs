using System.Numerics;

namespace Cinenic.Extensions.CSharp {
	
	public static class QuaternionExtensions {
		
		public static float GetYaw(this Quaternion q) {
			float siny_cosp = 2 * (q.W * q.Z + q.X * q.Y);
			float cosy_cosp = 1 - 2 * (q.Y * q.Y + q.Z * q.Z);
			
			return (float) Math.Atan2(siny_cosp, cosy_cosp);
		}

		public static float GetPitch(this Quaternion q) {
			float sinp = 2 * (q.W * q.Y - q.Z * q.X);
			
			if(Math.Abs(sinp) >= 1) {
				return (float) Math.CopySign(Math.PI / 2, sinp);
			}
			
			return (float) Math.Asin(sinp);
		}

		public static float GetRoll(this Quaternion q) {
			float sinr_cosp = 2 * (q.W * q.X + q.Y * q.Z);
			float cosr_cosp = 1 - 2 * (q.X * q.X + q.Y * q.Y);
			
			return (float) Math.Atan2(sinr_cosp, cosr_cosp);
		}
		
		public static void SetYaw(ref this Quaternion q, float yaw) {
			q = Quaternion.CreateFromYawPitchRoll(yaw, q.GetPitch(), q.GetRoll());
		}

		public static void SetPitch(ref this Quaternion q, float pitch) {
			q = Quaternion.CreateFromYawPitchRoll(q.GetYaw(), pitch, q.GetRoll());
		}

		public static void SetRoll(ref this Quaternion q, float roll) {
			q = Quaternion.CreateFromYawPitchRoll(q.GetYaw(), q.GetPitch(), roll);
		}

		public static void SetYawPitchRoll(ref this Quaternion q, float yaw, float pitch, float roll) {
			q = Quaternion.CreateFromYawPitchRoll(yaw, pitch, roll);
		}
	}
}
