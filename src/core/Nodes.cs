using System.Diagnostics;
using System;
using System.Data;
using System.Collections;
using BlackLight;
using BlackLight.Services.Error;
using BlackLight.Services.Core;

namespace BlackLight
{
	namespace Services
	{
		namespace Nodes
		{
			
			/// -----------------------------------------------------------------------------
			/// Project	 : BlackLight Services
			/// Class	 : IRCUser
			///
			/// -----------------------------------------------------------------------------
			/// <summary>
			/// Base class for all clients and servers that are held in memory
			/// </summary>
			/// <remarks>
			/// </remarks>
			/// <history>
			/// 	[Caleb]	6/18/2005	Created
			/// </history>
			/// -----------------------------------------------------------------------------
			public abstract class IRCUser : IDisposable
			{
				public string Name;
				public ServicesCore MyCore;
				public int ID;
				public void Dispose ()
				{
					Dispose(true);
					GC.SuppressFinalize(this);
				}
				protected abstract void Dispose (bool disposing);
				public IRCUser(string tName, ServicesCore tBase) {
					Name = "";
					ID = - Guid.NewGuid().ToString().GetHashCode();
					
					Name = tName;
					MyCore = tBase;
				}
			}
			public class TKL
			{
				
				
			}
			
			/// -----------------------------------------------------------------------------
			/// Project	 : BlackLight Services
			/// Class	 : Server
			///
			/// -----------------------------------------------------------------------------
			/// <summary>
			/// Public class Server inherits IRCUser contains all variables pertaining to the server
			/// </summary>
			/// <remarks>
			/// </remarks>
			/// <history>
			/// 	[Caleb]	6/18/2005	Created
			/// </history>
			/// -----------------------------------------------------------------------------
			public class Server : IRCUser
			{
				public string Description;
				public int Numeric;
				public ServiceList Users;
				public ServiceList Leafs;
				//Public TKLs As TKLList
				public Server HostServer;
				/// -----------------------------------------------------------------------------
				/// <summary>
				/// Returns a list of all servers linked to this instance
				/// </summary>
				/// <returns></returns>
				/// <remarks>
				/// </remarks>
				/// <history>
				/// 	[Caleb]	6/18/2005	Created
				/// </history>
				/// -----------------------------------------------------------------------------
				public ServiceList GetAllServers()
				{
					try
					{
						ServiceList n = new ServiceList();
						foreach (Server cptr in Leafs)
						{
							n.Add(cptr);
							cptr.GetAllServers(n);
						}
						return n;
					}
					catch (Exception ex)
					{
						MyCore.SendLogMessage("Server", "GetAllServers", BlackLight.Services.Error.Errors.ERROR, "Problem", "", ex.Message, ex.StackTrace);
					return null;
					}
				}
				private void GetAllServers (ServiceList n)
				{
					try
					{
						foreach (Server cptr in Leafs)
						{
							n.Add(cptr);
							cptr.GetAllServers(n);
						}
					}
					catch (Exception ex)
					{
						MyCore.SendLogMessage("Server", "GetAllServers(2)", BlackLight.Services.Error.Errors.ERROR, "Problem", "", ex.Message, ex.StackTrace);
					}
				}
				
				
				protected override void Dispose (bool disposing)
				{
					Server tLeaf;
					Client tUser;
					while (Leafs.Count > 0)
					{
						tLeaf = ((Server) Leafs[0]);
						tLeaf.Dispose();
					}
					while (Users.Count > 0)
					{
						tUser = ((Client) Users[0]);
						MyCore.Raise_Quit(tUser.Nick, "Lost in netsplit");
						tUser.Dispose();
					}
					tLeaf = null;
					tUser = null;
					Users = null;
					if (HostServer != null&& HostServer.Leafs.Contains(this))
					{
						HostServer.Leafs.Remove(this);
						HostServer = null;
					}
					else
					{
						MyCore.SendLogMessage("Server", "Dispose", BlackLight.Services.Error.Errors.DEBUG, "Host server is nothing or does not contain me", "", "", "");
					}
					Leafs = null;
					Description = null;
					Name = null;
				}
				public Server(string tName, ServicesCore tBase) : base(tName, tBase) {
					Users = new ServiceList();
					Leafs = new ServiceList();
					
				}
			}
			
			
			#region "Clients"
			/// -----------------------------------------------------------------------------
			/// Project	 : BlackLight Services
			/// Class	 : Client
			///
			/// -----------------------------------------------------------------------------
			/// <summary>
			/// Instance of a client on the remote servers
			/// </summary>
			/// <remarks>
			/// </remarks>
			/// <history>
			/// 	[Caleb]	6/18/2005	Created
			/// </history>
			/// -----------------------------------------------------------------------------
			public class Client : IRCUser
			{
				public string Host;
				public string VHost;
				public string Username;
				public string Realname;
				public Server HostServer;
				public string IdentNick;
				public string Modes;
				public ChannelsList Channels;
				public int Time;
				/// -----------------------------------------------------------------------------
				/// <summary>
				/// Returns the specific clients nickname
				/// </summary>
				/// <value></value>
				/// <remarks>
				/// </remarks>
				/// <history>
				/// 	[Caleb]	6/18/2005	Created
				/// </history>
				/// -----------------------------------------------------------------------------
				public string Nick
				{
					get{
						return base.Name;
					}
					set
					{
						base.Name = value;
					}
				}
				protected override void Dispose (bool disposing)
				{
					try
					{
						//Deal with the channels
						while (Channels.Count > 0)
						{
							if (Channels[0].ChannelMembers.Count == 1 && Channels[0].ChannelMembers.Contains(this))
							{
								MyCore.Channels.Remove(Channels[0]);
							}
							else
							{
								MyCore.Channels[Channels[0].Name].ChannelMembers.Remove(Channels[0].ChannelMembers[this]);
							}
							Channels.RemoveAt(0);
						}
						//Remove ourselves from teh list0rz
						Host = null;
						VHost = null;
						Username = null;
						Realname = null;
						Modes = null;
						Channels = null;
						if (HostServer != null&& HostServer.Users.Contains(this))
						{
							HostServer.Users.Remove(this);
							HostServer = null;
						}
					}
					catch (Exception ex)
					{
						MyCore.SendLogMessage("Client", "Dispose", BlackLight.Services.Error.Errors.ERROR, "Problem", "", ex.Message, ex.StackTrace);
					}
				}
				/// -----------------------------------------------------------------------------
				/// <summary>
				/// Will parse a mode string received from the server and set the proper variables
				/// </summary>
				/// <param name="ModeString"></param>
				/// <remarks>
				/// </remarks>
				/// <history>
				/// 	[Caleb]	6/18/2005	Created
				/// </history>
				/// -----------------------------------------------------------------------------
				public void ParseModeSet (string ModeString)
				{
					try
					{
						string[] tModeSet = ModeString.Split(' ');
						//short LastModeParamIndex = 1;
						bool Adding = true; //Assume we are adding :\
						foreach (char tChar in tModeSet[0])
						{
							if (tChar == '-')
							{
								Adding = false;
							}
							else if (tChar == '+')
							{
								Adding = true;
							}
							else
							{
								//Stuffy here gay
								if (Adding == true)
								{
									if (Modes.IndexOf(tChar) < 0)
									{
										Modes += tChar;
									}
								}
								else
								{
									Modes = Modes.Replace(tChar.ToString(), "");
								}
							}
						}
					}
					catch (Exception ex)
					{
						MyCore.SendLogMessage("Client", "ParseModeSet", BlackLight.Services.Error.Errors.ERROR, "Problem", "", ex.Message, ex.StackTrace);
					}
				}
				public Client(string tName, ServicesCore tBase) : base(tName, tBase) {
					Host = "";
					VHost = "";
					Username = "";
					Realname = "";
					IdentNick = "";
					Modes = "";
					Channels = new ChannelsList();
					
				}
			}
			#endregion
			
			
			/// -----------------------------------------------------------------------------
			/// Project	 : BlackLight Services
			/// Class	 : ServiceList
			///
			/// -----------------------------------------------------------------------------
			/// <summary>
			/// Base class to hold users
			/// </summary>
			/// <remarks>
			/// </remarks>
			/// <history>
			/// 	[Caleb]	6/18/2005	Created
			/// </history>
			/// -----------------------------------------------------------------------------
			public sealed class ServiceList : IList, ICollection, IEnumerable
			{
				
