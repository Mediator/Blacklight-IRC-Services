using System.Diagnostics;
using System;
using System.IO;
using System.Data;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Text;
using BlackLight;
using BlackLight.Services.Error;
using BlackLight.Services.Core;

namespace BlackLight
{
	namespace Services
	{
		namespace IRCProtocol
		{
			
			/// -----------------------------------------------------------------------------
			/// Project	 : BlackLight Services
			/// Class	 : IRCd
			///
			/// -----------------------------------------------------------------------------
			/// <summary>
			/// Class which will parse and contain all information regaurding the protocol in use
			/// </summary>
			/// <remarks>
			/// </remarks>
			/// <history>
			/// 	[Caleb]	6/18/2005	Created
			/// </history>
			/// -----------------------------------------------------------------------------
			public class IRCd
			{
				public string IRCdfile;
				public const short MODE_OWNER = 5;
				public const short MODE_ADMIN = 4;
				public const short MODE_OP = 3;
				public const short MODE_HALFOP = 2;
				public const short MODE_VOICE = 1;
				public const short MODE_USER = 0;
				
				#region "Channel Modes"
				public char OwnerMode;
				public char AdminMode;
				public char OpMode;
				public char HalfOpMode;
				public char VoiceMode;
				public string StatusModes;
				public string AccessModes;
				public string ParamModes;
				public string ChannelModes;
				#endregion
				public delegate void tNoFileEventHandler(string file);
				private tNoFileEventHandler tNoFileEvent;
				
				public event tNoFileEventHandler tNoFile
				{
					add
					{
						tNoFileEvent = (tNoFileEventHandler) System.Delegate.Combine(tNoFileEvent, value);
					}
					remove
					{
						tNoFileEvent = (tNoFileEventHandler) System.Delegate.Remove(tNoFileEvent, value);
					}
				}
				
				private ServicesCore MyCore;
				public delegate void CannotParseProtocolEventHandler(string Message);
				private static CannotParseProtocolEventHandler CannotParseProtocolEvent;
				
				public event CannotParseProtocolEventHandler CannotParseProtocol
				{
					add
					{
						CannotParseProtocolEvent = (CannotParseProtocolEventHandler) System.Delegate.Combine(CannotParseProtocolEvent, value);
					}
					remove
					{
						CannotParseProtocolEvent = (CannotParseProtocolEventHandler) System.Delegate.Remove(CannotParseProtocolEvent, value);
					}
				}
				
