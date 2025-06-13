using System.Numerics;
using System.Reflection;
using Hexa.NET.ImGui;

namespace GENESIS.PresentationFramework.Dialog {
	
	public class FilePrompt : IPromptDialog<string> {

		public bool IsOpen { get; set; } = false;
		public string? Result { get; private set; } = null;

		private readonly string _title;
		private readonly string[] _filters;

		private DirectoryInfo _currentDirectory;

		private TextPrompt? _pathPrompt;
		private TextPrompt? _newFilePrompt;
		private TextPrompt? _newDirectoryPrompt;
		private MessageDialog? _messageDialog;

		private FileInfo? _selectedFile = null;
		private string _selectedFileName = "";
		private string _searchPattern = "";
		
		public FilePrompt(string title, DirectoryInfo? startingDirectory = null, string[]? filters = null) {
			_title = title;
			_currentDirectory = startingDirectory ?? new(Directory.GetCurrentDirectory());
			_filters = filters ?? [ "*" ];
			
			IsOpen = true;
		}
		
		public bool Prompt(bool popup = false) {
			if(!IsOpen) return false;
			
			ImGui.SetNextWindowPos(
				ImGui.GetCenter(ImGui.GetMainViewport()),
				ImGuiCond.FirstUseEver,
				new Vector2(0.5f, 0.5f)
			);
			
			// TODO do drives on Windows need to be handled differently?
			ImGui.SetNextWindowSizeConstraints(new Vector2(300, 200), new Vector2(900, 600));
			ImGui.SetNextWindowSize(new Vector2(600, 350), ImGuiCond.Appearing);
			
			if(ImGui.Begin(_title, ImGuiWindowFlags.HorizontalScrollbar | ImGuiWindowFlags.AlwaysAutoResize))
			{
			#region Top bar
				// direct full path
				ImGui.PushID("topBarDirectPath");
				if(ImGui.Button("...")) {
					_pathPrompt = new("Set current path", "Relative or absolute path...");
				}
				ImGui.PopID();
				
			#region Current path
				string[] pathSegments = [ _currentDirectory.FullName.Trim() ];

				if(pathSegments[0].Length > 1) {
					pathSegments = pathSegments[0].Split(Path.DirectorySeparatorChar);
				}

				if(string.IsNullOrWhiteSpace(pathSegments[0])) {
					pathSegments[0] = Path.DirectorySeparatorChar.ToString();
				}
				
				for(int i = 0; i < pathSegments.Length; i++) {
					var pathSegment = pathSegments[i];
					
					ImGui.SameLine();
					
					ImGui.PushID($"topBarPath{i}");
					if(ImGui.Button(pathSegment)) {
						var fullPath = string.Join(Path.DirectorySeparatorChar, pathSegments[..(i + 1)]);
						_currentDirectory = new DirectoryInfo(fullPath);
					}
					ImGui.PopID();
				}
			#endregion
				
				ImGui.SetNextItemWidth(ImGui.GetWindowWidth() - ImGui.GetStyle().WindowPadding.X * 2);
				ImGui.InputTextWithHint("##search", "Search", ref _searchPattern, 128);
			#endregion

				ImGui.Separator();

				ImGui.BeginChild("##fileListing", ImGuiChildFlags.Borders | ImGuiChildFlags.ResizeY);
				{

				#region Directory listing
					if(_currentDirectory.Parent is not null && ImGui.Button("..")) {
						_currentDirectory = _currentDirectory.Parent;
					}

					foreach(var directory in _currentDirectory.EnumerateDirectories()) {
						if(!directory.Name.Contains(_searchPattern)) continue;

						if(ImGui.Button(directory.Name)) {
							_currentDirectory = directory;

							if(_filters.Contains("d")) {
								_selectedFileName = directory.Name;
							}
						}
					}
				#endregion

				#region File listing
					if(!_filters.Contains("d")) {
						foreach(var file in _currentDirectory.EnumerateFiles()) {
							if(!file.Name.Contains(_searchPattern)) continue;

							if(ImGui.Selectable(file.Name, _selectedFile == file)) {
								_selectedFile = file;
								_selectedFileName = file.Name;
							}
						}
					}
				#endregion
				}
				ImGui.EndChild();

			#region Footer
				ImGui.SetNextItemWidth(ImGui.GetWindowWidth() - ImGui.GetStyle().WindowPadding.X * 2);
				ImGui.InputText("##fileName", ref _selectedFileName, 256);

				if(ImGui.Button("OK")) {
					if(!_currentDirectory.Exists && !(_selectedFile?.Exists ?? false)) {
						_messageDialog = new(
							"Invalid file or directory",
							"Selected file or directory does not exist"
						);
					} else {
						if(_filters.Contains("d")) {
							Result = _currentDirectory.FullName;
						} else {
							Result = _selectedFile.FullName;
						}

						IsOpen = false;
					}
				}
				
				ImGui.SameLine();

				if(ImGui.Button("Cancel")) {
					Result = null;
					IsOpen = false;
				}

				ImGui.SameLine();
				ImGui.Button("+");
				
				if(ImGui.BeginPopupContextItem((string) null, ImGuiPopupFlags.MouseButtonLeft)) {
					if(ImGui.Button("New file...")) {
						_newFilePrompt = new("New file...", "File name");
					}

					if(ImGui.Button("New directory...")) {
						_newDirectoryPrompt = new("New directory...", "Directory name");
					}
					
					ImGui.EndPopup();
				}
			#endregion

			}
			ImGui.End();

			if(_pathPrompt?.Prompt() == true) {
				_currentDirectory = new DirectoryInfo(_pathPrompt.Result ?? "");

				if(!_currentDirectory.Exists) {
					_messageDialog = new(
						"Invalid path",
						$"{_currentDirectory.FullName}\ndoes not exist"
					);
				}
			}

			if(_newFilePrompt?.Prompt() == true) {
				var path = _currentDirectory.FullName + Path.DirectorySeparatorChar + _newFilePrompt.Result;
				
				try {
					File.Create(path).Close();
				} catch(Exception e) {
					_messageDialog = new("Error", e.Message);
				}
			}
			
			if(_newDirectoryPrompt?.Prompt() == true) {
				var path = _currentDirectory.FullName + Path.DirectorySeparatorChar + _newDirectoryPrompt.Result;
				
				try {
					Directory.CreateDirectory(path);
				} catch(Exception e) {
					_messageDialog = new("Error", e.Message);
				}
			}

			_messageDialog?.Prompt();

			return !IsOpen;
		}
	}
}
