using System.Diagnostics;
using System;
using System.Data;
using System.Collections;
using System.Reflection;
using System.IO;
using BlackLight;
using BlackLight.Services.Error;

namespace BlackLight
{
	namespace Services
	{
		namespace Modules
		{
			
			/// -----------------------------------------------------------------------------
			/// Project	 : BlackLight Services
			/// Class	 : BlackLightModule
			///
			/// -----------------------------------------------------------------------------
			/// <summary>
			/// Module base class to be inherited by all modules
			/// </summary>
			/// <remarks>
			/// </remarks>
			/// <history>
			/// 	[Caleb]	6/18/2005	Created
			/// </history>
			/// -----------------------------------------------------------------------------
			public abstract class BlackLightModule
			{
				public ServicesDaemon MyBase;
				private bool Suspended;
				private string tFileName;
				//Public MustOverride Sub ModInit()
				/// -----------------------------------------------------------------------------
				/// <summary>
				/// Called if module is unloaded
				/// </summary>
				/// <remarks>
				/// </remarks>
				/// <history>
				/// 	[Caleb]	6/18/2005	Created
				/// </history>
				/// -----------------------------------------------------------------------------
				public abstract void ModUnload ();
				/// -----------------------------------------------------------------------------
				/// <summary>
				/// Called when module is loaded
				/// </summary>
				/// <remarks>
				/// </remarks>
				/// <history>
				/// 	[Caleb]	6/18/2005	Created
				/// </history>
				/// -----------------------------------------------------------------------------
				public abstract bool ModLoad();
				
				/// -----------------------------------------------------------------------------
				/// <summary>
				/// If module supports rehashing, method is called
				/// </summary>
				/// <remarks>
				/// </remarks>
				/// <history>
				/// 	[Caleb]	6/18/2005	Created
				/// </history>
				/// -----------------------------------------------------------------------------
				public virtual void Rehash ()
				{
					
				}
				
				/// -----------------------------------------------------------------------------
				/// <summary>
				/// Should return the name of the module
				/// </summary>
				/// <value></value>
				/// <remarks>
				/// </remarks>
				/// <history>
				/// 	[Caleb]	6/18/2005	Created
				/// </history>
				/// -----------------------------------------------------------------------------
				public abstract string Name{
					get;
				}
				/// -----------------------------------------------------------------------------
				/// <summary>
				/// Should return a description of the module
				/// </summary>
				/// <value></value>
				/// <remarks>
				/// </remarks>
				/// <history>
				/// 	[Caleb]	6/18/2005	Created
				/// </history>
				/// -----------------------------------------------------------------------------
				public abstract string Description{
					get;
				}
				/// -----------------------------------------------------------------------------
				/// <summary>
				/// List of required modules
				/// </summary>
				/// <remarks>
				/// </remarks>
				/// <history>
				/// 	[Caleb]	6/18/2005	Created
				/// </history>
				/// -----------------------------------------------------------------------------
				public System.Collections.Specialized.StringCollection Requires;
				/// -----------------------------------------------------------------------------
				/// <summary>
				/// Adds a module to the required list
				/// </summary>
				/// <param name="tName">Should be the module filename for example NickServ.dll</param>
				/// <remarks>
				/// </remarks>
				/// <history>
				/// 	[Caleb]	6/18/2005	Created
				/// </history>
				/// -----------------------------------------------------------------------------
				public void AddRequired (string tName)
				{
					Requires.Add(tName);
				}
				
