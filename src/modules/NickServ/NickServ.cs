using System.Diagnostics;
using Microsoft.VisualBasic;
using System;
using System.Data;
using System.Collections;
using BlackLight;
using BlackLight.Services;
using BlackLight.Services.Core;
using BlackLight.Services.Nodes;
using BlackLight.Services.DB;
using BlackLight.Services.Error;
using BlackLight.Services.Modules.Help;

namespace BlackLight
{
	namespace Services
	{
		namespace Modules
		{
			namespace NickServ
			{
				
				public class NickServService
				{
					
					protected LocalClient MyClient;
					protected ServicesCore MyCore;
					protected ModuleList Modules;
					protected Help.Help Help;
					public DataBase NickDB;
					public void NullFunction (Client Source, string[] Message)
					{
						
					}
					public void NSHelp (Client Source, string[] Message)
					{
						Help.sendHelp("NickServ", MyClient, Source, Message);
					}
					
					public void NSNoCommand (Client Source, string[] Message)
					{
						Help.sendResponse("NickServ", MyClient, Source, "NO_SUCH_COMMAND", Message[0], MyClient.Name);
					}
					
					
					public void NSGroup (Client Source, string[] Params)
					{
						if (Params != null)
						{
							if (Params.Length == 2)
							{
								BlackLight.Services.DB.Row CurUser;
								CurUser = FindUser(Source.Nick);
								if (CurUser != null&& Source.IdentNick.ToLower() != System.Convert.ToString(CurUser["nickname"]).ToLower())
								{
									Help.sendResponse("NickServ", MyClient, Source, "GROUP_NICK_REGISTERED");
									//MyClient.Send_Notice(Source.Nick, "Your current nick is registered, and you are not identified to it.")
									return;
								}
								
								
								if (System.Convert.ToString(CurUser["owner"])!= "")
								{
									Help.sendResponse("NickServ", MyClient, Source, "GROUP_EXISTS");
									//MyClient.Send_Notice(Source.Nick, "You are already in a group.")
									return;
								}
								
								BlackLight.Services.DB.Row tUser = FindUser(Params[0]);
								if (tUser == null)
								{
									Help.sendResponse("NickServ", MyClient, Source, "GROUP_NO_SUCH_NICK");
									//MyClient.Send_Notice(Source.Nick, "That nickname does not exist")
									return;
								}
								
								if (System.Convert.ToString(tUser["owner"])!= "")
								{
									tUser = FindUser(System.Convert.ToString(tUser["owner"]));
								}
								
								if (System.Convert.ToString(tUser["password"])!= Params[1])
								{
									Help.sendResponse("NickServ", MyClient, Source, "GROUP_INVALID_PASS");
									//MyClient.Send_Notice(Source.Nick, "Incorrect password")
									return;
								}
								
								if (CurUser == null)
								{
									BlackLight.Services.DB.Row tRow = NickDB["users"].MakeRow();
									tRow["nickname"] = Source.Nick;
									tRow["owner"] = tUser["nickname"];
									NickDB["users"].AddRow(tRow);
									Source.IdentNick = Source.Nick;
									// If they are identified this should change their identification to the nick
									// They ar on
									MyCore.Commands.IdentifyClient(MyClient.Nick, Source.Nick);
									if (Source.IdentNick != "")
									{
										Help.sendResponse("NickServ", MyClient, Source, "GROUP_IDENTIFIED", Source.Nick);
										//MyClient.Send_Notice(Source.Nick, "You are now identified to " & Source.Nick)
									}
								}
								else
								{
									if (System.Convert.ToString(tUser["nickname"]).ToLower() == System.Convert.ToString(CurUser["nickname"]).ToLower())
									{
										//MyClient.Send_Notice(Source.Nick, String.Format("You are already in {0}'s group", tUser("nickname")))
										Help.sendResponse("NickServ", MyClient, Source, "GROUP_ALREADY_MEMBER", System.Convert.ToString(tUser["nickname"]));
									}
									else
									{
										CurUser["owner"] = tUser["nickname"];
									}
								}
								Help.sendResponse("NickServ", MyClient, Source, "GROUP_CREATED", System.Convert.ToString(tUser["nickname"]));
								// MyClient.Send_Notice(Source.Nick, String.Format("You are now in {0}'s group", tUser("nickname")))
								
							}
							else
							{
								Help.sendError("NickServ", MyClient, Source, "GROUP", "GROUP");
								// MyClient.Send_Notice(Source.Nick, String.Format("Syntax: {0}GROUP {1}target-nick password{1}{0}", MyCore.FORMAT_BOLD, MyCore.FORMAT_UNDERLINE))
							}
						}
						else
						{
							Help.sendError("NickServ", MyClient, Source, "GROUP", "GROUP");
							//MyClient.Send_Notice(Source.Nick, String.Format("Syntax: {0}GROUP {1}target-nick password{1}{0}", MyCore.FORMAT_BOLD, MyCore.FORMAT_UNDERLINE))
						}
					}
					
