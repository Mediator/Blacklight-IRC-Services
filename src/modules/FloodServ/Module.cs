using System.Diagnostics;
using Microsoft.VisualBasic;
using System;
using System.Data;
using System.Collections;
using BlackLight;
using BlackLight.Services;
using BlackLight.Services.Nodes;
using BlackLight.Services.Modules;
using BlackLight.Services.DB;

namespace BlackLight
{
	namespace Services
	{
		namespace Modules
		{
			namespace FloodServ
			{
				
				/// -----------------------------------------------------------------------------
				/// Project	 : NickServ
				/// Class	 : NickServ
				///
				/// -----------------------------------------------------------------------------
				/// <summary>
				/// Public module for nick management with the services
				/// </summary>
				/// <remarks>
				/// </remarks>
				/// <history>
				/// 	[Caleb]	12/1/2005	Created
				/// </history>
				/// -----------------------------------------------------------------------------
				public class FloodServ : BlackLightModule
				{
					private LocalClient mFloodServ;
					private FloodServService MyService;
					private DataBase FloodDB;
					private DataDriver DataDriver;
					public FloodServ(ServicesDeamon Base) : base(Base) {
						FloodDB = new DataBase();
						
						AddRequired("Help.dll");
					}
					public override void ModUnload ()
					{
						mFloodServ = null;
					}
					public override bool ModLoad()
					{
						DataDriver = MyBase.DataDriver;
						//Load the DB into memory
						try
						{
							if (DataDriver.DBExists("floodserv.db"))
							{
								FloodDB = DataDriver.LoadDB("floodserv.db");
								if (FloodDB == null)
								{
									throw (new Exception("NickServ: Unknown DB Load Error"));
//									return false;
								}
							}
							else
							{
								MakeDB();
							}
							BlackLight.Services.Timers.Timer FSSaveTimer;
							FSSaveTimer = new BlackLight.Services.Timers.Timer(new TimeSpan(0), new TimeSpan(0, 5, 0), - 1, new Timers.TimedCallBack(TimerSaveDB));
							MyBase.Timers.Add(FSSaveTimer);
							MyBase.Core.OnFinishedNetBurst += new BlackLight.Services.Core.ServicesCore.OnFinishedNetBurstEventHandler(this.OnConnect);
						}
						catch (Exception ex)
						{
							MyBase.Core.SendLogMessage("FloodServ", "ModLoad", BlackLight.Services.Error.Errors.ERROR, "Exception", "", ex.Message, ex.StackTrace);
							//show("FloodServ Error " + ex.Message + " " + ex.StackTrace);
							return false;
						}
						
						mFloodServ = new LocalClient("FloodServ",new MessageCallBack(FloodServCallBack) ,MyBase.Core);
						ModuleList tModules = new ModuleList();
						tModules.Add(MyBase.ModuleManage.Modules["Help"]);
						MyService = new FloodServService(mFloodServ, MyBase.Core, FloodDB, tModules);
						mFloodServ.Host = "services.com";
						mFloodServ.Modes = "S";
						mFloodServ.Realname = "FloodServ";
						mFloodServ.Time = BlackLight.Services.Converters.Time.GetTS(DateTime.Now);
						mFloodServ.Username = "FloodServ";
						MyBase.Core.onClientConnect += new BlackLight.Services.Core.ServicesCore.onClientConnectEventHandler(MyService.OnClientConnect);
						
						mFloodServ.Cmds.Add("HELP", new BlackLight.Services.Nodes.CommandCallBack( MyService.FSHelp));
						
						mFloodServ.Cmds.Add("NPWATCH", new BlackLight.Services.Nodes.CommandCallBack( MyService.FSNPWatch));
						
						mFloodServ.Cmds.Add("NSWATCH", new BlackLight.Services.Nodes.CommandCallBack( MyService.FSNSWatch));
						
						mFloodServ.Cmds.Add("REGWATCH", new BlackLight.Services.Nodes.CommandCallBack( MyService.FSRegWatch));
						
						mFloodServ.Cmds.Add("NPSCAN", new BlackLight.Services.Nodes.CommandCallBack( MyService.FSNPScan));
						
						mFloodServ.Cmds.Add("NSSCAN", new BlackLight.Services.Nodes.CommandCallBack( MyService.FSNSScan));
						
						mFloodServ.Cmds.Add("REGSCAN", new BlackLight.Services.Nodes.CommandCallBack( MyService.FSRegScan));

						MyBase.Core.LocalClients.AddClient(mFloodServ);
						return true;
					}
					private void FloodServCallBack (Client Source, string[] Message)
					{
						try
						{
							if (mFloodServ.CmdExec(Source, Message) == false)
							{
								MyService.FSNoCommand(Source, Message);
							}
						}
						catch (Exception)
						{
							
						}
					}
					private void MakeDB ()
					{
						FloodDB = new DB.DataBase();
						Table tWatchTable = new Table("npwatches");
						FloodDB.AddTable(tWatchTable);
						tWatchTable.Columns.NewColumn("match", ColumnType.STRING_TYPE);
						tWatchTable.PrimaryColumn = "match";
						tWatchTable.Columns.NewColumn("adder", ColumnType.STRING_TYPE);
						tWatchTable.Columns.NewColumn("date", ColumnType.INT_TYPE);
						
						
						tWatchTable = new Table("nswatches");
						FloodDB.AddTable(tWatchTable);
						tWatchTable.Columns.NewColumn("match", ColumnType.STRING_TYPE);
						tWatchTable.PrimaryColumn = "match";
						tWatchTable.Columns.NewColumn("adder", ColumnType.STRING_TYPE);
						tWatchTable.Columns.NewColumn("date", ColumnType.INT_TYPE);
						
						
						tWatchTable = new Table("regwatches");
						FloodDB.AddTable(tWatchTable);
						tWatchTable.Columns.NewColumn("match", ColumnType.STRING_TYPE);
						tWatchTable.PrimaryColumn = "match";
						tWatchTable.Columns.NewColumn("adder", ColumnType.STRING_TYPE);
						tWatchTable.Columns.NewColumn("date", ColumnType.INT_TYPE);
						
						DataDriver.SaveDB("floodserv.db", FloodDB);
					}
					private void OnConnect ()
					{
						mFloodServ.Join_Channel("#services", "");
					}
					private void TimerSaveDB (BlackLight.Services.Timers.Timer tTimer)
					{
						DataDriver.SaveDB("floodserv.db", FloodDB);
						mFloodServ.Send_Notice("#services", "Saving DataBase");
					}
					public override void Rehash ()
					{
						
					}
					public override string Name
					{
						get{
							return "FloodServ";
						}
					}
					public override string Description
					{
						get{
							return "FloodServ Module";
						}
					}
					public override bool NeedsDBDriver
					{
						get{
							return true;
						}
					}
				}
			}
		}
	}
}
