using NLog;

namespace Cinenic.Resources {
	
	public interface IRefCounted : IDisposable {

		private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
		
		uint ReferenceCount { get; protected set; }
		bool IsValidObject { get; protected set; }
		
		public delegate void FreedEventHandler(IRefCounted sender);
		public event FreedEventHandler? Freed;

		public void NewReference() {
			ReferenceCount++;
			IsValidObject = true;
			
			_logger.Trace("new reference to {Type}", GetType());
		}

		public void FreeReference() {
			ReferenceCount--;
			
			_logger.Trace("freed reference to {Type}", GetType());

			if(ReferenceCount <= 0) {
				_logger.Debug("disposing ref-counted object: {Type}", GetType());
				IsValidObject = false;
				Dispose();
			}
		}
	}
}
