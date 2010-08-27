using System.Diagnostics;
using Microsoft.VisualBasic;
using System;
using System.Data;
using System.Collections;
using BlackLight;
using BlackLight.Services;

namespace BlackLight.Services
{
	sealed class Services
	{
		static ServicesDaemon IRCServices;
		
		public static System.IO.StreamWriter tFileIO;
		static public void Main()
		{
			try
			{
				System.IO.File.Delete("debug.txt");
				tFileIO = System.IO.File.CreateText("debug.txt");

				IRCServices = new ServicesDaemon(BlackLight.Services.Error.Errors.ALL);
				IRCServices.onOutPut +=new BlackLight.Services.ServicesDaemon.onOutPutEventHandler(IRCServices_onOutPut);
				//Dim tDataBase As New DataBase
				//tDataBase.Test()
				//Exit Sub
				//CallyThingy()
				//System.Threading.Thread.Sleep(500);

				IRCServices.Begin();
				// Threading.Thread.CurrentThread.Suspend()
				while (true)
				{
					System.Threading.Thread.Sleep(2000);
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine("Connect Error: " + ex.Message + " " + ex.StackTrace);
				Console.ReadLine();
			}
		}
		
		private static void IRCServices_onOutPut (string OutPut)
		{
			try
			{
				Console.Write(OutPut);
				tFileIO.WriteLine(OutPut);
				tFileIO.Flush();
			}
			catch (Exception ex)
			{
				Console.WriteLine("Log Error Error: " + ex.Message + " " + ex.StackTrace);
			}
		}
	}
	
}
