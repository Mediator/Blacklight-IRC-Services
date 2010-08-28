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
				private int p_runs;
				public readonly TimeSpan interval;
                private bool p_paused;
                private DateTime p_nextRun;
                private int p_repeat;
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
					this.p_nextRun = this.startTime.Add(this.interval);
                    this.p_repeat = repeatCount;
					this.callback = callback;
                    this.p_paused = false;
                    this.p_runs = 0;
					this.@params = new ArrayList(@params);
				}
				public DateTime nextRun
				{
					get{
                        return p_nextRun;
					}
				}
				public int repeat
				{
					get{
                        return p_repeat;
					}
				}
				public int timesRan
				{
					get{
                        return p_runs;
					}
				}
				public bool paused
				{
					get{
                        return p_paused;
					}
					
				}
				public bool pause()
				{
                    if (p_paused == true)
					{
                        p_paused = false;
						return false;
					}
					else
					{
                        p_paused = true;
						return true;
					}
				}
				
				
				public void checkTimer ()
				{
                    if (p_repeat == 0)
					{
						return;
					}
                    if (p_paused == false && DateTime.Now > p_nextRun)
					{
						try
						{
							callback(this);
                            p_runs += 1;
						}
						catch (Exception)
						{
						}
                        if (p_repeat == -1)
						{
                            p_nextRun = p_nextRun.Add(interval);
							
						}
						else if (p_repeat > 0)
						{
                            p_repeat -= 1;
                            p_nextRun = p_nextRun.Add(interval);
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
				
				private ServicesDaemon p_base;
                private List<Timer> p_timers;
                private Queue<Timer> p_addQueue;
                private Queue<Timer> p_removeQueue;

				public void Begin ()
				{
                    while (p_base != null && p_timers != null)
					{
                        lock (p_addQueue)
                        {
                            while (p_addQueue.Count > 0)
                            {
                                p_timers.Add(p_addQueue.Dequeue());
                            }
                        }
                        lock (p_removeQueue)
                        {
                            while (p_removeQueue.Count > 0)
                            {
                                p_timers.Remove(p_removeQueue.Dequeue());
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
                        p_timers = null;
                        p_addQueue = null;
                        p_removeQueue = null;
                    }
                    catch
                    {
                        //Supress any errors that happen here.
                    }
                }
				private void checkTimers()
                {
                    foreach (Timer timer in p_timers)
                    {
                        timer.checkTimer();
                    }
                    
                }
                public void addTimer(Timer timer)
                {
                    lock (p_addQueue)
                    {
                        p_addQueue.Enqueue(timer);
                    }
                }
                public void removeTimer(Timer timer)
                {
                    lock (p_removeQueue)
                    {
                        p_removeQueue.Enqueue(timer);
                    }
                }
                private void disregardTimers()
                {
                    foreach (Timer timer in p_timers)
                    {
                        if (timer.repeat == 0)
                            removeTimer(timer);
                    }
                }
				public TimerController(ServicesDaemon daemon) {
					p_base = daemon;
                    p_timers = new List<Timer>();
                    p_addQueue = new Queue<Timer>();
                    p_removeQueue = new Queue<Timer>();
				}
			}
		}
	}
}
