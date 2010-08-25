using System.Diagnostics;
using System;
using System.Data;
using System.Collections;
using System.IO;
using System.Threading;
using BlackLight;
using BlackLight.Services.Error;
using BlackLight.Services;
using BlackLight.Services.Core;
using BlackLight.Services.Timers;
using BlackLight.Services.DB;
using BlackLight.Services.Modules;
using BlackLight.Services.Config;
using BlackLight.Services.Nodes;

namespace BlackLight
{
	namespace Services
	{
		/// -----------------------------------------------------------------------------
		/// Project	 : BlackLight Services
		/// Class	 : BlackLightServices
		///
		/// -----------------------------------------------------------------------------
		/// <summary>
		/// The wrapper for the core, modules, and timers
		/// </summary>
		/// <remarks>
		/// </remarks>
		/// <history>
		/// 	[Caleb]	6/18/2005	Created
		/// </history>
		/// -----------------------------------------------------------------------------
		public class ServicesDeamon
		{
			/// -----------------------------------------------------------------------------
			/// <summary>
			/// Instance of our core
			/// </summary>
			/// <remarks>
			/// </remarks>
			/// <history>
			/// 	[Caleb]	6/18/2005	Created
			/// </history>
			/// -----------------------------------------------------------------------------
			public ServicesCore Core;
			/// -----------------------------------------------------------------------------
			/// <summary>
			/// Instance of module management class
			/// </summary>
			/// <remarks>
			/// </remarks>
			/// <history>
			/// 	[Caleb]	6/18/2005	Created
			/// </history>
			/// -----------------------------------------------------------------------------
			public BlackLight.Services.Modules.ModuleManagement ModuleManage;
			/// -----------------------------------------------------------------------------
			/// <summary>
			/// Instance of module management class
			/// </summary>
			/// <remarks>
			/// </remarks>
			/// <history>
			/// 	[Caleb]	6/18/2005	Created
			/// </history>
			/// -----------------------------------------------------------------------------
			public BlackLight.Services.Config.ConfigHandler ConfigManager;
			/// -----------------------------------------------------------------------------
			/// <summary>
			/// List of all timers
			/// </summary>
			/// <remarks>
			/// </remarks>
			/// <history>
			/// 	[Caleb]	6/18/2005	Created
			/// </history>
			/// -----------------------------------------------------------------------------
			public BlackLight.Services.Timers.TimerList Timers;
			private BlackLight.Services.Timers.InitiateTimers TimerSystem;
			private Thread TimerChecker;
			public delegate void onOutPutEventHandler(string OutPut);
			private onOutPutEventHandler onOutPutEvent;
			
			public event onOutPutEventHandler onOutPut
			{
				add
				{
					onOutPutEvent = (onOutPutEventHandler) System.Delegate.Combine(onOutPutEvent, value);
				}
				remove
				{
					onOutPutEvent = (onOutPutEventHandler) System.Delegate.Remove(onOutPutEvent, value);
				}
			}
			
