using System.Diagnostics;
using System;
using System.Data;
using System.Collections;
using BlackLight.Services;
using BlackLight.Services.Core;

namespace BlackLight
{
	namespace Services
	{
		namespace Converters
		{
			
			/// -----------------------------------------------------------------------------
			/// Project	 : BlackLight Services
			/// Class	 : Base64
			///
			/// -----------------------------------------------------------------------------
			/// ''' <summary>
			/// Class used for converts to and from base64 format
			/// </summary>
			/// <remarks>
			/// </remarks>
			/// <history>
			/// 	[Caleb]	6/18/2005	Created
			/// </history>
			/// -----------------------------------------------------------------------------
			public class Base64
			{
                public static char[] b64buf = new char[6];
				/// -----------------------------------------------------------------------------
				/// <summary>
				/// Converts a timestamp integer to base64 format
				/// </summary>
				/// <param name="val">32 bit integer to be used as a timestamp to convert</param>
				/// <returns>String representation of base64 time</returns>
				/// <remarks>
				/// Barrowed from WinSE
				/// </remarks>
				/// <history>
				/// 	[Caleb]	6/18/2005	Created
				/// </history>
				/// -----------------------------------------------------------------------------
				public string IntToB64(int val)
				{
					string[] map;
					map = new string[] { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z", "a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l", "m", "n", "o", "p", "q", "r", "s", "t", "u", "v", "w", "x", "y", "z", "{", "}" };
					int i;
					i = 6;
					//Unreal does some weird check to see if val is over 2^31-1, but we don't need it since Integer can't do that.
					//Unreal's check just calls abort() if it is over, which we shouldn't do.
					//We probably should just check if val is under 0, since that shouldn't happen anyway.
					if (val < 0)
					{
						throw (new ArgumentException("Negative value not permitted.", "val"));
					}
					do
					{
						//i = i - 1;
						//if (i > 0)
						//{
						//	Console.WriteLine(b64buf.Substring(0,i));
						//	Console.WriteLine(map[val & 63]);
						//	Console.WriteLine(b64buf.Substring(i,b64buf.Length-1));
						//	b64buf.
						//	b64buf = b64buf.Substring(0,i) + map[val & 63] + b64buf.Substring(i+1);
						//}
						//else
						//	b64buf = map[val & 63] + b64buf.Substring(i+1);
						b64buf[--i] = map[val & 63][0];
						//Now we need to do a 6-bit right shift. Unreal's code uses a signed long, and by C's standard,
						//>> on a signed integer performs an arithmetic shift. This will play havoc if val is < 0 but that
						//shouldn't happen anyway.
						val = val >> 6;
					} while (val != 0);
					return new string(b64buf).Trim();
				}
				/// -----------------------------------------------------------------------------
				/// <summary>
				/// Converts a base64 timestamp into a 32 bit integer
				/// </summary>
				/// <param name="b64">The base64 formatted string to be converted</param>
				/// <returns>32 bit integer of base64 timestamp</returns>
				/// <remarks>
				/// </remarks>
				/// <history>
				/// 	[Caleb]	6/18/2005	Created
				/// </history>
				/// -----------------------------------------------------------------------------
				public int B64ToInt(string b64)
				{
					int[] map;
					map = new int[] { - 1, - 1, - 1, - 1, - 1, - 1, - 1, - 1, - 1, - 1, - 1, - 1, - 1, - 1, - 1, - 1, - 1, - 1, - 1, - 1, - 1, - 1, - 1, - 1, - 1, - 1, - 1, - 1, - 1, - 1, - 1, - 1, - 1, - 1, - 1, - 1, - 1, - 1, - 1, - 1, - 1, - 1, - 1, - 1, - 1, - 1, - 1, - 1, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, - 1, - 1, - 1, - 1, - 1, - 1, - 1, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20,
										21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, - 1, - 1, - 1, - 1, - 1, - 1, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 48, 49, 50, 51, 52, 53, 54, 55, 56, 57, 58, 59, 60, 61, 62, - 1, 63, - 1, - 1, - 1, - 1, - 1, - 1, - 1, - 1, - 1, - 1, - 1, - 1, - 1, - 1, - 1, - 1, - 1, - 1, - 1, - 1, - 1, - 1, - 1, - 1, - 1, - 1, - 1, - 1,
										- 1, - 1, - 1, - 1, - 1, - 1, - 1, - 1, - 1, - 1, - 1, - 1, - 1, - 1, - 1, - 1, - 1, - 1, - 1, - 1, - 1, - 1, - 1, - 1, - 1, - 1, - 1, - 1, - 1, - 1, - 1, - 1, - 1, - 1, - 1, - 1, - 1, - 1, - 1, - 1, - 1, - 1, - 1, - 1, - 1, - 1, - 1, - 1, - 1, - 1, - 1, - 1, - 1, - 1, - 1, - 1, - 1, - 1, - 1, - 1, - 1, - 1, - 1, - 1, - 1, - 1, - 1, - 1, - 1, - 1, - 1, - 1
										, - 1, - 1, - 1, - 1, - 1, - 1, - 1, - 1, - 1, - 1, - 1, - 1, - 1, - 1, - 1, - 1, - 1, - 1, - 1, - 1, - 1, - 1, - 1, - 1, - 1, - 1, - 1, - 1, - 1, - 1 };
					int idx;
					int v;
					idx = 0;
					v = map[(int)b64[idx]];
					idx = idx + 1;
					if (idx > b64.Length)
					{
						return 0;
					}
					for (; idx < b64.Length-1; idx++)
					{
						//Do a 6-bit left shift. Harder than a right.
						//Mask off bits that will fall off.
						v = v << 6;
						v += map[(int)b64[idx]];
					}
					return v;
				}
				
			}
			/// -----------------------------------------------------------------------------
			/// Project	 : BlackLight Services
			/// Class	 : BlackLight_Services.Converters.Time
			///
			/// -----------------------------------------------------------------------------
			/// <summary>
			/// Includes common time functions
			/// </summary>
			/// <remarks>
			/// </remarks>
			/// <history>
			/// 	[Caleb]	6/18/2005	Created
			/// </history>
			/// -----------------------------------------------------------------------------
			public class Time
			{
				
				/// -----------------------------------------------------------------------------
				/// <summary>
				/// Gets a timestamp integer based on date given
				/// </summary>
				/// <param name="d">Date to be converted</param>
				/// <returns></returns>
				/// <remarks>
				/// </remarks>
				/// <history>
				/// 	[Caleb]	6/18/2005	Created
				/// </history>
				/// -----------------------------------------------------------------------------
				public static int GetTS(DateTime d)
				{
					//return System.Convert.ToInt32(DateAndTime.DateDiff(DateInterval.Second, new DateTime(1970, 1, 1, 0, 0, 0), d.ToUniversalTime(), 1, 1));
				return System.Convert.ToInt32(new TimeSpan(d.ToUniversalTime().Ticks -new DateTime(1970, 1, 1, 0, 0, 0).Ticks).TotalSeconds);
				}
			}
		}
	}
}