					public void NSGList (Client Source, string[] Params)
					{
						if (Params == null)
						{
							
							if (Source.IdentNick == "")
							{
								Help.sendResponse("NickServ", MyClient, Source, "GLIST_NO_IDENT");
								//MyClient.Send_Notice(Source.Nick, "You are not identified to a nickname.")
								return;
							}
							
							BlackLight.Services.DB.Row tUser = FindUser(Source.IdentNick);
							
							if (System.Convert.ToString(tUser["owner"])!= "")
							{
								tUser = FindUser(System.Convert.ToString(tUser["owner"]));
							}
							string tOwner;
							string tNick;
							string tMatch;
							int tCount = 0;
							tMatch = System.Convert.ToString(tUser["nickname"]).ToLower();
							Help.sendResponse("NickServ", MyClient, Source, "GLIST_LIST_START");
							//MyClient.Send_Notice(Source.Nick, "Nicknames in your group:")
							
							foreach (Row tRow in NickDB["users"])
							{
								tOwner = System.Convert.ToString(tRow["owner"]);
								tNick = System.Convert.ToString(tRow["nickname"]);
								if ((tNick != null&& tNick.ToLower() == tMatch) ||(tOwner != null&& tOwner.ToLower() == tMatch))
								{
									tCount += 1;
									Help.sendResponse("NickServ", MyClient, Source, "GLIST_LISTING", tNick);
									//MyClient.Send_Notice(Source.Nick, tNick)
								}
							}
							Help.sendResponse("NickServ", MyClient, Source, "GLIST_LIST_END");
							//MyClient.Send_Notice(Source.Nick, tCount.ToString & " nicknames listed")
						}
						else
						{
							Help.sendError("NickServ", MyClient, Source, "GLIST", "GLIST");
							//MyClient.Send_Notice(Source.Nick, String.Format("Syntax: {0}GLIST{0}", MyCore.FORMAT_BOLD))
						}
					}
					
