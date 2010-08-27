using System.Diagnostics;
using System;
using System.Data;
using System.Collections;
using BlackLight;
using System.Threading;
using System.Collections.Generic;
namespace BlackLight
{
	namespace Services
	{
		namespace Timers
		{
			
			/// -----------------------------------------------------------------------------
			/// <summary>
			/// Delegated sub to be called when a timer executes
			/// </summary>
			/// <remarks>
			/// </remarks>
			/// <history>
			/// 	[Caleb]	6/18/2005	Created
			/// </history>
			/// -----------------------------------------------------------------------------
			public delegate void TimedCallBack(Timer TTimer);
			
			
			/// -----------------------------------------------------------------------------
			/// Project	 : BlackLight Services
			/// Class	 : Timer
			///
			/// -----------------------------------------------------------------------------
			/// <summary>
			/// Timer object
			/// </summary>
			/// <remarks>
			/// </remarks>
			/// <history>
			/// 	[Caleb]	6/18/2005	Created
			/// </history>
			/// -----------------------------------------------------------------------------
			public sealed class Timer
			{
				
				public readonly DateTime startTime;
				private int _runs;
				public readonly TimeSpan interval;
				private bool _paused;
				private DateTime _nextRun;
				private int _repeat;
				public readonly TimedCallBack callback;
				public readonly ArrayList @params;
				public Timer(TimeSpan delay, TimeSpan interval, int repeatCount, TimedCallBack callback, params object[] @params) {
					if (repeatCount < -1 || repeatCount == 0)
					{
						throw (new ArgumentException("Invalid repetition count. Use -1 for infinate", "repeatCount"));
					}
					if (callback == null)
					{
						throw (new ArgumentNullException("callback", "Callback cannot be nothing."));
					}
					this.startTime = DateTime.Now.Add(delay);
					this.interval = interval;
					this._nextRun = this.startTime.Add(this.interval);
					this._repeat = repeatCount;
					this.callback = callback;
					this._paused = false;
					this._runs = 0;
					this.@params = new ArrayList(@params);
				}
				public DateTime nextRun
				{
					get{
						return _nextRun;
					}
				}
				public int repeat
				{
					get{
						return _repeat;
					}
				}
				public int timesRan
				{
					get{
						return _runs;
					}
				}
				public bool paused
				{
					get{
						return _paused;
					}
					
				}
				public bool pause()
				{
					if (_paused == true)
					{
						_paused = false;
						return false;
					}
					else
					{
						_paused = true;
						return true;
					}
				}
				
				
				public void checkTimer ()
				{
					if (_repeat == 0)
					{
						return;
					}
					if (_paused == false && DateTime.Now > _nextRun)
					{
						try
						{
							callback(this);
							_runs += 1;
						}
						catch (Exception)
						{
						}
						if (_repeat == - 1)
						{
							_nextRun = _nextRun.Add(interval);
							
						}
						else if (_repeat > 0)
						{
							_repeat -= 1;
							_nextRun = _nextRun.Add(interval);
						}
					}
				}
			}
			
			
			/// -----------------------------------------------------------------------------
			/// Project	 : BlackLight Services
			/// Class	 : TimerController
			///
			/// -----------------------------------------------------------------------------
			/// <summary>
			/// Should never be called by anything other than the wrapper
			/// </summary>
			/// <remarks>
			/// </remarks>
			/// <history>
			/// 	[Caleb]	6/18/2005	Created
			/// </history>
			/// -----------------------------------------------------------------------------
			public class TimerController
			{
				
				private ServicesDaemon _base;
				private List<Timer> _timers;
                private Queue<Timer> _addQueue;
                private Queue<Timer> _removeQueue;

				public void Begin ()
				{
					while (_base != null && _timers != null)
					{
                        lock (_addQueue)
                        {
                            while (_addQueue.Count > 0)
                            {
                                _timers.Add(_addQueue.Dequeue());
                            }
                        }
                        lock (_removeQueue)
                        {
                            while (_removeQueue.Count > 0)
                            {
                                _timers.Remove(_removeQueue.Dequeue());
                            }
                        }
						checkTimers();
						disregardTimers();
						System.Threading.Thread.Sleep(150);
					}
				}
                public void Dispose()
                {
                    try
                    {
                        _timers = null;
                        _addQueue = null;
                        _removeQueue = null;
                    }
                    catch
                    {
                        //Supress any errors that happen here.
                    }
                }
				private void checkTimers()
                {
                    foreach (Timer timer in _timers)
                    {
                        timer.checkTimer();
                    }
                    
                }
                public void addTimer(Timer timer)
                {
                    lock (_addQueue)
                    {
                        _addQueue.Enqueue(timer);
                    }
                }
                public void removeTimer(Timer timer)
                {
                    lock (_removeQueue)
                    {
                        _removeQueue.Enqueue(timer);
                    }
                }
                private void disregardTimers()
                {
                    foreach (Timer timer in _timers)
                    {
                        if (timer.repeat == 0)
                            removeTimer(timer);
                    }
                }
				public TimerController(ServicesDaemon daemon) {
					_base = daemon;
                    _timers = new List<Timer>();
                    _addQueue = new Queue<Timer>();
                    _removeQueue = new Queue<Timer>();
				}
			}
		}
	}
}
