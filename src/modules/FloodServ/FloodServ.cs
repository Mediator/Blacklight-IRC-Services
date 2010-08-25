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
using System.Text.RegularExpressions;

namespace BlackLight
{
	namespace Services
	{
		namespace Modules
		{
			namespace FloodServ
			{
				
				public class FloodServService
				{
					
					protected LocalClient MyClient;
					protected BlackLight.Services.Core.ServicesCore MyCore;
					protected ModuleList Modules;
					protected Help.Help Help;
					private ArrayList m_NPWatches;
					private ArrayList m_NSWatches;
					private ArrayList m_RegWatches;
					private ServiceList m_Recent;
					public DataBase FloodDB;
					public void NullFunction (Client Source, string[] Message)
					{
						
					}
					
					public void FSHelp (Client Source, string[] Message)
					{
						Help.sendHelp("FloodServ", MyClient, Source, Message);
					}
					
					public void FSNoCommand (Client Source, string[] Message)
					{
						Help.sendResponse("FloodServ", MyClient, Source, "NO_SUCH_COMMAND", Message[0], MyClient.Name);
					}
					
					
					
					public void OnClientConnect (string nickname, Client data)
					{
						Console.WriteLine(nickname);
						int tInt;
						for (tInt = 0; tInt <= m_NPWatches.Count - 1; tInt++)
						{
							if (scan_np(System.Convert.ToString(m_NPWatches[tInt]), data))
							{
								return;
							}
						}
						for (tInt = 0; tInt <= m_NSWatches.Count - 1; tInt++)
						{
							if (scan_np(System.Convert.ToString(m_NSWatches[tInt]), data))
							{
								return;
							}
						}
						
						for (tInt = 0; tInt <= m_RegWatches.Count - 1; tInt++)
						{
							if (scan_reg(System.Convert.ToString(m_RegWatches[tInt]), data))
							{
								return;
							}
						}
						
						if (m_Recent.Contains(data.Name) == false)
						{
							m_Recent.Add(data);
						}
						for (tInt = 0; tInt <= m_Recent.Count - 1; tInt++)
						{
							if ((((Client) m_Recent[tInt]).Time - 120) > BlackLight.Services.Converters.Time.GetTS(DateTime.Now))
							{
								m_Recent.RemoveAt(tInt);
							}
						}
						ArrayList tPrefixes = new ArrayList();
						string tPattern;
						bool tFound;
						Prefix tPrefix = null;
						for (tInt = 0; tInt <= m_Recent.Count - 1; tInt++)
						{
                            if (((Client)m_Recent[tInt]).Name.Length >= 3)
                            {
                                tPattern = ((Client)m_Recent[tInt]).Name.Substring(0, 3);
                                tFound = false;
                                foreach (Prefix ttPrefix in tPrefixes)
                                {
                                    if (ttPrefix.Pattern == tPattern.ToLower())
                                    {
                                        tFound = true;
                                        tPrefix = ttPrefix;
                                        break;
                                    }
                                }
                                if (tFound == true)
                                {
                                    tPrefix.Hits += 1;
                                }
                                else
                                {
                                    tPrefixes.Add(new Prefix(tPattern));
                                }
                            }
						}
						try
						{
							foreach (Prefix mPrefix in tPrefixes)
							{
								if (mPrefix.Hits > 2)
								{
									scan_allusers(mPrefix.Pattern, 0);
									if (FindMatch(mPrefix.Pattern, 0) == null)
									{
										addNPWatch(mPrefix.Pattern, "FloodServ");
									}
									for (tInt = 0; tInt <= m_Recent.Count - 1; tInt++)
									{
										if (m_Recent[tInt].Name.ToLower().StartsWith(mPrefix.Pattern.ToLower()))
										{
											m_Recent.RemoveAt(tInt);
										}
									}
								}
							}
						}
						catch (Exception ex)
						{
							MyCore.SendLogMessage("Services", "OnConnect", Errors.ERROR, "Adding auto watch", "", ex.Message, ex.StackTrace);
						}
					}
					private class Prefix
					{
						public Prefix()
						{
							Pattern = "";
							Hits = 0;
							
						}
						public string Pattern;
						public int Hits;
						public Prefix(string Pattern) 
						{
							Pattern = "";
							Hits = 0;
							
							this.Pattern = Pattern.ToLower();
							Hits = 1;
						}
					}
					private delegate bool ScanCallBack(string pattern, Client user);
					private bool scan_np(string pattern, Client user)
					{
						if (user.Name.ToLower().StartsWith(System.Convert.ToString(pattern).ToLower()))
						{
							MyClient.gLine_Client(user.Name, user.Host, BlackLight.Services.Converters.Time.GetTS(DateTime.Now.AddHours(1F)), "Akilled For Network Abuse (If you feel this was a mistake email med_mediator@hotmail.com)");
							return true;
						}
						return false;
					}
					private bool scan_ns(string pattern, Client user)
					{
						if (user.Name.ToLower().EndsWith(System.Convert.ToString(pattern).ToLower()))
						{
							MyClient.gLine_Client(user.Name, user.Host, BlackLight.Services.Converters.Time.GetTS(DateTime.Now.AddHours(1F)), "Akilled For Network Abuse (If you feel this was a mistake email med_mediator@hotmail.com)");
							return true;
						}
						return false;
					}
					
