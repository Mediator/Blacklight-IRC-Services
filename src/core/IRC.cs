using System.Diagnostics;
using System;
using System.Data;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.Text;
using BlackLight;
using BlackLight.Services.Error;
using System.Threading;


namespace BlackLight
{
	namespace Services
	{
		namespace Core
		{
			
			
			/// -----------------------------------------------------------------------------
			/// Project	 : BlackLight Services
			/// Class	 : DataObject
			///
			/// -----------------------------------------------------------------------------
			/// <summary>
			/// Class used internally by the socket methods
			/// </summary>
			/// <remarks>
			/// </remarks>
			/// <history>
			/// 	[Caleb]	6/18/2005	Created
			/// </history>
			/// -----------------------------------------------------------------------------
			public class DataObject
			{
				public DataObject()
				{
					WorkSocket = null;
					BufferSize = 32767;
					Buffer = new byte[32768];
					StrBuilder = new StringBuilder();
					
				}
				public Socket WorkSocket;
				public short BufferSize;
				public byte[] Buffer;
				public StringBuilder StrBuilder;
			}
			
			/// -----------------------------------------------------------------------------
			/// Project	 : BlackLight Services
			/// Class	 : IRC
			///
			/// -----------------------------------------------------------------------------
			/// <summary>
			/// Socket base and command parsing
			/// </summary>
			/// <remarks>
			/// </remarks>
			/// <history>
			/// 	[Caleb]	6/18/2005	Created
			/// </history>
			/// -----------------------------------------------------------------------------
			public abstract class IRC
			{
				
				/// -----------------------------------------------------------------------------
				/// <summary>
				/// Raised one connect
				/// </summary>
				/// <param name="tString"></param>
				/// <remarks>
				/// </remarks>
				/// <history>
				/// 	[Caleb]	6/18/2005	Created
				/// </history>
				/// -----------------------------------------------------------------------------
				public delegate void OnConnectEventHandler(string tString);
				private OnConnectEventHandler OnConnectEvent;
				public const string CRLF = "\r\n";
					public const string CR = "\r";
						public const string LF = "\n";
				public event OnConnectEventHandler OnConnect
				{
					add
					{
						OnConnectEvent = (OnConnectEventHandler) System.Delegate.Combine(OnConnectEvent, value);
					}
					remove
					{
						OnConnectEvent = (OnConnectEventHandler) System.Delegate.Remove(OnConnectEvent, value);
					}
				}
				
				/// -----------------------------------------------------------------------------
				/// <summary>
				/// Raised when data arrives
				/// </summary>
				/// <param name="Data"></param>
				/// <param name="TotalBytes"></param>
				/// <remarks>
				/// </remarks>
				/// <history>
				/// 	[Caleb]	6/18/2005	Created
				/// </history>
				/// -----------------------------------------------------------------------------
				public delegate void onDataArrivalEventHandler(byte[] Data, int TotalBytes);
				private onDataArrivalEventHandler onDataArrivalEvent;
				
				public event onDataArrivalEventHandler onDataArrival
				{
					add
					{
						onDataArrivalEvent = (onDataArrivalEventHandler) System.Delegate.Combine(onDataArrivalEvent, value);
					}
					remove
					{
						onDataArrivalEvent = (onDataArrivalEventHandler) System.Delegate.Remove(onDataArrivalEvent, value);
					}
				}
				
				/// -----------------------------------------------------------------------------
				/// <summary>
				/// Raised when the socket has been disconnected
				/// </summary>
				/// <remarks>
				/// </remarks>
				/// <history>
				/// 	[Caleb]	6/18/2005	Created
				/// </history>
				/// -----------------------------------------------------------------------------
				public delegate void onDisconnectEventHandler();
				private onDisconnectEventHandler onDisconnectEvent;
				
				public event onDisconnectEventHandler onDisconnect
				{
					add
					{
						onDisconnectEvent = (onDisconnectEventHandler) System.Delegate.Combine(onDisconnectEvent, value);
					}
					remove
					{
						onDisconnectEvent = (onDisconnectEventHandler) System.Delegate.Remove(onDisconnectEvent, value);
					}
				}
				
