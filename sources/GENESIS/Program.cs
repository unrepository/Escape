using GENESIS.Simulation.PlateTectonics;
using Silk.NET.Maths;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Color = System.Drawing.Color;

var ptData = new PlateTectonicsData() {
	Map = new Dictionary<Vector2D<int>, TectonicPlateTile>(),
	MapSize = new Vector2D<int>(1024, 512),
	Seed = 548589
};

var rand = new Random(ptData.Seed);

for(int i = 0; i < 20; i++) {
	ptData.Map[new Vector2D<int>(rand.Next(0, ptData.MapSize.X), rand.Next(0, ptData.MapSize.Y))]
		= new TectonicPlateTile() {
			PlateIndex = i,
			IsBase = true
		};
}

var pt = new PlateTectonicsSimulation(ptData);

var colors = new Dictionary<int, byte[]>();

using(Image<Rgba32> img = new Image<Rgba32>(ptData.MapSize.X, ptData.MapSize.Y)) {
	for(int y = 0; y < ptData.MapSize.Y; y++) {
		for(int x = 0; x < ptData.MapSize.X; x++) {
			if(!pt.CurrentState.Data.Map.TryGetValue(new Vector2D<int>(x, y), out var t)) {
				img[x, y] = new Rgba32(0, 0, 0, 255);
				continue;
			}

			byte[] c;

			if(!colors.TryGetValue(t.PlateIndex, out c)) {
				c = new byte[4];
				rand.NextBytes(c);
				colors[t.PlateIndex] = c;
			}

			img[x, y] = new Rgba32(c[0], c[1], c[2], 255);
		}
	}
	
	img.Save("res.png");
}

// while(true) {
// 	Console.Clear();
// 	for(int y = 0; y < ptData.MapSize.Y; y++) {
// 		for(int x = 0; x < ptData.MapSize.X; x++) {
// 			Console.ForegroundColor = ConsoleColor.White;
// 			Console.BackgroundColor = ConsoleColor.Black;
// 		
// 			if(!pt.CurrentState.Data.Map.TryGetValue(new Vector2D<int>(x, y), out var t)) {
// 				Console.Write(".");
// 				continue;
// 			}
//
// 			Console.BackgroundColor = (ConsoleColor) t.PlateIndex + 1;
// 			if(t.IsEdge) Console.BackgroundColor = ConsoleColor.White;
// 			if(t.IsBase) Console.ForegroundColor = ConsoleColor.Black;
// 			//else Console.ForegroundColor = ConsoleColor.White;
// 			Console.Write(t.PlateIndex);
// 		}
//
// 		Console.BackgroundColor = ConsoleColor.Black;
// 		Console.Write("\n");
// 	}
//
// 	Console.ReadKey();
// 	pt.TickSingle();
// }

// foreach(var (k, v) in pt.CurrentState.Data.Map) {
// 	Console.WriteLine($"{k}: {v.PlateIndex}");
// }