					private bool scan_reg(string pattern, Client user)
					{
						Regex RegPattern = new Regex(System.Convert.ToString(pattern));
						Match tMatch;
						tMatch = RegPattern.Match(user.Name);
						if (tMatch.Success)
						{
							MyClient.gLine_Client(user.Name, user.Host, BlackLight.Services.Converters.Time.GetTS(DateTime.Now.AddHours(1F)), "Akilled For Network Abuse (If you feel this was a mistake email med_mediator@hotmail.com)");
							return true;
						}
						return false;
					}
					
					
					private void scan_allusers (string pattern, int type)
					{
						switch (type)
						{
							case 0:
								
								GetClients(pattern, new BlackLight.Services.Modules.FloodServ.FloodServService.ScanCallBack( scan_np));
								break;
							case 1:
								
								GetClients(pattern, new BlackLight.Services.Modules.FloodServ.FloodServService.ScanCallBack( scan_ns));
								break;
							case 2:
								
								GetClients(pattern, new BlackLight.Services.Modules.FloodServ.FloodServService.ScanCallBack( scan_reg));
								break;
						}
					}
					private void GetClient (string pattern, ScanCallBack action, BlackLight.Services.Nodes.Server Serv)
					{
						int tIndex;
						for (tIndex = 0; tIndex <= Serv.Users.Count - 1; tIndex++)
						{
							action(pattern,((Client) Serv.Users[tIndex]));
						}
						foreach (Server n in Serv.Leafs)
						{
							GetClient(pattern, action, n);
						}
					}
					private void GetClients (string pattern, ScanCallBack action)
					{
						try
						{
							//							Client n;
							int tIndex;
							for (tIndex = 0; tIndex <= MyCore.MyHost.Users.Count - 1; tIndex++)
							{
								action(pattern,((Client) MyCore.MyHost.Users[tIndex]));
							}
							foreach (Server srv in MyCore.MyHost.Leafs)
							{
								GetClient(pattern, action, srv);
							}
						}
						catch (Exception ex)
						{
							MyCore.SendLogMessage("Services", "GetClient", Errors.ERROR, "Problem finding client", "", ex.Message, ex.StackTrace);
						}
					}
					
					
					public void FSNPScan (Client Source, string[] Params)
					{
						if (Source.Modes.IndexOf("o") > - 1)
						{
							if (Params != null)
							{
								scan_allusers(Params[0], 0);
							}
							else
							{
								Help.sendError("FloodServ", MyClient, Source, "NPSCAN", "NPSCAN");
								return;
							}
						}
						else
						{
							Help.sendResponse("FloodServ", MyClient, Source, "NO_PERMISSION");
							return;
						}
					}
					
