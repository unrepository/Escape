using System;
using Escape.Extensions.UI.Dialog;
using Escape.Renderer;
using Escape.Resources;

public abstract class ResourceCreator<TResource, TResourceValue, TImportMetadata> : IPromptDialog<TResource>
	where TImportMetadata : ImportMetadata, new()
	where TResource : Resource<TResourceValue, TImportMetadata>
{
	public bool IsOpen { get; set; }
	public TResource? Result { get; protected set; }
	
	public IPlatform Platform { get; }
	public string ResourcePath { get; }
	public string FilePath { get; }

	public ResourceCreator(IPlatform platform, string filePath, string resourcePath) {
		Platform = platform;
		ResourcePath = resourcePath;
		FilePath = filePath;
	}

	public abstract bool Prompt(bool popup = true);
}
