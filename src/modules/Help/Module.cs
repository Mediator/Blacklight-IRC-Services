using System.Diagnostics;
using Microsoft.VisualBasic;
using System;
using System.Data;
using System.Collections;
using System.IO;
using BlackLight;
using BlackLight.Services;
using BlackLight.Services.Nodes;
using BlackLight.Services.Modules;
using BlackLight.Services.DB;
using BlackLight.Services.Config;
using BlackLight.Services.Modules.Help.Lists;
using BlackLight.Services.Modules.Help;
using System.Xml;

namespace BlackLight
{
	namespace Services
	{
		namespace Modules
		{
			namespace Help
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
				public class Help : BlackLightModule
				{
					
					public ResponseList HelpResponses;
					public Help(ServicesDaemon Base) : base(Base) {
					}
					public override void ModUnload ()
					{
					}
					public override bool ModLoad()
					{
						HelpResponses = new ResponseList();
						string loadDir;
						string loadFile;
						if (MyBase.ConfigManager.Configuration.ContainsKey("services-help1"))
						{
							OptionsList myConfig = MyBase.ConfigManager.Configuration["services-help1"];
							if (myConfig.ContainsKey("help-file-directory1"))
							{
								loadDir = myConfig["help-file-directory1"];
							}
							else
							{
								loadDir = "help/";
							}
							
							
							if (myConfig.ContainsKey("help-language1"))
							{
								loadFile = myConfig["help-language1"];
							}
							else
							{
								loadFile = "en.lang";
							}
							
							
						}
						else
						{
							MyBase.Core.SendLogMessage("Help", "ModLoad", BlackLight.Services.Error.Errors.ERROR, "Failed to find help configuration block", "", "", "");
							return false;
						}
						if (new DirectoryInfo(loadDir).Exists == false)
						{
							MyBase.Core.SendLogMessage("Help", "ModLoad", BlackLight.Services.Error.Errors.ERROR, "Failed to find help file directory", "Using default of \"help/\"", "", "");
							loadDir = "help/";
							if (new DirectoryInfo(loadDir).Exists == false)
							{
								MyBase.Core.SendLogMessage("Help", "ModLoad", BlackLight.Services.Error.Errors.ERROR, "Failed to find help file directory", "", "", "");
								return false;
							}
						}
						
						
						
						if (File.Exists(loadDir + loadFile) == false)
						{
							MyBase.Core.SendLogMessage("Help", "ModLoad", BlackLight.Services.Error.Errors.ERROR, "Failed to find language file", "Using default of \"en.lang\"", "", "");
							loadDir = "en.lang";
							if (File.Exists(loadDir + loadFile) == false)
							{
								MyBase.Core.SendLogMessage("Help", "ModLoad", BlackLight.Services.Error.Errors.ERROR, "Failed to find language file", "", "", "");
								return false;
							}
						}
						
						System.Xml.XmlDocument XMLDOC = new System.Xml.XmlDocument();
						XMLDOC.Load(loadDir + loadFile);
						
						string tName;
						string tErrorName;
						string tErrorValue;
						string[] tErrorValues;
						BlackLight.Services.Modules.Help.Lists.ErrorList tErrors = new BlackLight.Services.Modules.Help.Lists.ErrorList();
						BlackLight.Services.Modules.Help.Lists.CommandList tCommands = new BlackLight.Services.Modules.Help.Lists.CommandList();
						foreach (XmlNode tNode in XMLDOC.DocumentElement)
						{
							if (tNode.Name == "service")
							{
								if (tNode.Attributes["name"] != null && tNode.Attributes["name"].Value != "")
								{
									tName = tNode.Attributes["name"].Value;
								}
								else
								{
									MyBase.Core.SendLogMessage("Help", "ModLoad", BlackLight.Services.Error.Errors.ERROR, "XML Data Corruption", "", "", "");
									//MessageBox.Show("XML Data corruption");
									return false;
								}
								
								foreach (XmlNode tNodeInner in tNode)
								{
									if (tNodeInner.Name == "responses")
									{
										
										foreach (XmlNode tErrorNode in tNodeInner)
										{
											if (tErrorNode.Attributes["name"] != null && tErrorNode.Attributes["name"].Value != "")
											{
												tErrorName = tErrorNode.Attributes["name"].Value;
											}
											else
											{
												MyBase.Core.SendLogMessage("Help", "ModLoad", BlackLight.Services.Error.Errors.ERROR, "XML Data Corruption", "", "", "");
												//Show("XML Data corruption");
												return false;
											}
											if (tErrorNode.InnerText != null&& tErrorNode.InnerText != "")
											{
												tErrorValue = tErrorNode.InnerText.Trim();
											}
											else
											{
												MyBase.Core.SendLogMessage("Help", "ModLoad", BlackLight.Services.Error.Errors.ERROR, "XML Data Corruption", "", "", "");
												//MessageBox.Show("XML Data corruption");
												return false;
											}
											tErrorValue = tErrorValue.Replace("  ", " ");
											tErrorValue = tErrorValue.Replace("  ", " ");
											tErrorValue = tErrorValue.Replace("`B", "");
											tErrorValue = tErrorValue.Replace("`U", "");
											tErrorValue = tErrorValue.Replace("\r\n", "\r");
											tErrorValue = tErrorValue.Replace("\n", "\r");
											tErrorValues = tErrorValue.Split('\r');
											tErrors.Add(new ServiceErrorResponse(tErrorName, tErrorValues));
										}
									}
									else if (tNodeInner.Name == "commands")
									{
										if (GetAllCommands(tNodeInner, tCommands) == false)
										{
											return false;
										}
									}
									else
									{
										MyBase.Core.SendLogMessage("Help", "ModLoad", BlackLight.Services.Error.Errors.ERROR, "XML Data Corruption", "", "", "");
										//MessageBox.Show("XML Data corruption");
										return false;
									}
								}
								HelpResponses.Add(new ServiceResponses(tName, tErrors, tCommands));
							}
							else
							{
								MyBase.Core.SendLogMessage("Help", "ModLoad", BlackLight.Services.Error.Errors.ERROR, "XML Data Corruption", "", "", "");
								//MessageBox.Show("XML Data corruption");
								return false;
							}
						}
						//ListLoaded()
						return true;
					}
					private bool GetAllCommands(XmlNode tNode, BlackLight.Services.Modules.Help.Lists.CommandList TopList)
					{
						string tCmdName;
						string tCmdValue = "";
						string[] tCmdValues;
						BlackLight.Services.Modules.Help.Lists.CommandList tCommands = new BlackLight.Services.Modules.Help.Lists.CommandList();
						ServiceCommandResponse tCmd;
						foreach (XmlNode tCommandNode in tNode)
						{
							if (tCommandNode.Attributes["name"] != null && tCommandNode.Attributes["name"].Value != "")
							{
								tCmdName = tCommandNode.Attributes["name"].Value;
							}
							else
							{
								MyBase.Core.SendLogMessage("Help", "GetAllCommands()", BlackLight.Services.Error.Errors.ERROR, "XML Data Corruption", "", "", "");
								//MessageBox.Show("XML Data corruption");
								return false;
							}
							
							if (tCommandNode["commands"] != null)
							{
								try
								{
									GetAllCommands(tCommandNode["commands"], tCommands);
								}
								catch (Exception ex)
								{
									MyBase.Core.SendLogMessage("Help", "GetAllCommands()", BlackLight.Services.Error.Errors.ERROR, "Exception", "", ex.Message, ex.StackTrace);
									//MessageBox.Show(ex.Message + " " + ex.StackTrace);
								}
							}
							else
							{
								if (tCommandNode.InnerText != null&& tCommandNode.InnerText != "")
								{
									tCmdValue = tCommandNode.InnerText.Trim();
								}
								else
								{
									MyBase.Core.SendLogMessage("Help", "ModLoad", BlackLight.Services.Error.Errors.ERROR, "XML Data Corruption", "", "", "");
									//MessageBox.Show("XML Data corruption");
									return false;
								}
							}
							tCmdValue = tCmdValue.Replace("  ", " ");
							tCmdValue = tCmdValue.Replace("  ", " ");
							tCmdValue = tCmdValue.Replace("`B", "");
							tCmdValue = tCmdValue.Replace("`U", "");
							tCmdValue = tCmdValue.Replace("\r\n", "\r");
							tCmdValue = tCmdValue.Replace("\n", "\r");
							tCmdValues = tCmdValue.Split('\r');
							if (tCommands.Count == 0)
							{
								tCmd = new ServiceCommandResponse(tCmdName, tCmdValues, null);
							}
							else
							{
								tCmd = new ServiceCommandResponse(tCmdName, tCommands["help"].Value, tCommands);
							}
							
							TopList.Add(tCmd);
						}
						return true;
					}
					public void ListLoaded ()
					{
						foreach (BlackLight.Services.Modules.Help.ServiceResponses tService in HelpResponses)
						{
							Console.WriteLine(tService.Name);
							Console.WriteLine("Errors " + tService.Errors.Count);
							Console.WriteLine("Commands " + tService.Commands.Count);
							Console.WriteLine("Errors");
							foreach (ServiceErrorResponse tError in tService.Errors)
							{
								Console.WriteLine(tError.Name);
							}
							Console.WriteLine("Commands");
							ListCommands(tService.Commands);
							
						}
						
					}
					public void sendHelp (string Service, LocalClient ServiceClient, Client Source, string[] Params)
					{
						if (Params == null)
						{
							ServiceCommandResponse tResponse;
							tResponse = HelpResponses[Service].Commands["help"];
							foreach (string tString in tResponse.Value)
							{
								ServiceClient.Send_Notice(Source.Nick, tString);
							}
						}
						else
						{
							//bool tSent = false;
							//ServiceCommandResponse tResponse;
							BlackLight.Services.Modules.Help.Lists.CommandList tList = HelpResponses[Service].Commands;
							for (int tInt = 0; tInt <= Params.GetUpperBound(0); tInt++)
							{
								
								if (tList.Contains(Params[tInt]))
								{
									if (tInt == Params.GetUpperBound(0))
									{
										foreach (string tString in tList[Params[tInt]].Value)
										{
											ServiceClient.Send_Notice(Source.Nick, tString);
										}
										break;
									}
									else
									{
										tList = tList[Params[tInt]].Commands;
									}
								}
								else
								{
									string tString = "";
									foreach (string tVal in Params)
									{
										tString += " " + tVal;
									}
									ServiceClient.Send_Notice(Source.Nick, "No help available for" + tString);
									break;
								}
							}
						}
						
					}
					