					public void FSNSScan (Client Source, string[] Params)
					{
						if (Source.Modes.IndexOf("o") > - 1)
						{
							if (Params != null)
							{
								scan_allusers(Params[0], 1);
							}
							else
							{
								Help.sendError("FloodServ", MyClient, Source, "NSSCAN", "NSSCAN");
								return;
							}
						}
						else
						{
							Help.sendResponse("FloodServ", MyClient, Source, "NO_PERMISSION");
							return;
						}
					}
					
					public void FSRegScan (Client Source, string[] Params)
					{
						if (Source.Modes.IndexOf("o") > - 1)
						{
							if (Params != null)
							{
								scan_allusers(Params[0], 3);
							}
							else
							{
								Help.sendError("FloodServ", MyClient, Source, "REGSCAN", "REGSCAN");
								return;
							}
						}
						else
						{
							Help.sendResponse("FloodServ", MyClient, Source, "NO_PERMISSION");
							return;
						}
					}
					
					
					
					
					
					
					
					private bool addNPWatch(string match, string adder)
					{
						try
						{
							
							BlackLight.Services.DB.Row tRow = FloodDB["npwatches"].MakeRow();
							tRow["match"] = match;
							tRow["adder"] = adder;
							tRow["date"] = BlackLight.Services.Converters.Time.GetTS(DateTime.Now);
							//MsgBox(tRow("regdate"))
							FloodDB["npwatches"].AddRow(tRow);
							updateWatchList(0);
							return true;
						}
						catch (Exception ex)
						{
							MyCore.SendLogMessage("FloodServ", "addNPWatch", Errors.ERROR, "Failed to add watch", "", ex.Message, ex.StackTrace);
							return false;
						}
					}
					
					private bool addNSWatch(string match, string adder)
					{
						try
						{
							
							BlackLight.Services.DB.Row tRow = FloodDB["nswatches"].MakeRow();
							tRow["match"] = match;
							tRow["adder"] = adder;
							tRow["date"] = BlackLight.Services.Converters.Time.GetTS(DateTime.Now);
							//MsgBox(tRow("regdate"))
							FloodDB["nswatches"].AddRow(tRow);
							updateWatchList(1);
							return true;
						}
						catch (Exception)
						{
							return false;
						}
					}
					
					private bool addRegWatch(string match, string adder)
					{
						try
						{
							
							BlackLight.Services.DB.Row tRow = FloodDB["regwatches"].MakeRow();
							tRow["match"] = match;
							tRow["adder"] = adder;
							tRow["date"] = BlackLight.Services.Converters.Time.GetTS(DateTime.Now);
							//MsgBox(tRow("regdate"))
							FloodDB["regwatches"].AddRow(tRow);
							updateWatchList(2);
							return true;
						}
						catch (Exception)
						{
							return false;
						}
					}
					
					
					private bool delNPWatch(int match)
					{
						try
						{
							FloodDB["npwatches"].RemoveAt(match);
							updateWatchList(0);
							return true;
						}
						catch (Exception)
						{
							return false;
						}
					}
					private bool delNSWatch(int match)
					{
						try
						{
							FloodDB["nswatches"].RemoveAt(match);
							updateWatchList(1);
							return true;
						}
						catch (Exception)
						{
							return false;
						}
					}
					private bool delRegWatch(int match)
					{
						try
						{
							FloodDB["regwatches"].RemoveAt(match);
							updateWatchList(2);
							return true;
						}
						catch (Exception)
						{
							return false;
						}
					}
					
					
					private bool listNPWatch(Help.Help Help, Client target)
					{
						try
						{
							Row tRow;
							for (int tInt = 0; tInt <= FloodDB["npwatches"].Count - 1; tInt++)
							{
								tRow = FloodDB["npwatches"][tInt];
								Help.sendResponse("FloodServ", MyClient, target, "NPWATCH_LISTING", Convert.ToString(tInt + 1), System.Convert.ToString(tRow["match"]), System.Convert.ToString(tRow["adder"]), Convert.ToString(System.Convert.ToInt32(tRow["date"])));
							}
							return true;
						}
						catch (Exception ex)
						{
							MyCore.SendLogMessage("FloodServ", "listNPWatch", Errors.ERROR, "Failed to list watches", "", ex.Message, ex.StackTrace);
							return false;
						}
					}
					