				/// -----------------------------------------------------------------------------
				/// <summary>
				/// Raised when a data send is completed
				/// </summary>
				/// <param name="DataSize"></param>
				/// <remarks>
				/// </remarks>
				/// <history>
				/// 	[Caleb]	6/18/2005	Created
				/// </history>
				/// -----------------------------------------------------------------------------
				public delegate void onSendCompleteEventHandler(int DataSize);
				private onSendCompleteEventHandler onSendCompleteEvent;
				
				public event onSendCompleteEventHandler onSendComplete
				{
					add
					{
						onSendCompleteEvent = (onSendCompleteEventHandler) System.Delegate.Combine(onSendCompleteEvent, value);
					}
					remove
					{
						onSendCompleteEvent = (onSendCompleteEventHandler) System.Delegate.Remove(onSendCompleteEvent, value);
					}
				}
				
				
				private string m_SocketID;
				private Socket m_tmpSocket;
				
				
				public System.Threading.Thread SocketCheck;
				private static int port = 6667;
				public string inBuffered;
				internal static IPHostEntry ipHostInfo = Dns.Resolve("localhost");
				private static IPAddress ipAddress = ipHostInfo.AddressList[0];
				
				/// -----------------------------------------------------------------------------
				/// <summary>
				/// Create the instance of the class
				/// </summary>
				/// <param name="tmp_Socket"></param>
				/// <param name="tmp_SocketID"></param>
				/// <remarks>
				/// </remarks>
				/// <history>
				/// 	[Caleb]	6/18/2005	Created
				/// </history>
				/// -----------------------------------------------------------------------------
				public IRC(Socket tmp_Socket, string tmp_SocketID) {
					
					m_SocketID = tmp_SocketID;
					m_tmpSocket = tmp_Socket;
					
					Socket obj_Socket = tmp_Socket;
					
					DataObject obj_SocketState = new DataObject();
					
					obj_SocketState.WorkSocket = obj_Socket;
					
					obj_Socket.BeginReceive(obj_SocketState.Buffer, 0, obj_SocketState.BufferSize, 0, new AsyncCallback( MyDataIn), obj_SocketState);
					
				}
				/// -----------------------------------------------------------------------------
				/// <summary>
				/// Create the instance of the class
				/// </summary>
				/// <remarks>
				/// </remarks>
				/// <history>
				/// 	[Caleb]	6/18/2005	Created
				/// </history>
				/// -----------------------------------------------------------------------------
				public IRC() {
					
					m_tmpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
					
				}
				
