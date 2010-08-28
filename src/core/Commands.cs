using System.Diagnostics;
using System;
using System.Data;
using System.Collections;
using BlackLight.Services;
using BlackLight.Services.Core;
using BlackLight.Services.Nodes;
using BlackLight.Services.Error;
using System.Collections.Generic;
namespace BlackLight
{
	namespace Services
	{
		namespace IRCProtocol
		{
			
			///
			/// -----------------------------------------------------------------------------
			/// Project	 : BlackLight Services
			/// Class	 : Commands
			///
			/// -----------------------------------------------------------------------------
			/// <summary>
			/// Contains functions which will output data to the socket for specific IRC commands
			/// </summary>
			/// <remarks>
			/// </remarks>
			/// <history>
			/// 	[Caleb]	6/18/2005	Created
			/// </history>
			/// -----------------------------------------------------------------------------
			public class Commands
			{
				
				private IRCd MyIRCd;
				private ServicesCore MyCore;
				/// -----------------------------------------------------------------------------
				/// <summary>
				/// Creates instance
				/// </summary>
				/// <param name="Base">Istance of the Services Core</param>
				/// <param name="IRCd">Instance of the Services protocol class</param>
				/// <remarks>
				/// </remarks>
				/// <history>
				/// 	[Caleb]	6/18/2005	Created
				/// </history>
				/// -----------------------------------------------------------------------------
				public Commands(ServicesCore Base, IRCd IRCd) {
					MyIRCd = IRCd;
					MyCore = Base;
				}
				/// -----------------------------------------------------------------------------
				/// <summary>
				/// Sends a join command
				/// </summary>
				/// <param name="Source">String of client that wants to join</param>
				/// <param name="Channel">String representation of channel to join</param>
				/// <param name="Key">String representation of channel key</param>
				/// <remarks>
				/// </remarks>
				/// <history>
				/// 	[Caleb]	6/18/2005	Created
				/// </history>
				/// -----------------------------------------------------------------------------
				public void Join_Channel (string Source, string Channel, string Key)
				{
					try
					{
                        Dictionary<string, string> reps = new Dictionary<string, string>();
                        reps.Add("channel", Channel);
                        reps.Add("key", Key);
                        MyCore.SendData(MyIRCd.SendCommands["JOINSEND"].buildCommand(Source,reps));
						//MyCore.SendData(string.Format(MyIRCd.SendCommands["JOINSEND"].ToString(), Source, Channel, Key));
					}
					catch (Exception ex)
					{
						MyCore.SendLogMessage("Commands", "Join_Channel", BlackLight.Services.Error.Errors.ERROR, "Problem", "", ex.Message, ex.StackTrace);
					}
				}
				/// -----------------------------------------------------------------------------
				/// <summary>
				/// Sets the client as identified
				/// </summary>
				/// <param name="Source">String of the identifer</param>
				/// <param name="Dest">String representation of client to be identified</param>
				/// <remarks>
				/// </remarks>
				/// <history>
				/// 	[Caleb]	6/18/2005	Created
				/// </history>
				/// -----------------------------------------------------------------------------
				public void IdentifyClient (string Source, string Client)
				{
					try
					{
                        Dictionary<string, string> reps = new Dictionary<string, string>();
                        reps.Add("client", Client);
                        MyCore.SendData(MyIRCd.SendCommands["SETREGISTEREDSEND"].buildCommand(Source, reps));
						//MyCore.SendData(string.Format(MyIRCd.SendCommands["SETREGISTEREDSEND"].ToString(), Source, Client));
					}
					catch (Exception ex)
					{
						MyCore.SendLogMessage("Commands", "IdentifyClient", BlackLight.Services.Error.Errors.ERROR, "Problem", "", ex.Message, ex.StackTrace);
					}
				}
				/// -----------------------------------------------------------------------------
				/// <summary>
				/// Sends a NOTICE to the specified destination
				/// </summary>
				/// <param name="Source">String of client to sending notice</param>
				/// <param name="Dest">String representation of destination client</param>
				/// <param name="Message">String representation of message to be sent</param>
				/// <remarks>
				/// </remarks>
				/// <history>
				/// 	[Caleb]	6/18/2005	Created
				/// </history>
				/// -----------------------------------------------------------------------------
				public void Send_Notice (string Source, string Dest, string Message)
				{
					try
					{
                        Dictionary<string, string> reps = new Dictionary<string, string>();
                        reps.Add("receiver", Dest);
                        reps.Add("message", Message);
                        MyCore.SendData(MyIRCd.SendCommands["NOTICESEND"].buildCommand(Source, reps));
						//MyCore.SendData(string.Format(MyIRCd.SendCommands["NOTICESEND"].ToString(), Source, Dest, Message));
					}
					catch (Exception ex)
					{
						MyCore.SendLogMessage("Commands", "Send_Notice", BlackLight.Services.Error.Errors.ERROR, "Problem", "", ex.Message, ex.StackTrace);
					}
				}
				/// -----------------------------------------------------------------------------
				/// <summary>
				/// Sends a Quit message for a local client
				/// </summary>
				/// <param name="Source">String of the client to quit</param>
				/// <param name="Reason">String representation of reason to be sent</param>
				/// <remarks>
				/// </remarks>
				/// <history>
				/// 	[Caleb]	6/18/2005	Created
				/// </history>
				/// -----------------------------------------------------------------------------
				public void Send_Quit (string Source, string Reason)
				{
					try
					{
                        Dictionary<string, string> reps = new Dictionary<string, string>();
                        reps.Add("reason", Reason);
                        MyCore.SendData(MyIRCd.SendCommands["QUITSEND"].buildCommand(Source, reps));
						//MyCore.SendData(string.Format(MyIRCd.SendCommands["QUITSEND"].ToString(), Source, Reason));
					}
					catch (Exception ex)
					{
						MyCore.SendLogMessage("Commands", "Send_Quit", BlackLight.Services.Error.Errors.ERROR, "Problem", "", ex.Message, ex.StackTrace);
					}
				}
				/// -----------------------------------------------------------------------------
				/// <summary>
				/// Sends a KILL to the specified destination
				/// </summary>
				/// <param name="Source">String of client to sending notice</param>
				/// <param name="Dest">String representation of the client to be killed</param>
				/// <param name="Message">String representation of message to be sent</param>
				/// <remarks>
				/// </remarks>
				/// <history>
				/// 	[Caleb]	6/18/2005	Created
				/// </history>
				/// -----------------------------------------------------------------------------
				public void Send_Kill (string Source, string Dest, string Message)
				{
					try
					{
                        Dictionary<string, string> reps = new Dictionary<string, string>();
                        reps.Add("nickname", Dest);
                        reps.Add("reason", Message);
                        MyCore.SendData(MyIRCd.SendCommands["SVSKILLSEND"].buildCommand(Source, reps));
						//MyCore.SendData(string.Format(MyIRCd.SendCommands["SVSKILLSEND"].ToString(), Source, Dest, Message));
					}
					catch (Exception ex)
					{
						MyCore.SendLogMessage("Commands", "Send_Kill", BlackLight.Services.Error.Errors.ERROR, "Problem", "", ex.Message, ex.StackTrace);
					}
				}
				/// -----------------------------------------------------------------------------
				/// <summary>
				/// Sends a gline to the specified destination
				/// </summary>
				/// <param name="Source">String of client to sending notice</param>
				/// <param name="Host">String of client host</param>
				/// <param name="ExpireTime">Integer representing the expiration time</param>
				/// <param name="Reason">String representation of reason to be sent</param>
				/// <remarks>
				/// </remarks>
				/// <history>
				/// 	[Caleb]	6/18/2005	Created
				/// </history>
				/// -----------------------------------------------------------------------------
				public void Send_gLine (string Source, string Host, int ExpireTime, string Reason)
				{
					try
					{
                        Dictionary<string, string> reps = new Dictionary<string, string>();
                        reps.Add("user", "*");
                        reps.Add("adder", Source);
                        reps.Add("expires", ExpireTime.ToString());
                        reps.Add("reason", Reason);
                        reps.Add("time", Converters.Time.GetTS(DateTime.Now).ToString());
                        MyCore.SendData(MyIRCd.SendCommands["TKL_GSEND"].buildCommand(MyCore.MyName, reps));
						//MyCore.SendData(string.Format(MyIRCd.SendCommands["TKL_GSEND"].ToString(), MyCore.MyName, "*", Host, Source, ExpireTime, Converters.Time.GetTS(DateTime.Now), Reason));
					}
					catch (Exception ex)
					{
						MyCore.SendLogMessage("Commands", "Send_gLine", BlackLight.Services.Error.Errors.ERROR, "Problem", "", ex.Message, ex.StackTrace);
					}
				}
				/// -----------------------------------------------------------------------------
				/// <summary>
				/// Sends a zline to the specified destination
				/// </summary>
				/// <param name="Source">String of client to sending notice</param>
				/// <param name="Host">String of client host</param>
				/// <param name="ExpireTime">Integer representing the expiration time</param>
				/// <param name="Reason">String representation of reason to be sent</param>
				/// <remarks>
				/// </remarks>
				/// <history>
				/// 	[Caleb]	6/18/2005	Created
				/// </history>
				/// -----------------------------------------------------------------------------
				public void Send_zLine (string Source, string Host, int ExpireTime, string Reason)
				{
					try
					{
                        Dictionary<string, string> reps = new Dictionary<string, string>();
                        reps.Add("user", "*");
                        reps.Add("adder", Source);
                        reps.Add("expires", ExpireTime.ToString());
                        reps.Add("reason", Reason);
                        reps.Add("time", Converters.Time.GetTS(DateTime.Now).ToString());
                        MyCore.SendData(MyIRCd.SendCommands["TKL_ZSEND"].buildCommand(MyCore.MyName, reps));
						//MyCore.SendData(string.Format(MyIRCd.SendCommands["TKL_ZSEND"].ToString(), MyCore.MyName, "*", Host, Source, ExpireTime, Converters.Time.GetTS(DateTime.Now), Reason));
					}
					catch (Exception ex)
					{
						MyCore.SendLogMessage("Commands", "Send_zLine", BlackLight.Services.Error.Errors.ERROR, "Problem", "", ex.Message, ex.StackTrace);
					}
				}
				/// -----------------------------------------------------------------------------
				/// <summary>
				/// Sends a PRIVMSG to the specified destination
				/// </summary>
				/// <param name="Source">String of client to sending notice</param>
				/// <param name="Dest">String representation of destination client</param>
				/// <param name="Message">String representation of message to be sent</param>
				/// <remarks>
				/// </remarks>
				/// <history>
				/// 	[Caleb]	6/18/2005	Created
				/// </history>
				/// -----------------------------------------------------------------------------
				public void Send_PrivMSG (string Source, string Dest, string Message)
				{
					try
					{
                        Dictionary<string, string> reps = new Dictionary<string, string>();
                        reps.Add("receiver", Dest);
                        reps.Add("message", Message);
                        MyCore.SendData(MyIRCd.SendCommands["PRIVMSGSEND"].buildCommand(Source, reps));
						//MyCore.SendData(string.Format(MyIRCd.SendCommands["PRIVMSGSEND"].ToString(), Source, Dest, Message));
					}
					catch (Exception ex)
					{
						MyCore.SendLogMessage("Commands", "Send_PrivMsg", BlackLight.Services.Error.Errors.ERROR, "Problem", "", ex.Message, ex.StackTrace);
					}
				}
				/// -----------------------------------------------------------------------------
				/// <summary>
				/// Formats a PONG to be sent to the destination server
				/// </summary>
				/// <param name="SourceServer">The originating server of pong</param>
				/// <param name="DestServer">The server to receive the pong</param>
				/// <remarks>
				/// </remarks>
				/// <history>
				/// 	[Caleb]	6/18/2005	Created
				/// </history>
				/// -----------------------------------------------------------------------------
				public void Send_Ping (string SourceServer, string DestServer)
				{
					try
					{
                        Dictionary<string, string> reps = new Dictionary<string, string>();
                        reps.Add("serverreceived", DestServer);
                        MyCore.SendData(MyIRCd.SendCommands["PONGSEND"].buildCommand(SourceServer, reps));
						//MyCore.SendData(string.Format(MyIRCd.SendCommands["PONGSEND"].ToString(), SourceServer, DestServer));
					}
					catch (Exception ex)
					{
						MyCore.SendLogMessage("Commands", "Send_Ping", BlackLight.Services.Error.Errors.ERROR, "Problem", "", ex.Message, ex.StackTrace);
					}
				}
				/// -----------------------------------------------------------------------------
				/// <summary>
				/// Sends the command to create a server on the network
				/// </summary>
				/// <param name="ServerName">Name of server to be created</param>
				/// <param name="Hops">Number of hops for the server</param>
				/// <param name="Numeric">The server numeric</param>
				/// <param name="Description">The server description</param>
				/// <remarks>
				/// </remarks>
				/// <history>
				/// 	[Caleb]	6/18/2005	Created
				/// </history>
				/// -----------------------------------------------------------------------------
				public void Send_Server (string ServerName, short Hops, short Numeric, string Description)
				{
					try
					{
                        Dictionary<string, string> reps = new Dictionary<string, string>();
                        reps.Add("servicesname", ServerName);
                        reps.Add("hops", Hops.ToString());
                        reps.Add("servernumeric", Numeric.ToString());
                        reps.Add("description", Description);
                        MyCore.SendData(MyIRCd.SendCommands["SERVERSEND"].buildCommand(ServerName, reps));
						//MyCore.SendData(string.Format(MyIRCd.SendCommands["SERVERSEND"].ToString(), ServerName, Hops, Numeric, Description));
					}
					catch (Exception ex)
					{
						MyCore.SendLogMessage("Commands", "Send_Server", BlackLight.Services.Error.Errors.ERROR, "Problem", "", ex.Message, ex.StackTrace);
					}
				}
				/// -----------------------------------------------------------------------------
				/// <summary>
				/// Sends the protocol or capabilites message to the host server
				/// </summary>
				/// <remarks>
				/// </remarks>
				/// <history>
				/// 	[Caleb]	6/18/2005	Created
				/// </history>
				/// -----------------------------------------------------------------------------
				public void Send_Protocol ()
				{
					try
					{
                        Dictionary<string, string> reps = new Dictionary<string, string>();
                        MyCore.SendData(MyIRCd.SendCommands["PROTOCOLSEND"].buildCommand(MyCore.MyName, reps));
						//MyCore.SendData(MyIRCd.SendCommands["PROTOCOLSEND"].ToString());
					}
					catch (Exception ex)
					{
						MyCore.SendLogMessage("Commands", "Send_protocol", BlackLight.Services.Error.Errors.ERROR, "Problem", "", ex.Message, ex.StackTrace);
					}
				}
				/// -----------------------------------------------------------------------------
				/// <summary>
				/// Sends the PASS command to the server
				/// </summary>
				/// <param name="Password">Link password to be used</param>
				/// <param name="Time">Current time</param>
				/// <remarks>
				/// </remarks>
				/// <history>
				/// 	[Caleb]	6/18/2005	Created
				/// </history>
				/// -----------------------------------------------------------------------------
				public void Send_Password (string Password, int Time)
				{
					try
					{
                        Dictionary<string, string> reps = new Dictionary<string, string>();
                        reps.Add("serverpass", Password);
                        reps.Add("time", Time.ToString());
                        MyCore.SendData(MyIRCd.SendCommands["PASSSEND"].buildCommand(MyCore.MyName, reps));
						//MyCore.SendData(string.Format(MyIRCd.SendCommands["PASSSEND"].ToString(), Password, Time));
					}
					catch (Exception ex)
					{
						MyCore.SendLogMessage("Commands", "Send_Server", BlackLight.Services.Error.Errors.ERROR, "Problem", "", ex.Message, ex.StackTrace);
					}
				}
				/// -----------------------------------------------------------------------------
				/// <summary>
				/// Sends the protocols end of burst command
				/// </summary>
				/// <param name="ServerName">Server name originating from</param>
				/// <remarks>
				/// </remarks>
				/// <history>
				/// 	[Caleb]	6/18/2005	Created
				/// </history>
				/// -----------------------------------------------------------------------------
				public void Send_EOBURST (string ServerName)
				{
					try
					{
                        Dictionary<string, string> reps = new Dictionary<string, string>();
                        MyCore.SendData(MyIRCd.SendCommands["ENDOFBURSTSEND"].buildCommand(ServerName, reps));
						MyCore.SendData(string.Format(MyIRCd.SendCommands["ENDOFBURSTSEND"].ToString(), ServerName, null));
					}
					catch (Exception ex)
					{
						MyCore.SendLogMessage("Commands", "Send_Server", BlackLight.Services.Error.Errors.ERROR, "Problem", "", ex.Message, ex.StackTrace);
					}
				}
				/// -----------------------------------------------------------------------------
				/// <summary>
				/// Sends the command to create a new client on the network
				/// </summary>
				/// <param name="Client">The instance of the client to be created</param>
				/// <remarks>
				/// </remarks>
				/// <history>
				/// 	[Caleb]	6/18/2005	Created
				/// </history>
				/// -----------------------------------------------------------------------------
				public void Send_Connect (LocalClient Client)
				{
					try
					{
                        Dictionary<string, string> reps = new Dictionary<string, string>();
                        reps.Add("nickname", Client.nick);
                        reps.Add("hops", "1");
                        reps.Add("time", "!" + MyCore.Base64.IntToB64(Client.time).Trim());
                        reps.Add("username", Client.username.Trim());
                        reps.Add("host", Client.host);
                        reps.Add("servername", MyCore.MyName);
                        reps.Add("modes", Client.modes);
                        reps.Add("realname", Client.realname);
                        MyCore.SendData(MyIRCd.SendCommands["CONNECTSEND"].buildCommand(MyCore.MyName, reps));
						//MyCore.SendData(string.Format(MyIRCd.SendCommands["CONNECTSEND"].ToString(), Client.Nick, "1", "!" + MyCore.Base64.IntToB64(Client.Time), Client.Username, Client.Host, MyCore.MyName, Client.Modes, Client.Realname));
					}
					catch (Exception ex)
					{
						MyCore.SendLogMessage("Commands", "Send_Nick", BlackLight.Services.Error.Errors.ERROR, "Problem", "", ex.Message, ex.StackTrace);
					}
				}
			}
		}
	}
}
