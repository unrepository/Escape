using System.Numerics;
using Arch.Core;
using Arch.Core.Extensions;
using Cinenic.Components;
using Cinenic.Extensions.CSharp;
using Hexa.NET.ImGui;
using Hexa.NET.ImGuizmo;
using ImGui_ = Hexa.NET.ImGui.ImGui;

namespace Cinenic.Extensions.Debugging.Providers {
	
	public class TransformGizmoProvider : DebugInfoProvider {

		public override string Title => "Transform Editor";
		public override ImGuiWindowFlags WindowFlags =>
			ImGuiWindowFlags.NoDecoration
			| ImGuiWindowFlags.NoDocking
			| ImGuiWindowFlags.NoMove
			| ImGuiWindowFlags.NoSavedSettings;

		public World World { get; }
		public Camera3D Camera3D { get; }

		public ImGuizmoOperation Operation { get; private set; } = ImGuizmoOperation.Translate;
		public ImGuizmoMode Mode { get; private set; } = ImGuizmoMode.Local;
		
		private QueryDescription _entity3DQuery = new QueryDescription().WithAll<Transform3D>();

		public TransformGizmoProvider(World world, Camera3D camera3d) {
			World = world;
			Camera3D = camera3d;
		}
		
		public unsafe override void Render() {
			if(Controller is null) return;
			
			// if(ImGui_.Button("U")) Operation = ImGuizmoOperation.Universal;
			// ImGui_.SameLine();
			if(ImGui_.Button("Translate")) Operation = ImGuizmoOperation.Translate;
			ImGui_.SameLine();
			if(ImGui_.Button("Rotate")) Operation = ImGuizmoOperation.Rotate;
			ImGui_.SameLine();
			if(ImGui_.Button("Scale")) Operation = ImGuizmoOperation.Scale;

			if(ImGui_.Button("Local")) Mode = ImGuizmoMode.Local;
			ImGui_.SameLine();
			if(ImGui_.Button("World")) Mode = ImGuizmoMode.World;
			
			ImGuizmo.SetImGuiContext(Controller.Context);
			ImGuizmo.BeginFrame();
			ImGuizmo.SetOrthographic(false);
			ImGuizmo.SetDrawlist(ImGui_.GetWindowDrawList());
			ImGuizmo.Enable(true);
			
			var wPos = ImGui_.GetWindowPos();
			var wSize = ImGui_.GetWindowSize();
			
			ImGuizmo.SetRect(wPos.X, wPos.Y, wSize.X, wSize.Y);
			
			World.Query(in _entity3DQuery, (Entity e, ref Transform3D t3d) => {
				ImGuizmo.PushID(e.Id);
				
				var view = Camera3D.Camera.ViewMatrix;
				var projection = Camera3D.Camera.ProjectionMatrix;
				var matrix = t3d.GlobalMatrix;
				
				ImGuizmo.Manipulate(
					&view.M11, &projection.M11,
					Operation, Mode,
					&matrix.M11
				);

				if(ImGuizmo.IsUsing()) {
					var newMatrix = matrix;

					Entity? parent;
					if((parent = e.GetParent()) is not null && parent.Value.TryGet<Transform3D>(out var pt3d)) {
						Matrix4x4.Invert(pt3d.GlobalMatrix, out var pGlobalMatrix);

						newMatrix =
							Operation == ImGuizmoOperation.Translate
								? pGlobalMatrix + matrix
								: pGlobalMatrix * matrix;
					}

					Matrix4x4.Decompose(
						newMatrix,
						out var scale,
						out var rotation,
						out var translation
					);

					switch(Operation) {
						case ImGuizmoOperation.Translate:
							t3d.Position = translation;
							break;
						case ImGuizmoOperation.Rotate:
							t3d.Rotation = rotation;
							break;
						case ImGuizmoOperation.Scale:
							t3d.Scale = scale;
							break;
					}
				}
				
				ImGuizmo.PopID();
			});
		}
	}
}