				//Private CommandVariables As ArrayList
				private Dictionary<string,Command> CommandList;
				public Dictionary<char, string> SJOINPrefixList;
                private Dictionary<string, Command> OutCommandList;
				//Public ParseErrors As ArrayList
				/// -----------------------------------------------------------------------------
				/// <summary>
				/// Returns the commands list
				/// </summary>
				/// <value></value>
				/// <remarks>
				/// </remarks>
				/// <history>
				/// 	[Caleb]	6/18/2005	Created
				/// </history>
				/// -----------------------------------------------------------------------------
                public Dictionary<string, Command> Commands
				{
					get{
						return CommandList;
					}
				}
				/// -----------------------------------------------------------------------------
				/// <summary>
				/// Returns the list of commands that will be parsed by the commands class
				/// </summary>
				/// <value></value>
				/// <remarks>
				/// </remarks>
				/// <history>
				/// 	[Caleb]	6/18/2005	Created
				/// </history>
				/// -----------------------------------------------------------------------------
                public Dictionary<string, Command> SendCommands
				{
					get{
						return OutCommandList;
					}
				}
				public IRCd(string tIRCdfile, ServicesCore tBase) {
                    CommandList = new Dictionary<string, Command>();
                    OutCommandList = new Dictionary<string, Command>();
                    SJOINPrefixList = new Dictionary<char,string>(); // new Hashtable();
					
					try
					{
						this.IRCdfile = tIRCdfile;
						MyCore = tBase;
					}
					catch (Exception ex)
					{
						MyCore.SendLogMessage("IRCd", "New", BlackLight.Services.Error.Errors.ERROR, "Problem", "", ex.Message, ex.StackTrace);
					}
				}
				public void SetProtocol (string tIRCdfile)
				{
					this.IRCdfile = tIRCdfile;
				}
				/// -----------------------------------------------------------------------------
				/// <summary>
				/// Begins the protocol loading and parsing
				/// </summary>
				/// <remarks>
				/// </remarks>
				/// <history>
				/// 	[Caleb]	6/18/2005	Created
				/// </history>
				/// -----------------------------------------------------------------------------
				public void LoadProtocol ()
				{
					try
					{
						string ProtocolFile = ProtocolFileGet(IRCdfile + ".protocol");
						
						if (ProtocolFile.Length > 0)
						{
							if (ParseProtocol(ProtocolFile) == false)
							{
								if (CannotParseProtocolEvent != null)
									CannotParseProtocolEvent("Parse Protocol Error");
							}
						}
						else
						{
							if (CannotParseProtocolEvent != null)
								CannotParseProtocolEvent("Protocol Not Set, Protocol most likely Empty");
						}
					}
					catch (Exception ex)
					{
						MyCore.SendLogMessage("IRCd", "LoadPRotocol", BlackLight.Services.Error.Errors.DEBUG | BlackLight.Services.Error.Errors.FATAL, "Problem with protocol load", "", ex.Message, ex.StackTrace);
					}
				}
				private string ProtocolFileGet(string filename)
				{
					try
					{
						//System.IO.File tFile;
						System.IO.StreamReader tFileStream;
						if (System.IO.File.Exists("Protocols\\" + filename))
						{
							tFileStream = File.OpenText("Protocols\\" + filename);
							return tFileStream.ReadToEnd();
						}
						else
						{
							if (tNoFileEvent != null)
								tNoFileEvent(filename);
							return null;
						}
					}
					catch (Exception ex)
					{
						MyCore.SendLogMessage("IRCd", "ProtocolFileGet", BlackLight.Services.Error.Errors.ERROR, "Problem getting protocol file", "", ex.Message, ex.StackTrace);
					return null;
					}
				}
				private bool ParseProtocol(string Protocol)
				{
					try
					{
						Protocol.Replace("\r\n","\n");
						Protocol.Replace("\r","\n");
						string[] ProtocolArray = Protocol.Split('\n');
						//COMMAND STRINGS
						ParseCommandStrings(ProtocolArray);
						//PROTOCOL VARIABLES
						ParseSJOINVars(ProtocolArray);
						ParseChannelModeVars(ProtocolArray);
						return true;
					}
					catch (Exception ex)
					{
						MyCore.SendLogMessage("IRCd", "ParseProtocol", BlackLight.Services.Error.Errors.ERROR, "Problem parsing protocol", "", ex.Message, ex.StackTrace);
					return false;
					}
					
				}
				
