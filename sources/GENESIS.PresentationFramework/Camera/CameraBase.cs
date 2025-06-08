using System.Numerics;
using GENESIS.GPU;
using GENESIS.GPU.Shader;

namespace GENESIS.PresentationFramework.Camera {

	public abstract class CameraBase {

		public Matrix4x4 ProjectionMatrix { get; protected set; }
		public Matrix4x4 ViewMatrix { get; protected set; }

		public Matrix4x4 InverseProjectionMatrix { get; protected set; }
		public Matrix4x4 InverseViewMatrix { get; protected set; }

		public float FieldOfView {
			get => field;
			set {
				field = value;
				RecalculateProjectionMatrix();
			}
		} = 1;
		
		protected float Width { get; private set; }
		protected float Height { get; private set; }

		protected IShaderData<CameraData> ShaderData { get; }

		protected CameraBase(Window window, Shader shader)
			: this(window.Base.FramebufferSize.X, window.Base.FramebufferSize.Y, shader)
		{
			window.Base.FramebufferResize += size => {
				Width = size.X;
				Height = size.Y;
				
				RecalculateProjectionMatrix();
			};
		}

		protected CameraBase(int width, int height, Shader shader) {
			Width = width;
			Height = height;
			
			RecalculateProjectionMatrix();

			unsafe {
				ShaderData = IShaderData.Create(
					shader.Platform,
					0,
					new CameraData(),
					(uint) sizeof(CameraData)
				);
			}
			
			Update();
		}

		protected abstract void RecalculateProjectionMatrix();
		protected abstract void RecalculateViewMatrix();

		public abstract void Update();
	}
}
