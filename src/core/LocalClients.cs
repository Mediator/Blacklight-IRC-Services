using System.Diagnostics;
using System;
using System.Data;
using System.Collections;
using BlackLight;
using BlackLight.Services.Core;
using BlackLight.Services.Nodes;
using BlackLight.Services.Error;

namespace BlackLight
{
	namespace Services
	{
		namespace Nodes
		{
			
			/// -----------------------------------------------------------------------------
			/// <summary>
			/// Used for local clients call back if they receive a PRIVMSG
			/// </summary>
			/// <remarks>
			/// </remarks>
			/// <history>
			/// 	[Caleb]	6/18/2005	Created
			/// </history>
			/// -----------------------------------------------------------------------------
			public delegate void MessageCallBack(Client Source, string[] Message);
			/// -----------------------------------------------------------------------------
			/// <summary>
			/// This will be the callback for if the client receives a command
			/// </summary>
			/// <remarks>
			/// </remarks>
			/// <history>
			/// 	[Caleb]	6/18/2005	Created
			/// </history>
			/// -----------------------------------------------------------------------------
			public delegate void CommandCallBack(Client Source, string[] Args);
			
			
			/// -----------------------------------------------------------------------------
			/// Project	 : BlackLight Services
			/// Class	 : LocalClient
			///
			/// -----------------------------------------------------------------------------
			/// <summary>
			/// Class to for an instance of a local client
			/// </summary>
			/// <remarks>
			/// </remarks>
			/// <history>
			/// 	[Caleb]	6/18/2005	Created
			/// </history>
			/// -----------------------------------------------------------------------------
			public class LocalClient : Client
			{
				// Public Host As String = ""
				//   Public Username As String = ""
				//   Public Realname As String = ""
				//   Public Modes As String = ""
				//  Public Time As Integer
				public CommandList Cmds;
				public MessageCallBack OnMsg;
				public void Send_Notice (string Dest, string Message)
				{
					base.p_core.Commands.Send_Notice(base.name, Dest, Message);
				}
				public void Send_PrivMSG (string Dest, string Message)
				{
					base.p_core.Commands.Send_PrivMSG(base.name, Dest, Message);
				}
				public void Kill_Client (string Dest, string Message)
				{
					base.p_core.Commands.Send_Kill(base.name, Dest, Message);
					base.p_core.Raise_Quit(Dest, Message);
					Client tClient = base.p_core.GetClient(Dest);
					if (tClient != null)
					{
						tClient.Dispose();
					}
				}
				public void Quit (string Reason)
				{
					base.p_core.Commands.Send_Quit(base.name, Reason);
					this.Dispose();
				}
				public void gLine_Client (string Dest, string Host, int ExpireTime, string Reason)
				{
					base.p_core.Commands.Send_gLine(base.name, Host, ExpireTime, Reason);
				}
				public void zLine_Client (string Dest, string Host, int ExpireTime, string Reason)
				{
					base.p_core.Commands.Send_zLine(base.name, Host, ExpireTime, Reason);
				}
				public void Join_Channel (string Channel, string Key)
				{
					int tChannelIndex;
					base.p_core.Commands.Join_Channel(base.name, Channel, Key);
					if (base.p_core.Channels.Contains(Channel) == false)
					{
						tChannelIndex = base.p_core.Channels.Add(new Channel(Channel, base.p_core));
					}
					else
					{
						tChannelIndex = base.p_core.Channels.IndexOf(Channel);
					}
					//RaiseEvent onDebug("JOIN-Join Nick: " & tUser.Nick & " Channel: " & tMatch.Groups("CHANNEL").Value)
					base.p_core.Channels[tChannelIndex].channelMembers.Add(new ChanMember(this, base.p_core));
					base.channels.Add(base.p_core.Channels[tChannelIndex]);
					tChannelIndex = 0;
				}
				protected override void Dispose (bool disposing)
				{
					
					p_host = null;
					p_username = null;
					p_realname = null;
					p_modes = null;
					if (base.p_core.LocalClients.Contains(this))
					{
						base.p_core.LocalClients.Remove(this);
					}
					else
					{
						base.p_core.SendLogMessage("LocalClients", "Dispose", BlackLight.Services.Error.Errors.DEBUG, "Client list does not contain me", this.name, "", "");
					}
				}
				