				/// -----------------------------------------------------------------------------
				/// <summary>
				/// Replaces the special characters out of a nickname
				/// </summary>
				/// <param name="NickName"></param>
				/// <returns></returns>
				/// <remarks>
				/// </remarks>
				/// <history>
				/// 	[Caleb]	6/18/2005	Created
				/// </history>
				/// -----------------------------------------------------------------------------
				public string ReadySJOINNick(string NickName)
				{
					try
					{
                        for (int idx = 0; idx < NickName.Length; idx++)
                        {
                            if (!SJOINPrefixList.ContainsKey(NickName[idx]))
                                return NickName.Substring(idx);
                        }
                        return NickName;
					}
					catch (Exception ex)
					{
						MyCore.SendLogMessage("IRCd", "ReadSJOINNick", BlackLight.Services.Error.Errors.ERROR, "Problem", "", ex.Message, ex.StackTrace);
					return null;
					}
				}
				/// -----------------------------------------------------------------------------
				/// <summary>
				/// Returns the modes out of a sjoin nickname
				/// </summary>
				/// <param name="NickName"></param>
				/// <returns></returns>
				/// <remarks>
				/// </remarks>
				/// <history>
				/// 	[Caleb]	6/18/2005	Created
				/// </history>
				/// -----------------------------------------------------------------------------
				public string ReadySJOINModes(string NickName)
				{
					try
					{
						StringBuilder tString = new StringBuilder();
                        for (int idx = 0; idx < NickName.Length; idx++)
                        {
                            if (SJOINPrefixList.ContainsKey(NickName[idx]))
                                tString.Append(SJOINPrefixList[NickName[idx]]);
                            else
                                break;
                        }
						return tString.ToString();
					}
					catch (Exception ex)
					{
						MyCore.SendLogMessage("IRCd", "ReadySJOINModes", BlackLight.Services.Error.Errors.ERROR, "Problem", "", ex.Message, ex.StackTrace);
					return "";
					}
				}
                public short SJOINType(string Nickname)
                {
                    MyCore.SendLogMessage("IRCd", "SJOINType", BlackLight.Services.Error.Errors.DEBUG, "Running SJOIN For", Nickname, "","");
                    if (SJOINPrefixList.ContainsKey(Nickname[0]))
                    {
                        if (SJOINPrefixList[Nickname[0]] == "ban")
                            return 1;
                        else if (SJOINPrefixList[Nickname[0]] == "exempt")
                            return 2;
                        else if (SJOINPrefixList[Nickname[0]] == "invex")
                            return 3;
                        else
                            return 0;
                    }
                        return -1;
                }
				private bool ParseSJOINVars(string[] ProtocolArray)
				{
					try
					{
						Regex reg_chars = new Regex("^SJOIN_(?<name>[a-zA-Z]+)_PREFIX=\\\"(?<param>.)\\\"$", RegexOptions.IgnoreCase);
						Match tMatch;
                        SJOINPrefixList = new Dictionary<char, string>();
						foreach (string tVal in ProtocolArray)
						{
							if (tVal.Trim().Length > 0)
							{
								tMatch = reg_chars.Match(tVal.Trim());
								if (tMatch.Success == true)
								{

                                    SJOINPrefixList.Add(tMatch.Groups["param"].Value[0], tMatch.Groups["name"].Value.ToLower());
									//MsgBox(LCase(tMatch.Groups("name").Value) & " " & tMatch.Groups("param").Value)
								}
							}
						}
						return true;
					}
					catch (Exception ex)
					{
						MyCore.SendLogMessage("IRCd", "ReadySJOINVars", BlackLight.Services.Error.Errors.ERROR, "Problem", "", ex.Message, ex.StackTrace);
					return false;
					}
				}
				private bool ParseChannelModeVars(string[] ProtocolArray)
				{
					try
					{
						Regex reg_chars = new Regex("^CHANNELMODE_(?<name>[a-zA-Z]+)=\\\"(?<param>.+)\\\"$", RegexOptions.IgnoreCase);
						Match tMatch;
						char tChar;
						int count = 0;
						string tVal = "";
						for (count = 0;count < ProtocolArray.Length;count++)
						//foreach (string tVal in ProtocolArray)
						{
							tVal = ProtocolArray[count];
							if (tVal.Trim().Length > 0)
							{
								
								tMatch = reg_chars.Match(tVal.Trim());
								if (tMatch.Success == true)
								{
									//Console.WriteLine("PROTOCOL MODES: " + tVal.Trim());
									if (tMatch.Groups["param"].Value.Length == 1)
									{
										tChar = Convert.ToChar(tMatch.Groups["param"].Value);
									}
									else
									{
										tChar = '\0';
									}
									//Console.WriteLine(tMatch.Groups["name"].Value.ToUpper());
									switch (tMatch.Groups["name"].Value.ToUpper())
									{
										case "OWNER":
											
											OwnerMode = tChar;
											break;
										case "ADMIN":
											
											AdminMode = tChar;
											break;
										case "OP":
											
											OpMode = tChar;
											break;
										case "HALFOP":
											
											HalfOpMode = tChar;
											break;
										case "VOICE":
											
											VoiceMode = tChar;
											break;
										case "SUPPORTEDSTATUSMODES":
											Console.WriteLine("O KEWL");
											StatusModes = tMatch.Groups["param"].Value;
											break;
										case "SUPPORTEDACCESSMODES":
											
											AccessModes = tMatch.Groups["param"].Value;
											break;
										case "SUPPORTEDPARAMMODES":
											
											ParamModes = tMatch.Groups["param"].Value;
											break;
										case "SUPPORTEDMODES":
											
											ChannelModes = tMatch.Groups["param"].Value;
											break;
									}
								}
							}
						}
						return true;
					}
					catch (Exception ex)
					{
						Console.WriteLine("Exception");
						//MessageBox.Show("wtf exception" + ex.Message);
						MyCore.SendLogMessage("IRCd", "ParseChannelModeVars", BlackLight.Services.Error.Errors.ERROR, "Problem", "", ex.Message, ex.StackTrace);
					return false;
					}
				}
				
