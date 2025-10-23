namespace Escape.Extensions.CSharp {
	
	public static class ThreadScheduler {

		private static readonly Dictionary<Thread, List<Action>> _schedules = [];

		public static void ScheduleAction(this Thread thread, Action action) {
			if(_schedules.TryGetValue(thread, out var actions)) {
				actions.Add(action);
				return;
			}

			_schedules[thread] = [ action ];
		}

		public static void RunSchedules() {
			if(!_schedules.TryGetValue(Thread.CurrentThread, out var actions)) {
				return;
			}

			foreach(var action in new List<Action>(actions)) {
				action.Invoke();
				actions.Remove(action);
			}
		}
	}
}