				/// -----------------------------------------------------------------------------
				/// <summary>
				/// Returns whether or not the module is suspended
				/// </summary>
				/// <value></value>
				/// <remarks>
				/// The module should check this parameter and not execute based on its result
				/// </remarks>
				/// <history>
				/// 	[Caleb]	6/18/2005	Created
				/// </history>
				/// -----------------------------------------------------------------------------
				public bool Suspend
				{
					get{
						return Suspended;
					}
					set
					{
						Suspended = value;
					}
				}
				/// -----------------------------------------------------------------------------
				/// <summary>
				/// External property to be set when module is being loaded
				/// </summary>
				/// <value></value>
				/// <remarks>
				/// </remarks>
				/// <history>
				/// 	[Caleb]	6/18/2005	Created
				/// </history>
				/// -----------------------------------------------------------------------------
				public string FileName
				{
					get{
						return tFileName;
					}
					set
					{
						tFileName = value;
					}
				}
				public BlackLightModule(ServicesDaemon tBase) {
					tFileName = "";
					Requires = new System.Collections.Specialized.StringCollection();
					
					MyBase = tBase;
					tBase = null;
				}
				/// -----------------------------------------------------------------------------
				/// <summary>
				/// A property that must be overridin in all modules that specifies whether the
				/// modules requires database functionality to operate
				/// </summary>
				/// <value></value>
				/// <remarks>
				/// </remarks>
				/// <history>
				/// 	[Caleb]	12/1/2005	Created
				/// </history>
				/// -----------------------------------------------------------------------------
				public abstract bool NeedsDBDriver{
					get;
				}
			}
			
