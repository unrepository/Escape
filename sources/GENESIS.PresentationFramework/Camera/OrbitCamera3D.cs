using System.Numerics;
using GENESIS.GPU;
using GENESIS.GPU.Shader;
using GENESIS.LanguageExtensions;
using GENESIS.PresentationFramework.Extensions;
using Hexa.NET.ImGui;
using Silk.NET.Core;
using Silk.NET.Input;

namespace GENESIS.PresentationFramework.Camera {
	
	public class OrbitCamera3D {

		public Camera3D Camera { get; set; }

		public double Sensitivity { get; set; } = 10.0f;
		
		public Vector3 Target = Vector3.Zero;
		public float Distance { get; set; } = 10.0f;
		
		public IMouse MovementMouse { get; }
		public IKeyboard? MovementKeyboard { get; }

		private Window _window;
		private Vector2 _lastMousePosition;
		
		public OrbitCamera3D(Camera3D camera, Window window) {
			Camera = camera;
			_window = window;

			if(window.Input!.Mice.Count < 1) {
				throw new PlatformException("At least one mice required");
			}

			MovementMouse = window.Input.Mice[0];
			MovementKeyboard = window.Input.Keyboards[0];

			MovementMouse.Scroll += (_, scroll) => {
				Distance -= scroll.Y;
				Distance = Math.Clamp(Distance, 1, 1000);
			};
			
		#if DEBUG
			DebugScene.DebugInfoSlots["orbit_camera"] = _ => {
				ImGui.Text($"Distance: {Distance}");
				ImGui.Text($"Target: {Target}");
				ImGui.Text($"Position: {Camera.Position}");
				ImGui.Text($"Rotation (yaw/pitch)deg: {Camera.Yaw} {Camera.Pitch}");
			};
		#endif
		}

		public void Update(double delta) {
			var deltaX = (MovementMouse.Position.X - _lastMousePosition.X) * Sensitivity;
			var deltaY = (MovementMouse.Position.Y - _lastMousePosition.Y) * Sensitivity;
			_lastMousePosition = MovementMouse.Position;

			if(_window.HasImGuiContext()) {
				ImGui.SetCurrentContext(_window.CreateImGui());
				if(ImGui.GetIO().WantCaptureMouse) return;
			}

			// rotation
			if(MovementMouse.IsButtonPressed(MouseButton.Left)) {
				Camera.Yaw -= (float) (deltaX * delta);
				Camera.Pitch += (float) (deltaY * delta);
			
				Camera.Pitch = Math.Clamp(Camera.Pitch, -89.9f, 89.9f);
			}

			// translation X/Z
			if(MovementMouse.IsButtonPressed(MouseButton.Right)) {
				float prevY = Target.Y;
				
				Target -= Vector3.Multiply(Camera.ViewMatrix.PositiveX(),
					(float) (deltaX * delta * (Sensitivity / 50.0)));
				Target -= Vector3.Multiply(Camera.ViewMatrix.PositiveZ(),
					(float) (deltaY * delta * (Sensitivity / 50.0)));
				
				Target.Y = prevY;
			}
			
			// translation Y
			if(MovementMouse.IsButtonPressed(MouseButton.Middle)) {
				Target.Y -= (float) (deltaY * delta * (Sensitivity / 50.0));
			}
			
			var offset = new Vector3(
				Distance * MathF.Cos(Camera.Pitch.ToRadians()) * MathF.Sin(Camera.Yaw.ToRadians()),
				Distance * MathF.Sin(Camera.Pitch.ToRadians()),
				Distance * MathF.Cos(Camera.Pitch.ToRadians()) * MathF.Cos(Camera.Yaw.ToRadians())
			);
			
			Camera.Position = Target + offset;
			Camera.LookAt(Target);
		}
	}
}
