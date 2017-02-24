using System;

using System.Threading;

namespace RipBot.Services
{
	/// <summary>
	/// Service used to handle the MOTD timer.
	/// </summary>
	public class MOTDTimerService
	{
		private readonly Timer _timer;
		private double _intervalinminutes = 5;
		//private double _intervalinminutes = .20;

		/// <summary>
		/// Event fired when the timer elapses.
		/// </summary>
		public event EventHandler MOTDTimerFired;

		/// <summary>
		/// CStor.
		/// </summary>
		public MOTDTimerService()
		{
			_timer = new Timer(_ =>
			{
				// fire off the event if someone is suscribed
				//Console.WriteLine("Timer fired from MOTDTimerService at " + DateTime.Now.ToString());
				if (MOTDTimerFired != null)
					// someone is subscribed, throw event
					MOTDTimerFired(this, new EventArgs());
			},
			null,
			TimeSpan.FromMinutes(.20),  // Time that first event should fire after bot has started
			TimeSpan.FromMinutes(_intervalinminutes)); // Time after which message should repeat (`Timeout.Infinite` for no repeat)
		}

		/// <summary>
		/// Sets the timers interval in minutes.
		/// </summary>
		/// <param name="intervalinminutes">The number of minutes before the timer fires.</param>
		/// <returns>True if it was able to change it, otherwise false.</returns>
		public bool SetInterval(double intervalinminutes)
		{
			bool ret = false;
			try
			{
				// update the timers interval
				_intervalinminutes = intervalinminutes;
				ret =_timer.Change(TimeSpan.FromMinutes(.20), TimeSpan.FromMinutes(_intervalinminutes));
			}
			catch
			{
				//
			}

			return ret;
		}

		/// <summary>
		/// Stop the timer from firing.
		/// </summary>
		public void Stop()
		{
			_timer.Change(Timeout.Infinite, Timeout.Infinite);
		}

		/// <summary>
		/// Restart the timer.
		/// </summary>
		public void Restart()
		{
			_timer.Change(TimeSpan.FromMinutes(.20), TimeSpan.FromMinutes(_intervalinminutes));
		}
	}
}