					private bool listNSWatch(Help.Help Help, Client target)
					{
						try
						{
							Row tRow;
							for (int tInt = 0; tInt <= FloodDB["nswatches"].Count - 1; tInt++)
							{
								tRow = FloodDB["nswatches"][tInt];
								Help.sendResponse("FloodServ", MyClient, target, "NSWATCH_LISTING", Convert.ToString(tInt + 1), System.Convert.ToString(tRow["match"]), System.Convert.ToString(tRow["adder"]), Convert.ToString(System.Convert.ToInt32(tRow["date"])));
							}
							return true;
						}
						catch (Exception)
						{
							return false;
						}
					}
					
					
					private bool listRegWatch(Help.Help Help, Client target)
					{
						try
						{
							Row tRow;
							for (int tInt = 0; tInt <= FloodDB["regwatches"].Count - 1; tInt++)
							{
								tRow = FloodDB["regwatches"][tInt];
								Help.sendResponse("FloodServ", MyClient, target, "REGWATCH_LISTING", Convert.ToString(tInt + 1), System.Convert.ToString(tRow["match"]), System.Convert.ToString(tRow["adder"]), Convert.ToString(System.Convert.ToInt32(tRow["date"])));
							}
							return true;
						}
						catch (Exception)
						{
							return false;
						}
					}
					
					
					public void FSNPWatch (Client Source, string[] Params)
					{
						if (Source.Modes.IndexOf("o") > - 1)
						{
							if (Params != null)
							{
								switch (Params[0].ToUpper())
								{
									case "ADD":
										
										if (Params.Length != 2)
										{
											Help.sendError("FloodServ", MyClient, Source, "NPWATCH_ADD", "NPWATCH ADD");
											return;
										}
										if (!System.Convert.ToBoolean(FindMatch(Params[1], 0) == null))
										{
											Help.sendResponse("FloodServ", MyClient, Source, "NPWATCH_ADD_PREFIX_EXISTS");
											return;
										}
										if (addNPWatch(Params[1], Source.Nick) == true)
										{
											Help.sendResponse("FloodServ", MyClient, Source, "NPWATCH_ADD_SUCESS");
											return;
										}
										else
										{
											Help.sendResponse("FloodServ", MyClient, Source, "NPWATCH_ADD_FAILURE");
											return;
										}
										//										break;
										//										case "DEL":
										//										
										//										if (Params.Length != 2)
										//										{
										//											Help.sendError("FloodServ", MyClient, Source, "NPWATCH_DEL", "NPWATCH DEL");
										//											return;
										//											}
										//											int tMatch = 0;
										//											if (isNumber(Params[1]))
										//											{
										//												
										//												try
										//												{
										//													tMatch = int.Parse(Params[1]);
										//													}
										//													catch
										//													{
										//														Help.sendResponse("FloodServ", MyClient, Source, "NPWATCH_DEL_NO_MATCH");
										//														return;
										//														}
										//														if (tMatch <= 0 || tMatch > FloodDB["npwatches"].Count)
										//														{
										//															Help.sendResponse("FloodServ", MyClient, Source, "NPWATCH_DEL_NO_MATCH");
										//															return;
										//															}
										//															tMatch = tMatch - 1;
										//															}
										//															else
										//															{
										//																tMatch = FindMatchIndex(Params[1], 0);
										//																if (tMatch < 0)
										//																{
										//																	Help.sendResponse("FloodServ", MyClient, Source, "NPWATCH_DEL_NO_MATCH");
										//																	return;
										//																	}
										//																	}
										//																	
										//																	if (delNPWatch(tMatch) == true)
										//																	{
										//																		Help.sendResponse("FloodServ", MyClient, Source, "NPWATCH_DEL_SUCESS");
										//																		return;
										//																		}
										//																		else
										//																		{
										//																			Help.sendResponse("FloodServ", MyClient, Source, "NPWATCH_DEL_FAILURE");
										//																			return;
										//																			}
										//																			break;
										//																			case "LIST":
										//																			
										//																			if (FloodDB["npwatches"].Count <= 0)
										//																			{
										//																				Help.sendResponse("FloodServ", MyClient, Source, "NPWATCH_LIST_EMPTY");
										//																				return;
										//																				}
										//																				Help.sendResponse("FloodServ", MyClient, Source, "NPWATCH_LIST_START");
										//																				if (listNPWatch(Help, Source) == true)
										//																				{
										//																					Help.sendResponse("FloodServ", MyClient, Source, "NPWATCH_LIST_END");
										//																					}
										//																					else
										//																					{
										//																						Help.sendResponse("FloodServ", MyClient, Source, "NPWATCH_LIST_FAILURE");
										//																						}
										//																						break;
								}
							}
							else
							{
								Help.sendError("FloodServ", MyClient, Source, "NPWATCH", "NPWATCH");
								return;
							}
						}
						else
						{
							Help.sendResponse("FloodServ", MyClient, Source, "NO_PERMISSION");
							return;
						}
					}
																	
																	
					public void FSNSWatch (Client Source, string[] Params)
					{
						if (Source.Modes.IndexOf("o") > - 1)
						{
							if (Params != null)
							{
								switch (Params[0].ToUpper())
								{
									case "ADD":
																						
										if (Params.Length != 2)
										{
											Help.sendError("FloodServ", MyClient, Source, "NSWATCH_ADD", "NSWATCH ADD");
											return;
										}
										if (!System.Convert.ToBoolean(FindMatch(Params[1], 1) == null))
										{
											Help.sendResponse("FloodServ", MyClient, Source, "NSWATCH_ADD_PREFIX_EXISTS");
											return;
										}
										if (addNSWatch(Params[1], Source.Nick) == true)
										{
											Help.sendResponse("FloodServ", MyClient, Source, "NSWATCH_ADD_SUCESS");
											return;
										}
										else
										{
											Help.sendResponse("FloodServ", MyClient, Source, "NSWATCH_ADD_FAILURE");
											return;
										}
										//																						break;
										//																						case "DEL":
										//																						
										//																						if (Params.Length != 2)
										//																						{
										//																							Help.sendError("FloodServ", MyClient, Source, "NSWATCH_DEL", "NSWATCH DEL");
										//																							return;
										//																							}
										//																							int tMatch = 0;
										//																							if (isNumber(Params[1]))
										//																							{
										//																								
										//																								try
										//																								{
										//																									tMatch = int.Parse(Params[1]);
										//																									}
										//																									catch
										//																									{
										//																										Help.sendResponse("FloodServ", MyClient, Source, "NSWATCH_DEL_NO_MATCH");
										//																										return;
										//																										}
										//																										if (tMatch <= 0 || tMatch > FloodDB["nSwatches"].Count)
										//																										{
										//																											Help.sendResponse("FloodServ", MyClient, Source, "NSWATCH_DEL_NO_MATCH");
										//																											return;
										//																											}
										//																											tMatch = tMatch - 1;
										//																											}
										//																											else
										//																											{
										//																												tMatch = FindMatchIndex(Params[1], 1);
										//																												if (tMatch < 0)
										//																												{
										//																													Help.sendResponse("FloodServ", MyClient, Source, "NSWATCH_DEL_NO_MATCH");
										//																													return;
										//																													}
										//																													}
										//																													
										//																													if (delNSWatch(tMatch) == true)
										//																													{
										//																														Help.sendResponse("FloodServ", MyClient, Source, "NSWATCH_DEL_SUCESS");
										//																														return;
										//																														}
										//																														else
										//																														{
										//																															Help.sendResponse("FloodServ", MyClient, Source, "NSWATCH_DEL_FAILURE");
										//																															return;
										//																															}
										//																															break;
										//																															case "LIST":
										//																															
										//																															if (FloodDB["nswatches"].Count <= 0)
										//																															{
										//																																Help.sendResponse("FloodServ", MyClient, Source, "NSWATCH_LIST_EMPTY");
										//																																return;
										//																																}
										//																																Help.sendResponse("FloodServ", MyClient, Source, "NSWATCH_LIST_START");
										//																																if (listNSWatch(Help, Source) == true)
										//																																{
										//																																	Help.sendResponse("FloodServ", MyClient, Source, "NSWATCH_LIST_END");
										//																																	}
										//																																	else
										//																																	{
										//																																		Help.sendResponse("FloodServ", MyClient, Source, "NSWATCH_LIST_FAILURE");
										//																																		}
										//																																		break;
								}
							}
							else
							{
								Help.sendError("FloodServ", MyClient, Source, "NSWATCH", "NSWATCH");
								return;
							}
						}
						else
						{
							Help.sendResponse("FloodServ", MyClient, Source, "NO_PERMISSION");
							return;
						}
					}
																													
