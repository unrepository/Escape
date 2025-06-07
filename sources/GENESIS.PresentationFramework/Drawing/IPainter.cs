namespace GENESIS.PresentationFramework.Drawing {
	
	public interface IPainter : IDisposable {
		
		public void BeginDrawList();
		public void EndDrawList();
		public bool SetDrawList(int index);

		public void Paint();
		
		public bool RemoveDrawList(int index);
		public bool ClearDrawList(int index);
		public int Clear();
	}
}
