using System.Numerics;
using Hexa.NET.ImGui;

namespace GENESIS.PresentationFramework {
	
	public static class ImGuiExtras {
		
		private static readonly Dictionary<string, float[]> _histograms = [];
		private const int HISTOGRAM_RESOLUTION = 512;
		
		public static void UpdateHistogram(string id, float value) {
			float maxValue = value;
			
			if(!_histograms.TryGetValue(id, out var graph)) {
				_histograms[id] = new float[HISTOGRAM_RESOLUTION + 1];
				graph = _histograms[id];
			}
			
			for(int i = HISTOGRAM_RESOLUTION; i >= 1; i--) {
				if(graph[i] > maxValue) maxValue = graph[i];
				graph[i - 1] = graph[i];
				graph[i] = value;
			}
			
			// max value is stored at index 0
			graph[0] = maxValue;
		}

		public static void DrawHistogram(string id, string title, string overlayText, Vector2 size) {
			ImGui.PlotHistogram(
				title,
				ref _histograms[id][1],
				HISTOGRAM_RESOLUTION,
				overlayText, 0, _histograms[id][0],
				size
			);
		}
	}
}
