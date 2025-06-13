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

		public bool AllowZooming { get; set; } = true;
		public bool AllowPanning { get; set; } = true;
		public bool AllowRotating { get; set; } = true;
		
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
				if(!AllowZooming) return;

				int multiplier = 1;
				if(MovementKeyboard?.IsKeyPressed(Key.ShiftLeft) == true) multiplier = 10;
				if(MovementKeyboard?.IsKeyPressed(Key.ControlLeft) == true) multiplier = 50;
				if(MovementKeyboard?.IsKeyPressed(Key.AltLeft) == true) multiplier = 100;
				
				Distance -= scroll.Y * multiplier;
				Distance = MathF.Max(Distance, 1);
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

			var mouseCaptured = false;
			
			if(_window.HasImGuiContext()) {
				ImGui.SetCurrentContext(_window.CreateImGui());
				if(ImGui.GetIO().WantCaptureMouse) mouseCaptured = true;
			}

			if(!mouseCaptured) {
				// rotation
				if(AllowRotating && MovementMouse.IsButtonPressed(MouseButton.Left)) {
					Camera.Yaw -= (float) (deltaX * delta);
					Camera.Pitch += (float) (deltaY * delta);
			
					Camera.Pitch = Math.Clamp(Camera.Pitch, -89, 89);
				}

				// translation X/Z
				if(AllowPanning && MovementMouse.IsButtonPressed(MouseButton.Right)) {
					float prevY = Target.Y;
				
					Target -= Vector3.Multiply(Camera.ViewMatrix.PositiveX(),
						(float) (deltaX * delta * (Sensitivity / 50.0)));
					Target -= Vector3.Multiply(Camera.ViewMatrix.PositiveZ(),
						(float) (deltaY * delta * (Sensitivity / 50.0)));
				
					Target.Y = prevY;
				}
			
				// translation Y
				if(AllowPanning && MovementMouse.IsButtonPressed(MouseButton.Middle)) {
					Target.Y -= (float) (deltaY * delta * (Sensitivity / 50.0));
				}
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
