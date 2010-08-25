using System.Diagnostics;
using System;
using System.Data;
using System.Collections;
using BlackLight;
using System.Threading;
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
			/// Class	 : Timers
			///
			/// -----------------------------------------------------------------------------
			/// <summary>
			/// Container for a list of timers
			/// </summary>
			/// <remarks>
			/// </remarks>
			/// <history>
			/// 	[Caleb]	6/18/2005	Created
			/// </history>
			/// -----------------------------------------------------------------------------
			public sealed class TimerList : IList, ICollection, IEnumerable
			{
				
				readonly ArrayList a;
				public TimerList(ServicesDeamon tBase) {
					a = new ArrayList();
					this.Base = tBase;
				}
				private ServicesDeamon Base;
				public void CopyTo (System.Array array, int index)
				{
					a.CopyTo(array, index);
				}
				public int Count
				{
					get{
						return a.Count;
					}
				}
				bool ICollection.IsSynchronized
				{
					get{
						return false;
					}
				}
				object ICollection.SyncRoot
				{
					get{
						return this;
					}
				}
				public System.Collections.IEnumerator GetEnumerator()
				{
					return a.GetEnumerator();
				}
				int IList.Add(object value)
				{
					return Add((Timer)value);
				}
				public int Add(Timer value)
				{
					return a.Add(value);
				}
				public void Clear ()
				{
					a.Clear();
				}
				bool IList.Contains(object value)
				{
					return Contains((Timer)value);
				}
				public bool Contains(Timer value)
				{
					return a.Contains(value);
				}
				int IList.IndexOf(object value)
				{
					return IndexOf((Timer)value);
				}
				public int IndexOf(Timer value)
				{
					return a.IndexOf(value);
				}
				void IList.Insert (int index, object value)
				{
					Insert(index, (Timer)value);
				}
				public void Insert (int index, Timer value)
				{
					a.Insert(index, value);
				}
				bool IList.IsFixedSize
				{
					get{
						return false;
					}
				}
				bool IList.IsReadOnly
				{
					get{
						return false;
					}
				}
				object IList.this[int index]
				{
					get
					{
						return ((Timer)(a[index]));
					}
					set
					{
						a[index] = value;
					}
				}
				public Timer this[int index]
				{
					get{
						return ((Timer)(a[index]));
					}
					set
					{
						a[index] = value;
					}
				}
				void IList.Remove (object value)
				{
					Remove((Timer)value);
				}
				public void Remove (Timer value)
				{
					a.Remove(value);
				}
				public void RemoveAt (int index)
				{
					a.RemoveAt(index);
				}
				public void CheckTimers ()
				{
					int idx = 0;
					while (idx < Count)
					{
						this[idx].CheckTimer();
						idx += 1;
					}
				}
				public void DisregaurdTimers ()
				{
					int idx = 0;
					while (idx < Count)
					{
						if (this[idx].Repeat == 0)
						{
							RemoveAt(idx);
						}
						else
						{
							idx += 1;
						}
					}
				}
				
			}
			
			
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
				
				public readonly DateTime StartTime;
				private int tRuns;
				public readonly TimeSpan Interval;
				private bool tPaused;
				private DateTime tNextRun;
				private int tRepeat;
				public readonly TimedCallBack Callback;
				public readonly ArrayList Params;
				public Timer(TimeSpan delay, TimeSpan interval, int repeatcount, TimedCallBack callback, params object[] @params) {
					if (repeatcount < - 1 || repeatcount == 0)
					{
						throw (new ArgumentException("Invalid repetition count. Use -1 for infinate", "repeatcount"));
					}
					if (callback == null)
					{
						throw (new ArgumentNullException("callback", "Callback cannot be nothing."));
					}
					this.StartTime = DateTime.Now.Add(delay);
					this.Interval = interval;
					tNextRun = StartTime.Add(interval);
					tRepeat = repeatcount;
					this.Callback = callback;
					tPaused = false;
					tRuns = 0;
					this.Params = new ArrayList(@params);
				}
				public DateTime NextRun
				{
					get{
						return tNextRun;
					}
				}
				public int Repeat
				{
					get{
						return tRepeat;
					}
				}
				public int TimesRan
				{
					get{
						return tRuns;
					}
				}
				public bool Paused
				{
					get{
						return tPaused;
					}
					
				}
				public bool Pause()
				{
					if (tPaused == true)
					{
						tPaused = false;
						return false;
					}
					else
					{
						tPaused = true;
						return true;
					}
				}
				
				
				public void CheckTimer ()
				{
					if (tRepeat == 0)
					{
						return;
					}
					if (tPaused == false && DateTime.Now > tNextRun)
					{
						try
						{
							Callback(this);
							tRuns += 1;
						}
						catch (Exception)
						{
						}
						if (tRepeat == - 1)
						{
							tNextRun = tNextRun.Add(Interval);
							
						}
						else if (tRepeat > 0)
						{
							tRepeat -= 1;
							tNextRun = tNextRun.Add(Interval);
						}
					}
				}
			}
			
			
			/// -----------------------------------------------------------------------------
			/// Project	 : BlackLight Services
			/// Class	 : InitiateTimers
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
			public class InitiateTimers
			{
				
				ServicesDeamon tBase;
				TimerList tTimers;
				public void Begin ()
				{
					while (tBase != null && tTimers != null)
					{
						tTimers.CheckTimers();
						tTimers.DisregaurdTimers();
						System.Threading.Thread.Sleep(150);
					}
				}
				
				public InitiateTimers(TimerList cTimers, ServicesDeamon cBase) {
					tBase = cBase;
					tTimers = cTimers;
				}
			}
		}
	}
}