			/// -----------------------------------------------------------------------------
			/// Project	 : BlackLight Services
			/// Class	 : ModuleManagement
			///
			/// -----------------------------------------------------------------------------
			/// <summary>
			/// Class which contains and loads modules
			/// </summary>
			/// <remarks>
			/// </remarks>
			/// <history>
			/// 	[Caleb]	6/18/2005	Created
			/// </history>
			/// -----------------------------------------------------------------------------
			public class ModuleManagement
			{
				public ServicesDaemon Base;
				/// -----------------------------------------------------------------------------
				/// <summary>
				/// List of functioning and loaded modules
				/// </summary>
				/// <remarks>
				/// </remarks>
				/// <history>
				/// 	[Caleb]	6/18/2005	Created
				/// </history>
				/// -----------------------------------------------------------------------------
				public ModuleList Modules;
				public string DataDriver;
				/// -----------------------------------------------------------------------------
				/// <summary>
				/// Will load all modules in given list
				/// </summary>
				/// <param name="ModFiles"></param>
				/// <remarks>
				/// Once a module is loaded it will be added the the Module list,
				/// and will then have ModLoad called and the filename property set
				/// </remarks>
				/// <history>
				/// 	[Caleb]	6/18/2005	Created
				/// </history>
				/// -----------------------------------------------------------------------------
				public void LoadModules (string[] ModFiles)
				{
					System.Collections.Specialized.StringCollection ModuleFiles = new System.Collections.Specialized.StringCollection();
					Assembly tObject;
					//File tFile;
					int tIndex;
					short tCount = 0;
					string tMod;
					string tString;
					System.Collections.Specialized.StringCollection tFound = new System.Collections.Specialized.StringCollection();
					foreach (string xMod in ModFiles)
					{
						ModuleFiles.Add(xMod);
					}
					BlackLightModule tModule;
					//Cycle through the modules untill they are all loaded or removed
					while (ModuleFiles.Count > 0)
					{
						try
						{
							//Infinate loop check
							if (tCount > 1000)
							{
								Base.Core.SendLogMessage("ModuleManagement", "LoadModules", BlackLight.Services.Error.Errors.DEBUG | BlackLight.Services.Error.Errors.WARNING, "It appears we have gotten into an infinate loop while loading modules, loading halted", "Most likely cause is two or more modules require each other and will never be able to load", "", "");
								return;
							}
							tCount += Convert.ToInt16(1);
							
							//Get the filename
							tMod = ModuleFiles[0].ToString();
							
							//Make sure it exists
							if (File.Exists("modules/" + tMod))
							{
								
								//Use .NET to attempt to load the class inside the dll
								tObject = @Assembly.LoadFrom("modules/" + tMod);
								
								//Make sure everything is still hawt or remove the failed module
								if (tObject == null)
								{
									ModuleFiles.RemoveAt(0);
								}
								
								//Get a list of the classes the module contains
								Type[] tList = tObject.GetTypes();
								//Cycle through them all
								foreach (Type tType in tList)
								{
									
									//Make sure it uses our module base somewhere
									if (tType.IsSubclassOf(typeof(BlackLightModule)) == true)
									{
										//Alrighty it's a valid module now what?
										
										//Well lets check if its a special module, currently only datadrivers
										
										//Ooooo isn't this sexy
										tModule = ((BlackLightModule) tType.GetConstructor(new Type[] { typeof(ServicesDaemon) }).Invoke(new object[] {Base}));
										
										//Or not :'(
										if (tModule == null)
										{
											ModuleFiles.RemoveAt(0);
										}
										
										
										if (tType.IsSubclassOf(typeof(DB.DataDriver)) == true)
										{
											if (DataDriver == "")
											{
												DataDriver = tModule.Name;
											}
											else
											{
												Base.Core.SendLogMessage("ModuleManagement", "LoadModules", BlackLight.Services.Error.Errors.DEBUG | BlackLight.Services.Error.Errors.WARNING, "DataBase driver already loaded!", "File does not exist", "", "");
												ModuleFiles.RemoveAt(0);
											}
										}
										
										//Set the internal filename for the ModuleList Class
										tModule.FileName = tMod;
										
										//Clear the list of found modules for requirements
										tFound.Clear();
										
										//Malicious module coder?
										if (tModule.Requires.Count < 1000)
										{
											
											//Check if they actually have any requirements (Hope they don't)
											if (tModule.Requires.Count > 0)
											{
												//What kind of peice of shit coder requires other modules...oh right chanserv needs
												//nickserv
												
												//Check if its loaded already and work is saved
												foreach (string xString in tModule.Requires)
												{
													tString = xString.ToLower();
													//Make sure the dumbass didn't require themselves
													if (tMod.ToLower() != tString)
													{
														
														//Make sure we havn't already found it
														if (tFound.Contains(tString) == false)
														{
															
															//Now check if its loaded
															if (Modules.Contains(tString) == false)
															{
																//Nope not loaded, does the file actually exist?
																if (File.Exists("modules/" + tMod) == false)
																{
																	Base.Core.SendLogMessage("ModuleManagement", "LoadModules", BlackLight.Services.Error.Errors.DEBUG | BlackLight.Services.Error.Errors.WARNING, "Requirements for " + tMod + " not met", "File does not exist", "", "");
																	ModuleFiles.RemoveAt(0);
																	break;
																}
																else
																{
																	//It does, heh, so the module still could be loaded
																	
																	//Well maybe, lets see if its possible
																	if (ModuleFiles.Contains(tString) == false)
																	{
																		//Nope? Well thats the end of that module
																		Base.Core.SendLogMessage("ModuleManagement", "LoadModules", BlackLight.Services.Error.Errors.DEBUG | BlackLight.Services.Error.Errors.WARNING, "Requirements for " + tMod + " not met", "Requirement not found in avalability list", "", "");
																		ModuleFiles.RemoveAt(0);
																		break;
																	}
																}
															}
															else
															{
																//Alrighty then, so the module has already been loaded, add it to the found list
																tFound.Add(tString);
																// Base.Service.SendLogMessage("ModuleManagement", "LoadModules", Errors.LOG_DEBUG_AND_WARNING, "Found", "", "", "")
															}
														}
													}
													else
													{
														//One word: Dumbass
														Base.Core.SendLogMessage("ModuleManagement", "LoadModules", BlackLight.Services.Error.Errors.DEBUG | BlackLight.Services.Error.Errors.WARNING, "A module cannot require itself", "", "", "");
														ModuleFiles.RemoveAt(0);
														break;
													}
												}

												//Make sure we didn't remove our module already
												if (ModuleFiles.Contains(tMod))
												{
													
													//Check if all the requirements are met
													if (tFound.Count == tModule.Requires.Count)
													{
														
														//Verify its not already loaded
														if (Modules.Contains(tMod) == false && Modules.Contains(tModule.Name) == false)
														{
															//Add it
															Modules.Add(tModule);
															// tModule.ModLoad()
														}
														else
														{
															Base.Core.SendLogMessage("ModuleManagement", "LoadModules", BlackLight.Services.Error.Errors.DEBUG | BlackLight.Services.Error.Errors.WARNING, "Module with duplicate name already loaded: " + tMod, "", "", "");
														}
														//Remove it and goto the next module
														ModuleFiles.RemoveAt(0);
													}
													else
													{
														//The requirements are not met, so we
														//Send it to the back?
														ModuleFiles.RemoveAt(0);
														ModuleFiles.Add(tMod);
													}
												}
											}
											else
											{
												//YAY! No requirements. Now just make sure its not already loaded and add it to the list
												if (Modules.Contains(tMod) == false && Modules.Contains(tModule.Name) == false)
												{
													Modules.Add(tModule);
													// tModule.ModLoad()
												}
												else
												{
													Base.Core.SendLogMessage("ModuleManagement", "LoadModules", BlackLight.Services.Error.Errors.DEBUG | BlackLight.Services.Error.Errors.WARNING, "Module with duplicate name already loaded: " + tMod, "", "", "");
												}
												ModuleFiles.RemoveAt(0);
											}
										}
										else
										{
											Base.Core.SendLogMessage("ModuleManagement", "LoadModules", BlackLight.Services.Error.Errors.DEBUG | BlackLight.Services.Error.Errors.WARNING, "Module contains to many requirements: " + tMod, "", "", "");
										}
									}
								}
								
							}
							else
							{
								Base.Core.SendLogMessage("ModuleManagement", "LoadModules", BlackLight.Services.Error.Errors.DEBUG | BlackLight.Services.Error.Errors.WARNING, "File does not exist: " + tMod, "", "", "");
								ModuleFiles.RemoveAt(0);
							}
						}
						catch (Exception e)
						{
							Base.Core.SendLogMessage("ModuleManagement", "LoadModules", BlackLight.Services.Error.Errors.DEBUG | BlackLight.Services.Error.Errors.ERROR, "Exception", "", e.Message, e.StackTrace);
							//MessageBox.Show(e.Message + " " + e.StackTrace);
						}
					}
					//Now give our user a nice pretty list of which modules fucked up
					for (tIndex = 0; tIndex <= ModFiles.Length - 1; tIndex++)
					{
						if (Modules.Contains(ModFiles[tIndex]) == false)
						{
							Base.Core.SendLogMessage("ModuleManagement", "LoadModules", BlackLight.Services.Error.Errors.DEBUG | BlackLight.Services.Error.Errors.WARNING, "Failed to load: " + ModFiles[tIndex].ToString(), "", "", "");
						}
						else
						{
							if (Modules[ModFiles[tIndex]].NeedsDBDriver == true)
							{
								if (DataDriver == "")
								{
									Modules.Remove(Modules[ModFiles[tIndex]]);
									Base.Core.SendLogMessage("ModuleManagement", "LoadModules", BlackLight.Services.Error.Errors.DEBUG | BlackLight.Services.Error.Errors.WARNING, "Failed to load: " + ModFiles[tIndex].ToString(), "No DataDriver Found", "", "");
								}
								else
								{
									try
									{
										if (Modules[ModFiles[tIndex]].ModLoad())
										{
											Base.Core.SendLogMessage("ModuleManagement", "LoadModules", BlackLight.Services.Error.Errors.DEBUG, "Loaded Module: " + ModFiles[tIndex].ToString(), "", "", "");
										}
										else
										{
											Base.Core.SendLogMessage("ModuleManagement", "LoadModules", BlackLight.Services.Error.Errors.DEBUG | BlackLight.Services.Error.Errors.WARNING, "Failed to load: " + ModFiles[tIndex].ToString(), "ModLoad Error", "", "");
											RemoveAllRequired(ModFiles[tIndex]);
											Modules.Remove(Modules[ModFiles[tIndex]]);
										}
									}
									catch (Exception ex)
									{
										Base.Core.SendLogMessage("ModuleManagement", "LoadModules", BlackLight.Services.Error.Errors.DEBUG | BlackLight.Services.Error.Errors.WARNING, "Failed to load: " + ModFiles[tIndex].ToString(), "ModLoad Error", ex.Message, ex.StackTrace);
									}

								}
							}
							else
							{
								try
									{
								if (Modules[ModFiles[tIndex]].ModLoad())
								{
									Base.Core.SendLogMessage("ModuleManagement", "LoadModules", BlackLight.Services.Error.Errors.DEBUG, "Loaded Module: " + ModFiles[tIndex].ToString(), "", "", "");
								}
								else
								{
									Base.Core.SendLogMessage("ModuleManagement", "LoadModules", BlackLight.Services.Error.Errors.DEBUG | BlackLight.Services.Error.Errors.WARNING, "Failed to load: " + ModFiles[tIndex].ToString(), "ModLoad Error", "", "");
									RemoveAllRequired(ModFiles[tIndex]);
									Modules.Remove(Modules[ModFiles[tIndex]]);
								}
								}
								catch (Exception ex)
								{
									Base.Core.SendLogMessage("ModuleManagement", "LoadModules", BlackLight.Services.Error.Errors.DEBUG | BlackLight.Services.Error.Errors.WARNING, "Failed to load: " + ModFiles[tIndex].ToString(), "ModLoad Error", ex.Message, ex.StackTrace);
								}
							}
						}
					}
					//And we are done
				}
				private void RemoveAllRequired (string tModule)
				{
					for (int tIndex = 0; tIndex <= Modules.Count - 1; tIndex++)
					{
						if (tIndex > Modules.Count - 1)
						{
							return;
						}
						BlackLightModule xModule = Modules[tIndex];
						if (xModule.Requires.Contains(tModule.ToLower()))
						{
							Base.Core.SendLogMessage("ModuleManagement", "LoadModules", BlackLight.Services.Error.Errors.DEBUG | BlackLight.Services.Error.Errors.WARNING, "Failed to load: " + xModule.FileName.ToString(), "Requirements no longer met", "", "");
							RemoveAllRequired(xModule.FileName);
							Modules.Remove(xModule);
						}
					}
				}
				public void LoadDataDriver (string Drivername)
				{
					ArrayList ModuleFiles = new ArrayList();
					Assembly tObject;
					//File tFile;
					//int tIndex;
					string tMod;
					ArrayList tFound = new ArrayList();
					BlackLightModule tModule;
					//Cycle through the modules untill they are all loaded or removed
					try
					{
						
						
						//Get the filename
						tMod = Drivername;
						
						//Make sure it exists
						if (File.Exists("modules/" + tMod))
						{
							
							//Use .NET to attempt to load the class inside the dll
							tObject = @Assembly.LoadFrom("modules/" + tMod);
							
							//Make sure everything is still hawt or remove the failed module
							if (tObject == null)
							{
								ModuleFiles.RemoveAt(0);
							}
							
							//Get a list of the classes the module contains
							Type[] tList = tObject.GetTypes();
							//Cycle through them all
							foreach (Type tType in tList)
							{
								
								//Make sure it uses our module base somewhere
								if (tType.IsSubclassOf(typeof(BlackLightModule)) == true)
								{
									//Alrighty it's a valid module now what?
									
									//Well lets check if its a special module, currently only datadrivers
									
									//Ooooo isn't this sexy
									tModule = ((BlackLightModule) tType.GetConstructor(new Type[] { typeof(ServicesDaemon) }).Invoke(new object[] {Base}));
									
									//Or not :'(
									if (tModule == null)
									{
										Base.Core.SendLogMessage("ModuleManagement", "LoadDataDriver", BlackLight.Services.Error.Errors.DEBUG | BlackLight.Services.Error.Errors.WARNING, "DataBase driver cannot be loaded", "File does not exist", "", "");
										return;
									}
									
									
									if (tType.IsSubclassOf(typeof(DB.DataDriver)) == true)
									{
										if (DataDriver == "")
										{
											DataDriver = tModule.Name;
										}
										else
										{
											Base.Core.SendLogMessage("ModuleManagement", "LoadDataDriver", BlackLight.Services.Error.Errors.DEBUG | BlackLight.Services.Error.Errors.WARNING, "DataBase driver already loaded!", "", "", "");
											return;
										}
									}
									
									//Set the internal filename for the ModuleList Class
									tModule.FileName = tMod;
									
									if (Modules.Contains(tMod) == false && Modules.Contains(tModule.Name) == false)
									{
										Modules.Add(tModule);
										tModule.ModLoad();
										return;
									}
									else
									{
										Base.Core.SendLogMessage("ModuleManagement", "LoadDataDriver", BlackLight.Services.Error.Errors.DEBUG | BlackLight.Services.Error.Errors.WARNING, "Module with duplicate name already loaded: " + tMod, "", "", "");
									}
								}
							}
						}
						else
						{
							Base.Core.SendLogMessage("ModuleManagement", "LoadDataDriver", BlackLight.Services.Error.Errors.DEBUG | BlackLight.Services.Error.Errors.WARNING, "File does not exist: " + tMod, "", "", "");
							ModuleFiles.RemoveAt(0);
						}
					}
					catch (Exception e)
					{
						Base.Core.SendLogMessage("ModuleManagement", "LoadDataDriver", BlackLight.Services.Error.Errors.DEBUG | BlackLight.Services.Error.Errors.ERROR, "Exception", "", e.Message, e.StackTrace);
						//MessageBox.Show(e.Message + " " + e.StackTrace);
					}
				}
				public void LoadModule (string tModule)
				{
					//TODO
				}
				public void UnloadModule ()
				{
					//TODO
				}
				
