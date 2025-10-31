using System;
using System.IO;
using System.Numerics;
using Arch.Core;
using Escape.Core;
using Escape.Core.Components;
using Escape.Core.Scripting;
using Escape.Extensions.CSharp;
using Escape.Renderer;
using Escape.Renderer.Shader.Pipelines;
using Hexa.NET.ImGui;
using Silk.NET.Maths;

[CSharpScript("ui/SceneEditor.cs")]
public class SceneEditor : CSharpScript {

	public Scene Scene { get; }
	public RenderQueue RenderQueue { get; }

	public RenderPipeline SceneRenderPipeline { get; private set; }
	
	private bool _running = false;
	private Entity? _selectedEntity = null;

	public SceneEditor(IPlatform platform, Scene scene) {
		Scene = scene;
		RenderQueue = RenderQueueManager.Create(platform, "editor_" + Scene.Id);
		
		var framebuffer = Framebuffer.Create(platform, RenderQueue, new Vector2D<uint>(512, 512));
		framebuffer.CreateAttachment(Framebuffer.AttachmentType.Color);
		
		RenderQueue.RenderTarget = framebuffer;
		
		ESCAPE.RenderThread.ScheduleAction(() => {
			SceneRenderPipeline = RenderPipelineManager.Create(
				platform,
				"editor_" + Scene.Id,
				RenderQueue,
				new DefaultPBRShaderPipeline(platform)
			);
			
			Scene.RenderQueue = RenderQueue;
			
			SceneEngine.SetScene(RenderQueue, Scene, doEvents: false);
		});
	}
	
	public override void OnRender(RenderQueue queue, TimeSpan delta) {
		if(ImGui.Begin("Scene editor - " + Scene.Id)) {
			ImGui.BeginDisabled(_running);
			if(ImGui.Button("Run")) {
				Scene.Open();
			}
			ImGui.EndDisabled();
			
			ImGui.SameLine();
			
			ImGui.BeginDisabled(!_running);
			if(ImGui.Button("Stop")) {
				Scene.Close();
			}
			ImGui.EndDisabled();
			
			ImGui.Columns(3, true);

			{
				// world entity tree
				void DrawEntityTree(Entity entity) {
					var entityLabel = entity.GetName() ?? entity.Id.ToString();

					if(ImGui.Selectable(entityLabel)) {
						_selectedEntity = entity;
					}
					
					ImGui.TreePush(entityLabel);

					foreach(var child in entity.GetChildren()) {
						DrawEntityTree(child);
					}
					
					ImGui.TreePop();
				}
			
				DrawEntityTree(Scene.World.GetRootEntity());
			}
			
			ImGui.NextColumn();

			{
				// scene output
				var pos = ImGui.GetCursorScreenPos();

				/*unsafe {
					ImGui.GetWindowDrawList().AddImage(
						new ImTextureRef(texId: new ImTextureID(RenderQueue.RenderTarget!.GetTextureAttachments()[0].Handle)),
						pos,
						new Vector2(pos.X + 512, pos.Y + 512),
						new Vector2(0, 1),
						new Vector2(1, 0)
					);
				}*/
			}
			
			ImGui.NextColumn();

			{
				if(_selectedEntity is not null) {
					ImGui.Text(_selectedEntity.Value.Id.ToString());
				}
			}

			ImGui.End();
		}
	}
}