				/// -----------------------------------------------------------------------------
				/// <summary>
				/// Method which will begin a socket connection to specified host on specified port
				/// </summary>
				/// <param name="remotehost">Host to connect to</param>
				/// <param name="remoteport">Port to connect to</param>
				/// <remarks>
				/// </remarks>
				/// <history>
				/// 	[Caleb]	6/18/2005	Created
				/// </history>
				/// -----------------------------------------------------------------------------
				public void Connect (string remotehost, int remoteport)
				{
					try
					{
						Socket obj_Socket = m_tmpSocket;
						if (obj_Socket.Connected == false)
						{
							port = remoteport;
							ipHostInfo = Dns.Resolve(remotehost);
							ipAddress = ipHostInfo.AddressList[0];
							IPEndPoint remoteEP = new IPEndPoint(ipAddress, port);
							obj_Socket.BeginConnect(remoteEP, new AsyncCallback( MyConnect), obj_Socket);
							remotehost = null;
							remoteport = 0;
							remoteEP = null;
							obj_Socket = null;
						}
						else
						{
							return;
						}
					}
					catch (SocketException)
					{
						SendLogMessage("IRC", "Connect", BlackLight.Services.Error.Errors.ERROR, "Problem with connect", "Unable to connect to specified host", "", "");
						return;
					}
					catch (Exception ex)
					{
						SendLogMessage("IRC", "Connect", BlackLight.Services.Error.Errors.ERROR, "Problem with connect", "", ex.Message, ex.StackTrace);
						return;
					}
				}
				/// -----------------------------------------------------------------------------
				/// <summary>
				/// Used to directly send data to the host server
				/// </summary>
				/// <param name="Data">Data in string format to be sent, if its missing a CRLF one will be appended</param>
				/// <remarks>
				/// </remarks>
				/// <history>
				/// 	[Caleb]	6/18/2005	Created
				/// </history>
				/// -----------------------------------------------------------------------------
				public void SendData (string Data)
				{
					try
					{
						if (m_tmpSocket.Connected == true)
						{
							SendLogMessage("IRC", "SendData", BlackLight.Services.Error.Errors.WARNING, "Sending: " + Data, "", "", "");
							if (Data.Substring(Data.Length - 2, 2)!= "\r\n")
							{
								Data += "\r\n";
							}
							
							DataObject obj_StateObject = new DataObject();
							
							obj_StateObject.WorkSocket = m_tmpSocket;
							
							byte[] Buffer = Encoding.ASCII.GetBytes(Data);
							try
							{
								m_tmpSocket.BeginSend(Buffer, 0, Buffer.Length, 0, new AsyncCallback( MySent), obj_StateObject);
							}
							catch (Exception ex)
							{
								if (m_tmpSocket.Connected == true)
								{
									SendLogMessage("IRC", "SendData", BlackLight.Services.Error.Errors.ERROR, "Problem with sending data", "", ex.Message, ex.StackTrace);
								}
								else
								{
									SendLogMessage("IRC", "SendData", BlackLight.Services.Error.Errors.DEBUG, "Socket was closed while attempting to send data", "", "", "");
								}
							}
							Data = null;
							Buffer = null;
							obj_StateObject = null;
						}
						else
						{
							SendLogMessage("IRC", "SendData", BlackLight.Services.Error.Errors.DEBUG, "Attempted to send but failed due to not being connected: " + Data, "", "", "");
							return;
						}
					}
					catch (Exception ex)
					{
						SendLogMessage("IRC", "SendData", BlackLight.Services.Error.Errors.ERROR, "Problem with sending data", "", ex.Message, ex.StackTrace);
						return;
					}
				}
				
				/// -----------------------------------------------------------------------------
				/// <summary>
				/// Will close the socket and disconnect from the server
				/// </summary>
				/// <remarks>
				/// </remarks>
				/// <history>
				/// 	[Caleb]	6/18/2005	Created
				/// </history>
				/// -----------------------------------------------------------------------------
				public void Disconnect ()
				{
					try
					{
						SendLogMessage("IRC", "Disconnect", BlackLight.Services.Error.Errors.DEBUG, "Disconnecting", "", "", "");
						m_tmpSocket.Shutdown(SocketShutdown.Both);
						if (onDisconnectEvent != null)
							onDisconnectEvent();
					}
					catch (Exception ex)
					{
						SendLogMessage("IRC", "Disconnect", BlackLight.Services.Error.Errors.ERROR, "Problem with disconnection", "", ex.Message, ex.StackTrace);
						return;
					}
					try
					{
						m_tmpSocket.Close();
					}
					catch (Exception ex)
					{
						SendLogMessage("IRC", "Disconnect", BlackLight.Services.Error.Errors.ERROR, "Problem closing the socket", "", ex.Message, ex.StackTrace);
					}
				}
				/// -----------------------------------------------------------------------------
				/// <summary>
				/// Callback for connection
				/// </summary>
				/// <param name="ar"></param>
				/// <remarks>
				/// </remarks>
				/// <history>
				/// 	[Caleb]	6/18/2005	Created
				/// </history>
				/// -----------------------------------------------------------------------------
				private void MyConnect (IAsyncResult ar)
				{
					try
					{
						//RaiseEvent OnSomething("Found Connection")
						m_tmpSocket = ((Socket) ar.AsyncState);
						
						m_tmpSocket.EndConnect(ar);
						
						if (m_tmpSocket.Connected == true)
						{
							Socket obj_Socket = m_tmpSocket;
							
							DataObject obj_SocketState = new DataObject();
							
							obj_SocketState.WorkSocket = obj_Socket;
							
							obj_Socket.BeginReceive(obj_SocketState.Buffer, 0, obj_SocketState.BufferSize, 0, new AsyncCallback( MyDataIn), obj_SocketState);
							if (OnConnectEvent != null)
								OnConnectEvent("");
							// SocketCheck = New Threading.Thread(AddressOf AmConnected)
							//SocketCheck.Start()
							obj_SocketState = null;
							obj_Socket = null;
							ar = null;
							// GC.Collect()
						}
						else
						{
							SendLogMessage("IRC", "MyConnect", BlackLight.Services.Error.Errors.ERROR, "Recieved call back, but not connected", "", "", "");
							return;
						}
					}
					catch (Exception ex)
					{
						SendLogMessage("IRC", "MyConnect", BlackLight.Services.Error.Errors.ERROR, "Problem with connection callback", "", ex.Message, ex.StackTrace);
						return;
					}
				}
				/// -----------------------------------------------------------------------------
				/// <summary>
				/// Used to convert a bytes array to a commaon string
				/// </summary>
				/// <param name="Data">Bytes array to be converted</param>
				/// <param name="tLen">Number of bytes to convert</param>
				/// <returns></returns>
				/// <remarks>
				/// </remarks>
				/// <history>
				/// 	[Caleb]	6/18/2005	Created
				/// </history>
				/// -----------------------------------------------------------------------------
				public string BytestoString(byte[] Data, int tLen)
				{
					try
					{
						return System.Text.ASCIIEncoding.ASCII.GetString(Data).Substring(0,tLen);
//						Data = null;
//						tLen = null;
					}
					catch (Exception ex)
					{
						SendLogMessage("IRC", "BytesToString", BlackLight.Services.Error.Errors.ERROR, "Problem with converting bytes to string", "", ex.Message, ex.StackTrace);
					return null;
					}
				}
				