			public BlackLight.Services.DB.DataDriver DataDriver;
			public void AddText (string tString)
			{
				if (onOutPutEvent != null)
					onOutPutEvent(tString);
			}
			public BlackLight.Services.Error.Errors LogType;
			public ServicesDeamon(BlackLight.Services.Error.Errors LogType) {
				this.LogType = LogType;
				Core = new ServicesCore();
				Core.OnConnect += new BlackLight.Services.Core.IRC.OnConnectEventHandler(Core_onConnect);
				Core.onClientConnect += new BlackLight.Services.Core.ServicesCore.onClientConnectEventHandler(Core_onClientConnect);
				Core.onServer += new BlackLight.Services.Core.ServicesCore.onServerEventHandler(Core_onServer);
				Core.onQuit += new BlackLight.Services.Core.ServicesCore.onQuitEventHandler(Core_onQuit);
				Core.onNick += new BlackLight.Services.Core.ServicesCore.onNickEventHandler(Core_onNick);
				Core.onJoin += new BlackLight.Services.Core.ServicesCore.onJoinEventHandler(Core_onJoin);
				Core.onDisconnect += new BlackLight.Services.Core.IRC.onDisconnectEventHandler(Core_onDisconnect);
				Core.LogMessage += new BlackLight.Services.Core.ServicesCore.LogMessageEventHandler(Core_LogMessage);
//				//ModuleManage = new BlackLight.Services.Modules.ModuleManagement(this);
				ConfigManager = new BlackLight.Services.Config.ConfigHandler();
				//Timers = new BlackLight.Services.Timers.TimerList(this);
				

			}
			/// -----------------------------------------------------------------------------
			/// <summary>
			/// Initiates entire services
			/// </summary>
			/// <remarks>
			/// Begins by loading all modules,continues by starting timers,
			/// and finishes by beginning the connection to the remote server
			/// </remarks>
			/// <history>
			/// 	[Caleb]	6/18/2005	Created
			/// </history>
			/// -----------------------------------------------------------------------------
			public void Begin ()
			{
				try
				{
					AddText("Begining" + "\r\n");
					BlackLight.Services.Config.ConfigurationReturn configReturn = ConfigManager.Load("services.conf");
					//Exit Sub
					foreach (string tReturn in configReturn.Data)
					{
AddText("Config: " + tReturn + "\r\n");
					}
					if (configReturn.Loaded == true)
					{
						AddText("Configuration loaded" + "\r\n");
					}
					else
					{
						AddText("Configuration failed to load" + "\r\n");
						return;
					}
					
					AddText("Config Items" + "\r\n");
					
					foreach (string tItem2 in ConfigManager.Configuration.Keys)
					{
						AddText(tItem2 + "\r\n");
						foreach (string tItem3 in ((BlackLight.Services.Config.OptionsList) ConfigManager.Configuration[tItem2]).Keys)
						{
							AddText(tItem2 + "::" + tItem3 + "->" +((BlackLight.Services.Config.OptionsList) ConfigManager.Configuration[tItem2])[tItem3] + "\r\n");
						}
					}
					
					//After we load the config the FIRST thing we need todo is try and set the IRCd protocol
					//The core will auto initialize with a default of unreal, we can change that from a config property
					string remotehost = "";
					string remoteport;
					if (ConfigManager.Configuration.ContainsKey("services"))
					{
						BlackLight.Services.Config.OptionsList myConfig = ConfigManager.Configuration["services"];
						if (myConfig.ContainsKey("protocol"))
						{
							Core.MyIRCd.SetProtocol(myConfig["protocol"]);
						}
						if (myConfig.ContainsKey("remote-host"))
						{
							remotehost = myConfig["remote-host"].Trim();
						}
						else
						{
							Core_LogMessage("ServicesDeamon", "Begin()", BlackLight.Services.Error.Errors.DEBUG | BlackLight.Services.Error.Errors.FATAL, "Missing directive: remote-host", "", "", "");
							return;
						}
						if (myConfig.ContainsKey("remote-port"))
						{
							remoteport = myConfig["remote-port"].Trim();
						}
						else
						{
							Core_LogMessage("ServicesDeamon", "Begin()", BlackLight.Services.Error.Errors.DEBUG | BlackLight.Services.Error.Errors.FATAL, "Missing directive: remote-port", "", "", "");
							return;
						}
					}
					else
					{
						Core_LogMessage("ServicesDeamon", "Begin()", BlackLight.Services.Error.Errors.DEBUG | BlackLight.Services.Error.Errors.FATAL, "Failed to find services configuration block", "", "", "");
						return;
					}
                    Core.MyIRCd.LoadProtocol();
                    //Timer Stuff
                    Timers = new TimerList(this);
                    TimerSystem = new BlackLight.Services.Timers.InitiateTimers(Timers, this);
                    TimerChecker = new System.Threading.Thread(new System.Threading.ThreadStart(TimerSystem.Begin));
                    TimerChecker.Start();
                    //End Timer Stuff
					
					ArrayList tModules = new ArrayList();
					ArrayList tDataDriver = new ArrayList();
					foreach (BlackLight.Services.Config.OptionsList tItem in ConfigManager.Configuration.GetSet("modules"))
					{
						foreach (string tOption in tItem.GetSet("loadmodule"))
						{
							tModules.Add(tOption);
						}
					}
					foreach (BlackLight.Services.Config.OptionsList tItem in ConfigManager.Configuration.GetSet("modules"))
					{
						foreach (string tOption in tItem.GetSet("loaddatadriver"))
						{
							tDataDriver.Add(tOption);
						}
					}
                    ModuleManage = new ModuleManagement(this);

					//Module Stuff
					//Dim tModules As String() = {"NickServ.dll", "ChanServ.dll", "DebugServ.dll", "FloodServ.dll", "Help.dll"}
					if (tDataDriver.Count > 1)
					{
						Core_LogMessage("ServicesDaemon", "Begin()", BlackLight.Services.Error.Errors.DEBUG | BlackLight.Services.Error.Errors.WARNING, "Configuration contains multiple loads for datadriver", "Using first in list", "", "");
						ModuleManage.LoadDataDriver(System.Convert.ToString(tDataDriver[0]));
					}
					else if (tDataDriver.Count == 0)
					{
						Core_LogMessage("ServicesDaemon", "Begin()", BlackLight.Services.Error.Errors.DEBUG | BlackLight.Services.Error.Errors.WARNING, "Configuration contains no load for datadriver", "", "", "");
					}
					else
					{
						ModuleManage.LoadDataDriver(System.Convert.ToString(tDataDriver[0]));
					}
					tDataDriver = null;
					DataDriver = ((BlackLight.Services.DB.DataDriver) ModuleManage.Modules[ModuleManage.DataDriver]);
					ModuleManage.LoadModules((string[]) tModules.ToArray(typeof(string)));
					int tIndex;
					for (tIndex = 0; tIndex <= ModuleManage.Modules.Count - 1; tIndex++)
					{
						AddText(ModuleManage.Modules[tIndex].Name + "\r\n");
					}
AddText("DataDriver: " + DataDriver.Name + "\r\n");
					//End Module Stuff
					tModules = null;
					
					//Table1.newcol
					//Dim Table2 As New Table
					//Dim DataBase As New DataBase
					//Table1.NewColumn("id", GetType(Integer))
					//Table1.Columns("id").AutoIncrement = True
					//Table1.Columns("id").AutoIncrementSeed = 1
					//Table1.NewColumn("host", GetType(String))
					
					//Table1.NewColumn("nickname", GetType(String))
					//Table1.NewColumn("host", GetType(String))
					//Table1.NewColumn("registered", GetType(Int32))
					
					
					//DataBase.AddTable(Table1)
					//DataBase.AddTable(Table2)
					
					//AddText(DataBase.Tables.Count & vbCrLf)
					//DataDriver.SaveDB("test.xml", DataBase)
					//Exit Sub

					
					
					if (remotehost == "")
					{
						Core_LogMessage("ServicesDeamon", "Begin()", BlackLight.Services.Error.Errors.DEBUG | BlackLight.Services.Error.Errors.FATAL, "Directive contains invalid data: remote-port", "", "", "");
						return;
					}
					if (remoteport == "")
					{
						Core_LogMessage("ServicesDeamon", "Begin()", BlackLight.Services.Error.Errors.DEBUG | BlackLight.Services.Error.Errors.FATAL, "Directive contains invalid data: remote-port", "", "", "");
						return;
					}
					else
					{
						try
						{
							int.Parse(remoteport);
						}
						catch (Exception ex)
						{
							Core_LogMessage("ServicesDeamon", "Begin()", BlackLight.Services.Error.Errors.DEBUG | BlackLight.Services.Error.Errors.FATAL, "Directive contains invalid data: remote-port", "", ex.Message, ex.StackTrace);
							return;
						}
					}
					Core.Connect(remotehost, int.Parse(remoteport));
				}
				catch (Exception ex)
				{
					Core_LogMessage("ServicesDaemon", "Begin()", BlackLight.Services.Error.Errors.DEBUG | BlackLight.Services.Error.Errors.FATAL, "Unable to complete startup rutine", "", ex.Message, ex.StackTrace);
				}
			}
			private void Core_onConnect (string result)
			{
				try
				{
					// DataOut.Clear()
					AddText("Connected" + "\r\n");
					
					
					
					string connectpass = "";
					string servername = "";
					string servernumeric = "";
					string serverdesc = "";
					BlackLight.Services.Config.OptionsList myConfig = ConfigManager.Configuration["services"];
					if (myConfig.ContainsKey("connect-pass"))
					{
						connectpass = myConfig["connect-pass"].Trim();
					}
					else
					{
						Core_LogMessage("ServicesDeamon", "IRCService_onConnect()", BlackLight.Services.Error.Errors.DEBUG | BlackLight.Services.Error.Errors.FATAL, "Missing directive: connect-pass", "", "", "");
						return;
					}
					if (myConfig.ContainsKey("server-name"))
					{
						servername = myConfig["server-name"].Trim();
					}
					else
					{
						Core_LogMessage("ServicesDeamon", "IRCService_onConnect()", BlackLight.Services.Error.Errors.DEBUG | BlackLight.Services.Error.Errors.FATAL, "Missing directive: server-name", "", "", "");
						return;
					}
					
					if (myConfig.ContainsKey("server-desc"))
					{
						serverdesc = myConfig["server-desc"].Trim();
					}
					else
					{
						Core_LogMessage("ServicesDeamon", "IRCService_onConnect()", BlackLight.Services.Error.Errors.DEBUG | BlackLight.Services.Error.Errors.FATAL, "Missing directive: server-desc", "", "", "");
						return;
					}
					
					
					if (myConfig.ContainsKey("server-numeric"))
					{
						servernumeric = myConfig["server-numeric"].Trim();
					}
					
					
					if (connectpass == "")
					{
						Core_LogMessage("ServicesDeamon", "IRCService_onConnect()", BlackLight.Services.Error.Errors.DEBUG | BlackLight.Services.Error.Errors.FATAL, "Directive contains invalid data: connect-pass", "", "", "");
						return;
					}
					
					if (servername == "")
					{
						Core_LogMessage("ServicesDeamon", "IRCService_onConnect()", BlackLight.Services.Error.Errors.DEBUG | BlackLight.Services.Error.Errors.FATAL, "Directive contains invalid data: server-name", "", "", "");
						return;
					}
					
					if (serverdesc == "")
					{
						Core_LogMessage("ServicesDeamon", "IRCService_onConnect()", BlackLight.Services.Error.Errors.DEBUG | BlackLight.Services.Error.Errors.FATAL, "Directive contains invalid data: server-desc", "", "", "");
						return;
					}
					
					if (servernumeric == "")
					{
						servernumeric = "0";
					}
					else
					{
						try
						{
							int.Parse(servernumeric);
						}
						catch (Exception)
						{
							servernumeric = "0";
						}
					}
					
					//IRCService.SendData("PASS :testybob")
					//IRCService.SendData("PROTOCTL SJOIN SJOIN2 NOQUIT NICKv2 SJB64 VL")
					// IRCService.SendData(String.Format(IRCService.MyIRCd.SendCommands("PASSSEND"), "testybob"))
					// AddText2(String.Format(IRCService.MyIRCd.SendCommands("PASSSEND"), "testybob") & vbCrLf)
					// IRCService.SendData(String.Format(IRCService.MyIRCd.SendCommands("SERVERSEND"), "noc.rscheatnet.com", "1", "7", "Services"))
					// AddText2(String.Format(IRCService.MyIRCd.SendCommands("SERVERSEND"), "noc.rscheatnet.com", "1", "7", "Services") & vbCrLf)
					//Core.CreateServer("niggernuts21", "special.efak.net", 1, 7, "Services")
					Core.CreateServer(connectpass, servername, 1, short.Parse(servernumeric), serverdesc);
					// Service.CreateServer("sht", "services.unifiedevil.org", 1, 7, "Services")
					//IRCService.SendData("SERVER noc.rscheatnet.com 1 :Testing" & vbCrLf)
					// Dim tClient As New LocalClient("Services2Bot", Nothing, Service)
					// tClient.Host = "lala.com"
					// tClient.Username = "userident"
					// tClient.Realname = "realname"
					//tClient.Modes = "oS"
					//tClient.Time = Service.GetTS(Now)
					//Service.LocalClients.AddClient(tClient)
					//Service.Commands.Send_Connect(tClient)
					//IRCService.SendData("NICK Services2Bot 1 1 real2 lala.com noc.rscheatnet.com :real3")
					//Service.SendData(":Services2Bot JOIN #opers")
					//tClient = Nothing
					result = null;
				}
				catch (Exception ex)
				{
					Core_LogMessage("ServicesDeamon", "IRCService_onConnect()", BlackLight.Services.Error.Errors.DEBUG | BlackLight.Services.Error.Errors.FATAL, "Exception", "", ex.Message, ex.StackTrace);
					//MessageBox.Show("connect");
				}
			}
			public void TerminateCore ()
			{
				if (Core != null)
				{
					Core.Disconnect();
					Core = null;
					if (ModuleManage != null)
					{
						foreach (BlackLight.Services.Modules.BlackLightModule tModule in ModuleManage.Modules)
						{
							tModule.ModUnload();
						}
						while (ModuleManage.Modules.Count > 0)
						{
							ModuleManage.Modules[0] = null;
							ModuleManage.Modules.RemoveAt(0);
						}
						ModuleManage = null;
					}
					if (Timers != null)
					{
						Timers.Clear();
						Timers = null;
					}
					if (TimerChecker != null)
					{
						TimerChecker.Abort();
						TimerChecker = null;
					}
					if (TimerSystem != null)
					{
						TimerSystem = null;
					}
					if (ConfigManager != null&& ConfigManager.Configuration != null)
					{
						ConfigManager.Configuration.Clear();
						ConfigManager.Configuration = null;
					}
					if (ConfigManager != null)
					{
						ConfigManager = null;
					}
					Core_LogMessage("ServicesDeamon", "TerminateCore()", BlackLight.Services.Error.Errors.DEBUG, "Core Terminated", "", "", "");
				}
				else
				{
					Core_LogMessage("ServicesDeamon", "TerminateCore()", BlackLight.Services.Error.Errors.DEBUG, "Core already terminated", "", "", "");
				}
			}
			
			
			private void Core_onClientConnect (string nickname, BlackLight.Services.Nodes.Client data)
			{
				try
				{
					//  UsersList.Items.Add(nickname)
					// UsersList.Refresh()
					nickname = null;
				}
				catch (Exception ex)
				{
					Core_LogMessage("ServicesDeamon", "IRCService_onClientConnect()", BlackLight.Services.Error.Errors.DEBUG | BlackLight.Services.Error.Errors.ERROR, "Exception", "", ex.Message, ex.StackTrace);
					//MessageBox.Show("Form1 Error: ClientConnect");
				}
			}
			
			
			private void Core_onServer (string name, BlackLight.Services.Nodes.Server data)
			{
				try
				{
					//  ServersList.Items.Add(name & "-" & data.Numeric)
					// ServersList.Refresh()
					name = null;
					data = null;
				}
				catch (Exception ex)
				{
					Core_LogMessage("ServicesDeamon", "IRCService_onServer()", BlackLight.Services.Error.Errors.DEBUG | BlackLight.Services.Error.Errors.ERROR, "Exception", "", ex.Message, ex.StackTrace);
					//MessageBox.Show("Form1 Error: OnServer");
				}
			}
			