				public ModuleManagement(ServicesDaemon tBase) {
					Modules = new ModuleList();
					DataDriver = "";
					
					Base = tBase;
				}
			}
			
			
			/// -----------------------------------------------------------------------------
			/// Project	 : BlackLight Services
			/// Class	 : ModuleList
			///
			/// -----------------------------------------------------------------------------
			/// <summary>
			/// Container for the list of modules
			/// </summary>
			/// <remarks>
			/// </remarks>
			/// <history>
			/// 	[Caleb]	6/18/2005	Created
			/// </history>
			/// -----------------------------------------------------------------------------
			/*public sealed class StringArrayList : IList, ICollection, IEnumerable
			{
				
				readonly ArrayList a;
				public StringArrayList() {
					a = new ArrayList();
				}
				public void CopyTo (System.Array array, int index)
				{
					a.CopyTo(array, index);
				}
				public int Count
				{
					get{
						return a.Count;
					}
				}
				public bool IsSynchronized
				{
					get{
						return false;
					}
				}
				public object SyncRoot
				{
					get{
						return this;
					}
				}
				public System.Collections.IEnumerator GetEnumerator()
				{
					return a.GetEnumerator();
				}
				public int Add(string value)
				{
					return a.Add(value);
				}
				public void Clear ()
				{
					a.Clear();
				}
				public bool Contains(string value)
				{
					return IndexOf(value) >= 0;
				}
				public int IndexOf(string value)
				{
					for (int idx = 0; idx <= a.Count - 1; idx++)
					{
						if (Strings.LCase(System.Convert.ToString(a[idx])) == Strings.LCase(value))
						{
							return idx;
						}
					}
					return - 1;
				}
				public void Insert (int index, string value)
				{
					a.Insert(index, value);
				}
				public bool IsFixedSize
				{
					get{
						return false;
					}
				}
				public bool IsReadOnly
				{
					get{
						return false;
					}
				}
				public string this[int index]
				{
					get{
						return System.Convert.ToString(a[index]);
					}
					set
					{
						a[index] = value;
					}
				}
				public string this[string val]
				{
					get{
						int idx = IndexOf(val);
						if (idx < 0)
						{
							throw (new IndexOutOfRangeException("Object not found"));
						}
						return Item[idx];
					}
				}
				public void Remove (string value)
				{
					int idx = IndexOf(value);
					if (idx < 0)
					{
						throw (new IndexOutOfRangeException("Object not found"));
					}
					a.RemoveAt(idx);
				}
				public void RemoveAt (int index)
				{
					a.RemoveAt(index);
				}
			}
			
			*/
			public sealed class ModuleList : IList, ICollection, IEnumerable
			{
				
