using System.Diagnostics;
using Microsoft.VisualBasic;
using System;
using System.Data;
using System.Collections;
using BlackLight;
using BlackLight.Services;

namespace BlackLight.Services
{
	sealed class Main
	{
		static ServicesDeamon IRCServices = new ServicesDeamon(BlackLight.Services.Error.Errors.ALL);
		public static System.IO.File tFile;
		public static System.IO.StreamWriter tFileIO;
		static public void Main_Renamed ()
		{
			try
			{
				//Dim tDataBase As New DataBase
				//tDataBase.Test()
				//Exit Sub
				//CallyThingy()
				System.Threading.Thread.Sleep(500);
				System.IO.File.Delete("debug.txt");
				tFileIO = System.IO.File.CreateText("debug.txt");
				IRCServices.Begin();
				// Threading.Thread.CurrentThread.Suspend()
				while (true)
				{
					System.Threading.Thread.Sleep(2000);
				}
			}
			catch (Exception ex)
			{
				show("Connect Error: " + ex.Message + " " + ex.StackTrace);
			}
		}
		
		private static void IRCService_onOutPut (string OutPut)
		{
			try
			{
				Console.Write(OutPut, null);
				tFileIO.WriteLine(OutPut, null);
				tFileIO.Flush();
			}
			catch (Exception ex)
			{
				show("Log Error Error: " + ex.Message + " " + ex.StackTrace);
			}
		}
	}
	
}