					public void NSRegister (Client Source, string[] Params)
					{
						if (Params != null)
						{
							//Verify Args for email
							if ((true && Params.Length == 2) ||(false && Params.Length == 1))
							{
								//Email
								if (Params[0].Length < 5)
								{
									Help.sendResponse("NickServ", MyClient, Source, "REGISTER_BAD_LENGTH");
									//MyClient.Send_Notice(Source.Nick, "Invalid password: minimum length is 5 characters")
									return;
								}
								//Needs Email
								if (true)
								{
									if (IsValidEmail(Params[1]))
									{
										Help.sendResponse("NickServ", MyClient, Source, "REGISTER_BAD_EMAIL");
										//MyClient.Send_Notice(Source.Nick, "Invalid email address")
										return;
									}
								}
								if (!System.Convert.ToBoolean(FindUser(Source.Nick) == null))
								{
									Help.sendResponse("NickServ", MyClient, Source, "REGISTER_INUSE", Source.Nick);
									//  MyClient.Send_Notice(Source.Nick, String.Format("Nickname {0}{1}{0} is already registered!", MyCore.FORMAT_BOLD, Source.Nick))
									return;
								}
								if (Source.IdentNick != "")
								{
									Help.sendResponse("NickServ", MyClient, Source, "REGISTER_ALREADY_IDENTIFIED");
									// MyClient.Send_Notice(Source.Nick, "You are identified and cannot register another nickname in this manner while identified")
									return;
								}
								try
								{
									BlackLight.Services.DB.Row tRow = NickDB["users"].MakeRow();
									tRow["nickname"] = Source.Nick;
									tRow["password"] = Params[0];
									tRow["email"] = Params[1];
									tRow["regdate"] = BlackLight.Services.Converters.Time.GetTS(DateTime.Now);
									//MsgBox(tRow("regdate"))
									NickDB["users"].AddRow(tRow);
									Source.IdentNick = Source.Nick;
									Help.sendResponse("NickServ", MyClient, Source, "REGISTER_REGISTERED");
									// MyClient.Send_Notice(Source.Nick, "You are now registered")
									MyCore.Commands.IdentifyClient(MyClient.Nick, Source.Nick);
								}
								catch (Exception ex)
								{
									MyCore.SendLogMessage("FloodServ", "NSRegister", BlackLight.Services.Error.Errors.ERROR, "Exception", "", ex.Message, ex.StackTrace);
									//show("NickServ Error " + ex.Message + " " + ex.StackTrace);
								}
								//ElseIf True = False AndAlso Params.Length = 1 Then
								//    'No Email
								//    If Params(0).Length < 5 Then
								//        MyClient.Send_Notice(Source.Nick, "Invalid password: minimum length is 5 characters")
								//        Exit Sub
								//    End If
								//    If Not FindUser(Source.Nick) Is Nothing Then
								//        MyClient.Send_Notice(Source.Nick, String.Format("Nickname {0}{1}{0} is already registered!", MyCore.FORMAT_BOLD, Source.Nick))
								//        Exit Sub
								//    End If
								//    If Source.IdentNick <> "" Then
								//        MyClient.Send_Notice(Source.Nick, "You are identified and cannot register another nickname while this is true")
								//        Exit Sub
								//    End If
								//    Try
								//        Dim tRow As DB.Row = NickDB("users").MakeRow
								//        tRow("nickname") = Source.Nick
								//        tRow("password") = Params(0)
								//        tRow("email") = ""
								//        tRow("regdate") = MyCore.Time.GetTS(Now)
								//        NickDB("users").AddRow(tRow)
								//        Source.IdentNick = Source.Nick
								//        MyCore.Commands.IdentifyClient(MyClient.Nick, Source.Nick)
								//        MyClient.Send_Notice(Source.Nick, "You are now registered")
								//    Catch ex As Exception
								//        MsgBox("NickServ Error " & ex.Message & " " & ex.StackTrace)
								//    End Try
							}
							else
							{
								//Check Email
								if (true)
								{
									Help.sendError("NickServ", MyClient, Source, "REGISTER_WITHEMAIL", "REGISTER");
									//MyClient.Send_Notice(Source.Nick, String.Format("Syntax: {0}REGISTER {1}password email{1}{0}", MyCore.FORMAT_BOLD, MyCore.FORMAT_UNDERLINE))
								}
								else
								{
//									Help.sendError("NickServ", MyClient, Source, "REGISTER", "REGISTER");
//									//MyClient.Send_Notice(Source.Nick, String.Format("Syntax: {0}REGISTER {1}password{1}{0}", MyCore.FORMAT_BOLD, MyCore.FORMAT_UNDERLINE))
								}
							}
						}
						else
						{
							if (true)
							{
								Help.sendError("NickServ", MyClient, Source, "REGISTER_WITHEMAIL", "REGISTER");
								//MyClient.Send_Notice(Source.Nick, String.Format("Syntax: {0}REGISTER {1}password email{1}{0}", MyCore.FORMAT_BOLD, MyCore.FORMAT_UNDERLINE))
							}
							else
							{
//								Help.sendError("NickServ", MyClient, Source, "REGISTER", "REGISTER");
//								//MyClient.Send_Notice(Source.Nick, String.Format("Syntax: {0}REGISTER {1}password{1}{0}", MyCore.FORMAT_BOLD, MyCore.FORMAT_UNDERLINE))
							}
						}
					}
					public void NSIdentify (Client Source, string[] Params)
					{
						if (Params != null)
						{
							if (Params.Length == 1)
							{
								//Normal Identify
								if (Source.IdentNick != "")
								{
									Help.sendResponse("NickServ", MyClient, Source, "IDENTIFY_ALREADY_IDENTIFIED");
									//MyClient.Send_Notice(Source.Nick, "You are already identified.")
									return;
								}
								
								BlackLight.Services.DB.Row tUser = FindUser(Source.Nick);
								if (tUser == null)
								{
									Help.sendResponse("NickServ", MyClient, Source, "IDENTIFY_NOT_REGISTERED");
									//MyClient.Send_Notice(Source.Nick, "Your nickname isn't registerd")
									return;
								}
								if (System.Convert.ToString(tUser["owner"])!= "")
								{
									tUser = FindUser(System.Convert.ToString(tUser["owner"]));
								}
								
								
								if (Params[0].Length < 5)
								{
									Help.sendResponse("NickServ", MyClient, Source, "IDENTIFY_BAD_PASS");
									// MyClient.Send_Notice(Source.Nick, "Invalid password")
									return;
								}
								
								if (System.Convert.ToString(tUser["password"])== Params[0])
								{
									Source.IdentNick = Source.Nick;
									MyCore.Commands.IdentifyClient(MyClient.Nick, Source.Nick);
									// MyClient.Send_Notice(Source.Nick, "You are now identified")
									Help.sendResponse("NickServ", MyClient, Source, "IDENTIFY_IDENTIFIED");
								}
								else
								{
									Help.sendResponse("NickServ", MyClient, Source, "IDENTIFY_BAD_PASS");
									//MyClient.Send_Notice(Source.Nick, "Incorrect password")
									return;
								}
							}
							else if (Params.Length == 2)
							{
								//Remote Identify
								if (Source.IdentNick != "")
								{
									Help.sendResponse("NickServ", MyClient, Source, "IDENTIFY_ALREADY_IDENTIFIED");
									//MyClient.Send_Notice(Source.Nick, "You are already identified.")
									return;
								}
								BlackLight.Services.DB.Row tUser = FindUser(Params[0]);
								if (tUser == null)
								{
									Help.sendResponse("NickServ", MyClient, Source, "IDENTIFY_NOT_REGISTERED");
									//MyClient.Send_Notice(Source.Nick, "That nickname isn't registerd")
									return;
								}
								
								if (System.Convert.ToString(tUser["owner"])!= "")
								{
									tUser = FindUser(System.Convert.ToString(tUser["owner"]));
								}
								
								if (Params[1].Length < 5)
								{
									Help.sendResponse("NickServ", MyClient, Source, "IDENTIFY_BAD_PASS");
									MyClient.Send_Notice(Source.Nick, "Invalid password");
									return;
								}
								
								if (System.Convert.ToString(tUser["password"])== Params[1])
								{
									Source.IdentNick = Params[0];
									MyCore.Commands.IdentifyClient(MyClient.Nick, Source.Nick);
									MyClient.Send_Notice(Source.Nick, "You are now identified");
								}
								else
								{
									MyClient.Send_Notice(Source.Nick, "Incorrect password");
									return;
								}
							}
							else
							{
								MyClient.Send_Notice(Source.Nick, string.Format("Syntax: {0}IDENTIFY {1}password{1}{0} OR {0}IDENTIFY {1}nickname password{1}{0}", BlackLight.Services.Core.ServicesCore.FORMAT_BOLD, BlackLight.Services.Core.ServicesCore.FORMAT_UNDERLINE));
							}
						}
						else
						{
							MyClient.Send_Notice(Source.Nick, string.Format("Syntax: {0}IDENTIFY {1}password{1}{0} OR {0}IDENTIFY {1}nickname password{1}{0}", BlackLight.Services.Core.ServicesCore.FORMAT_BOLD, BlackLight.Services.Core.ServicesCore.FORMAT_UNDERLINE));
						}
					}
					
					
					public void NSDrop (Client Source, string[] Params)
					{
						if (Params != null)
						{
							if (Params.Length == 1)
							{
								if (Source.IdentNick == "")
								{
									MyClient.Send_Notice(Source.Nick, "You are not identified.");
									return;
								}
								
								BlackLight.Services.DB.Row CurUser;
								CurUser = FindUser(Source.Nick);
								
								if (CurUser == null)
								{
									MyClient.Send_Notice(Source.Nick, "Your nick isn\'t registered.");
									return;
								}
								
								if (Source.IdentNick.ToLower() != System.Convert.ToString(CurUser["nickname"]).ToLower())
								{
									//O Okay...
									//Get the owner of the ident nick, and the owner of the current nick
									BlackLight.Services.DB.Row tIdentOwner = FindUser(Source.IdentNick);
									if (System.Convert.ToString(tIdentOwner["owner"])!= "")
									{
										tIdentOwner = FindUser(System.Convert.ToString(tIdentOwner["owner"]));
									}
									
									BlackLight.Services.DB.Row tCurOwner;
									if (System.Convert.ToString(CurUser["owner"])!= "")
									{
										tCurOwner = FindUser(System.Convert.ToString(CurUser["owner"]));
									}
									else
									{
										tCurOwner = CurUser;
									}
									if (System.Convert.ToString(tCurOwner["nickname"]).ToLower() != System.Convert.ToString(tIdentOwner["nickname"]).ToLower())
									{
										MyClient.Send_Notice(Source.Nick, "Your current nick is registered, and you are not identified to it.");
										return;
									}
								}
								
								//Ok loop through all the registered nicks and make sure this nick
								//isn't the owner of any groups
								Row RemUser = CurUser;
								string tOwner;
								string fNick = "";
								bool ChangeOwner = false;
								string tNick = System.Convert.ToString(CurUser["nickname"]).ToLower();
								foreach (Row tRow in NickDB["users"])
								{
									tOwner = System.Convert.ToString(tRow["owner"]).ToLower();
									if (tOwner == tNick)
									{
										if (ChangeOwner == false)
										{
											//FK
											// Well I guess the best way to handle this, is to rename
											// the current nick, to this new one...so all info
											// is retained
											CurUser["nickname"] = tRow["nickname"];
											RemUser = tRow;
											fNick = System.Convert.ToString(tRow["nickname"]);
											ChangeOwner = true;
										}
										else
										{
											tRow["owner"] = fNick;
										}
									}
								}
								
								NickDB["users"].RemoveRow(RemUser);
								MyClient.Send_Notice(Source.Nick, "Your nickname has been dropped");
							}
							else if (Params.Length == 2)
							{
								//Dropping External Nick
								//Oper or group only
								if (Source.IdentNick == "")
								{
									MyClient.Send_Notice(Source.Nick, "You are not identified.");
									return;
								}
								
								BlackLight.Services.DB.Row CurUser;
								CurUser = FindUser(Params[0]);
								
								if (CurUser == null)
								{
									MyClient.Send_Notice(Source.Nick, "That nick isn\'t registered.");
									return;
								}
								
								if (Source.IdentNick.ToLower() != System.Convert.ToString(CurUser["nickname"]).ToLower())
								{
									//O Okay...
									//Get the owner of the ident nick, and the owner of the current nick
									BlackLight.Services.DB.Row tIdentOwner = FindUser(Source.IdentNick);
									if (System.Convert.ToString(tIdentOwner["owner"])!= "")
									{
										tIdentOwner = FindUser(System.Convert.ToString(tIdentOwner["owner"]));
									}
									
									BlackLight.Services.DB.Row tCurOwner;
									if (System.Convert.ToString(CurUser["owner"])!= "")
									{
										tCurOwner = FindUser(System.Convert.ToString(CurUser["owner"]));
									}
									else
									{
										tCurOwner = CurUser;
									}
									//Stuff for opers here
									if (System.Convert.ToString(tCurOwner["nickname"]).ToLower() != System.Convert.ToString(tIdentOwner["nickname"]).ToLower())
									{
										MyClient.Send_Notice(Source.Nick, "Permission Denied.");
										return;
									}
								}
								
								//Ok loop through all the registered nicks and make sure this nick
								//isn't the owner of any groups
								Row RemUser = CurUser;
								string tOwner;
								string fNick = "";
								bool ChangeOwner = false;
								string tNick = System.Convert.ToString(CurUser["nickname"]).ToLower();
								foreach (Row tRow in NickDB["users"])
								{
									tOwner = System.Convert.ToString(tRow["owner"]).ToLower();
									if (tOwner == tNick)
									{
										if (ChangeOwner == false)
										{
											//FK
											// Well I guess the best way to handle this, is to rename
											// the current nick, to this new one...so all info
											// is retained
											CurUser["nickname"] = tRow["nickname"];
											RemUser = tRow;
											fNick = System.Convert.ToString(tRow["nickname"]);
											ChangeOwner = true;
										}
										else
										{
											tRow["owner"] = fNick;
										}
									}
								}
								
								NickDB["users"].RemoveRow(RemUser);
								MyClient.Send_Notice(Source.Nick, "The nickname has been dropped");
								
							}
							else
							{
								MyClient.Send_Notice(Source.Nick, string.Format("Syntax: {0}DROP [{1}nickname{1}]{0}", BlackLight.Services.Core.ServicesCore.FORMAT_BOLD, BlackLight.Services.Core.ServicesCore.FORMAT_UNDERLINE));
							}
						}
						else
						{
							MyClient.Send_Notice(Source.Nick, string.Format("Syntax: {0}DROP [{1}nickname{1}[{0}", BlackLight.Services.Core.ServicesCore.FORMAT_BOLD, BlackLight.Services.Core.ServicesCore.FORMAT_UNDERLINE));
						}
					}
					
