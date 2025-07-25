using System.Numerics;

namespace Cinenic.Extensions.CSharp {
	
	public static class QuaternionExtensions {
		
		public static float GetYaw(this Quaternion q) {
			q.GetEulerAngles(out var yaw, out _, out _);
			return yaw;
		}

		public static float GetPitch(this Quaternion q) {
			q.GetEulerAngles(out _, out var pitch, out _);
			return pitch;
		}

		public static float GetRoll(this Quaternion q) {
			q.GetEulerAngles(out _, out _, out var roll);
			return roll;
		}
		
	    public static void SetYaw(ref this Quaternion q, float yaw) {
	        q.GetEulerAngles(out _, out var pitch, out var roll);
	        q = Quaternion.CreateFromYawPitchRoll(yaw, pitch, roll);
	    }

	    public static void SetPitch(ref this Quaternion q, float pitch) {
	        q.GetEulerAngles(out var yaw, out _, out var roll);
	        q = Quaternion.CreateFromYawPitchRoll(yaw, pitch, roll);
	    }

	    public static void SetRoll(ref this Quaternion q, float roll) {
	        q.GetEulerAngles(out var yaw, out var pitch, out _);
	        q = Quaternion.CreateFromYawPitchRoll(yaw, pitch, roll);
	    }

	    public static void GetEulerAngles(this Quaternion q, out float yaw, out float pitch, out float roll) {
	        // roll (Z-axis rotation)
	        float sinr_cosp = 2 * (q.W * q.Z + q.X * q.Y);
	        float cosr_cosp = 1 - 2 * (q.Y * q.Y + q.Z * q.Z);
			
	        roll = MathF.Atan2(sinr_cosp, cosr_cosp);

	        // pitch (X-axis rotation)
	        float sinp = 2 * (q.W * q.X - q.Z * q.Y);
			
	        if(Math.Abs(sinp) >= 1) pitch = MathF.PI / 2 * MathF.Sign(sinp);
	        else pitch = MathF.Asin(sinp);

	        // yaw (Y-axis rotation)
	        float siny_cosp = 2 * (q.W * q.Y + q.X * q.Z);
	        float cosy_cosp = 1 - 2 * (q.X * q.X + q.Y * q.Y);
			
	        yaw = MathF.Atan2(siny_cosp, cosy_cosp);
	    }

		public static Vector3 GetDirectionVector(this Quaternion q) {
			return Vector3.Transform(Vector3.UnitZ, q);
		}
	}
}