				readonly ArrayList a;
				public ServiceList() {
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
				bool ICollection.IsSynchronized
				{
					get{
						return false;
					}
				}
				object ICollection.SyncRoot
				{
					get{
						return this;
					}
				}
				public System.Collections.IEnumerator GetEnumerator()
				{
					return a.GetEnumerator();
				}
				int IList.Add(object value)
				{
					return Add((IRCUser)value);
				}
				public int Add(IRCUser value)
				{
					return a.Add(value);
				}
				public void Clear ()
				{
					a.Clear();
				}
				bool IList.Contains(object value)
				{
					return Contains((IRCUser)value);
				}
				public bool Contains(IRCUser value)
				{
					return a.Contains(value);
				}
				public bool Contains(string name)
				{
					return IndexOf(name) >= 0;
				}
				int IList.IndexOf(object value)
				{
					return IndexOf((IRCUser)value);
				}
				public int IndexOf(IRCUser value)
				{
					return a.IndexOf(value);
				}
				public int IndexOf(string name)
				{
					for (int idx = 0; idx <= a.Count - 1; idx++)
					{
						if (((IRCUser)(a[idx])).Name.ToLower() == name.ToLower())
						{
							return idx;
						}
					}
					return - 1;
				}
				void IList.Insert (int index, object value)
				{
					Insert(index, (IRCUser)value);
				}
				public void Insert (int index, IRCUser value)
				{
					a.Insert(index, value);
				}
				bool IList.IsFixedSize
				{
					get{
						return false;
					}
				}
				bool IList.IsReadOnly
				{
					get{
						return false;
					}
				}
				object IList.this[int index]
				{
					get
					{
						return ((IRCUser)(a[index]));
					}
					set
					{
						a[index] = value;
					}
				}
				public IRCUser this[int index]
				{
					get{
						return ((IRCUser)(a[index]));
					}
					set
					{
						a[index] = value;
					}
				}
				public IRCUser this[string name]
				{
					get{
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
					Remove((IRCUser)value);
				}
				public void Remove (IRCUser value)
				{
					a.Remove(value);
				}
				public void RemoveAt (int index)
				{
					a.RemoveAt(index);
				}
			}
			
			
			
			
			
			
			#region "Channels"
			
			/// -----------------------------------------------------------------------------
			/// Project	 : BlackLight Services
			/// Class	 : ChanMembers
			///
			/// -----------------------------------------------------------------------------
			/// <summary>
			/// Base class to hold all channel members
			/// </summary>
			/// <remarks>
			/// </remarks>
			/// <history>
			/// 	[Caleb]	6/18/2005	Created
			/// </history>
			/// -----------------------------------------------------------------------------
			public sealed class ChanMembers : IList, ICollection, IEnumerable
			{
				
				readonly ArrayList a;
				public ChanMembers() {
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
				bool ICollection.IsSynchronized
				{
					get{
						return false;
					}
				}
				object ICollection.SyncRoot
				{
					get{
						return this;
					}
				}
				public System.Collections.IEnumerator GetEnumerator()
				{
					return a.GetEnumerator();
				}
				int IList.Add(object value)
				{
					return Add((ChanMember) value);
				}
				public int Add(ChanMember value)
				{
					return a.Add(value);
				}
				public void Clear ()
				{
					a.Clear();
				}
				bool IList.Contains(object value)
				{
					return Contains((ChanMember) value);
				}
				public bool Contains(ChanMember value)
				{
					return a.Contains(value);
				}
				public bool Contains(Client tUser)
				{
					return IndexOf(tUser) >= 0;
				}
				public bool Contains(string name)
				{
					return IndexOf(name) >= 0;
				}
				int IList.IndexOf(object value)
				{
					return IndexOf((ChanMember) value);
				}
				public int IndexOf(ChanMember value)
				{
					return a.IndexOf(value);
				}
				public int IndexOf(Client tUser)
				{
					for (int idx = 0; idx <= a.Count - 1; idx++)
					{
						if (((ChanMember)(a[idx])).User == tUser)
						{
							return idx;
						}
					}
					return - 1;
				}
				public int IndexOf(string name)
				{
					for (int idx = 0; idx <= a.Count - 1; idx++)
					{
						if (((ChanMember)(a[idx])).User.Name.ToLower() == name.ToLower())
						{
							return idx;
						}
					}
					return - 1;
				}
				void IList.Insert(int index, object value)
				{
					Insert(index,(ChanMember)value);
				}
				public void Insert (int index, ChanMember value)
				{
					a.Insert(index, value);
				}
				bool IList.IsFixedSize
				{
					get{
						return false;
					}
				}
				bool IList.IsReadOnly
				{
					get{
						return false;
					}
				}
				object IList.this[int index]
				{
					get
					{
						return ((ChanMember)(a[index]));
					}
					set
					{
						a[index] = value;
					}
				}
				public ChanMember this[int index]
				{
					get{
						return ((ChanMember)(a[index]));
					}
					set
					{
						a[index] = value;
					}
				}
				public ChanMember this[Client tUser]
				{
					get{
						int idx = IndexOf(tUser);
						if (idx < 0)
						{
							throw (new IndexOutOfRangeException("Object not found"));
						}
						return this[idx];
					}
				}
				public ChanMember this[string name]
				{
					get{
						int idx = IndexOf(name);
						if (idx < 0)
						{
							throw (new IndexOutOfRangeException("Object not found"));
						}
						return this[idx];
					}
				}
				void IList.Remove(object o)
				{
					Remove((ChanMember)o);
				}
				public void Remove (ChanMember value)
				{
					a.Remove(value);
				}
				public void RemoveAt (int index)
				{
					a.RemoveAt(index);
				}
			}
			public sealed class ChannelsList : IList, ICollection, IEnumerable
			{
				
				readonly ArrayList a;
				public ChannelsList() {
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
				bool ICollection.IsSynchronized
				{
					get{
						return false;
					}
				}
				object ICollection.SyncRoot
				{
					get{
						return this;
					}
				}
				public System.Collections.IEnumerator GetEnumerator()
				{
					return a.GetEnumerator();
				}
				int IList.Add(object value)
				{
					return Add((Channel)value);
				}
				public int Add(Channel value)
				{
					return a.Add(value);
				}
				public void Clear ()
				{
					a.Clear();
				}
				bool IList.Contains(object value)
				{
					return Contains((Channel)value);
				}
				public bool Contains(Channel value)
				{
					return a.Contains(value);
				}
				public bool Contains(string name)
				{
					return IndexOf(name) >= 0;
				}
				int IList.IndexOf(object value)
				{
					return IndexOf((Channel)value);
				}
				public int IndexOf(Channel value)
				{
					return a.IndexOf(value);
				}
				public int IndexOf(string name)
				{
					for (int idx = 0; idx <= a.Count - 1; idx++)
					{
						if (((Channel)(a[idx])).Name.ToLower() == name.ToLower())
						{
							return idx;
						}
					}
					return - 1;
				}
				void IList.Insert (int index, object value)
				{
					Insert(index, (Channel)value);
				}
				public void Insert (int index, Channel value)
				{
					a.Insert(index, value);
				}
				bool IList.IsFixedSize
				{
					get{
						return false;
					}
				}
				bool IList.IsReadOnly
				{
					get{
						return false;
					}
				}
				object IList.this[int index]
				{
					get
					{
						return ((Channel)(a[index]));
					}
					set
					{
						a[index] = value;
					}
				}
				public Channel this[int index]
				{
					get{
						return ((Channel)(a[index]));
					}
					set
					{
						a[index] = value;
					}
				}
				public Channel this[string name]
				{
					get{
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
					Remove((Channel)value);
				}
				public void Remove (Channel value)
				{
					a.Remove(value);
				}
				public void RemoveAt (int index)
				{
					a.RemoveAt(index);
				}
			}
			
			/// -----------------------------------------------------------------------------
			/// Project	 : BlackLight Services
			/// Class	 : Topic
			///
			/// -----------------------------------------------------------------------------
			/// <summary>
			/// Simple class for topics should be a datatype
			/// </summary>
			/// <remarks>
			/// </remarks>
			/// <history>
			/// 	[Caleb]	6/18/2005	Created
			/// </history>
			/// -----------------------------------------------------------------------------
			public class Topic
			{
				public Topic()
				{
					Text = "";
					SetBy = "";
					
				}
				public string Text;
				public string SetBy;
				public int Time;
			}
			
			/// -----------------------------------------------------------------------------
			/// Project	 : BlackLight Services
			/// Class	 : ChanMember
			///
			/// -----------------------------------------------------------------------------
			/// <summary>
			/// Instance of a channel member
			/// </summary>
			/// <remarks>
			/// </remarks>
			/// <history>
			/// 	[Caleb]	6/18/2005	Created
			/// </history>
			/// -----------------------------------------------------------------------------
			public class ChanMember
			{
				public Client User;
				public short Status;
				public string Modes;
				
				public ServicesCore MyCore;
				public ChanMember(Client tUser, ServicesCore tBase) {
					Modes = "";
					
					User = tUser;
					MyCore = tBase;
				}
				
				
				/// -----------------------------------------------------------------------------
				/// <summary>
				/// Will remove a mode from the users status
				/// </summary>
				/// <param name="tMode"></param>
				/// <returns></returns>
				/// <remarks>
				/// </remarks>
				/// <history>
				/// 	[Caleb]	6/18/2005	Created
				/// </history>
				/// -----------------------------------------------------------------------------
				public void RemoveMode(char tMode)
				{
					try
					{
						if (Modes.IndexOf(tMode) >= 0)
						{
							Modes.Replace(tMode.ToString(), "");
						}
						
						if (Modes.IndexOf(Modes, MyCore.MyIRCd.OwnerMode) >= 0)
						{
							Status = IRCProtocol.IRCd.MODE_OWNER;
						}
						else if (Modes.IndexOf(Modes, MyCore.MyIRCd.AdminMode) >= 0)
						{
							Status = IRCProtocol.IRCd.MODE_ADMIN;
						}
						else if (Modes.IndexOf(Modes, MyCore.MyIRCd.OpMode) >= 0)
						{
							Status = IRCProtocol.IRCd.MODE_OP;
						}
						else if (Modes.IndexOf(Modes, MyCore.MyIRCd.HalfOpMode) >= 0)
						{
							Status = IRCProtocol.IRCd.MODE_HALFOP;
						}
						else if (Modes.IndexOf(Modes, MyCore.MyIRCd.VoiceMode) >= 0)
						{
							Status = IRCProtocol.IRCd.MODE_VOICE;
						}
						else
						{
							Status = IRCProtocol.IRCd.MODE_USER;
						}
					}
					catch (Exception ex)
					{
						MyCore.SendLogMessage("ChanMember", "RemoveMode", BlackLight.Services.Error.Errors.ERROR, "Problem", "", ex.Message, ex.StackTrace);
					}
				}
				/// -----------------------------------------------------------------------------
				/// <summary>
				/// Will add a mode to the users status
				/// </summary>
				/// <param name="tMode"></param>
				/// <returns></returns>
				/// <remarks>
				/// </remarks>
				/// <history>
				/// 	[Caleb]	6/18/2005	Created
				/// </history>
				/// -----------------------------------------------------------------------------
				public void AddMode(char tMode)
				{
					try
					{
						if (Modes.IndexOf(tMode) < 0)
						{
							Modes += tMode;
						}
						if (Modes.IndexOf(MyCore.MyIRCd.OwnerMode) >= 0)
						{
							Status = IRCProtocol.IRCd.MODE_OWNER;
						}
						else if (Modes.IndexOf(MyCore.MyIRCd.AdminMode) >= 0)
						{
							Status = IRCProtocol.IRCd.MODE_ADMIN;
						}
						else if (Modes.IndexOf(MyCore.MyIRCd.OpMode) >= 0)
						{
							Status = IRCProtocol.IRCd.MODE_OP;
						}
						else if (Modes.IndexOf(MyCore.MyIRCd.HalfOpMode) >= 0)
						{
							Status = IRCProtocol.IRCd.MODE_HALFOP;
						}
						else if (Modes.IndexOf(MyCore.MyIRCd.VoiceMode) >= 0)
						{
							Status = IRCProtocol.IRCd.MODE_VOICE;
						}
						else
						{
							Status = IRCProtocol.IRCd.MODE_USER;
						}
					}
					catch (Exception ex)
					{
						MyCore.SendLogMessage("ChanMember", "AddMode", BlackLight.Services.Error.Errors.ERROR, "Problem", "", ex.Message, ex.StackTrace);
					}
				}
				public void ParseModeSet (string ModeString)
				{
					
				}
				/// -----------------------------------------------------------------------------
				/// <summary>
				/// Returns whether or not the user is a channel owner
				/// </summary>
				/// <returns></returns>
				/// <remarks>
				/// </remarks>
				/// <history>
				/// 	[Caleb]	6/18/2005	Created
				/// </history>
				/// -----------------------------------------------------------------------------
				public bool IsOwner()
				{
					return Status == IRCProtocol.IRCd.MODE_OWNER;
				}
				/// -----------------------------------------------------------------------------
				/// <summary>
				/// Returns whether or not the user is a channel admin or is protected
				/// </summary>
				/// <returns></returns>
				/// <remarks>
				/// </remarks>
				/// <history>
				/// 	[Caleb]	6/18/2005	Created
				/// </history>
				/// -----------------------------------------------------------------------------
				public bool IsAdmin()
				{
					return Status >= IRCProtocol.IRCd.MODE_ADMIN;
				}
				/// -----------------------------------------------------------------------------
				/// <summary>
				/// Returns whether or not the user is a channel operator
				/// </summary>
				/// <returns></returns>
				/// <remarks>
				/// </remarks>
				/// <history>
				/// 	[Caleb]	6/18/2005	Created
				/// </history>
				/// -----------------------------------------------------------------------------
				public bool IsOp()
				{
					return Status >= IRCProtocol.IRCd.MODE_OP;
				}
				/// -----------------------------------------------------------------------------
				/// <summary>
				/// Returns whether or not the user is a channel halfop
				/// </summary>
				/// <returns></returns>
				/// <remarks>
				/// </remarks>
				/// <history>
				/// 	[Caleb]	6/18/2005	Created
				/// </history>
				/// -----------------------------------------------------------------------------
				public bool IsHalfOp()
				{
					return Status >= IRCProtocol.IRCd.MODE_HALFOP;
				}
				/// -----------------------------------------------------------------------------
				/// <summary>
				/// Returns whether or not the user is voiced
				/// </summary>
				/// <returns></returns>
				/// <remarks>
				/// </remarks>
				/// <history>
				/// 	[Caleb]	6/18/2005	Created
				/// </history>
				/// -----------------------------------------------------------------------------
				public bool IsVoice()
				{
					return Status >= IRCProtocol.IRCd.MODE_VOICE;
				}
				/// -----------------------------------------------------------------------------
				/// <summary>
				/// Returns whether or not the user is a regular channel user
				/// </summary>
				/// <returns></returns>
				/// <remarks>
				/// </remarks>
				/// <history>
				/// 	[Caleb]	6/18/2005	Created
				/// </history>
				/// -----------------------------------------------------------------------------
				
				public bool IsUser()
				{
					return Status == IRCProtocol.IRCd.MODE_USER;
				}
			}
			
			/// -----------------------------------------------------------------------------
			/// Project	 : BlackLight Services
			/// Class	 : Channel
			///
			/// -----------------------------------------------------------------------------
			/// <summary>
			/// Class for channels
			/// </summary>
			/// <remarks>
			/// </remarks>
			/// <history>
			/// 	[Caleb]	6/18/2005	Created
			/// </history>
			/// -----------------------------------------------------------------------------
			public class Channel
			{
				public string Name;
				public ServicesCore MyCore;
				//Public Modes As ChannelModes
				public string Modes;
				public Topic Topic;
				public ChanMembers ChannelMembers;
				public Channel(string tName, ServicesCore tBase) {
					Modes = "";
					Topic = new Topic();
					ChannelMembers = new ChanMembers();
					
					Name = tName;
					MyCore = tBase;
				}
				/// -----------------------------------------------------------------------------
				/// <summary>
				/// Parsed mode strings for channel
				/// </summary>
				/// <param name="ModeString"></param>
				/// <remarks>
				/// </remarks>
				/// <history>
				/// 	[Caleb]	6/18/2005	Created
				/// </history>
				/// -----------------------------------------------------------------------------
				public void ParseModeSet (string ModeString)
				{
					try
					{
						string[] tModeSet = ModeString.Split(' ');
						// Dim ContainsExtend As Boolean = False
						// Dim Extended As String
						MyCore.SendLogMessage("Channel", "ParseModeSet", BlackLight.Services.Error.Errors.DEBUG, "Running", ModeString, "", "");
						MyCore.SendLogMessage("Channel", "ParseModeSet", BlackLight.Services.Error.Errors.DEBUG, "String?", tModeSet[0], "", "");
						//Dim RemoveModes As String
						// Dim AddModes As String
						short LastModeParamIndex = 1;
						bool Adding = true; //Lets assume we are adding :\
						foreach (char tChar in tModeSet[0])
						{
							if (tChar == '-')
							{
								Adding = false;
							}
							else if (tChar == '+')
							{
								Adding = true;
							}
							else
							{
								MyCore.SendLogMessage("Channel", "ParseModeSet", BlackLight.Services.Error.Errors.DEBUG, "Adding?", Adding.ToString(), "", "");
								//Going by what is most common to save time
								//First status modes
								if (MyCore.MyIRCd.StatusModes.IndexOf(tChar) >= 0)
								{
									MyCore.SendLogMessage("Channel", "ParseModeSet", BlackLight.Services.Error.Errors.DEBUG, "Status", "", "", "");
									//I don't like this but meh
									if (tModeSet.GetUpperBound(0) >= LastModeParamIndex)
									{
										if (Adding == true)
										{
											ChannelMembers[tModeSet[LastModeParamIndex]].AddMode(tChar);
										}
										else
										{
											ChannelMembers[tModeSet[LastModeParamIndex]].RemoveMode(tChar);
										}
										LastModeParamIndex += Convert.ToInt16(1);
									}
									//Next Access Modes
								}
								else if (MyCore.MyIRCd.AccessModes.IndexOf(tChar) >= 0)
								{
									MyCore.SendLogMessage("Channel", "ParseModeSet", BlackLight.Services.Error.Errors.DEBUG, "Access", "", "", "");
									//Then Param Modes
								}
								else if (MyCore.MyIRCd.ParamModes.IndexOf(tChar) >= 0)
								{
									MyCore.SendLogMessage("Channel", "ParseModeSet", BlackLight.Services.Error.Errors.DEBUG, "Param", "", "", "");
									// ContainsExtend = True
									// Extended &= tchar
									//Now this isn't really right this should probably
									//be after status modes, but do I really want to sacrafice a clock cycle if it isn't?
									//we will see...
								}
								else
								{
									MyCore.SendLogMessage("Channel", "ParseModeSet", BlackLight.Services.Error.Errors.DEBUG, "Normal", "", "", "");
									if (Adding == true)
									{
										if (Modes.IndexOf(tChar) < 0)
										{
											Modes += tChar;
										}
									}
									else
									{
										Modes = Modes.Replace(tChar.ToString(), "");
									}
								}
							}
						}
					}
					catch (Exception ex)
					{
						MyCore.SendLogMessage("Channel", "ParseModeSet", BlackLight.Services.Error.Errors.ERROR, "Problem", "", ex.Message, ex.StackTrace);
					}
				}
				
			}
			#endregion
		}
	}
}