					public void NSGhost (Client Source, string[] Params)
					{
						if (Params != null)
						{
							if (Params.Length == 2)
							{
								
								//First Make sure the user is online
								if (MyCore.GetClient(Params[0]) == null)
								{
									MyClient.Send_Notice(Source.Nick, "No such nickname");
									return;
								}
								
								if (Params[0].ToLower() == Source.Nick.ToLower())
								{
									MyClient.Send_Notice(Source.Nick, "You cannot ghost yourself");
									return;
								}
								
								BlackLight.Services.DB.Row tUser = FindUser(Params[0]);
								if (tUser == null)
								{
									MyClient.Send_Notice(Source.Nick, "That nickname isn\'t registered");
									return;
								}
								
								BlackLight.Services.DB.Row tOwner = null;
								if (System.Convert.ToString(tUser["owner"])!= "")
								{
									tOwner = FindUser(System.Convert.ToString(tUser["owner"]));
								}
								
								if (tOwner == null)
								{
									if (System.Convert.ToString(tUser["password"])!= Params[1])
									{
										MyClient.Send_Notice(Source.Nick, "Incorrect password");
										return;
									}
								}
								else
								{
									if (System.Convert.ToString(tOwner["password"])!= Params[1])
									{
										MyClient.Send_Notice(Source.Nick, "Incorrect password");
										return;
									}
								}
								
								MyClient.Kill_Client(Params[0], "GHOST command used by " + Source.Nick);
								MyClient.Send_Notice(Source.Nick, "Your nickname has been killed");
								
							}
							else if (Params.Length == 1)
							{
								
								//First Make sure the user is online
								if (MyCore.GetClient(Params[0]) == null)
								{
									MyClient.Send_Notice(Source.Nick, "No such nickname");
									return;
								}
								
								if (Params[0].ToLower() == Source.Nick.ToLower())
								{
									MyClient.Send_Notice(Source.Nick, "You cannot ghost yourself");
									return;
								}
								
								if (Source.IdentNick == "")
								{
									MyClient.Send_Notice(Source.Nick, string.Format("Syntax: {0}GHOST {1}target-nick password{1}{0}", BlackLight.Services.Core.ServicesCore.FORMAT_BOLD, BlackLight.Services.Core.ServicesCore.FORMAT_UNDERLINE));
									return;
								}
								
								BlackLight.Services.DB.Row CurUser = FindUser(Source.IdentNick);
								if (System.Convert.ToString(CurUser["owner"])!= "")
								{
									CurUser = FindUser(System.Convert.ToString(CurUser["owner"]));
								}
								
								
								BlackLight.Services.DB.Row tUser = FindUser(Params[0]);
								if (tUser == null)
								{
									MyClient.Send_Notice(Source.Nick, "That nickname isn\'t registered");
									return;
								}
								
								BlackLight.Services.DB.Row tOwner = null;
								if (System.Convert.ToString(tUser["owner"])!= "")
								{
									tOwner = FindUser(System.Convert.ToString(tUser["owner"]));
								}
								
								if (tOwner == null)
								{
									if (System.Convert.ToString(CurUser["nickname"]).ToLower() != System.Convert.ToString(tUser["nickname"]).ToLower())
									{
										MyClient.Send_Notice(Source.Nick, "Permission Denied");
										return;
									}
								}
								else
								{
									if (System.Convert.ToString(CurUser["nickname"]).ToLower() != System.Convert.ToString(tOwner["nickname"]).ToLower())
									{
										MyClient.Send_Notice(Source.Nick, "Permission Denied");
										return;
									}
								}
								
								MyClient.Kill_Client(Params[0], MyClient.Nick + " (GHOST command used by " + Source.Nick + ")");
								MyClient.Send_Notice(Source.Nick, "Your nickname has been killed");
								
							}
							else
							{
								MyClient.Send_Notice(Source.Nick, string.Format("Syntax: {0}GHOST {1}target-nick password{1}{0}", BlackLight.Services.Core.ServicesCore.FORMAT_BOLD, BlackLight.Services.Core.ServicesCore.FORMAT_UNDERLINE));
							}
						}
						else
						{
							MyClient.Send_Notice(Source.Nick, string.Format("Syntax: {0}GHOST {1}target-nick password{1}{0}", BlackLight.Services.Core.ServicesCore.FORMAT_BOLD, BlackLight.Services.Core.ServicesCore.FORMAT_UNDERLINE));
						}
					}
					
					public NickServService(LocalClient tMyClient, ServicesCore tMyCore, DataBase tNickDB, ModuleList Modules) {
						this.MyClient = tMyClient;
						this.MyCore = tMyCore;
						this.NickDB = tNickDB;
						this.Modules = Modules;
						Help = ((Help.Help) this.Modules["Help"]);
					}
					private bool IsValidEmail(string address)
					{
						string pattern=@"^[a-z][a-z|0-9|]*([_][a-z|0-9]+)*([.][a-z|" + 
							@"0-9]+([_][a-z|0-9]+)*)?@[a-z][a-z|0-9|]*\.([a-z]" + 
							@"[a-z|0-9]*(\.[a-z][a-z|0-9]*)?)$";
						return System.Text.RegularExpressions.Regex.IsMatch(address, pattern, System.Text.RegularExpressions.RegexOptions.IgnoreCase);
					}
					private Row FindUser(string nickname)
					{
						Row tRow;
						tRow = NickDB["users"].IRow(nickname);
						if (tRow != null)
						{
							return tRow;
						}
						else
						{
							return null;
						}
					}
				}
			}
		}
	}
}