				public LocalClient(string tName, MessageCallBack tOnMsg, ServicesCore tBase) : base(tName, tBase) {
					Cmds = new CommandList();
					
					OnMsg = tOnMsg;
				}
				/// -----------------------------------------------------------------------------
				/// <summary>
				/// Called by local client when they receive a privmsg. This will determin
				/// whether or not the privmsg is a command and if so it will execute it
				/// </summary>
				/// <param name="Source">Source of privmsg</param>
				/// <param name="Cmd">The privmsg message in an array</param>
				/// <remarks>
				///
				/// </remarks>
				/// <history>
				/// 	[Caleb]	6/18/2005	Created
				/// </history>
				/// -----------------------------------------------------------------------------
				public bool CmdExec(Client Source, string[] Cmd)
				{
					try
					{
						
						if (Cmds.Contains(Cmd[0]))
						{
							
							string[] tCmdArray = null;;
							if (Cmd.GetUpperBound(0) > 0)
							{
								tCmdArray = new string[Cmd.GetUpperBound(0)-1 + 1];
								for (short x = 1; x <= Convert.ToInt16(Cmd.GetUpperBound(0)); x++)
								{
									tCmdArray[x - 1] = Cmd[x].Trim();
								}
							}
							Cmds[Cmd[0]](Source, tCmdArray);
							return true;
						}
						else
						{
							return false;
						}
					}
					catch (Exception ex)
					{
						base.p_core.SendLogMessage("LocalClients", "CmdExec", BlackLight.Services.Error.Errors.ERROR, "Problem executing actual command", "", ex.Message, ex.StackTrace);
						return false;
					}
				}
			}
			
			
			/// -----------------------------------------------------------------------------
			/// Project	 : BlackLight Services
			/// Class	 : LocalClientsList
			///
			/// -----------------------------------------------------------------------------
			/// <summary>
			/// Container of localclients
			/// </summary>
			/// <remarks>
			/// </remarks>
			/// <history>
			/// 	[Caleb]	6/18/2005	Created
			/// </history>
			/// -----------------------------------------------------------------------------
			public sealed class LocalClientsList : IList, ICollection, IEnumerable
			{
				
				readonly ArrayList a;
				private ServicesCore MyCore;
				public LocalClientsList(ServicesCore tBase) {
					a = new ArrayList();
					MyCore = tBase;
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
					return Add((LocalClient)value);
				}
				private int Add(LocalClient value)
				{
					return a.Add(value);
				}
				public void Clear ()
				{
					a.Clear();
				}
				bool IList.Contains(object value)
				{
					return Contains((LocalClient)value);
				}
				public bool Contains(LocalClient value)
				{
					return a.Contains(value);
				}
				public bool Contains(string name)
				{
					return IndexOf(name) >= 0;
				}
				int IList.IndexOf(object value)
				{
					return IndexOf((LocalClient)value);
				}
				public int IndexOf(LocalClient value)
				{
					return a.IndexOf(value);
				}
				public int IndexOf(string name)
				{
					for (int idx = 0; idx <= a.Count - 1; idx++)
					{
						if (((LocalClient)(a[idx])).name.ToLower() == name.ToLower())
						{
							return idx;
						}
					}
					return - 1;
				}
				void IList.Insert (int index, object value)
				{
					Insert(index, (LocalClient)value);
				}
				public void Insert (int index, LocalClient value)
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
						return ((LocalClient)(a[index]));
					}
					set
					{
						a[index] = value;
					}
				}
				public LocalClient this[int index]
				{
					get{
						return ((LocalClient)(a[index]));
					}
					set
					{
						a[index] = value;
					}
				}
				public LocalClient this[string name]
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
					Remove((LocalClient)value);
				}
				public void Remove (LocalClient value)
				{
					a.Remove(value);
				}
				public void RemoveAt (int index)
				{
					a.RemoveAt(index);
				}
				public int AddClient(LocalClient tClient)
				{
					if (MyCore.Connected())
					{
						MyCore.Commands.Send_Connect(tClient);
					}
					return Add(tClient);
				}
			}
			
			
			public sealed class CommandList : IDictionary
			{
				public CommandList()
				{
					a = new Hashtable();
					
				}
				readonly Hashtable a;
				public void CopyTo (Array array, int index)
				{
					a.CopyTo(array, index);
				}
				void IDictionary.Add (object key, object value)
				{
					Add(((string)key).ToUpper(), (CommandCallBack)value);
				}
				public void Add (string key, CommandCallBack value)
				{
					a.Add(key.ToUpper(), value);
				}
				void IDictionary.Remove (object cmd)
				{
					Remove((string)cmd);
				}
				public void Remove (string cmd)
				{
					a.Remove(cmd.ToUpper());
				}
				object IDictionary.this[object key]
				{
					get
					{
						return ((CommandCallBack) a[((string)key).ToUpper()]);
					}
					set
					{
						a[((string)key).ToUpper()] = value;
					}
				}
				public CommandCallBack this[string key]
				{
					get{
						return ((CommandCallBack) a[key.ToUpper()]);
					}
					set
					{
						a[key.ToUpper()] = value;
					}
				}
				public System.Collections.IDictionaryEnumerator GetEnumerator()
				{
					return a.GetEnumerator();
				}
				System.Collections.IEnumerator IEnumerable.GetEnumerator()
				{
					return a.GetEnumerator();
				}
				
				bool IDictionary.IsFixedSize
				{
					get{
						return false;
					}
				}
				bool IDictionary.IsReadOnly
				{
					get{
						return false;
					}
				}
				bool IDictionary.Contains(object key)
				{
					return Contains((string)key);
				}
				public bool Contains(string key)
				{
					return a.Contains(key.ToUpper());
				}
				public System.Collections.ICollection Keys
				{
					get{
						return a.Keys;
					}
				}
				public void Clear ()
				{
					a.Clear();
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
				public System.Collections.ICollection Values
				{
					get{
						return a.Values;
					}
				}
			}
			
		}
	}
	
}