					public void sendError (string Service, LocalClient ServiceClient, Client Source, string message, string command)
					{
						if (message != null&& message.Length > 0)
						{
							if (HelpResponses[Service].Errors.Contains(message))
							{
								foreach (string tString in HelpResponses[Service].Errors[message])
								{
									ServiceClient.Send_Notice(Source.Nick, tString);
								}
								string tError = HelpResponses[Service].Errors["error"][0];
								
								ServiceClient.Send_Notice(Source.Nick, string.Format(tError, ServiceClient.Nick, command));
							}
							else
							{
								ServiceClient.Send_Notice(Source.Nick, "Information missing for: " + message);
							}
							Console.WriteLine("o no");
						}
						
					}
					
					public void sendResponse (string Service, LocalClient ServiceClient, Client Source, string message)
					{
						if (message != null&& message.Length > 0)
						{
							if (HelpResponses[Service].Errors.Contains(message))
							{
								foreach (string tString in HelpResponses[Service].Errors[message])
								{
									ServiceClient.Send_Notice(Source.Nick, tString);
								}
							}
							else
							{
								ServiceClient.Send_Notice(Source.Nick, "Information missing for: " + message);
							}
						}
						
					}
					
					public void sendResponse (string Service, LocalClient ServiceClient, Client Source, string message, params string[] Params)
					{
						if (message != null&& message.Length > 0)
						{
							if (HelpResponses[Service].Errors.Contains(message))
							{
								foreach (string tString in HelpResponses[Service].Errors[message])
								{
									ServiceClient.Send_Notice(Source.Nick, string.Format(tString, Params));
								}
							}
							else
							{
								ServiceClient.Send_Notice(Source.Nick, "Information missing for: " + message);
							}
						}
						
					}
					