				/// -----------------------------------------------------------------------------
				/// <summary>
				/// Used by sockets to parse incomming data
				/// </summary>
				/// <param name="data"></param>
				/// <param name="bytesRead"></param>
				/// <remarks>
				/// </remarks>
				/// <history>
				/// 	[Caleb]	6/18/2005	Created
				/// </history>
				/// -----------------------------------------------------------------------------
				protected void DataParse (byte[] data, int bytesRead)
				{
					object synclockObject = typeof(IRC);
					Monitor.Enter(synclockObject);
					try
					{
						object synclockObject2 = typeof(Socket);
						Monitor.Enter(synclockObject2);
						try
						{
							object synclockObject3 = typeof(AsyncCallback);
							Monitor.Enter(synclockObject3);
							try
							{
								try
								{
									string inData = BytestoString(data, bytesRead);
									string tCmd;
									inData = inData.Replace("\r\n", IRC.CR);
									inData = inData.Replace("\0", "");
									inBuffered += inData;
									inData = null;
									data = null;
									bytesRead = 0;
									while (inBuffered.IndexOf(IRC.CR) >= 0)
									{
										
										tCmd = inBuffered.Split(IRC.CR.ToCharArray(),2)[0];
										inBuffered = inBuffered.Split(IRC.CR.ToCharArray(), 2)[1];
										//if ((tCmd.IndexOf(" EOS") >= 0) || (tCmd.IndexOf(" NETINFO") >= 0))
										CommandExec(tCmd.TrimStart());
									}
									
									tCmd = null;
								}
								catch (Exception ex)
								{
									SendLogMessage("IRC", "DataParse", BlackLight.Services.Error.Errors.ERROR, "Unable to add to command que", "", ex.Message, ex.StackTrace);
								}
							}
							finally
							{
								Monitor.Exit(synclockObject3);
							}
						}
						finally
						{
							Monitor.Exit(synclockObject2);
						}
					}
					finally
					{
						Monitor.Exit(synclockObject);
					}
				}
				/// -----------------------------------------------------------------------------
				/// <summary>
				/// Used by a thead to constantly verify the connection
				/// </summary>
				/// <remarks>
				/// </remarks>
				/// <history>
				/// 	[Caleb]	6/18/2005	Created
				/// </history>
				/// -----------------------------------------------------------------------------
				private void AmConnected ()
				{
					SendLogMessage("IRC", "AmConnected", BlackLight.Services.Error.Errors.DEBUG, "Running worker thread", "", "", "");
					return;
//					bool ConsiderConnected = true;
//					while (ConsiderConnected == true)
//					{
//						if (m_tmpSocket.Connected == false)
//						{
//							ConsiderConnected = false;
							
//							SendLogMessage("IRC", "AmConnected", BlackLight.Services.Error.Errors.DEBUG, "Lost Connection to remote host", "", "", "");
//							}
							//RaiseEvent OnSomething("Connection Status: " & m_tmpSocket.Connected)
//							System.Threading.Thread.CurrentThread.Sleep(2000);
//							}
						}
						protected abstract void CommandExec (string tCmd);
						/// -----------------------------------------------------------------------------
						/// <summary>
						/// Converts a string array to a string
						/// </summary>
						/// <param name="tArray">Array to be converted</param>
						/// <param name="delimiter">Delemeter of array</param>
						/// <param name="sIndex">Starting index</param>
						/// <returns></returns>
						/// <remarks>
						/// </remarks>
						/// <history>
						/// 	[Caleb]	6/18/2005	Created
						/// </history>
						/// -----------------------------------------------------------------------------
						public string ArrayToLine(string[] tArray, string delimiter, int sIndex)
						{
							try
							{
								string tString = "";
								int x;
								for (x = sIndex; x <= tArray.GetUpperBound(0); x++)
								{
									tString += tArray[x] + delimiter;
								}
								tString = tString.Substring(0, tString.Length- 1);
								return tString;
							}
							catch (Exception ex)
							{
								SendLogMessage("IRC", "ArrayToLine", BlackLight.Services.Error.Errors.ERROR, "Problem converting an array to a line", "", ex.Message, ex.StackTrace);
							return "";
							}
						}
						