				readonly ArrayList a;
				public ModuleList() 
				{
					a = new ArrayList();
				}
				void ICollection.CopyTo (System.Array array, int index)
				{
					a.CopyTo(array, index);
				}
				public int Count
				{
					get
					{
						return a.Count;
					}
				}
				bool ICollection.IsSynchronized
				{
					get
					{
						return false;
					}
				}
				object ICollection.SyncRoot
				{
					get
					{
						return this;
					}
				}
				public System.Collections.IEnumerator GetEnumerator()
				{
					return a.GetEnumerator();
				}
				int IList.Add(object o) 
				{
					return Add((BlackLightModule) o);
				}
				public int Add(BlackLightModule value)
				{
					return a.Add(value);
				}
				public void Clear ()
				{
					a.Clear();
				}
				bool IList.Contains(object value)
				{
					return Contains((BlackLightModule)value);
				}
				public bool Contains(BlackLightModule value)
				{
					return a.Contains(value);
				}
				public bool Contains(string name)
				{
					return IndexOf(name) >= 0;
				}
				int IList.IndexOf(object value)
				{
					return IndexOf((BlackLightModule)value);
				}
				public int IndexOf(BlackLightModule value)
				{
					return a.IndexOf(value);
				}
				public int IndexOf(string name)
				{
					for (int idx = 0; idx <= a.Count - 1; idx++)
					{
						if (((BlackLightModule)(a[idx])).Name.ToLower() == name.ToLower()|| ((BlackLightModule)(a[idx])).FileName.ToLower() == name.ToLower())
						{
							return idx;
						}
					}
					return - 1;
				}
				void IList.Insert(int index, object value)
			{
					Insert(index,(BlackLightModule)value);
			}
				public void Insert (int index, BlackLightModule value)
				{
					a.Insert(index, value);
				}
				bool IList.IsFixedSize
				{
					get
					{
						return false;
					}
				}
				bool IList.IsReadOnly
				{
					get
					{
						return false;
					}
				}
				object IList.this[int index]
				{
					get
					{
						return ((BlackLightModule)(a[index]));
					}
					set
					{
						a[index] = value;
					}
				}
				public BlackLightModule this[int index]
				{
					get
					{
						return ((BlackLightModule)(a[index]));
					}
					set
					{
						a[index] = value;
					}
				}
				public BlackLightModule this[string name]
				{
					get
					{
						int idx = IndexOf(name);
						if (idx < 0)
						{
							throw (new IndexOutOfRangeException("Object not found"));
						}
						return this[idx];
					}
				}
				void IList.Remove (object value)
				{
					Remove((BlackLightModule)value);
				}
				public void Remove (BlackLightModule value)
				{
					a.Remove(value);
				}
				public void RemoveAt (int index)
				{
					a.RemoveAt(index);
				}
			}
		}
	}
}
