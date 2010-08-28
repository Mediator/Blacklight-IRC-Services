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
				protected string p_name;
				protected ServicesCore p_core;
                protected int p_id;

                public string name
                {
                    get
                    {
                        return p_name;
                    }
                    set
                    {
                       p_name = value;
                    }
                }
                public int id
                {
                    get
                    {
                        return p_id;
                    }
                }
				public void Dispose ()
				{
					Dispose(true);
					GC.SuppressFinalize(this);
				}
				protected abstract void Dispose (bool disposing);
				public IRCUser(string name, ServicesCore core) {
					p_id = - Guid.NewGuid().ToString().GetHashCode();	
					p_name = name;
					p_core = core;
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
				private string p_description;
				private int p_numeric;
				public ServiceList users;
				public ServiceList leafs;
				//Public TKLs As TKLList
				private Server p_hostServer;
                public string description
                {
                    get
                    {
                        return p_description;
                    }
                    set
                    {
                        p_description = value;
                    }
                }
                public int numeric
                {
                    get
                    {
                        return p_numeric;
                    }
                    set
                    {
                        p_numeric = value;
                    }
                }
                public Server hostServer
                {
                    get
                    {
                        return p_hostServer;
                    }
                    set
                    {
                        p_hostServer = value;
                    }
                }
              
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
						ServiceList sl = new ServiceList();
						foreach (Server leaf in leafs)
						{
							sl.Add(leaf);
							leaf.GetAllServers(sl);
						}
						return sl;
					}
					catch (Exception ex)
					{
						p_core.SendLogMessage("Server", "GetAllServers", BlackLight.Services.Error.Errors.ERROR, "Problem", "", ex.Message, ex.StackTrace);
					return null;
					}
				}
				private void GetAllServers (ServiceList sl)
				{
					try
					{
						foreach (Server leaf in leafs)
						{
							sl.Add(leaf);
							leaf.GetAllServers(sl);
						}
					}
					catch (Exception ex)
					{
						p_core.SendLogMessage("Server", "GetAllServers(2)", BlackLight.Services.Error.Errors.ERROR, "Problem", "", ex.Message, ex.StackTrace);
					}
				}
				
				
				protected override void Dispose (bool disposing)
				{
					Server tLeaf;
					Client tUser;
					while (leafs.Count > 0)
					{
						tLeaf = ((Server) leafs[0]);
						tLeaf.Dispose();
					}
					while (users.Count > 0)
					{
						tUser = ((Client) users[0]);
						p_core.Raise_Quit(tUser.nick, "Lost in netsplit");
						tUser.Dispose();
					}
					tLeaf = null;
					tUser = null;
					users = null;
					if (p_hostServer != null && p_hostServer.leafs.Contains(this))
					{
						p_hostServer.leafs.Remove(this);
						p_hostServer = null;
					}
					else
					{
						p_core.SendLogMessage("Server", "Dispose", BlackLight.Services.Error.Errors.DEBUG, "Host server is nothing or does not contain me", "", "", "");
					}
					leafs = null;
					p_description = null;
					p_name = null;
				}
				public Server(string name, ServicesCore core) : base(name, core) {
					users = new ServiceList();
					leafs = new ServiceList();
					
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
				protected string p_host;
                protected string p_vHost;
                protected string p_username;
                protected string p_realname;
                protected Server p_hostServer;
                protected string p_identNick;
                protected string p_modes;
                protected ChannelsList p_channels;
                protected int p_time;
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
				public string nick
				{
					get{
						return base.p_name;
					}
					set
					{
						base.p_name = value;
					}
				}
                public string host
                {
                    get
                    {
                        return this.p_host;
                    }
                    set
                    {
                        this.p_host = value;
                    }
                }
                public string vHost
                {
                    get
                    {
                        return this.p_vHost;
                    }
                    set
                    {
                        this.p_vHost = value;
                    }
                }
                public string username
                {
                    get
                    {
                        return this.p_username;
                    }
                    set
                    {
                        this.p_username = value;
                    }
                }
                public string realname
                {
                    get
                    {
                        return this.p_realname;
                    }
                    set
                    {
                        this.p_realname = value;
                    }
                }
                public Server hostServer
                {
                    get
                    {
                        return this.p_hostServer;
                    }
                    set
                    {
                        this.p_hostServer = value;
                    }
                }
                public string identNick
                {
                    get
                    {
                        return this.p_identNick;
                    }
                    set
                    {
                        this.p_identNick = value;
                    }
                }
                public string modes
                {
                    get
                    {
                        return this.p_modes;
                    }
                    set
                    {
                        this.p_modes = value;
                    }
                }
                public ChannelsList channels
                {
                    get
                    {
                        return this.p_channels;
                    }
                }
                public int time
                {
                    get
                    {
                        return this.p_time;
                    }
                    set
                    {
                        this.p_time = value;
                    }
                }
				protected override void Dispose (bool disposing)
				{
					try
					{
						//Deal with the channels
						while (p_channels.Count > 0)
						{
                            if (p_channels[0].channelMembers.Count == 1 && p_channels[0].channelMembers.Contains(this))
							{
                                p_core.Channels.Remove(p_channels[0]);
							}
							else
							{
                                p_core.Channels[p_channels[0].name].channelMembers.Remove(p_channels[0].channelMembers[this]);
							}
							p_channels.RemoveAt(0);
						}
						//Remove ourselves from teh list0rz
						p_host = null;
						p_vHost = null;
						p_username = null;
						p_realname = null;
						p_modes = null;
						p_channels = null;
						if (p_hostServer != null && p_hostServer.users.Contains(this))
						{
							p_hostServer.users.Remove(this);
							p_hostServer = null;
						}
					}
					catch (Exception ex)
					{
						p_core.SendLogMessage("Client", "Dispose", BlackLight.Services.Error.Errors.ERROR, "Problem", "", ex.Message, ex.StackTrace);
					}
				}
				/// -----------------------------------------------------------------------------
				/// <summary>
				/// Will parse a mode string received from the server and set the proper variables
				/// </summary>
				/// <param name="modeString"></param>
				/// <remarks>
				/// </remarks>
				/// <history>
				/// 	[Caleb]	6/18/2005	Created
				/// </history>
				/// -----------------------------------------------------------------------------
				public void ParseModeSet (string modeString)
				{
					try
					{
                        string[] modeSet = modeString.Split(' ');
						bool adding = true; //Assume we are adding :\
                        foreach (char tChar in modeSet[0])
						{
							if (tChar == '-')
							{
                                adding = false;
							}
							else if (tChar == '+')
							{
                                adding = true;
							}
							else
							{
								//Stuffy here gay
								if (adding == true)
								{
									if (p_modes.IndexOf(tChar) < 0)
									{
										p_modes += tChar;
									}
								}
								else
								{
									p_modes = p_modes.Replace(tChar.ToString(), "");
								}
							}
						}
					}
					catch (Exception ex)
					{
						p_core.SendLogMessage("Client", "ParseModeSet", BlackLight.Services.Error.Errors.ERROR, "Problem", "", ex.Message, ex.StackTrace);
					}
				}
				public Client(string name, ServicesCore core) : base(name, core) {
					p_host = "";
					p_vHost = "";
					p_username = "";
					p_realname = "";
					p_identNick = "";
					p_modes = "";
					p_channels = new ChannelsList();
					
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
						if (((IRCUser)(a[idx])).name.ToLower() == name.ToLower())
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
						if (((ChanMember)(a[idx])).user == tUser)
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
						if (((ChanMember)(a[idx])).user.name.ToLower() == name.ToLower())
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
						if (((Channel)(a[idx])).name.ToLower() == name.ToLower())
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
					p_text = "";
					p_setBy = "";
					
				}
				private string p_text;
				private string p_setBy;
				private int p_time;
                public string text
                {
                    get
                    {
                        return p_text;
                    }
                    set
                    {
                        p_text = value;
                    }
                }
                public string setBy
                {
                    get
                    {
                        return p_setBy;
                    }
                    set
                    {
                        p_setBy = value;
                    }
                }
                public int time
                {
                    get
                    {
                        return p_time;
                    }
                    set
                    {
                        p_time = value;
                    }
                }
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
				protected Client p_user;
				private string p_modes;
                public string modes
                {
                    get
                    {
                        return p_modes;
                    }
                    set
                    {
                        p_modes = value;
                    }
                }
                public Client user
                {
                    get
                    {
                        return p_user;
                    }
                }
				private ServicesCore p_core;
				public ChanMember(Client user, ServicesCore core) {
					this.p_modes = "";
					
					this.p_user = user;
					this.p_core = core;
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
				public void removeMode(char mode)
				{
					try
					{
						if (p_modes.IndexOf(mode) >= 0)
						{
                            p_modes.Replace(mode.ToString(), "");
						}
					}
					catch (Exception ex)
					{
						p_core.SendLogMessage("ChanMember", "RemoveMode", BlackLight.Services.Error.Errors.ERROR, "Problem", "", ex.Message, ex.StackTrace);
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
				public void addMode(char mode)
				{
					try
					{
                        if (p_modes.IndexOf(mode) < 0)
						{
                            p_modes += mode;
						}
					}
					catch (Exception ex)
					{
						p_core.SendLogMessage("ChanMember", "AddMode", BlackLight.Services.Error.Errors.ERROR, "Problem", "", ex.Message, ex.StackTrace);
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
                    return (p_modes.IndexOf(p_core.MyIRCd.OwnerMode) >= 0);
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
                    return (p_modes.IndexOf(p_core.MyIRCd.AdminMode) >= 0);
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
                    return (p_modes.IndexOf(p_core.MyIRCd.OpMode) >= 0);
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
                    return (p_modes.IndexOf(p_core.MyIRCd.HalfOpMode) >= 0);
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
                    return (p_modes.IndexOf(p_core.MyIRCd.VoiceMode) >= 0);
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
                    return (p_modes.IndexOf(p_core.MyIRCd.OwnerMode) < 0)
                           && (p_modes.IndexOf(p_core.MyIRCd.AdminMode) < 0)
                           && (p_modes.IndexOf(p_core.MyIRCd.OpMode) < 0)
                           && (p_modes.IndexOf(p_core.MyIRCd.HalfOpMode) < 0)
                           && (p_modes.IndexOf(p_core.MyIRCd.VoiceMode) < 0);
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
				protected string p_name;
				protected ServicesCore p_core;
				private string p_modes;
                private Topic p_topic;
                private ChanMembers p_channelMembers;
				public Channel(string name, ServicesCore core) {
					this.p_modes = "";
					this.p_topic = new Topic();
					this.p_channelMembers = new ChanMembers();
					
					this.p_name = name;
					this.p_core = core;
				}
                public string name
                {
                    get
                    {
                        return p_name;
                    }
                }
                public string modes
                {
                    get
                    {
                        return p_modes;
                    }
                    set
                    {
                        p_modes = value; 
                    }
                }
                public Topic topic
                {
                    get
                    {
                        return p_topic;
                    }
                    set
                    {
                        p_topic = value;
                    }
                }
                public ChanMembers channelMembers
                {
                    get
                    {
                        return this.p_channelMembers;
                    }
                }
				/// -----------------------------------------------------------------------------
				/// <summary>
				/// Parsed mode strings for channel
				/// </summary>
				/// <param name="modeString"></param>
				/// <remarks>
				/// </remarks>
				/// <history>
				/// 	[Caleb]	6/18/2005	Created
				/// </history>
				/// -----------------------------------------------------------------------------
				public void parseModeSet (string modeString)
				{
					try
					{
                        string[] modeSet = modeString.Split(' ');
						p_core.SendLogMessage("Channel", "ParseModeSet", BlackLight.Services.Error.Errors.DEBUG, "Running", modeString, "", "");
						p_core.SendLogMessage("Channel", "ParseModeSet", BlackLight.Services.Error.Errors.DEBUG, "String?", modeSet[0], "", "");
						short lastModeParamIndex = 1;
						bool adding = true; //Lets assume we are adding :\
                        foreach (char tChar in modeSet[0])
						{
							if (tChar == '-')
							{
                                adding = false;
							}
							else if (tChar == '+')
							{
                                adding = true;
							}
							else
							{
								p_core.SendLogMessage("Channel", "ParseModeSet", BlackLight.Services.Error.Errors.DEBUG, "Adding?", adding.ToString(), "", "");
								//Going by what is most common to save time
								//First status modes
								if (p_core.MyIRCd.StatusModes.IndexOf(tChar) >= 0)
								{
									p_core.SendLogMessage("Channel", "ParseModeSet", BlackLight.Services.Error.Errors.DEBUG, "Status", "", "", "");
									//I don't like this but meh
                                    if (modeSet.GetUpperBound(0) >= lastModeParamIndex)
									{
										if (adding == true)
										{
                                            p_channelMembers[modeSet[lastModeParamIndex]].addMode(tChar);
										}
										else
										{
                                            p_channelMembers[modeSet[lastModeParamIndex]].removeMode(tChar);
										}
                                        lastModeParamIndex += 1;
									}
									
								}
                                //Next Access Modes
								else if (p_core.MyIRCd.AccessModes.IndexOf(tChar) >= 0)
								{
									p_core.SendLogMessage("Channel", "ParseModeSet", BlackLight.Services.Error.Errors.DEBUG, "Access", "", "", "");
								}
                                //Then Param Modes
                                else if (p_core.MyIRCd.ParamModes.IndexOf(tChar) >= 0)
								{
                                    p_core.SendLogMessage("Channel", "ParseModeSet", BlackLight.Services.Error.Errors.DEBUG, "Param", "", "", "");
									// ContainsExtend = True
									// Extended &= tchar
									//Now this isn't really right this should probably
									//be after status modes, but do I really want to sacrafice a clock cycle if it isn't?
									//we will see...
								}
								else
								{
                                    p_core.SendLogMessage("Channel", "ParseModeSet", BlackLight.Services.Error.Errors.DEBUG, "Normal", "", "", "");
									if (adding == true)
									{
										if (p_modes.IndexOf(tChar) < 0)
										{
											p_modes += tChar;
										}
									}
									else
									{
										p_modes = p_modes.Replace(tChar.ToString(), "");
									}
								}
							}
						}
					}
					catch (Exception ex)
					{
						p_core.SendLogMessage("Channel", "ParseModeSet", BlackLight.Services.Error.Errors.ERROR, "Problem", "", ex.Message, ex.StackTrace);
					}
				}
				
			}
			#endregion
		}
	}
}