			private void Core_onQuit (string nickname, string reason)
			{
				try
				{
					AddText("Recieved event for quit" + "\r\n");
					//  UsersList.Items.Remove(nickname)
					// UsersList.Refresh()
					nickname = null;
				}
				catch (Exception ex)
				{
					Core_LogMessage("ServicesDeamon", "IRCService_onQuit()", BlackLight.Services.Error.Errors.DEBUG | BlackLight.Services.Error.Errors.ERROR, "Exception", "", ex.Message, ex.StackTrace);
					//MessageBox.Show("Form1 Error: onQuit");
				}
			}
			
			
			
			
			private void Core_onNick (string Oldnick, string Newnick)
			{
				try
				{
					// UsersList.Items.Remove(Oldnick)
					//UsersList.Items.Add(Newnick)
					Oldnick = null;
					Newnick = null;
				}
				catch (Exception ex)
				{
					Core_LogMessage("ServicesDeamon", "IRCService_onNick()", BlackLight.Services.Error.Errors.DEBUG | BlackLight.Services.Error.Errors.ERROR, "Exception", "", ex.Message, ex.StackTrace);
					//MessageBox.Show("Form1 Error: OnNick");
				}
			}
			
			private void Core_onJoin (string NickName, string Channel)
			{
				try
				{
					//  ChannelsList.Items.Add(Channel & " (" & NickName & ") " & IRCService.Channels(Channel).ChannelMembers(NickName).IsOp)
					// ChannelsList.Refresh()
					NickName = null;
					Channel = null;
					
				}
				catch (Exception ex)
				{
					Core_LogMessage("ServicesDeamon", "IRCService_onJoin()", BlackLight.Services.Error.Errors.DEBUG | BlackLight.Services.Error.Errors.ERROR, "Exception", "", ex.Message, ex.StackTrace);
					//MessageBox.Show("Form1 Error: OnJoin");
				}
			}
			
			
			private void Core_onDisconnect ()
			{
				try
				{
					AddText("The Socket Was ShutDown" + "\r\n");
				}
				catch (Exception ex)
				{
					Core_LogMessage("ServicesDeamon", "IRCService_onDisconnect()", BlackLight.Services.Error.Errors.DEBUG | BlackLight.Services.Error.Errors.ERROR, "Exception", "", ex.Message, ex.StackTrace);
					//MessageBox.Show("Form1 Error: OnDisconnect");
				}
			}
			
