using System.Diagnostics;
using System.Reflection;
using GENESIS.GPU;
using GENESIS.PresentationFramework;
using GENESIS.PresentationFramework.Dialog;
using GENESIS.Project;
using GENESIS.Simulation;
using Hexa.NET.ImGui;

namespace GENESIS.UI.Project {
	
	public class ObjectManagerWindow : ImGuiScene {

		public ProjectScreen Parent { get; }
		
		public DirectoryInfo SelectedDirectory { get; private set; }
		public IProjectObject? SelectedObject { get; private set; }

		private TextPrompt? _folderCreatePrompt;
		private TextPrompt? _objectCreatePrompt;
		private Type? _objectCreateType;
		
		private MessagePrompt? _folderDeleteConfirmPrompt;
		private MessagePrompt? _fileDeleteConfirmPrompt;

		public ObjectManagerWindow(IPlatform platform, ProjectScreen parent) : base(platform, "project_om") {
			Parent = parent;
			SelectedDirectory = ProjectManager.BaseDirectory;
		}
		
		protected override void Paint(double delta) {
			if(ImGui.Begin("Object Manager")) {
			#region Directory tree
				void DrawDirectoryTree(DirectoryInfo directory) {
					ImGui.PushID(directory.FullName);
					{
						if(ImGui.Selectable(directory.Name, SelectedDirectory.FullName == directory.FullName)) {
							SelectedDirectory = directory;
						}
					}
					ImGui.PopID();
					
					foreach(var subDirectory in directory.GetDirectories()) {
						ImGui.Indent();
						DrawDirectoryTree(subDirectory);
						ImGui.Unindent();
					}
				}
				
				DrawDirectoryTree(ProjectManager.BaseDirectory);
			#endregion
				
				ImGui.SeparatorText($"Objects ({SelectedDirectory.GetFiles().Length})");

			#region Object list
				foreach(var file in SelectedDirectory.EnumerateFiles()) {
					var obj = IProjectObject.CreateFromFile(file);
					
					if(ImGui.Selectable(file.Name, SelectedObject?.File.FullName == file.FullName)) {
						SelectedObject = obj;
					}

					if(ImGui.BeginPopupContextItem((string) null, ImGuiPopupFlags.MouseButtonRight)) {
						if(ImGui.Selectable("Open")) {
							Window!.ScheduleLater(() => obj.Open(Platform, Window));
						}
						
						ImGui.EndPopup();
					}
				}
			#endregion

				ImGui.Button("+");
				if(ImGui.BeginPopupContextItem((string) null, ImGuiPopupFlags.MouseButtonLeft)) {
					if(ImGui.Selectable("Folder")) {
						_folderCreatePrompt = new("New folder...", "Name");
					}
					
					ImGui.Separator();

					if(ImGui.BeginMenu("Simulation")) {
						if(ImGui.MenuItem("Planetary (N-body)")) {
							_objectCreateType = typeof(NBodySimulationObject);
							_objectCreatePrompt = new("New N-body simulation...", "Name");
						}
						
						ImGui.EndMenu();
					}

					if(ImGui.Selectable("Map")) {
						_objectCreateType = typeof(MapObject);
						_objectCreatePrompt = new("New map...", "Name");
					}
					
					ImGui.EndPopup();
				}
				
				ImGui.SameLine();
				ImGui.Button("-");

				if(ImGui.BeginPopupContextItem((string) null, ImGuiPopupFlags.MouseButtonLeft)) {
					if(ImGui.Selectable("Folder")) {
						if(SelectedDirectory != ProjectManager.BaseDirectory) {
							_fileDeleteConfirmPrompt = new(
								"Delete?",
								"WARNING\n"
								+ "This will delete all files PERMANENTLY in the selected directory:\n"
								+ SelectedDirectory.FullName,
								MessagePrompt.Buttons.YesNo
							);
						}
					}

					if(ImGui.Selectable("Object")) {
						Debug.Assert(SelectedObject is not null);
						
						_fileDeleteConfirmPrompt = new(
							"Delete?",
							"WARNING\n"
							+ "This will PERMANENTLY delete the following file:\n"
							+ SelectedObject.File.FullName,
							MessagePrompt.Buttons.YesNo
						);
					}
					
					ImGui.EndPopup();
				}
			}

		#region Prompts
			if(_folderCreatePrompt?.Prompt(false) == true
			   && _folderCreatePrompt.Result is not null)
			{
				SelectedDirectory.CreateSubdirectory(_folderCreatePrompt.Result);
			}
			
			if(_objectCreatePrompt?.Prompt(false) == true
			   && _objectCreatePrompt.Result is not null) {
				Debug.Assert(_objectCreateType is not null);

				var extensionProperty = _objectCreateType.GetProperty("Extension",
					BindingFlags.Public | BindingFlags.Static);

				Debug.Assert(extensionProperty is not null);

				var file = new FileInfo(
					SelectedDirectory.FullName
					+ Path.DirectorySeparatorChar
					+ _objectCreatePrompt.Result
					+ extensionProperty.GetValue(null));

				file.Create().Close();
			}
			
			if(_fileDeleteConfirmPrompt?.Prompt() == true
			   && _fileDeleteConfirmPrompt.Result == MessagePromptResult.Yes)
			{
				SelectedObject?.File.Delete();
				SelectedObject = null;
			}
			
			if(_folderDeleteConfirmPrompt?.Prompt() == true
			   && _folderDeleteConfirmPrompt.Result == MessagePromptResult.Yes
			   && SelectedDirectory != ProjectManager.BaseDirectory)
			{
				var dir = SelectedDirectory;
				
				SelectedDirectory.Delete();
				SelectedDirectory = dir.Parent ?? ProjectManager.BaseDirectory;
			}
		#endregion
			
			ImGui.End();
		}
	}
}
