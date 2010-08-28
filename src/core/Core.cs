using System.Diagnostics;
using System;
using System.Data;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using BlackLight;
using BlackLight.Services.Nodes;
using BlackLight.Services.Error;
using BlackLight.Services.Converters;
using BlackLight.Services.IRCProtocol;

namespace BlackLight.Services.Core
		{
			
			
			/// -----------------------------------------------------------------------------
			/// Project	 : BlackLight Services
			/// Class	 : ServicesCore
			///
			/// -----------------------------------------------------------------------------
			/// <summary>
			/// Core class for entire project
			/// </summary>
			/// <remarks>
			/// Name for class is being considered
			/// </remarks>
			/// <history>
			/// 	[Caleb]	6/18/2005	Created
			/// </history>
			/// -----------------------------------------------------------------------------
			public partial class ServicesCore : IRC
			{
				
				public const char FORMAT_BOLD = '\u0002';
				public const char FORMAT_UNDERLINE = '\u001F';
				public const char FORMAT_COLOR = '\u0003';
				
				/// -----------------------------------------------------------------------------
				/// <summary>
				/// The IRCd instance for protocols
				/// </summary>
				/// <remarks>
				/// </remarks>
				/// <history>
				/// 	[Caleb]	6/18/2005	Created
				/// </history>
				/// -----------------------------------------------------------------------------
				public IRCd MyIRCd;
				
				/// -----------------------------------------------------------------------------
				/// <summary>
				/// Instance of server we are linked to
				/// </summary>
				/// <remarks>
				/// </remarks>
				/// <history>
				/// 	[Caleb]	6/18/2005	Created
				/// </history>
				/// -----------------------------------------------------------------------------
				public BlackLight.Services.Nodes.Server MyHost;
				/// -----------------------------------------------------------------------------
				/// <summary>
				/// List using container of all local clients
				/// </summary>
				/// <remarks>
				/// </remarks>
				/// <history>
				/// 	[Caleb]	6/18/2005	Created
				/// </history>
				/// -----------------------------------------------------------------------------
				public LocalClientsList LocalClients;
				/// -----------------------------------------------------------------------------
				/// <summary>
				/// List of all avaliable channels
				/// </summary>
				/// <remarks>
				/// </remarks>
				/// <history>
				/// 	[Caleb]	6/18/2005	Created
				/// </history>
				/// -----------------------------------------------------------------------------
				public BlackLight.Services.Nodes.ChannelsList Channels;
				/// -----------------------------------------------------------------------------
				/// <summary>
				/// Name of our server
				/// </summary>
				/// <remarks>
				/// Example: services.blacklight.com
				/// </remarks>
				/// <history>
				/// 	[Caleb]	6/18/2005	Created
				/// </history>
				/// -----------------------------------------------------------------------------
				public string MyName;
				/// -----------------------------------------------------------------------------
				/// <summary>
				/// Instance of base64 conversion class
				/// </summary>
				/// <remarks>
				/// </remarks>
				/// <history>
				/// 	[Caleb]	6/18/2005	Created
				/// </history>
				/// -----------------------------------------------------------------------------
				public BlackLight.Services.Converters.Base64 Base64;
				/// -----------------------------------------------------------------------------
				/// <summary>
				/// List of commands that can be sent to the server
				/// </summary>
				/// <remarks>
				/// Needs new name?
				/// </remarks>
				/// <history>
				/// 	[Caleb]	6/18/2005	Created
				/// </history>
				/// -----------------------------------------------------------------------------
				public BlackLight.Services.IRCProtocol.Commands Commands;
				/// -----------------------------------------------------------------------------
				/// <summary>
				/// Instance of time converters object
				/// </summary>
				/// <remarks>
				/// </remarks>
				/// <history>
				/// 	[Caleb]	6/18/2005	Created
				/// </history>
				/// -----------------------------------------------------------------------------
				public BlackLight.Services.Converters.Time Time;

                /// <summary>
                /// Instance of events object
                /// </summary>
                /// <remarks>
                /// </remarks>
                /// <history>
                ///     [Caleb] 8/28/2010   Created
                /// </history>
                public BlackLight.Services.Core.ServicesCore.ServicesEvents events;


				
				
				
				
				/// -----------------------------------------------------------------------------
				/// <summary>
				/// Contains a list of all command the services recognise from the server
				/// </summary>
				/// <remarks>
				/// Needs new name?
				/// </remarks>
				/// <history>
				/// 	[Caleb]	6/18/2005	Created
				/// </history>
				/// -----------------------------------------------------------------------------
				public Hashtable Cmds;
				
				
				/// -----------------------------------------------------------------------------
				/// <summary>
				/// Will load the protocol, and command sets
				/// </summary>
				/// <remarks>
				/// </remarks>
				/// <history>
				/// 	[Caleb]	6/18/2005	Created
				/// </history>
				/// -----------------------------------------------------------------------------
				public ServicesCore() 
				{
                    events = new ServicesEvents();
					MyIRCd = new IRCd("Unreal", this);
					MyIRCd.CannotParseProtocol += new IRCd.CannotParseProtocolEventHandler(IRCd_ParseError);
					MyIRCd.tNoFile += new IRCd.tNoFileEventHandler(this.IRCd_NoFile);
					LocalClients = new LocalClientsList(this);
					Channels = new BlackLight.Services.Nodes.ChannelsList();
					Base64 = new BlackLight.Services.Converters.Base64();
					Cmds = new Hashtable();
					
					try
					{
						LoadCmds();
						Commands = new BlackLight.Services.IRCProtocol.Commands(this, MyIRCd);
					}
					catch (Exception ex)
					{
						SendLogMessage("Services", "New", BlackLight.Services.Error.Errors.ERROR, "Problem loading default startup", "", ex.Message, ex.StackTrace);
					}
				}
				
				
				/// -----------------------------------------------------------------------------
				/// <summary>
				/// Should be called on connect to send the data required to create a serverShould be called on connect to send the data required to create a server
				/// </summary>
				/// <param name="Password">Link password to be sent</param>
				/// <param name="Servername">Services servername to be sent</param>
				/// <param name="Hops">Number of hops to be sent</param>
				/// <param name="Numeric">Server numeric to be sent</param>
				/// <param name="Description">Description of server to be sent</param>
				/// <remarks>
				/// During netburst it will send all local clients not yet connected
				/// </remarks>
				/// <history>
				/// 	[Caleb]	6/18/2005	Created
				/// </history>
				/// -----------------------------------------------------------------------------
				public System.DateTime starttime;
				public void CreateServer (string Password, string Servername, short Hops, short Numeric, string Description)
				{
					try
					{
						//SendData(MyIRCd.SendCommands("PROTOCOLSEND").ToString)
						//SendData(String.Format(MyIRCd.SendCommands("PASSSEND").ToString, Password))
						//SendData(String.Format(MyIRCd.SendCommands("SERVERSEND").ToString, Servername, Hops, Numeric, Description))
						//SendData(String.Format(MyIRCd.SendCommands("EOSSEND").ToString, Servername))
						starttime = System.DateTime.Now;
						MyName = Servername;
						Commands.Send_Protocol();
						Commands.Send_Password(Password, Time.GetTS(DateTime.Now));
						Commands.Send_Server(Servername, Hops, Numeric, Description);
						//Start The burst
						foreach (LocalClient tClient in LocalClients)
						{
							Commands.Send_Connect(tClient);
						}
						Commands.Send_EOBURST(Servername);
						if (events.OnFinishedNetBurstEvent != null)
                            events.OnFinishedNetBurstEvent();
						Password = null;
						Servername = null;
						Hops = 0;
						Numeric = 0;
						Description = null;
						
					}
					catch (Exception ex)
					{
						SendLogMessage("Services", "CreateServer", BlackLight.Services.Error.Errors.ERROR, "Problem creating our server on network", "", ex.Message, ex.StackTrace);
					}
				}
				private void LoadCmds ()
				{
					try
					{
						AddCommand("PING", new BlackLight.Services.Core.ServicesCore.IRCInterpretor( Ping), Cmds); // PING
						AddCommand("&", new BlackLight.Services.Core.ServicesCore.IRCInterpretor( Nick), Cmds); // User
						AddCommand("NICK", new BlackLight.Services.Core.ServicesCore.IRCInterpretor( Nick), Cmds); // User
						AddCommand("CLIENT", new BlackLight.Services.Core.ServicesCore.IRCInterpretor( Nick), Cmds); // User
						AddCommand("SVINFO", new BlackLight.Services.Core.ServicesCore.IRCInterpretor( NetInfo), Cmds);
						AddCommand("NETCTRL", new BlackLight.Services.Core.ServicesCore.IRCInterpretor( NullFunction), Cmds);
						AddCommand("CAPAB", new BlackLight.Services.Core.ServicesCore.IRCInterpretor( NullFunction), Cmds);
						AddCommand("PROTOCTL", new BlackLight.Services.Core.ServicesCore.IRCInterpretor( NullFunction), Cmds);
						AddCommand("EOS", new BlackLight.Services.Core.ServicesCore.IRCInterpretor( EOS), Cmds);
						AddCommand("GNOTICE", new BlackLight.Services.Core.ServicesCore.IRCInterpretor( NullFunction), Cmds);
						AddCommand("SJOIN", new BlackLight.Services.Core.ServicesCore.IRCInterpretor( Server_Channel_Join), Cmds);
						AddCommand("JOIN", new BlackLight.Services.Core.ServicesCore.IRCInterpretor( Channel_Join), Cmds); // JOIN
						AddCommand("PART", new BlackLight.Services.Core.ServicesCore.IRCInterpretor( Channel_Part), Cmds); // PART
						AddCommand("QUIT", new BlackLight.Services.Core.ServicesCore.IRCInterpretor( UserQuit), Cmds); //QUIT
						AddCommand("MODE", new BlackLight.Services.Core.ServicesCore.IRCInterpretor( ModeChange), Cmds); //MODE
						AddCommand("UMODE2", new BlackLight.Services.Core.ServicesCore.IRCInterpretor( ModeChange), Cmds); //MODE
						AddCommand("TOPIC", new BlackLight.Services.Core.ServicesCore.IRCInterpretor( Channel_Topic), Cmds); //TOPIC
						AddCommand("INVITE", new BlackLight.Services.Core.ServicesCore.IRCInterpretor( NullFunction), Cmds); //INVITE
						AddCommand("KICK", new BlackLight.Services.Core.ServicesCore.IRCInterpretor( Channel_Kick), Cmds); //KICK
						AddCommand("PRIVMSG", new BlackLight.Services.Core.ServicesCore.IRCInterpretor( PrivMsg), Cmds); //PRIVMSG
						AddCommand("NOTICE", new BlackLight.Services.Core.ServicesCore.IRCInterpretor( Notice), Cmds); //NOTICE
						AddCommand("SERVER", new BlackLight.Services.Core.ServicesCore.IRCInterpretor( NewServer), Cmds);
						AddCommand("SQUIT", new BlackLight.Services.Core.ServicesCore.IRCInterpretor( SQuit), Cmds);
						AddCommand("KILL", new BlackLight.Services.Core.ServicesCore.IRCInterpretor( Kill), Cmds);
						AddCommand("SVSKILL", new BlackLight.Services.Core.ServicesCore.IRCInterpretor( SVSKill), Cmds);
						AddCommand("SVSMODE", new BlackLight.Services.Core.ServicesCore.IRCInterpretor( SVSMode), Cmds);
						AddCommand("SVS2MODE", new BlackLight.Services.Core.ServicesCore.IRCInterpretor( SVSMode), Cmds);
						AddCommand("NETINFO", new BlackLight.Services.Core.ServicesCore.IRCInterpretor( NetInfo), Cmds);
					}
					catch (Exception ex)
					{
						SendLogMessage("Services", "LoadCmds", BlackLight.Services.Error.Errors.ERROR, "Problem loading commands for services", "", ex.Message, ex.StackTrace);
					}
				}
				
				private void AddCommand (string tCmd, IRCInterpretor tInterpetor, Hashtable tArray)
				{
					try
					{
						tArray.Add(tCmd.ToUpper(),((IRCInterpretor) tInterpetor));
					}
					catch (Exception ex)
					{
						SendLogMessage("Services", "AddCommand", BlackLight.Services.Error.Errors.ERROR, "Problem adding command to list", "", ex.Message, ex.StackTrace);
					}
				}
				
				protected override void MyDataIn (IAsyncResult ar)
				{
					try
					{
						DataObject obj_SocketState = ((DataObject) ar.AsyncState);
						Socket obj_Socket = obj_SocketState.WorkSocket;
						
						int BytesRead = obj_Socket.EndReceive(ar);
						
						
						try
						{
							
							
							if (BytesRead > 0)
							{
								// RaiseEvent onDebug("-----DATA-----Got Data-----DATA-----")
								byte[] DataIn = obj_SocketState.Buffer;
								DataParse(DataIn, BytesRead);
								DataIn = null;
								try
								{
									obj_Socket.BeginReceive(obj_SocketState.Buffer, 0, obj_SocketState.BufferSize, 0, new AsyncCallback( MyDataIn), obj_SocketState);
								}
								catch
								{
									SendLogMessage("Services", "MyDataIn", BlackLight.Services.Error.Errors.ERROR, "Socket appears to have forcefully killed the connection", "", "", "");
									Disconnect();
								}
								//  RaiseEvent onDebug("-----DATA-----Finished Parse-----DATA-----")
								//RaiseEvent onDebug("---------------------------------------------")
							}
							else
							{
								SendLogMessage("Services", "MyDataIn", BlackLight.Services.Error.Errors.DEBUG, "Socket attempting to close connection", "", "", "");
								Disconnect();
								return;
							}
							
							
							
							obj_SocketState = null;
							obj_Socket = null;
							BytesRead = 0;
							System.GC.Collect();
						}
						catch (Exception ex)
						{
							SendLogMessage("Services", "MyDataIn", BlackLight.Services.Error.Errors.ERROR, "Problem recieving data", "", ex.Message, ex.StackTrace);
						}
					}
					catch (Exception ex)
					{
						SendLogMessage("Services", "MyDataIn", BlackLight.Services.Error.Errors.ERROR, "Low level error", "", ex.Message, ex.StackTrace);
					}
				}
				
				public void Raise_Quit (string Nickname, string Reason)
				{
                    if (events.onQuitEvent != null)
                        events.onQuitEvent(Nickname, Reason);
				}
				
				/// -----------------------------------------------------------------------------
				/// <summary>
				/// Function to raise the log message event
				/// </summary>
				/// <param name="Origin">The class of origin</param>
				/// <param name="Method">The method of origin</param>
				/// <param name="Type">The type of error</param>
				/// <param name="Message">Message of error</param>
				/// <param name="Extra">any extra information</param>
				/// <param name="Exception">Exception if avaliable</param>
				/// <param name="Trace">Trace of exception if avaliable</param>
				/// <remarks>
				/// </remarks>
				/// <history>
				/// 	[Caleb]	6/18/2005	Created
				/// </history>
				/// -----------------------------------------------------------------------------
				private ArrayList log_buffer;
				public override void SendLogMessage (string Origin, string Method, BlackLight.Services.Error.Errors Type, string Message, string Extra, string Exception, string Trace)
				{
					try
					{
                        if (events.LogMessageEvent != null)
						{
							if (log_buffer != null && log_buffer.Count > 0)
							{
								LogEvent tEvent;
								while (log_buffer.Count > 0)
								{
									tEvent = (LogEvent)log_buffer[0];
                                    events.LogMessageEvent(tEvent.Origin, tEvent.Method, tEvent.Type, tEvent.Message, tEvent.Extra, tEvent.Exception, tEvent.Trace);
									log_buffer.RemoveAt(0);
								}
							}
							else
                                events.LogMessageEvent(Origin, Method, Type, Message, Extra, Exception, Trace);
						}
						else
						{
							Console.WriteLine("Adding to buffer");
							if (log_buffer == null)
								log_buffer = new ArrayList();
							
							log_buffer.Add(new LogEvent(Origin,Method,Type,Message,Extra,Exception,Trace));
					
						}
					}
					catch (Exception ex)
					{
						//throw ex;
						Console.WriteLine(ex.Message + " " + ex.StackTrace);
						//MessageBox.Show("hrm");
					}
				}
				private class LogEvent
				{
					public string Origin = "";
					public string Method = "";
					public BlackLight.Services.Error.Errors Type;
					public string Message = "";
					public string Extra = "";
					public string Exception = "";
					public string Trace = "";
					public LogEvent(string Origin, string Method, BlackLight.Services.Error.Errors Type, string Message, string Extra, string Exception, string Trace) 
					{
						this.Origin = Origin;
						this.Method = Method;
						this.Type = Type;
						this.Message = Message;
						this.Extra = Extra;
						this.Exception = Exception;
						this.Trace = Trace;
					}

				}
                /*
                protected string[] toIRCLine(string[] array, int index)
                {
                    int spt;
                    for (int x = index; x < array.Length; x++)
                        if (array[x][x] == ':')
                            spt = x; 
                    int ct = array.Length - index - spt;
                    string[] ret = new string[ct];
                    for (int x = index, y = 0; x < spt; x++,y++)
                    {
                        ret[y] = array[x];
                    }
                }*/
                protected static string parseLineSource(string commandString, ref int pos)
                {
                    string source = "";
                    pos = 0;
                    if (commandString[0] == ':')
                    {
                        pos = commandString.IndexOf(' ');
                        source = commandString.Substring(1, pos++ - 1);
                    }
                    return source;
                }
                protected static string parseLineCommand(string commandString, ref int pos)
                {
                    string command = "";
                    int pix = 0;
                    command = (pix = commandString.IndexOf(' ', pos)) > 0 ? commandString.Substring(pos, pix - pos) : commandString.Substring(pos);
                    pos = (pix > 0) ? ++pix : -1;
                    return command;
                }
                protected static List<string> parseLineArguments(string commandString, ref int pos)
                {
                    if (pos <= 0) return null;
                    List<string> parameters = new List<string>();
                    int pix = commandString.IndexOf(' ', pos);
                    while (true)
                    {
                        pix = commandString.IndexOf(' ', pos);
                        if (commandString[pos] == ':')
                            break;
                        else if (pix == -1)
                        {
                            parameters.Add(commandString.Substring(pos));
                            pos = commandString.Length;
                            break;
                        }
                        parameters.Add(commandString.Substring(pos, pix - pos));
                        pos = ++pix;
                    }
                    if (++pos < commandString.Length)
                        parameters.Add(commandString.Substring(pos));
                    return parameters;
                }
                protected override void CommandExec(string commandString)
                {
                    try
                    {
                        SendLogMessage("Services", "CommandExec", BlackLight.Services.Error.Errors.DEBUG, "Recieved: " + commandString, "", "", "");
                        //string[] lineArray;
                        //lineArray = commandString.Split(' ');
                        try
                        {
                            //RaiseEvent onDebug("-----Command----Preparing to Execute Command-----Command-----")
                            BlackLight.Services.Nodes.IRCUser Source = null;
                            string tSource,tCommand = "";
                            List<string> arguements;
                            int pos = 0;
                            commandString = commandString.Trim();
                            tSource = parseLineSource(commandString, ref pos);
                            tCommand = parseLineCommand(commandString, ref pos);
                            arguements = parseLineArguments(commandString,ref pos);
                            //If we havn't setup our host server then do not process it as normal
                            if (MyHost == null)
                            {
                                //RaiseEvent onDebug("My Host Is Nothing")
                                //Don't really check anything just send it through
                                if (Cmds.ContainsKey(tCommand.ToUpper()))
                                {

                                    CmdExec(tCommand)(Source, tCommand, arguements);
                                }

                            }
                            else if (tSource != "")
                            {
                                if (tSource.IndexOf(".") >= 0)
                                {

                                    if (tSource == MyHost.name)
                                    {
                                        Source = MyHost;
                                    }
                                    else
                                    {
                                        Source = GetServer(tSource);
                                    }
                                }
                                else
                                {
                                    Source = GetClient(tSource);
                                    if (Source == null)
                                    {
                                        //RaiseEvent onDebug("Got a message for a client that does not exist. Desync or Invalid Protocol is likely cause")
                                        SendLogMessage("Services", "CommandExec", BlackLight.Services.Error.Errors.DEBUG, "Recieved message from/for user that does not exist", "This could be caused by a number of things at the worst a desync, otherwise it could be something wrong with your protocol, or simply and most likely the user was killed and we received a quit for it", "", "");
                                        return;
                                    }
                                }
                            }
                            else
                            {
                                Source = MyHost;
                            }
                            //RaiseEvent onDebug(String.Format("Command: {0} From: {1} As: {2}", tCommand, tSource, commandString))
                            if (Cmds.ContainsKey(tCommand.ToUpper()))
                            {
                                CmdExec(tCommand)(Source, tCommand, arguements);
                            }
                            Source = null;
                            tSource = null;
                            tCommand = null;
                            arguements = null;
                            commandString = null;
                            System.GC.Collect();
                            // RaiseEvent onDebug("Total Memory : " & System.GC.GetTotalMemory(False) & " Generation: " & System.GC.GetGeneration(Me))
                            // RaiseEvent onDebug("-----Command-----Finished Executing Command-----Command-----")
                        }
                        catch (Exception ex)
                        {
                            SendLogMessage("Services", "CommandExec", BlackLight.Services.Error.Errors.ERROR, "Problem processing command from server", "", ex.Message, ex.StackTrace);
                        }
                    }
                    catch (Exception ex)
                    {
                        SendLogMessage("Services", "CommandExec", BlackLight.Services.Error.Errors.ERROR, "Low level error", "", ex.Message, ex.StackTrace);
                    }
                }
                /*
				protected override void CommandExec (string commandString)
				{
					try
					{
						SendLogMessage("Services", "CommandExec", BlackLight.Services.Error.Errors.DEBUG, "Recieved: " + commandString, "", "", "");
						//string[] lineArray;
						//lineArray = commandString.Split(' ');
						try
						{
							//RaiseEvent onDebug("-----Command----Preparing to Execute Command-----Command-----")
							BlackLight.Services.Nodes.IRCUser tSource = null;
							string tCommand = "";
                            List<string> arguements;
							short tParam = 0;
							//If we havn't setup our host server then do not process it as normal
							if (MyHost == null)
							{
								//RaiseEvent onDebug("My Host Is Nothing")
								//Don't really check anything just send it through

                                if (commandString[0] == ':')
                                {
                                    tCommand = commandString.Substring(commandString.IndexOf(' ') + 2);
                                    if (Cmds.ContainsKey(tCommand))
                                    {

                                        CmdExec(lineArray[1])(tSource, tCommand, toIRCLine(lineArray, 2));
                                    }
                                }
                                else
                                {
                                    if (Cmds.ContainsKey(lineArray[0].ToUpper()))
                                    {
                                        tParam = Convert.ToInt16(lineArray.GetUpperBound(0) + 1);
                                        CmdExec(lineArray[0])(tSource, commandString, tParam);
                                    }
                                }
								
							}
							else if (commandString[0] == ':')
							{
								string tName = lineArray[0].Substring(1);
								tCommand = lineArray[1];
								tParam = Convert.ToInt16(lineArray.GetUpperBound(0));
								if (lineArray[0].IndexOf(".") >= 0)
								{
									
									if (tName == MyHost.Name)
									{
										tSource = MyHost;
										commandString = commandString.Substring(tSource.Name.Length + 3 - 1);
									}
									else
									{
										//Dim tServList As ServiceList
										//tServList = MyHost.GetAllServers
										//For Each srv As Server In tServList
										//    If tName = srv.Name Then
										//        tSource = srv
										//        commandString = Mid(commandString, tSource.Name.Length + 3)
										//        Exit For
										//    End If
										//Next
										tSource = GetServer(tName);
										commandString = commandString.Substring(tSource.Name.Length + 3 - 1);
									}
								}
								else
								{
									tSource = GetClient(tName);
									if (tSource != null)
									{
										commandString = commandString.Substring(tSource.Name.Length + 3 - 1);
									}
									else
									{
										//RaiseEvent onDebug("Got a message for a client that does not exist. Desync or Invalid Protocol is likely cause")
										SendLogMessage("Services", "CommandExec", BlackLight.Services.Error.Errors.DEBUG, "Recieved message from/for user that does not exist", "This could be caused by a number of things at the worst a desync, otherwise it could be something wrong with your protocol, or simply and most likely the user was killed and we received a quit for it", "", "");
										return;
									}
								}
							}
							else
							{
								tSource = MyHost;
								tCommand = lineArray[0];
								tParam = Convert.ToInt16(lineArray.GetUpperBound(0) + 1);
							}
							//RaiseEvent onDebug(String.Format("Command: {0} From: {1} As: {2}", tCommand, tSource, commandString))
							if (Cmds.ContainsKey(tCommand.ToUpper()))
							{
								CmdExec(tCommand)(tSource, commandString, tParam);
							}
							tSource = null;
							tCommand = null;
							commandString = null;
							tParam = 0;
							lineArray = null;
							System.GC.Collect();
							// RaiseEvent onDebug("Total Memory : " & System.GC.GetTotalMemory(False) & " Generation: " & System.GC.GetGeneration(Me))
							// RaiseEvent onDebug("-----Command-----Finished Executing Command-----Command-----")
						}
						catch (Exception ex)
						{
							SendLogMessage("Services", "CommandExec", BlackLight.Services.Error.Errors.ERROR, "Problem processing command from server", "", ex.Message, ex.StackTrace);
						}
					}
					catch (Exception ex)
					{
						SendLogMessage("Services", "CommandExec", BlackLight.Services.Error.Errors.ERROR, "Low level error", "", ex.Message, ex.StackTrace);
					}
				}*/
				private IRCInterpretor CmdExec(string Cmd)
				{
					try
					{
						return ((IRCInterpretor) Cmds[Cmd.ToUpper()]);
					}
					catch (Exception ex)
					{
						SendLogMessage("Services", "CmdExec", BlackLight.Services.Error.Errors.ERROR, "Problem executing actual command", "", ex.Message, ex.StackTrace);
						return null;
					}
                }



                #region "Interpreters"
                private delegate bool IRCInterpretor(BlackLight.Services.Nodes.IRCUser Source, string IRCCommand, List<string> arguments);
				private bool NewServer(BlackLight.Services.Nodes.IRCUser Source, string command, List<string> arguments)
				{
					//Viable String Options
					//--SERVERNAME
					//--SERVERNUMERIC
                    //--HOPS
					//--SERVERDESCRIPTION
					try
					{
						if (MyIRCd.Commands.ContainsKey("SERVERRECEIVE") == false)
						{
							SendLogMessage("Services", "NewServer", BlackLight.Services.Error.Errors.FATAL, "SERVERRECEIVE missing from protocol", "", "", "");
							return false;
						}
                        Command cmd = MyIRCd.Commands["SERVERRECEIVE"];
                        Command cmdAlt = MyIRCd.Commands["SERVERALTRECEIVE"];
						if (Source != null)
						{
							
                            if (Source is BlackLight.Services.Nodes.Server)
							{
									BlackLight.Services.Nodes.Server tHostServer = ((BlackLight.Services.Nodes.Server) Source);
									BlackLight.Services.Nodes.Server tServer = GetServer(arguments[cmd.getParameterIndex("SERVERNAME")]);
									if (tServer != null)
									{
										SendLogMessage("Services", "NewServer", BlackLight.Services.Error.Errors.FATAL, "Collision, received message for a server that already exists", "", "", "");
										//NOTE TO SELF MAKE THIS HAWT
										Disconnect();
										return false;
									}
                                    tServer = new BlackLight.Services.Nodes.Server(arguments[cmd.getParameterIndex("SERVERNAME")], this);
                                    if (cmd.hasParameter("SERVERNUMERIC"))
                                    {
                                        if (cmd.parameterCount() == 4 && arguments.Count < 4)
                                        {
                                            if (arguments[arguments.Count - 1].ToUpper().StartsWith("U"))
                                            {
                                                string[] unrealData = arguments[arguments.Count - 1].Split(new char[] { ' ' });
                                                if (unrealData[0].Contains("-"))
                                                {
                                                    if (unrealData.Length > 1)
                                                    {
                                                        for (int x = 1; x < unrealData.Length - 1; x++)
                                                        {
                                                            MyHost.description += unrealData[x] + " ";
                                                        }
                                                    }
                                                    tServer.description += unrealData[unrealData.Length - 1];
                                                    tServer.numeric = int.Parse(unrealData[0].Substring(unrealData[0].LastIndexOf("-") + 1));
                                                    SendLogMessage("Services", "NewServer", BlackLight.Services.Error.Errors.ERROR, "Word", MyHost.numeric.ToString(), "", "");
                                                }
                                                else
                                                {
                                                    if (cmdAlt.hasParameter("SERVERDESCRIPTION"))
                                                    {
                                                        if (cmdAlt.getParameterIndex("SERVERDESCRIPTION") < arguments.Count - 1)
                                                        {
                                                            MyHost.description = arguments[cmdAlt.getParameterIndex("SERVERDESCRIPTION")];
                                                        }
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                if (cmdAlt.hasParameter("SERVERDESCRIPTION"))
                                                {
                                                    if (cmdAlt.getParameterIndex("SERVERDESCRIPTION") < arguments.Count - 1)
                                                    {
                                                        tServer.description = arguments[cmdAlt.getParameterIndex("SERVERDESCRIPTION")];
                                                    }
                                                }
                                            }
                                        }
                                        else
                                        {
                                            if (cmd.hasParameter("SERVERDESCRIPTION"))
                                                tServer.description = arguments[cmd.getParameterIndex("SERVERDESCRIPTION")];
                                            tServer.numeric = Convert.ToInt16(arguments[cmd.getParameterIndex("SERVERNUMERIC")]);
                                        }
                                    }
                                    else
                                    {
                                        if (cmd.hasParameter("SERVERDESCRIPTION"))
                                            tServer.description = arguments[cmd.getParameterIndex("SERVERDESCRIPTION")];
                                        tServer.numeric = tServer.id;
                                    }
									tServer.hostServer = tHostServer;
									tHostServer.leafs.Add(tServer);
                                    if (events.onServerEvent != null)
                                        events.onServerEvent(tServer.name, tServer);
									tServer = null;
								}
							}
						else
						{
                            MyHost = new BlackLight.Services.Nodes.Server(arguments[cmd.getParameterIndex("SERVERNAME")], this);

                            if (cmd.hasParameter("SERVERNUMERIC"))
                            {
                                if (cmd.parameterCount() == 4 && arguments.Count < 4)
                                {
                                    if (arguments[arguments.Count - 1].ToUpper().StartsWith("U"))
                                    {
                                        string[] unrealData = arguments[arguments.Count - 1].Split(new char[] { ' ' });
                                        if (unrealData[0].Contains("-"))
                                        {
                                            if (unrealData.Length > 1)
                                            {
                                                for (int x = 1; x < unrealData.Length - 1; x++)
                                                {
                                                    MyHost.description += unrealData[x] + " ";
                                                }
                                            }
                                            MyHost.description += unrealData[unrealData.Length - 1];
                                            MyHost.numeric = int.Parse(unrealData[0].Substring(unrealData[0].LastIndexOf("-")+1));
                                            SendLogMessage("Services", "NewServer", BlackLight.Services.Error.Errors.ERROR, "Word", MyHost.numeric.ToString(), "", "");
                                        }
                                        else
                                        {
                                            if (cmdAlt.hasParameter("SERVERDESCRIPTION"))
                                            {
                                                if (cmdAlt.getParameterIndex("SERVERDESCRIPTION") < arguments.Count - 1)
                                                {
                                                    MyHost.description = arguments[cmdAlt.getParameterIndex("SERVERDESCRIPTION")];
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (cmdAlt.hasParameter("SERVERDESCRIPTION"))
                                        {
                                            if (cmdAlt.getParameterIndex("SERVERDESCRIPTION") < arguments.Count - 1)
                                            {
                                                MyHost.description = arguments[cmdAlt.getParameterIndex("SERVERDESCRIPTION")];
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    if (cmd.hasParameter("SERVERDESCRIPTION"))
                                        MyHost.description = arguments[cmd.getParameterIndex("SERVERDESCRIPTION")];
                                    MyHost.numeric = Convert.ToInt16(arguments[cmd.getParameterIndex("SERVERNUMERIC")]);
                                }
                            }
                            else
                            {
                                if (cmd.hasParameter("SERVERDESCRIPTION"))
                                    MyHost.description = arguments[cmd.getParameterIndex("SERVERDESCRIPTION")];
                                MyHost.numeric = MyHost.id;
                            }
                            if (events.onServerEvent != null)
                                events.onServerEvent(MyHost.name, MyHost);
						}
						return true;
					}
					catch (Exception ex)
					{
						SendLogMessage("Services", "NewServer", BlackLight.Services.Error.Errors.ERROR, "Problem parsing", "", ex.Message, ex.StackTrace);
						return false;
					}
				}
                private bool Nick(BlackLight.Services.Nodes.IRCUser Source, string command, List<string> arguments)
				{
					//Viable String Options
					//--NICKNAME
					//--HOPS
					//--SERVERNAME
					//---OR-SERVERNUMERIC
					//--USERNAME
					//--REALNAME
					//--TIME
					//--MODES
					//--HOST
					//--VHOST (OPTIONAL)
					try
					{
						if (Source != null)
						{
                            Command cmd = MyIRCd.Commands["CONNECTRECEIVE"];
							if (arguments.Count == cmd.parameterCount())
							{
                                /*
                                 * What?
								// Here is the issue with the invention of timestamp its possible that the
								// Remote server may think its up to us to resolve the collision
								// In which case we should...
								// The problem is when servers don't trust us enough to deal with it
								// If thats the case we will deal with it, and then we will receive kill
								// messages and other such rubbish later on...that sadly will
								// Cause the services to think its desynced and spit out a log message
								// Until I find another way to deal with this (add info in protocol file?)
								// we are just going to have to live with it
								if (LocalClients.Contains(tMatch.Groups["NICKNAME"].Value))
								{
									LocalClient tClient = LocalClients[tMatch.Groups["NICKNAME"].Value];
									SendLogMessage("Services", "Nick", BlackLight.Services.Error.Errors.DEBUG, "New user collided with local client", "", "", "");
									if (tClient != null)
									{
										if (tClient.Time >= Base64.B64ToInt(tMatch.Groups["TIME"].Value))
										{
											SendLogMessage("Services", "Nick", BlackLight.Services.Error.Errors.DEBUG, "New user is older, passivly kill our client", "", "", "");
											tClient.Quit("Nick collision");
										}
									}
								}
                                 */
                                if (LocalClients.Contains(arguments[cmd.getParameterIndex("NICKNAME")]))
                                {
                                    
                                    LocalClient tClient = LocalClients[arguments[cmd.getParameterIndex("NICKNAME")]];
                                    Commands.Send_Kill(this.MyName, arguments[cmd.getParameterIndex("NICKNAME")], "Nick Collision");
                                    SendLogMessage("Services", "Nick", BlackLight.Services.Error.Errors.DEBUG, "New user collided with local client", "", "", "");
                                }
                                if (GetClient(arguments[cmd.getParameterIndex("NICKNAME")]) == null)
								{
                                    BlackLight.Services.Nodes.Client tUser = new BlackLight.Services.Nodes.Client(arguments[cmd.getParameterIndex("NICKNAME")], this);
									//tMatch.Groups("REALNAME")
                                    tUser.username = arguments[cmd.getParameterIndex("USERNAME")];
                                    tUser.host = arguments[cmd.getParameterIndex("HOST")];
									short tNumeric;
									if (cmd.hasParameter("SERVERNUMERIC"))
									{
                                        tNumeric = Convert.ToInt16(arguments[cmd.getParameterIndex("SERVERNUMERIC")]);
									}
									else
									{
										tNumeric = - 1;
									}

                                    if (arguments[cmd.getParameterIndex("SERVERNAME")] == MyHost.name || tNumeric == MyHost.numeric)
									{
										tUser.hostServer = MyHost;
									}
									else
									{
										BlackLight.Services.Nodes.ServiceList tServList;
										tServList = MyHost.GetAllServers();
										foreach (BlackLight.Services.Nodes.Server srv in tServList)
										{
                                            if (arguments[cmd.getParameterIndex("SERVERNAME")] == srv.name || tNumeric == srv.numeric)
											{
												tUser.hostServer = srv;
											}
										}
									}
                                    if (cmd.hasParameter("VHOST"))
                                        tUser.vHost = arguments[cmd.getParameterIndex("VHOST")];
                                    tUser.realname = arguments[cmd.getParameterIndex("REALNAME")];
									tUser.hostServer.users.Add(tUser);
                                    tUser.ParseModeSet(arguments[cmd.getParameterIndex("MODES")]);
                                    tUser.time = Base64.B64ToInt(arguments[cmd.getParameterIndex("TIME")]);
                                    if (events.onClientConnectEvent != null)
                                        events.onClientConnectEvent(tUser.name, tUser);
									tUser = null;
								}
								else
								{
                                    SendLogMessage("Services", "Nick", BlackLight.Services.Error.Errors.ERROR, "New user nick collison", arguments[cmd.getParameterIndex("NICKNAME")], "", "");
								}
							}
							else
							{
								if (Source is BlackLight.Services.Nodes.Client)
								{
                                    cmd = MyIRCd.Commands["NICKRECEIVE"];
									if (arguments.Count == cmd.parameterCount())
									{
                                        if (GetClient(arguments[cmd.getParameterIndex("NEWNICK")]) == null)
										{
											BlackLight.Services.Nodes.Client tUser;
											tUser = ((BlackLight.Services.Nodes.Client) Source);
											string tOldNick = tUser.nick;
                                            tUser.nick = arguments[cmd.getParameterIndex("NEWNICK")];
                                            if (events.onNickEvent != null)
                                                events.onNickEvent(tOldNick, tUser.nick);
											tUser = null;
											tOldNick = null;
										}
										else
										{
											SendLogMessage("Services", "Nick", BlackLight.Services.Error.Errors.ERROR, "Nickname collision", "", "", "");
										}
									}
									else
									{
										SendLogMessage("Services", "Nick", BlackLight.Services.Error.Errors.DEBUG | BlackLight.Services.Error.Errors.ERROR, "Unable to match protocol with message received", "", "", "");
									}
								}
							}
						}
						return true;
					}
					catch (Exception ex)
					{
						SendLogMessage("Services", "Nick", BlackLight.Services.Error.Errors.ERROR, "Problem parsing", "", ex.Message, ex.StackTrace);
						return false;
					}
				}
                private bool Channel_Join(BlackLight.Services.Nodes.IRCUser Source, string command, List<string> arguments)
				{
					//Viable String Options
					//--NICKNAME
					//--CHANNEL
					try
					{
						if (Source != null && Source is BlackLight.Services.Nodes.Client)
						{
                            Command cmd = MyIRCd.Commands["JOINRECEIVE"];
							if (arguments.Count == cmd.parameterCount())
							{
								BlackLight.Services.Nodes.Client tUser = ((BlackLight.Services.Nodes.Client) Source);
                                if (arguments[cmd.getParameterIndex("CHANNEL")][0] == '#' || arguments[cmd.getParameterIndex("CHANNEL")][0] == '&')
								{
									int tChannelIndex;

                                    if (Channels.Contains(arguments[cmd.getParameterIndex("CHANNEL")]) == false)
									{
                                        tChannelIndex = Channels.Add(new BlackLight.Services.Nodes.Channel(arguments[cmd.getParameterIndex("CHANNEL")], this));
									}
									else
									{
                                        tChannelIndex = Channels.IndexOf(arguments[cmd.getParameterIndex("CHANNEL")]);
									}
									//RaiseEvent onDebug("JOIN-Join Nick: " & tUser.Nick & " Channel: " & tMatch.Groups("CHANNEL").Value)
									Channels[tChannelIndex].channelMembers.Add(new BlackLight.Services.Nodes.ChanMember(tUser, this));
									tUser.channels.Add(Channels[tChannelIndex]);
                                    if (events.onJoinEvent != null)
                                        events.onJoinEvent(tUser.nick, Channels[tChannelIndex].name);
									tChannelIndex = 0;
								}
                                else if (arguments[cmd.getParameterIndex("CHANNEL")][0] == '0')
								{
									
									while (tUser.channels.Count > 0)
									{
										if (tUser.channels[0].channelMembers.Count == 1 && tUser.channels[0].channelMembers.Contains(tUser))
										{
											Channels.Remove(tUser.channels[0]);
										}
										else
										{
											tUser.channels[0].channelMembers.Remove(tUser.channels[0].channelMembers[tUser]);
											tUser.channels.RemoveAt(0);
										}
									}
								}
								tUser = null;

							}
							else
							{
								SendLogMessage("Services", "Channel_Join", BlackLight.Services.Error.Errors.DEBUG | BlackLight.Services.Error.Errors.ERROR, "Unable to match protocol with message received", "", "", "");
								return false;
							}
							return true;
						}
						return false;
					}
					catch (Exception ex)
					{
						SendLogMessage("Services", "Channel_Join", BlackLight.Services.Error.Errors.ERROR, "Problem parsing", "", ex.Message, ex.StackTrace);
						return false;
					}
				}
                private bool Channel_Kick(BlackLight.Services.Nodes.IRCUser Source, string command, List<string> arguments)
				{
					//Viable String Options
					//--NICKNAME
					//--CHANNEL
                    //--REASON
					try
					{
						if (Source != null)
						{
                            Command cmd = MyIRCd.Commands["KICKRECEIVE"];
							if (arguments.Count == cmd.parameterCount())
							{
                                int tChannelIndex = Channels.IndexOf(arguments[cmd.getParameterIndex("CHANNEL")]);
								if (tChannelIndex < 0)
								{
                                    SendLogMessage("Services", "Kick", BlackLight.Services.Error.Errors.ERROR, "Recived kick for channel that does not exist", arguments[cmd.getParameterIndex("CHANNEL")], "", "");
									return false;
								}
								
								// Dim tUser As Client = Source
                                BlackLight.Services.Nodes.Client tKicked = GetClient(arguments[cmd.getParameterIndex("NICKNAME")]);
								if (Channels[tChannelIndex].channelMembers.Contains(tKicked))
								{
									Channels[tChannelIndex].channelMembers.Remove(Channels[tChannelIndex].channelMembers[tKicked]);
									if (tKicked.channels.Contains(Channels[tChannelIndex]))
									{
										tKicked.channels.Remove(Channels[tChannelIndex]);
									}
									else
									{
										SendLogMessage("Services", "Kick", BlackLight.Services.Error.Errors.ERROR, "Channel Lists Desync: member has no record of channel", "", "", "");
									}
								}
								else if (tKicked.channels.Contains(Channels[tChannelIndex]))
								{
									tKicked.channels.Remove(Channels[tChannelIndex]);
									SendLogMessage("Services", "Kick", BlackLight.Services.Error.Errors.ERROR, "Channel Lists Desync: channel has no record of member", "", "", "");
								}
								else
								{
									SendLogMessage("Services", "Kick", BlackLight.Services.Error.Errors.ERROR, "Recieved kick for invalid channel member", "", "", "");
								}
								
								if (Channels[tChannelIndex].channelMembers.Count == 0)
								{
									Channels.RemoveAt(tChannelIndex);
								}
                                if (events.onKickEvent != null)
                                    events.onKickEvent(Source.name, tKicked.nick, arguments[cmd.getParameterIndex("REASON")]);
							}
							else
							{
								SendLogMessage("Services", "Kick", BlackLight.Services.Error.Errors.DEBUG | BlackLight.Services.Error.Errors.ERROR, "Unable to match protocol with message received", "", "", "");
							}
							return true;
						}
						return false;
					}
					catch (Exception ex)
					{
						SendLogMessage("Services", "Kick", BlackLight.Services.Error.Errors.ERROR, "Problem parsing", "", ex.Message, ex.StackTrace);
						return false;
					}
				}
                private bool Channel_Part(BlackLight.Services.Nodes.IRCUser Source, string command, List<string> arguments)
				{
					//Viable String Options
					//--NICKNAME
					//--CHANNEL
					try
					{
						if (Source != null&& Source is BlackLight.Services.Nodes.Client)
						{
                            Command cmd = MyIRCd.Commands["PARTRECEIVE"];
							if (arguments.Count == cmd.parameterCount())
							{
                                int tChannelIndex = Channels.IndexOf(arguments[cmd.getParameterIndex("CHANNEL")]);
								if (tChannelIndex < 0)
								{
                                    SendLogMessage("Services", "Part", BlackLight.Services.Error.Errors.ERROR, "Channel does not exist", arguments[cmd.getParameterIndex("CHANNEL")], "", "");
									return false;
								}
								BlackLight.Services.Nodes.Client tUser = ((BlackLight.Services.Nodes.Client) Source);
								if (Channels[tChannelIndex].channelMembers.Contains(tUser))
								{
									Channels[tChannelIndex].channelMembers.Remove(Channels[tChannelIndex].channelMembers[tUser]);
									if (tUser.channels.Contains(Channels[tChannelIndex]))
									{
										tUser.channels.Remove(Channels[tChannelIndex]);
									}
									else
									{
										SendLogMessage("Services", "Part", BlackLight.Services.Error.Errors.ERROR, "Channel Lists Desync member has no record of channel, channel has record of member", "", "", "");
									}
								}
								else if (tUser.channels.Contains(Channels[tChannelIndex]))
								{
									tUser.channels.Remove(Channels[tChannelIndex]);
									SendLogMessage("Services", "Part", BlackLight.Services.Error.Errors.ERROR, "Channel Lists Desync member has record of channel, channel has no record of member", "", "", "");
								}
								else
								{
									SendLogMessage("Services", "Part", BlackLight.Services.Error.Errors.ERROR, "Recieved part for invalid channel member", "", "", "");
								}
								
								if (Channels[tChannelIndex].channelMembers.Count == 0)
								{
									Channels.RemoveAt(tChannelIndex);
								}
                                if (events.onPartEvent != null)
                                    events.onPartEvent(tUser.name, arguments[cmd.getParameterIndex("CHANNEL")]);
							}
							else
							{
								SendLogMessage("Services", "Part", BlackLight.Services.Error.Errors.DEBUG | BlackLight.Services.Error.Errors.ERROR, "Unable to match protocol with message received", "", "", "");
							}
							return true;
						}
						return false;
					}
					catch (Exception ex)
					{
						SendLogMessage("Services", "Part", BlackLight.Services.Error.Errors.ERROR, "Problem parsing", "", ex.Message, ex.StackTrace);
						return false;
					}
				}
                private bool Channel_Topic(BlackLight.Services.Nodes.IRCUser Source, string command, List<string> arguments)
				{
					//Viable String Options
					//--WHOSET
                    //--TIME
					//--CHANNEL
                    //--TOPIC
					try
					{
						if (Source != null)
						{
                            Command cmd = MyIRCd.Commands["TOPICRECEIVE"];
							if (arguments.Count == cmd.parameterCount())
							{
                                int tChannelIndex = Channels.IndexOf(arguments[cmd.getParameterIndex("CHANNEL")]);
								if (tChannelIndex < 0)
								{
                                    SendLogMessage("Services", "Topic", BlackLight.Services.Error.Errors.ERROR, "Channel does not exist", arguments[cmd.getParameterIndex("CHANNEL")], "", "");
									return false;
								}
                                
                                Channels[tChannelIndex].topic.text = arguments[cmd.getParameterIndex("TOPIC")];
                                Channels[tChannelIndex].topic.setBy = arguments[cmd.getParameterIndex("WHOSET")];
                                if (arguments[cmd.getParameterIndex("TIME")].StartsWith("!"))
                                {
                                    Channels[tChannelIndex].topic.time = Base64.B64ToInt(arguments[cmd.getParameterIndex("TIME")]);
                                }
                                else
                                {
                                    Channels[tChannelIndex].topic.time = int.Parse(arguments[cmd.getParameterIndex("TIME")]);
                                }
							}
							else
							{
								SendLogMessage("Services", "Topic", BlackLight.Services.Error.Errors.DEBUG | BlackLight.Services.Error.Errors.ERROR, "Unable to match protocol with message received", "", "", "");
							}
							return true;
						}
						return false;
					}
					catch (Exception ex)
					{
						SendLogMessage("Services", "Topic", BlackLight.Services.Error.Errors.ERROR, "Problem parsing", "", ex.Message, ex.StackTrace);
						return false;
					}
				}
                private bool Server_Channel_Join(BlackLight.Services.Nodes.IRCUser Source, string command, List<string> arguments)
				{
					//Viable String Options
					//--USERLIST
					//--CHANNEL
					//--TIME
					//--MODESTRING
					try
					{
						if (Source != null && Source is BlackLight.Services.Nodes.Server)
						{
                            Command cmd = MyIRCd.Commands["SJOINRECEIVE"];
							if (arguments.Count == cmd.parameterCount())
							{
								int tChannelIndex;
                                if (Channels.Contains(arguments[cmd.getParameterIndex("CHANNEL")]) == false)
								{
                                    tChannelIndex = Channels.Add(new BlackLight.Services.Nodes.Channel(arguments[cmd.getParameterIndex("CHANNEL")], this));
								}
								else
								{
                                    tChannelIndex = Channels.IndexOf(arguments[cmd.getParameterIndex("CHANNEL")]);
								}
                                Channels[tChannelIndex].parseModeSet(arguments[cmd.getParameterIndex("MODESTRING")]);
                                string[] tUserList = arguments[cmd.getParameterIndex("USERLIST")].Split(' ');
								BlackLight.Services.Nodes.Client tUser;
								short tInt = 0;
                                short sType = -1;
								string tModes = "";
								foreach (string TNick in tUserList)
								{
									// RaiseEvent onDebug("SJOIN-Join Nick: " & TNick & " Channel: " & tMatch.Groups("CHANNEL").Value)
                                    sType = MyIRCd.SJOINType(TNick);
                                    if (sType > 0)
                                    {
                                        // for later support of channel access lists
                                        switch (sType)
                                        {
                                                //Ban
                                            case 1:
                                                break;
                                                //Except
                                            case 2:
                                                break;
                                                //Invite
                                            case 3:
                                                break;
                                        }
                                    }
                                    else
                                    {
                                        if (sType == 0)
                                        {
                                            tModes = MyIRCd.ReadySJOINModes(TNick);
                                            tUser = GetClient(MyIRCd.ReadySJOINNick(TNick));
                                        }
                                        else
                                            tUser = GetClient(TNick);
                                        
                                        if (tUser != null)
                                        {
                                            int tMemberIndex = Channels[tChannelIndex].channelMembers.Add(new BlackLight.Services.Nodes.ChanMember(tUser, this));
                                            tUser.channels.Add(Channels[tChannelIndex]);
                                            foreach (char tChar in tModes)
                                            {
                                                if (MyIRCd.StatusModes.IndexOf(tModes[tInt]) < 0)
                                                {
                                                    tModes.Replace(tChar.ToString(), "");
                                                    //RaiseEvent onIRCError("Invalid mode received")
                                                    SendLogMessage("Services", "SJOIN", BlackLight.Services.Error.Errors.DEBUG | BlackLight.Services.Error.Errors.ERROR, "Invalid mode received", "Bad protocol?", "", "");
                                                }
                                                else
                                                {
                                                    Channels[tChannelIndex].channelMembers[tMemberIndex].addMode(tChar);
                                                }
                                            }
                                            if (events.onJoinEvent != null)
                                                events.onJoinEvent(MyIRCd.ReadySJOINNick(TNick), arguments[cmd.getParameterIndex("CHANNEL")]);
                                        }
                                        else
                                        {
                                            SendLogMessage("Services", "SJOIN", BlackLight.Services.Error.Errors.ERROR, "Recieved SJOIN for a client that does not exist", MyIRCd.ReadySJOINNick(TNick), "", "");
                                        }
                                    }
								}
								
							}
							else
							{
								SendLogMessage("Services", "SJOIN", BlackLight.Services.Error.Errors.DEBUG | BlackLight.Services.Error.Errors.ERROR, "Unable to match protocol with message received", "", "", "");
							}
							return true;
						}
						return false;
					}
					catch (Exception ex)
					{
						SendLogMessage("Services", "SJOIN", BlackLight.Services.Error.Errors.ERROR, "Problem parsing", "", ex.Message, ex.StackTrace);
						return false;
					}
				}
                private bool ModeChange(BlackLight.Services.Nodes.IRCUser Source, string command, List<string> arguments)
				{
					//Viable String Options
					//--OBJECT
					//--CHANNEL
					try
					{
						if (Source != null)
						{
                            Command cmd = MyIRCd.Commands["MODERECEIVE"];
							if (arguments.Count == cmd.parameterCount())
							{
                                if (arguments[cmd.getParameterIndex("OBJECT")][0] == '#' || arguments[cmd.getParameterIndex("OBJECT")][0] == '&')
								{
                                    int tChannelIndex = Channels.IndexOf(arguments[cmd.getParameterIndex("OBJECT")]);
									if (tChannelIndex < 0)
									{
                                        SendLogMessage("Services", "ModeChange", BlackLight.Services.Error.Errors.ERROR, "Channel does not exist", arguments[cmd.getParameterIndex("OBJECT")], "", "");
										return false;
									}
                                    Channels[tChannelIndex].parseModeSet(arguments[cmd.getParameterIndex("MODESTRING")]);
                                    if (events.onModeEvent != null)
                                        events.onModeEvent();
								}
								else
								{
									//Erm....
									//RaiseEvent onDebug("Mode Command matched channel protocol, but is not for a channel")
									SendLogMessage("Services", "ModeChange", BlackLight.Services.Error.Errors.WARNING, "Mode Command matched channel protocol, but is not for a channel", "", "", "");
								}
							}
							else
							{
								SendLogMessage("Services", "ModeChange", BlackLight.Services.Error.Errors.DEBUG, "UMODE", "", "", "");
								if (Source is BlackLight.Services.Nodes.Client)
								{
                                    cmd = MyIRCd.Commands["UMODERECEIVE"];
									if (arguments.Count == cmd.parameterCount())
									{
										BlackLight.Services.Nodes.Client tUser;
										if (!cmd.hasParameter("OBJECT"))
										{
											tUser = GetClient(Source.name);
										}
										else
										{
                                            tUser = GetClient(arguments[cmd.getParameterIndex("OBJECT")]);
										}
										
										if (tUser != null)
										{
											tUser.ParseModeSet(arguments[cmd.getParameterIndex("MODESTRING")]);
										}
										else
										{
											SendLogMessage("Services", "ModeChange", BlackLight.Services.Error.Errors.DEBUG | BlackLight.Services.Error.Errors.ERROR, "Unable to match find client specified in mode change", "protocol error?", "", "");
										}
										tUser = null;
									}
									else
									{
										SendLogMessage("Services", "ModeChange", BlackLight.Services.Error.Errors.DEBUG | BlackLight.Services.Error.Errors.ERROR, "Unable to match protocol with message received", "", "", "");
									}
								}
								else
								{
									SendLogMessage("Services", "ModeChange", BlackLight.Services.Error.Errors.DEBUG | BlackLight.Services.Error.Errors.ERROR, "Invalid Source", "", "", "");
								}
							}
							
							return true;
						}
						return false;
					}
					catch (Exception ex)
					{
						SendLogMessage("Services", "Mode", BlackLight.Services.Error.Errors.ERROR, "Problem parsing", "", ex.Message, ex.StackTrace);
						return false;
					}
				}
                private bool SVSMode(BlackLight.Services.Nodes.IRCUser Source, string command, List<string> arguments)
				{
					//Viable String Options
					//--OBJECT
					//--MODESTRING
                    //--TIME
					try
					{
						if (Source != null)
						{
                            Command cmd = MyIRCd.Commands["SVSMODERECEIVE"];
							if (command == cmd.name && arguments.Count == cmd.parameterCount())
							{
                                BlackLight.Services.Nodes.Client tUser = GetClient(arguments[cmd.getParameterIndex("OBJECT")]);
								if (tUser != null)
								{
                                    tUser.ParseModeSet(arguments[cmd.getParameterIndex("MODESTRING")]);
								}
								tUser = null;
							}
							else
							{
                                cmd = MyIRCd.Commands["SVS2MODERECEIVE"];
								if (command == cmd.name && arguments.Count == cmd.parameterCount())
								{
                                    BlackLight.Services.Nodes.Client tUser = GetClient(arguments[cmd.getParameterIndex("OBJECT")]);
									if (tUser != null)
									{
                                        tUser.ParseModeSet(arguments[cmd.getParameterIndex("MODESTRING")]);
									}
									tUser = null;
								}
								else
								{
									
									SendLogMessage("Services", "SVSMODE", BlackLight.Services.Error.Errors.DEBUG | BlackLight.Services.Error.Errors.ERROR, "Unable to match protocol with message received", "", "", "");
								}
							}
							
							return true;
						}
						return false;
					}
					catch (Exception ex)
					{
						SendLogMessage("Services", "SVSMode", BlackLight.Services.Error.Errors.ERROR, "Problem parsing", "", ex.Message, ex.StackTrace);
						return false;
					}
				}
                private bool NewTKL(BlackLight.Services.Nodes.IRCUser Source, string command, List<string> arguments)
				{
					return false;
				}
                private bool NullFunction(BlackLight.Services.Nodes.IRCUser Source, string command, List<string> arguments)
				{
					return false;
				}
                private bool EOS(BlackLight.Services.Nodes.IRCUser Source, string command, List<string> arguments)
				{
					TimeSpan x = new TimeSpan(System.DateTime.Now.Ticks-starttime.Ticks);
					//Console.WriteLine("Recieved EOS At:" + x.Seconds + " " + x.Milliseconds);
					SendLogMessage("Services", "EOS", BlackLight.Services.Error.Errors.DEBUG, "Recieved EOS At:" + x.Seconds + " " + x.Milliseconds, "", "", "");
					return true;;
				}
                private bool PrivMsg(BlackLight.Services.Nodes.IRCUser Source, string command, List<string> arguments)
				{
                    // Make comply with RFC and all receiver types
					//Viable String Options
                    //--RECEIVER
                    //--MESSAGE
					try
					{
						if (Source != null&& Source is BlackLight.Services.Nodes.Client)
						{
                            Command cmd = MyIRCd.Commands["PRIVMSGRECEIVE"];
							if (arguments.Count == cmd.parameterCount())
							{
								BlackLight.Services.Nodes.Client tUser = ((BlackLight.Services.Nodes.Client) Source);
								short LocalIndex;
								LocalClient tDest;
								string tReciever;
                                if (arguments[cmd.getParameterIndex("RECEIVER")].IndexOf("@") >= 1)
								{
                                    tReciever = arguments[cmd.getParameterIndex("RECEIVER")].Substring(0, arguments[cmd.getParameterIndex("RECEIVER")].IndexOf("@"));
								}
								else
								{
                                    tReciever = arguments[cmd.getParameterIndex("RECEIVER")];
								}
								LocalIndex = Convert.ToInt16(LocalClients.IndexOf(tReciever));
                                string[] tMsgArray = arguments[cmd.getParameterIndex("MESSAGE")].Split(' ');
								if (LocalIndex >= 0)
								{
									tDest = LocalClients[LocalIndex];
									tDest.OnMsg(tUser, tMsgArray);
								}

                                if (events.onPrivMsgEvent != null)
                                    events.onPrivMsgEvent(tUser, tReciever, tMsgArray);
								
							}
							else
							{
								SendLogMessage("Services", "PrivMSG", BlackLight.Services.Error.Errors.DEBUG | BlackLight.Services.Error.Errors.ERROR, "Unable to match protocol with message received", "", "", "");
							}
						}
						return true;
					}
					catch (Exception ex)
					{
						SendLogMessage("Services", "PrivMSG", BlackLight.Services.Error.Errors.ERROR, "Problem parsing", "", ex.Message, ex.StackTrace);
						return false;
					}
				}
                private bool Notice(BlackLight.Services.Nodes.IRCUser Source, string command, List<string> arguments)
				{
                    //TODO Make comply with all parameters in RFC, and receiver types
					//Viable String Options
                    //--RECEIVER
                    //--MESSAGE
					try
					{
						if (Source != null&& Source is BlackLight.Services.Nodes.Client)
						{
                            Command cmd = MyIRCd.Commands["NOTICERECEIVE"];
							if (arguments.Count == cmd.parameterCount())
							{
								BlackLight.Services.Nodes.Client tUser = ((BlackLight.Services.Nodes.Client) Source);
                                string[] tMsgArray = arguments[cmd.getParameterIndex("MESSAGE")].Split(' ');
                                if (events.onNoticeEvent != null)
                                    events.onNoticeEvent(tUser, arguments[cmd.getParameterIndex("RECEIVER")], tMsgArray);
								tUser = null;
							}
							else
							{
								SendLogMessage("Services", "Notice", BlackLight.Services.Error.Errors.DEBUG | BlackLight.Services.Error.Errors.ERROR, "Unable to match protocol with message received", "", "", "");
							}
						}
						return true;
					}
					catch (Exception ex)
					{
						SendLogMessage("Services", "Notice", BlackLight.Services.Error.Errors.ERROR, "Problem parsing", "", ex.Message, ex.StackTrace);
						return false;
					}
				}
                private bool Ping(BlackLight.Services.Nodes.IRCUser Source, string command, List<string> arguments)
				{
					//Viable String Options
					//--REMOTESERVERNAME
                    //--SERVERNAME
					try
					{
                        Command cmd = MyIRCd.Commands["PINGLOCALRECEIVE"];
						if (arguments.Count == cmd.parameterCount())
						{
                            Commands.Send_Ping(MyName, arguments[cmd.getParameterIndex("REMOTESERVERNAME")]);
							SendLogMessage("Services", "Ping", BlackLight.Services.Error.Errors.DEBUG, "PoooING PooooONG", "", "", "");
                            if (events.onPingEvent != null)
                                events.onPingEvent();
						}
						else
						{
                            cmd = MyIRCd.Commands["PINGREMOTERECEIVE"];
							if (arguments.Count == cmd.parameterCount())
							{
                                Commands.Send_Ping(MyName, arguments[cmd.getParameterIndex("REMOTESERVERNAME")]);
								SendLogMessage("Services", "Ping", BlackLight.Services.Error.Errors.DEBUG, "PoooING PooooONG", "", "", "");
                                if (events.onPingEvent != null)
                                    events.onPingEvent();
							}
							else
							{
								SendLogMessage("Services", "Ping", BlackLight.Services.Error.Errors.DEBUG | BlackLight.Services.Error.Errors.ERROR, "Unable to match protocol with message received", "", "", "");
							}
						}
						return true;
					}
					catch (Exception ex)
					{
						SendLogMessage("Services", "Ping", BlackLight.Services.Error.Errors.ERROR, "Problem parsing", "", ex.Message, ex.StackTrace);
						return false;
					}
				}
                private bool UserQuit(BlackLight.Services.Nodes.IRCUser Source, string command, List<string> arguments)
				{
					//Viable String Options
					//--REASON
					try
					{
						if (Source != null&& Source is BlackLight.Services.Nodes.Client)
						{
                            Command cmd = MyIRCd.Commands["QUITRECEIVE"];
							if (arguments.Count == cmd.parameterCount())
							{
                                if (events.onQuitEvent != null)
                                    events.onQuitEvent(Source.name, arguments[cmd.getParameterIndex("REASON")]);
								Source.Dispose();
							}
							else
							{
								SendLogMessage("Services", "Quit", BlackLight.Services.Error.Errors.DEBUG | BlackLight.Services.Error.Errors.ERROR, "Unable to match protocol with message received", "", "", "");
							}
							return true;
						}
						return false;
					}
					catch (Exception ex)
					{
						SendLogMessage("Services", "Quit", BlackLight.Services.Error.Errors.ERROR, "Problem parsing", "", ex.Message, ex.StackTrace);
						return false;
					}
				}
                private bool SVSKill(BlackLight.Services.Nodes.IRCUser Source, string command, List<string> arguments)
				{
					//Viable String Options
					//--NICKNAME
					//--REASON
					try
					{
						if (Source != null)
						{
                            Command cmd = MyIRCd.Commands["SVSKILLRECEIVE"];
							if (arguments.Count == cmd.parameterCount())
							{
                                BlackLight.Services.Nodes.IRCUser tUser = GetClientALL(arguments[cmd.getParameterIndex("NICKNAME")]);
								if (tUser != null)
								{
                                    if (events.onSVSKillEvent != null)
                                        events.onSVSKillEvent(tUser.name, arguments[cmd.getParameterIndex("REASON")]);
									tUser.Dispose();
								}
								tUser = null;
							}
							else
							{
								SendLogMessage("Services", "SVSKIll", BlackLight.Services.Error.Errors.DEBUG | BlackLight.Services.Error.Errors.ERROR, "Unable to match protocol with message received", "", "", "");
							}
							return true;
						}
						return false;
					}
					catch (Exception ex)
					{
						SendLogMessage("Services", "SVSKILL", BlackLight.Services.Error.Errors.ERROR, "Problem parsing", "", ex.Message, ex.StackTrace);
						return false;
					}
				}
                private bool Kill(BlackLight.Services.Nodes.IRCUser Source, string command, List<string> arguments)
				{
					//Viable String Options
					//--NICKNAME
					//--REASON
					try
					{
						if (Source != null)
						{
                            Command cmd = MyIRCd.Commands["KILLRECEIVE"];
							if (arguments.Count == cmd.parameterCount())
							{
                                BlackLight.Services.Nodes.IRCUser tUser = GetClientALL(arguments[cmd.getParameterIndex("NICKNAME")]);
								if (tUser != null)
								{
                                    if (events.onKillEvent != null)
                                        events.onKillEvent(tUser.name, arguments[cmd.getParameterIndex("REASON")]);
									tUser.Dispose();
								}
								tUser = null;
							}
							else
							{
								SendLogMessage("Services", "Kill", BlackLight.Services.Error.Errors.DEBUG | BlackLight.Services.Error.Errors.ERROR, "Unable to match protocol with message received", "", "", "");
							}
							return true;
						}
						return false;
					}
					catch (Exception ex)
					{
						SendLogMessage("Services", "Kill", BlackLight.Services.Error.Errors.ERROR, "Problem parsing", "", ex.Message, ex.StackTrace);
						return false;
					}
				}
                private bool SQuit(BlackLight.Services.Nodes.IRCUser Source, string command, List<string> arguments)
				{
					//Viable String Options
					//--SERVERNAME
					//--REASON
					try
					{
						if (Source != null&& Source is BlackLight.Services.Nodes.Server)
						{
                            Command cmd = MyIRCd.Commands["SQUITRECEIVE"];
							if (arguments.Count == cmd.parameterCount())
							{
                                if (events.onSQUITEvent != null)
                                    events.onSQUITEvent(arguments[cmd.getParameterIndex("SERVERNAME")], arguments[cmd.getParameterIndex("REASON")]);
								foreach (BlackLight.Services.Nodes.Server tServer in MyHost.GetAllServers())
								{
                                    if (tServer.name == arguments[cmd.getParameterIndex("SERVERNAME")])
									{
										tServer.Dispose();
										break;
									}
								}
							}
							else
							{
								SendLogMessage("Services", "SQUIT", BlackLight.Services.Error.Errors.DEBUG | BlackLight.Services.Error.Errors.ERROR, "Unable to match protocol with message received", "", "", "");
							}
							return true;
						}
						return false;
					}
					catch (Exception ex)
					{
						SendLogMessage("Services", "SQUIT", BlackLight.Services.Error.Errors.ERROR, "Problem parsing", "", ex.Message, ex.StackTrace);
						return false;
					}
				}
                private bool NetInfo(BlackLight.Services.Nodes.IRCUser Source, string command, List<string> arguments)
				{
					//Viable String Options
					//--NICKNAME
					//--REASON
					try
					{
						if (Source != null&& Source is BlackLight.Services.Nodes.Server)
						{
                            Command cmd = MyIRCd.Commands["NETINFORECEIVE"];
							if (arguments.Count == cmd.parameterCount())
							{
                                SendData(string.Format(MyIRCd.SendCommands["NETINFOSEND"].ToString(), MyName, Time.GetTS(DateTime.Now), arguments[cmd.getParameterIndex("PROTOCOL")], arguments[cmd.getParameterIndex("CLOAK")], arguments[cmd.getParameterIndex("NETWORKNAME")], Time.GetTS(DateTime.Now)));
							}
							else
							{
								SendLogMessage("Services", "NetInfo", BlackLight.Services.Error.Errors.DEBUG | BlackLight.Services.Error.Errors.ERROR, "Unable to match protocol with message received", "", "", "");
							}
							return true;
						}
						return false;
					}
					catch (Exception ex)
					{
						//MsgBox("NetInfo: " & e.Message & vbCrLf & e.StackTrace)
						SendLogMessage("Services", "NetInfo", BlackLight.Services.Error.Errors.ERROR, "Problem parsing", "", ex.Message, ex.StackTrace);
						return false;
					}
				}
				#endregion
				
				
				
				
				
				#region "User Stuffy"
				
				/// -----------------------------------------------------------------------------
				/// <summary>
				/// Will search for a client and return the client instance or Nothing based on search results
				/// </summary>
				/// <param name="name"></param>
				/// <returns></returns>
				/// <remarks>
				/// </remarks>
				/// <history>
				/// 	[Caleb]	6/18/2005	Created
				/// </history>
				/// -----------------------------------------------------------------------------
				public BlackLight.Services.Nodes.Client GetClient(string name)
				{
					try
					{
						BlackLight.Services.Nodes.Client n;
						short tIndex;
						tIndex = Convert.ToInt16(MyHost.users.IndexOf(name));
						if (tIndex >= 0)
						{
							return ((BlackLight.Services.Nodes.Client) MyHost.users[tIndex]);
						}
						foreach (BlackLight.Services.Nodes.Server srv in MyHost.leafs)
						{
							n = GetClient(name, srv);
							if (n != null)
							{
								return n;
							}
						}
						return null;
					}
					catch (Exception ex)
					{
						SendLogMessage("Services", "GetClient", BlackLight.Services.Error.Errors.ERROR, "Problem finding client", "", ex.Message, ex.StackTrace);
						return null;
					}
				}
				/// -----------------------------------------------------------------------------
				/// <summary>
				/// Will search for a client in both local and remote users
				/// </summary>
				/// <param name="name"></param>
				/// <returns></returns>
				/// <remarks>
				/// </remarks>
				/// <history>
				/// 	[Caleb]	6/18/2005	Created
				/// </history>
				/// -----------------------------------------------------------------------------
				public BlackLight.Services.Nodes.IRCUser GetClientALL(string name)
				{
					try
					{
						BlackLight.Services.Nodes.Client n;
						short tIndex;
						tIndex = Convert.ToInt16(LocalClients.IndexOf(name));
						if (tIndex >= 0)
						{
							return LocalClients[tIndex];
						}
						else
						{
							
							tIndex = Convert.ToInt16(MyHost.users.IndexOf(name));
							if (tIndex >= 0)
							{
								return MyHost.users[tIndex];
							}
							foreach (BlackLight.Services.Nodes.Server srv in MyHost.leafs)
							{
								n = GetClient(name, srv);
								if (n != null)
								{
									return n;
								}
							}
							return null;
						}
					}
					catch (Exception ex)
					{
						SendLogMessage("Services", "GetClientAll", BlackLight.Services.Error.Errors.ERROR, "Problem finding client", "", ex.Message, ex.StackTrace);
						return null;
					}
				}
				/// -----------------------------------------------------------------------------
				/// <summary>
				/// Search for a client based on server
				/// </summary>
				/// <param name="name"></param>
				/// <param name="Serv"></param>
				/// <returns></returns>
				/// <remarks>
				/// </remarks>
				/// <history>
				/// 	[Caleb]	6/18/2005	Created
				/// </history>
				/// -----------------------------------------------------------------------------
				public BlackLight.Services.Nodes.Client GetClient(string name, BlackLight.Services.Nodes.Server Serv)
				{
					try
					{
						short tIndex;
						tIndex = Convert.ToInt16(Serv.users.IndexOf(name));
						if (tIndex >= 0)
						{
							return ((BlackLight.Services.Nodes.Client) Serv.users[tIndex]);
						}
						BlackLight.Services.Nodes.Client tClient;
						foreach (BlackLight.Services.Nodes.Server n in Serv.leafs)
						{
							tClient = GetClient(name, n);
							if (tClient != null)
							{
								return tClient;
							}
						}
						return null;
					}
					catch (Exception ex)
					{
						SendLogMessage("Services", "GetClient(2)", BlackLight.Services.Error.Errors.ERROR, "Problem finding client", "", ex.Message, ex.StackTrace);
						return null;
					}
				}
				
				#endregion
				
				#region "IRCd Stuffy"
				
				
				
				
				/// -----------------------------------------------------------------------------
				/// <summary>
				/// Will search for a server and return the server instance or nothing based on result
				/// </summary>
				/// <param name="name"></param>
				/// <returns></returns>
				/// <remarks>
				/// </remarks>
				/// <history>
				/// 	[Caleb]	6/18/2005	Created
				/// </history>
				/// -----------------------------------------------------------------------------
				public BlackLight.Services.Nodes.Server GetServer(string name)
				{
					try
					{
						BlackLight.Services.Nodes.Server n;
						short tIndex;
						tIndex = Convert.ToInt16(MyHost.leafs.IndexOf(name));
						if (tIndex >= 0)
						{
							return ((BlackLight.Services.Nodes.Server) MyHost.leafs[tIndex]);
						}
						foreach (BlackLight.Services.Nodes.Server srv in MyHost.leafs)
						{
							n = GetServer(name, srv);
							if (n != null)
							{
								return n;
							}
						}
						return null;
					}
					catch (Exception ex)
					{
						SendLogMessage("Services", "GetServer", BlackLight.Services.Error.Errors.ERROR, "Problem finding server", "", ex.Message, ex.StackTrace);
						return null;
					}
				}
				private BlackLight.Services.Nodes.Server GetServer(string name, BlackLight.Services.Nodes.Server Serv)
				{
					try
					{
						short tIndex;
						tIndex = Convert.ToInt16(Serv.leafs.IndexOf(name));
						if (tIndex >= 0)
						{
							return ((BlackLight.Services.Nodes.Server) Serv.leafs[tIndex]);
						}
						BlackLight.Services.Nodes.Server tServer;
						foreach (BlackLight.Services.Nodes.Server n in Serv.leafs)
						{
							tServer = GetServer(name, n);
							if (tServer != null)
							{
								return tServer;
							}
						}
						return null;
					}
					catch (Exception ex)
					{
						SendLogMessage("Services", "GetServer(2)", BlackLight.Services.Error.Errors.ERROR, "Problem finding server", "", ex.Message, ex.StackTrace);
						return null;
					}
				}
				
				#endregion
				
				#region "IRCd Stuffy"
				public void IRCd_NoFile (string file)
				{
					try
					{
						SendLogMessage("Services", "IRCd_NoFile", BlackLight.Services.Error.Errors.ERROR, "Unable to find IRCd file", file, "", "");
					}
					catch (Exception ex)
					{
						SendLogMessage("Services", "IRCd_NoFile", BlackLight.Services.Error.Errors.ERROR, "Problem", "", ex.Message, ex.StackTrace);
					}
				}
				public void IRCd_ParseError (string Message)
				{
					try
					{
						SendLogMessage("Services", "IRCd_ParseError", BlackLight.Services.Error.Errors.ERROR, "Cannot parse protocol", Message, "", "");
					}
					catch (Exception ex)
					{
						SendLogMessage("Services", "IRCd_ParseError", BlackLight.Services.Error.Errors.ERROR, "Problem", Message, ex.Message, ex.StackTrace);
					}
				}
				#endregion
				
			}
		}