			private void Core_LogMessage (string Origin, string Method, BlackLight.Services.Error.Errors Type, string Message, string Extra, string Exception, string Trace)
			{
				//return;
				string tMessage;
				bool Terminate = false;
				//bool Debug = false;
				//Select Case Type
				//    Case Errors.LOG_NOTICE
				//        tMessage = "!NOTICE! "
				//    Case Errors.LOG_DEBUG
				//        tMessage = "!DEBUG! "
				//        Debug = True
				//    Case Errors.LOG_NOTICE_AND_DEBUG
				//        tMessage = "!NOTICE/DEBUG! "
				//        Debug = True
				//    Case Errors.LOG_WARNING
				//        tMessage = "!WARNING! "
				//    Case Errors.LOG_NOTICE_AND_WARNING
				//        tMessage = "!NOTICE/WARNING! "
				//    Case Errors.LOG_DEBUG_AND_WARNING
				//        tMessage = "!DEBUG/WARNING! "
				//        Debug = True
				//    Case Errors.LOG_ERROR
				//        tMessage = "!ERROR! "
				//    Case Errors.LOG_NOTICE_AND_ERROR
				//        tMessage = "!NOTICE/ERROR! "
				//    Case Errors.LOG_DEBUG_AND_ERROR
				//        tMessage = "!DEBUG/ERROR! "
				//        Debug = True
				//    Case Errors.LOG_WARNING_AND_ERROR
				//        tMessage = "!WARNING/ERROR! "
				//    Case Errors.LOG_FATAL
				//        tMessage = "!PANIC! "
				//        Terminate = True
				//    Case Errors.LOG_NOTICE_AND_FATAL
				//        tMessage = "!NOTICE/PANIC! "
				//        Terminate = True
				//    Case Errors.LOG_DEBUG_AND_FATAL
				//        tMessage = "!DEBUG/PANIC! "
				//        Debug = True
				//        Terminate = True
				//    Case Errors.LOG_WARNING_AND_FATAL
				//        tMessage = "!WARNING/PANIC! "
				//        Terminate = True
				//    Case Errors.LOG_ERROR_AND_FATAL
				//        tMessage = "!ERROR/PANIC! "
				//        Terminate = True
				//    Case Else
				//        tMessage = "!UNKNOWN ERROR! "
				//End Select
				if ((Type & BlackLight.Services.Error.Errors.FATAL) == BlackLight.Services.Error.Errors.FATAL)
				{
					Terminate = true;
					AddText("Terminating" + IRC.CRLF);
				}
				if ( Convert.ToBoolean((Type & LogType) & LogType) || ((Type & BlackLight.Services.Error.Errors.FATAL) == BlackLight.Services.Error.Errors.FATAL))
				{
					tMessage = "!" + @Enum.Format(typeof(BlackLight.Services.Error.Errors), Type, "g").Replace(", ", "/") + ((char)(161)) + " ";
					
					tMessage += Message + " (Path: ";
					tMessage += Origin + "->" + Method + ") ";
					if (Exception != "" && ((LogType & BlackLight.Services.Error.Errors.DEBUG) == BlackLight.Services.Error.Errors.DEBUG))
					{
						tMessage += Exception + " ";
						if (Trace != "")
						{
							tMessage += "- " + Trace + " ";
						}
					}
					if (Extra != "")
					{
						tMessage += "*" + Extra + "*";
					}
					tMessage = tMessage.Trim()+ "\r\n";
					AddText(tMessage);
					if (Terminate)
					{
						TerminateCore();
					}
					
				}
				
			}
		}
	}
}