						protected abstract void MyDataIn (IAsyncResult ar);
						public abstract void SendLogMessage (string Origin, string Method, BlackLight.Services.Error.Errors Type, string Message, string Extra, string Exception, string Trace);
						
						/// -----------------------------------------------------------------------------
						/// <summary>
						/// Call back method for when data is send
						/// </summary>
						/// <param name="ar"></param>
						/// <remarks>
						/// </remarks>
						/// <history>
						/// 	[Caleb]	6/18/2005	Created
						/// </history>
						/// -----------------------------------------------------------------------------
						private void MySent (IAsyncResult ar)
						{
							try
							{
								DataObject obj_SocketState = ((DataObject) ar.AsyncState);
								Socket obj_Socket = obj_SocketState.WorkSocket;
								if (onSendCompleteEvent != null)
									onSendCompleteEvent(0);
								obj_SocketState = null;
								obj_Socket = null;
							}
							catch (Exception ex)
							{
								SendLogMessage("IRC", "MySent", BlackLight.Services.Error.Errors.ERROR, "Problem with data sending callback", "", ex.Message, ex.StackTrace);
								return;
							}
						}
						/// -----------------------------------------------------------------------------
						/// <summary>
						/// Function to verify whether or not the socket is still connected
						/// </summary>
						/// <returns></returns>
						/// <remarks>
						/// </remarks>
						/// <history>
						/// 	[Caleb]	6/18/2005	Created
						/// </history>
						/// -----------------------------------------------------------------------------
						public bool Connected()
						{
							try
							{
								return m_tmpSocket.Connected;
							}
							catch (Exception ex)
							{
								SendLogMessage("IRC", "Connected", BlackLight.Services.Error.Errors.ERROR, "Problem with connection status check", "", ex.Message, ex.StackTrace);
								return false;
							}
						}
						
						
						
						protected virtual void Event_onDataArrival (byte[] Data, int TotalBytes)
						{
							if (onDataArrivalEvent != null)
								onDataArrivalEvent(Data, TotalBytes);
						}
						
						protected virtual void Event_onDisconnect ()
						{
							if (onDisconnectEvent != null)
								onDisconnectEvent();
						}
						
						protected virtual void Event_onSendComplete (int DataSize)
						{
							if (onSendCompleteEvent != null)
								onSendCompleteEvent(DataSize);
						}
						
					}
				}
			}
		}