				private bool ParseCommandStrings(string[] ProtocolArray)
				{
					try
					{
                        Regex reg_incmds = new Regex("^(?<name>[0-9a-zA-Z_]+receive):(?<command>[0-9a-zA-Z_]+)=\\\"(?<params>[a-zA-Z0-9 ,]*)\\\"$", RegexOptions.IgnoreCase);
                        Regex reg_outcmds = new Regex("^(?<name>[0-9a-zA-Z_]+send):(?<command>[0-9a-zA-Z_]+)=\\\"(?<params>[\\*%:\\-a-z0-9A-Z ,]*)\\\"$", RegexOptions.IgnoreCase);
						
						Match tMatch;
						foreach (string tVal in ProtocolArray)
						{
							if (tVal.Trim().Length > 0)
							{
								tMatch = reg_incmds.Match(tVal.Trim());
								//MsgBox(tVal)
								if (tMatch.Success == true)
								{
                                    CommandList.Add(tMatch.Groups["name"].Value.ToUpper(), new Command(tMatch.Groups["command"].Value.ToUpper(), tMatch.Groups["params"].Value));
                                    MyCore.SendLogMessage("IRCd", "ParseCommandStrings", BlackLight.Services.Error.Errors.DEBUG, "Protocol In Command", tMatch.Groups["name"].Value.ToUpper() + " " + CommandList[tMatch.Groups["name"].Value.ToUpper()].name, "", "");
                                }
								else
								{
									tMatch = reg_outcmds.Match(tVal.Trim());
									if (tMatch.Success == true)
									{
										//tOutList.Add(tMatch.Groups["name"].Value.ToUpper(), tMatch.Groups["param"].Value);
                                        
                                        OutCommandList.Add(tMatch.Groups["name"].Value.ToUpper(), new Command(tMatch.Groups["command"].Value.ToUpper(), tMatch.Groups["params"].Value));
                                        MyCore.SendLogMessage("IRCd", "ParseCommandStrings", BlackLight.Services.Error.Errors.DEBUG, "Protocol Out Command", tMatch.Groups["name"].Value.ToUpper() + " " + OutCommandList[tMatch.Groups["name"].Value.ToUpper()].name + "---" + tMatch.Groups["params"].Value, "", "");
									}
								}
							}
						}
						
						//CommandList = tPreparedList;
						//OutCommandList = tPreparedOutList;
					return true;
					}
					catch (Exception ex)
					{
						MyCore.SendLogMessage("IRCd", "ParseCommandStrings", BlackLight.Services.Error.Errors.ERROR, "Problem", "", ex.Message, ex.StackTrace);
					return false;
					}
				}
			}
		}
	}
}