					private void ListCommands (BlackLight.Services.Modules.Help.Lists.CommandList Commands)
					{
						foreach (BlackLight.Services.Modules.Help.ServiceCommandResponse tCommand in Commands)
						{
							Console.WriteLine(tCommand.Name);
							if (tCommand.Commands != null&& tCommand.Commands.Count > 0)
							{
								ListCommands(tCommand.Commands);
							}
						}
					}
					public override void Rehash ()
					{
						
					}
					public override string Name
					{
						get{
							return "Help";
						}
					}
					public override string Description
					{
						get{
							return "Help Management Module";
						}
					}
					public override bool NeedsDBDriver
					{
						get{
							return false;
						}
					}
				}
				
				
				public class ServiceErrorResponse
				{
					
					public string Name;
					public string[] Value;
					public ServiceErrorResponse(string Name, string[] Value) {
						this.Name = Name;
						this.Value = Value;
					}
				}
				
				public class ServiceCommandResponse
				{
					
					public string Name;
					public string[] Value;
					public BlackLight.Services.Modules.Help.Lists.CommandList Commands;
					public ServiceCommandResponse(string Name, string[] Value, BlackLight.Services.Modules.Help.Lists.CommandList Commands) {
						this.Name = Name;
						this.Value = Value;
						this.Commands = Commands;
					}
				}
				
				public class ServiceResponses
				{
					
					public string Name;
					public BlackLight.Services.Modules.Help.Lists.ErrorList Errors;
					public BlackLight.Services.Modules.Help.Lists.CommandList Commands;
					public ServiceResponses(string Name, BlackLight.Services.Modules.Help.Lists.ErrorList Errors, BlackLight.Services.Modules.Help.Lists.CommandList Commands) {
						this.Name = Name;
						this.Errors = Errors;
						this.Commands = Commands;
					}
				}
				
			}
		}
	}
}