					public void FSRegWatch (Client Source, string[] Params)
					{
																														
						if (Source.Modes.IndexOf("o") > - 1)
						{
							if (Params != null)
							{
								switch (Params[0].ToUpper())
								{
									case "ADD":
																																		
										if (Params.Length != 2)
										{
											Help.sendError("FloodServ", MyClient, Source, "REGWATCH_ADD", "REGWATCH ADD");
											return;
										}
										if (!System.Convert.ToBoolean(FindMatch(Params[1], 2) == null))
										{
											Help.sendResponse("FloodServ", MyClient, Source, "REGWATCH_ADD_PREFIX_EXISTS");
											return;
										}
										if (addRegWatch(Params[1], Source.Nick) == true)
										{
											Help.sendResponse("FloodServ", MyClient, Source, "REGWATCH_ADD_SUCESS");
											return;
										}
										else
										{
											Help.sendResponse("FloodServ", MyClient, Source, "REGWATCH_ADD_FAILURE");
											return;
										}
										//																																		break;
										//																																		case "DEL":
										//																																		
										//																																		if (Params.Length != 2)
										//																																		{
										//																																			Help.sendError("FloodServ", MyClient, Source, "REGWATCH_DEL", "REGWATCH DEL");
										//																																			return;
										//																																			}
										//																																			int tMatch = 0;
										//																																			if (isNumber(Params[1]))
										//																																			{
										//																																				
										//																																				try
										//																																				{
										//																																					tMatch = int.Parse(Params[1]);
										//																																					}
										//																																					catch
										//																																					{
										//																																						Help.sendResponse("FloodServ", MyClient, Source, "REGWATCH_DEL_NO_MATCH");
										//																																						return;
										//																																						}
										//																																						if (tMatch <= 0 || tMatch > FloodDB["regwatches"].Count)
										//																																						{
										//																																							Help.sendResponse("FloodServ", MyClient, Source, "REGWATCH_DEL_NO_MATCH");
										//																																							return;
										//																																							}
										//																																							tMatch = tMatch - 1;
										//																																							}
										//																																							else
										//																																							{
										//																																								tMatch = FindMatchIndex(Params[1], 2);
										//																																								if (tMatch < 0)
										//																																								{
										//																																									Help.sendResponse("FloodServ", MyClient, Source, "REGWATCH_DEL_NO_MATCH");
										//																																									return;
										//																																									}
										//																																									}
										//																																									
										//																																									if (delRegWatch(tMatch) == true)
										//																																									{
										//																																										Help.sendResponse("FloodServ", MyClient, Source, "REGWATCH_DEL_SUCESS");
										//																																										return;
										//																																										}
										//																																										else
										//																																										{
										//																																											Help.sendResponse("FloodServ", MyClient, Source, "REGWATCH_DEL_FAILURE");
										//																																											return;
										//																																											}
										//																																											break;
										//																																											case "LIST":
										//																																											
										//																																											if (FloodDB["regwatches"].Count <= 0)
										//																																											{
										//																																												Help.sendResponse("FloodServ", MyClient, Source, "REGWATCH_LIST_EMPTY");
										//																																												return;
										//																																												}
										//																																												Help.sendResponse("FloodServ", MyClient, Source, "REGWATCH_LIST_START");
										//																																												if (listRegWatch(Help, Source) == true)
										//																																												{
										//																																													Help.sendResponse("FloodServ", MyClient, Source, "REGWATCH_LIST_END");
										//																																													}
										//																																													else
										//																																													{
										//																																														Help.sendResponse("FloodServ", MyClient, Source, "REGWATCH_LIST_FAILURE");
										//																																														}
										//																																														break;
								}
							}
							else
							{
								Help.sendError("FloodServ", MyClient, Source, "REGWATCH", "REGWATCH");
								return;
							}
						}
						else
						{
							Help.sendResponse("FloodServ", MyClient, Source, "NO_PERMISSION");
							return;
						}
					}
																																									
																																									
					private void updateWatchList (int type)
					{
						switch (type)
						{
							case 0:
																																												
								m_NPWatches.Clear();
								for (int tInt = 0; tInt <= FloodDB["npWatches"].Count - 1; tInt++)
								{
									m_NPWatches.Add(FloodDB["npwatches"][tInt]["match"]);
								}
								break;
							case 1:
																																												
								m_NSWatches.Clear();
								for (int tInt = 0; tInt <= FloodDB["nsWatches"].Count - 1; tInt++)
								{
									m_NSWatches.Add(FloodDB["nswatches"][tInt]["match"]);
								}
								break;
							case 2:
																																												
								m_RegWatches.Clear();
								for (int tInt = 0; tInt <= FloodDB["regWatches"].Count - 1; tInt++)
								{
									m_RegWatches.Add(FloodDB["regwatches"][tInt]["match"]);
								}
								break;
						}
					}
																																									
																																									
																																									
																																									
					public FloodServService(LocalClient tMyClient, ServicesCore tMyCore, DataBase tFloodDB, ModuleList Modules) 
					{
						this.MyClient = tMyClient;
						this.MyCore = tMyCore;
						this.FloodDB = tFloodDB;
						this.Modules = Modules;
						Help = ((Help.Help) this.Modules["Help"]);
						m_NPWatches = new ArrayList();
						m_NSWatches = new ArrayList();
						m_RegWatches = new ArrayList();
						m_Recent = new ServiceList();
						updateWatchList(0);
						updateWatchList(1);
						updateWatchList(2);
					}
					private Row FindMatch(string match, int type)
					{
						Row tRow = null;
						switch (type)
						{
							case 0:
																																												
								tRow = FloodDB["npwatches"].IRow(match);
								break;
							case 1:
																																												
								tRow = FloodDB["nswatches"].IRow(match);
								break;
							case 2:
																																												
								tRow = FloodDB["regwatches"].IRow(match);
								break;
						}
																																										
						if (tRow != null)
						{
							return tRow;
						}
						else
						{
							return null;
						}
					}
																																									
					private int FindMatchIndex(string match, int type)
					{
						int tRow = 0;
						switch (type)
						{
							case 0:
																																												
								tRow = FloodDB["npwatches"].IIndexOf(match);
								break;
							case 1:
																																												
								tRow = FloodDB["nswatches"].IIndexOf(match);
								break;
							case 2:
																																												
								tRow = FloodDB["regwatches"].IIndexOf(match);
								break;
						}
																																										
						return tRow;
					}
																																									
					private bool isNumber(string val)
					{
						for (int tInt = 0; tInt <= val.Length - 1; tInt++)
						{
							if (char.IsDigit(val, tInt) == false)
							{
								return false;
							}
						}
						return true;
					}
				}
			}
		}
	}
}
