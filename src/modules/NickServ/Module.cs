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
			namespace NickServ
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
				public class NickServ : BlackLightModule
				{
					public LocalClient mNickServ;
					public NickServService MyService;
					public DataBase NickDB;
					public DataDriver DataDriver;
					public NickServ(ServicesDaemon Base) : base(Base) {
						NickDB = new DataBase();
						
						AddRequired("Help.dll");
					}
					public override void ModUnload ()
					{
						mNickServ = null;
					}
					public override bool ModLoad()
					{
						
						DataDriver = MyBase.DataDriver;
						//Load the DB into memory
						try
						{
							if (DataDriver.DBExists("nickserv.db"))
							{
								NickDB = DataDriver.LoadDB("nickserv.db");
								if (NickDB == null)
								{
									throw (new Exception("NickServ: Unknown DB Load Error"));
//									return false;
								}
							}
							else
							{
								MakeDB();
							}
							BlackLight.Services.Timers.Timer NSSaveTimer;
							NSSaveTimer = new BlackLight.Services.Timers.Timer(new TimeSpan(0), new TimeSpan(0, 5, 0), - 1, new Timers.TimedCallBack( TimerSaveDB));
							MyBase.timerController.addTimer(NSSaveTimer);
							MyBase.Core.events.OnFinishedNetBurst += new BlackLight.Services.Core.ServicesCore.ServicesEvents.OnFinishedNetBurstEventHandler(this.OnConnect);
						}
						catch (Exception ex)
						{
							MyBase.Core.SendLogMessage("Nick", "ModLoad", BlackLight.Services.Error.Errors.ERROR, "Exception", "", ex.Message, ex.StackTrace);
							//show("NickServ Error " + ex.Message + " " + ex.StackTrace);
							return false;
						}
						
						mNickServ = new LocalClient("NickServ", new MessageCallBack(NickServCallBack), MyBase.Core);
						ModuleList tModules = new ModuleList();
						tModules.Add(MyBase.ModuleManage.Modules["Help"]);
						MyService = new NickServService(mNickServ, MyBase.Core, NickDB, tModules);
						mNickServ.host = "services.com";
						mNickServ.modes = "S";
						mNickServ.realname = "NickyServ";
						mNickServ.time = BlackLight.Services.Converters.Time.GetTS(DateTime.Now);
						mNickServ.username = "NickServIdent";
						
						// Client
						mNickServ.Cmds.Add("REGISTER", new BlackLight.Services.Nodes.CommandCallBack( MyService.NSRegister));
						// Client
						mNickServ.Cmds.Add("IDENTIFY", new BlackLight.Services.Nodes.CommandCallBack( MyService.NSIdentify));
						// Client
						mNickServ.Cmds.Add("GROUP", new BlackLight.Services.Nodes.CommandCallBack( MyService.NSGroup));
						// Client
						mNickServ.Cmds.Add("GLIST", new BlackLight.Services.Nodes.CommandCallBack( MyService.NSGList));
						// Client/Oper
						mNickServ.Cmds.Add("DROP", new BlackLight.Services.Nodes.CommandCallBack( MyService.NSDrop));
						// Client
						mNickServ.Cmds.Add("GHOST", new BlackLight.Services.Nodes.CommandCallBack( MyService.NSGhost));
						
						mNickServ.Cmds.Add("HELP", new BlackLight.Services.Nodes.CommandCallBack( MyService.NSHelp));
						// Client/Open - (oper gets more)
						// bNickServ.Cmds.Add("INFO", AddressOf MyService.NullFunction)
						// Client
						// bNickServ.Cmds.Add("LOGOUT", AddressOf MyService.NullFunction)
						// Client
						// bNickServ.Cmds.Add("RECOVER", AddressOf MyService.NullFunction)
						// Client
						// bNickServ.Cmds.Add("RELEASE", AddressOf MyService.NullFunction)
						// Client
						//  bNickServ.Cmds.Add("AJOIN", AddressOf MyService.NullFunction)
						// Client / Oper
						//  bNickServ.Cmds.Add("ACCESS", AddressOf MyService.NullFunction)
						// Client / Oper
						//  bNickServ.Cmds.Add("ALIST", AddressOf MyService.NullFunction)
						// Client
						//  bNickServ.Cmds.Add("STATUS", AddressOf MyService.NullFunction)
						// Client/Oper
						// bNickServ.Cmds.Add("SET", AddressOf MyService.NullFunction)
						
						
						// Oper
						//  bNickServ.Cmds.Add("FIND", AddressOf MyService.NullFunction)
						// Oper
						// bNickServ.Cmds.Add("FORBID", AddressOf MyService.NullFunction)
						// Oper
						// bNickServ.Cmds.Add("UNFORBID", AddressOf MyService.NullFunction)
						// Oper
						//  bNickServ.Cmds.Add("SUSPEND", AddressOf MyService.NullFunction)
						// Oper
						// bNickServ.Cmds.Add("UNSUSPEND", AddressOf MyService.NullFunction)
						// Oper - de oper/dc
						// bNickServ.Cmds.Add("NOOP", AddressOf MyService.NullFunction)
						// Oper
						//bNickServ.Cmds.Add("UNIDENTIFY", AddressOf MyService.NullFunction)
						
						
						
						MyBase.Core.LocalClients.AddClient(mNickServ);
						return true;
					}
					private void NickServCallBack (Client Source, string[] Message)
					{
						try
						{
							if (mNickServ.CmdExec(Source, Message) == false)
							{
								MyService.NSNoCommand(Source, Message);
							}
						}
						catch (Exception)
						{
							
						}
					}
					public void MakeDB ()
					{
						NickDB = new DB.DataBase();
						Table tUsersTable = new Table("users");
						NickDB.AddTable(tUsersTable);
						tUsersTable.Columns.NewColumn("nickname", ColumnType.STRING_TYPE);
						tUsersTable.PrimaryColumn = "nickname";
						tUsersTable.Columns.NewColumn("password", ColumnType.STRING_TYPE);
						tUsersTable.Columns.NewColumn("email", ColumnType.STRING_TYPE);
						tUsersTable.Columns.NewColumn("owner", ColumnType.STRING_TYPE);
						tUsersTable.Columns.NewColumn("regdate", ColumnType.INT_TYPE);
						//   tUsersTable.Columns.NewColumn("password", ColumnType.STRING_TYPE)
						//  tUsersTable.Columns.NewColumn("password", ColumnType.STRING_TYPE)
						//  tUsersTable.Columns.NewColumn("password", ColumnType.STRING_TYPE)
						DataDriver.SaveDB("nickserv.db", NickDB);
					}
					public void OnConnect ()
					{
						mNickServ.Join_Channel("#services", "");
					}
					public void TimerSaveDB (BlackLight.Services.Timers.Timer tTimer)
					{
						DataDriver.SaveDB("nickserv.db", NickDB);
						mNickServ.Send_Notice("#services", "Saving DataBase");
					}
					public override void Rehash ()
					{
						
					}
					public override string Name
					{
						get{
							return "NickServ";
						}
					}
					public override string Description
					{
						get{
							return "NickServ Module";
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
