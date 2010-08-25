using System.Diagnostics;
using System;
using System.Data;
using System.Collections;
using System.IO;
using System.Text.RegularExpressions;
using System.Text;

namespace BlackLight
{
	namespace Services
	{
		namespace Config
		{
			
			/// -----------------------------------------------------------------------------
			/// Project	 : BlackLight Services
			/// Class	 : ConfigurationReturn
			///
			/// -----------------------------------------------------------------------------
			/// <summary>
			/// A completely loaded configuration
			/// </summary>
			/// <remarks>
			/// </remarks>
			/// <history>
			/// 	[Caleb]	12/1/2005	Created
			/// </history>
			/// -----------------------------------------------------------------------------
			public class ConfigurationReturn
			{
				public ConfigurationReturn()
				{
					Data = new ArrayList();
					
				}
				public bool Loaded;
				public ArrayList Data;
			}
			/// -----------------------------------------------------------------------------
			/// Project	 : BlackLight Services
			/// Class	 : Config
			///
			/// -----------------------------------------------------------------------------
			/// <summary>
			/// Configuration management
			/// </summary>
			/// <remarks>
			/// </remarks>
			/// <history>
			/// 	[Caleb]	12/1/2005	Created
			/// </history>
			/// -----------------------------------------------------------------------------
			public class ConfigHandler
			{
				public ConfigHandler()
				{
					Configuration = new ConfigurationList();
					
				}
				/// -----------------------------------------------------------------------------
				/// <summary>
				/// The active list of configuration parameters
				/// </summary>
				/// <remarks>
				/// </remarks>
				/// <history>
				/// 	[Caleb]	12/1/2005	Created
				/// </history>
				/// -----------------------------------------------------------------------------
				public ConfigurationList Configuration;
				/// -----------------------------------------------------------------------------
				/// <summary>
				/// Method loads a file into memory and parses the directives
				/// </summary>
				/// <param name="file">File with full path containing configuration data</param>
				/// <returns>Method returns a fully parsed configuration</returns>
				/// <remarks>
				/// </remarks>
				/// <history>
				/// 	[Caleb]	12/1/2005	Created
				/// </history>
				/// -----------------------------------------------------------------------------
				public ConfigurationReturn Load(string file)
				{
					try
					{
						string FileData = "";
						//System.IO.File tFile;
						System.IO.StreamReader tFileStream;
						if (System.IO.File.Exists(file))
						{
							tFileStream = System.IO.File.OpenText(file);
							FileData = tFileStream.ReadToEnd();
						}
						ConfigurationReturn ConfigData = new ConfigurationReturn();
						if (FileData != null)
						{
							//MsgBox("here")
							Regex multilinecomments = new Regex("/\\*[^*]*\\*+([^/*][^*]*\\*+)*/", RegexOptions.Multiline);
							Regex linecomments = new Regex("//.+$", RegexOptions.Multiline);
							Regex linecomments2 = new Regex("\\#.+$", RegexOptions.Multiline);
							Match tMatch;
							Match uMatch;
							FileData = multilinecomments.Replace(FileData, "");
							FileData = linecomments.Replace(FileData, "");
							FileData = linecomments2.Replace(FileData, "");
							// FileData = Replace(FileData, vbCrLf, vbLf)
							// FileData = Replace(FileData, vbCr, vbLf)
							//Dim tInt As Int16 = 0
							OptionsList CurrentConfigGroup = new OptionsList();
							Regex blocks = new Regex("(?<name>[\\w\\d-_]*)[\\s]*\\{(?<values>[^\\}]*)\\}\\;", RegexOptions.IgnoreCase | RegexOptions.Multiline);
							string tMatchString;
							Regex stringoptions = new Regex("(?<optionname>[\\w\\d-_]+)[\\s]*\"(?<optionvalue>[^\"]*)\"\\;", RegexOptions.IgnoreCase | RegexOptions.Multiline);
							Regex numericoptions = new Regex("(?<optionname>[\\w\\d-_]*)[\\s]*(?<optionvalue>[\\d]+)\\;", RegexOptions.IgnoreCase | RegexOptions.Multiline);
							if (FileData.IndexOf("/*") >= 0)
							{
								Configuration.Clear();
								ConfigData.Loaded = false;
								ConfigData.Data.Add("File contains comment which does not end");
								return ConfigData;
							}
							while (blocks.Matches(FileData).Count > 0)
							{
								tMatch = blocks.Matches(FileData)[0];
								tMatchString = tMatch.Groups["values"].Value;
								// MsgBox(tMatch.Groups("name").Value & " " & tMatch.Groups("values").Value)
								while (stringoptions.Matches(tMatchString).Count > 0)
								{
									uMatch = stringoptions.Matches(tMatchString)[0];
									if (CurrentConfigGroup.ContainsKey(uMatch.Groups["optionname"].Value + "1"))
									{
										//ConfigData.Data.Add(String.Format("Option ""{1}"" already exists in block ""{0}""", tMatch.Groups("name").Value, uMatch.Groups("optionname").Value))
										int tInt = 2;
										bool tExists = false;
										while (tExists == false)
										{
											if (CurrentConfigGroup.ContainsKey(uMatch.Groups["optionname"].Value + tInt))
											{
												tInt += 1;
											}
											else
											{
												tExists = true;
											}
										}
										CurrentConfigGroup.Add(uMatch.Groups["optionname"].Value + tInt, uMatch.Groups["optionvalue"].Value);
									}
									else
									{
										CurrentConfigGroup.Add(uMatch.Groups["optionname"].Value + "1", uMatch.Groups["optionvalue"].Value);
									}
									tMatchString = stringoptions.Replace(tMatchString, "", 1);
								}
								while (numericoptions.Matches(tMatchString).Count > 0)
								{
									uMatch = numericoptions.Matches(tMatchString)[0];
									if (CurrentConfigGroup.ContainsKey(uMatch.Groups["optionname"].Value + "1"))
									{
										//ConfigData.Data.Add(String.Format("Option ""{1}"" already exists in block ""{0}""", tMatch.Groups("name").Value, uMatch.Groups("optionname").Value))
										int tInt = 2;
										bool tExists = false;
										while (tExists == false)
										{
											if (CurrentConfigGroup.ContainsKey(uMatch.Groups["optionname"].Value + tInt))
											{
												tInt += 1;
											}
											else
											{
												tExists = true;
											}
										}
										CurrentConfigGroup.Add(uMatch.Groups["optionname"].Value + tInt, uMatch.Groups["optionvalue"].Value);
									}
									else
									{
										CurrentConfigGroup.Add(uMatch.Groups["optionname"].Value + "1", uMatch.Groups["optionvalue"].Value);
									}
									tMatchString = numericoptions.Replace(tMatchString, "", 1);
								}
								tMatchString = tMatchString.Replace(Core.IRC.CRLF, "");
								tMatchString = tMatchString.Replace(Core.IRC.LF, "");
								tMatchString = tMatchString.Replace(Core.IRC.CR, "");
								tMatchString = tMatchString.Trim();
								if (tMatchString.Length > 0)
								{
									Configuration.Clear();
									ConfigData.Loaded = false;
									ConfigData.Data.Add("Block contains erronous text \"" + tMatch.Groups["name"].Value + "\"");
									return ConfigData;
								}
								else
								{
									if (Configuration.ContainsKey(tMatch.Groups["name"].Value + "1"))
									{
										int tInt = 2;
										bool tExists = false;
										while (tExists == false)
										{
											if (Configuration.ContainsKey(tMatch.Groups["name"].Value + tInt))
											{
												tInt += 1;
											}
											else
											{
												tExists = true;
											}
										}
										// ConfigData.Data.Add(String.Format("Block ""{0}"" already exists", tMatch.Groups("name").Value))
										Configuration.Add(tMatch.Groups["name"].Value + tInt, CurrentConfigGroup);
									}
									else
									{
										Configuration.Add(tMatch.Groups["name"].Value + "1", CurrentConfigGroup);
									}
									CurrentConfigGroup = new OptionsList();
								}
								FileData = blocks.Replace(FileData, "", 1);
								// MsgBox(FileData)
							}
							while (stringoptions.Matches(FileData).Count > 0)
							{
								uMatch = stringoptions.Matches(FileData)[0];
								CurrentConfigGroup.Add(uMatch.Groups["optionname"].Value, uMatch.Groups["optionvalue"].Value);
								FileData = stringoptions.Replace(FileData, "", 1);
							}
							while (numericoptions.Matches(FileData).Count > 0)
							{
								uMatch = numericoptions.Matches(FileData)[0];
								CurrentConfigGroup.Add(uMatch.Groups["optionname"].Value, uMatch.Groups["optionvalue"].Value);
								FileData = numericoptions.Replace(FileData, "", 1);
							}
							Configuration.Add("global1", CurrentConfigGroup);
							FileData = FileData.Replace(Core.IRC.CRLF, "");
							FileData = FileData.Replace(Core.IRC.LF, "");
							FileData = FileData.Replace(Core.IRC.CR, "");
							FileData = FileData.Trim();
							if (FileData.Length > 0)
							{
								Configuration.Clear();
								ConfigData.Loaded = false;
								ConfigData.Data.Add("Configuration contains erronous text");
								return ConfigData;
							}
							else
							{
								ConfigData.Loaded = true;
								return ConfigData;
							}
						}
						else
						{
							Configuration.Clear();
							ConfigData.Loaded = false;
							ConfigData.Data.Add("Unable to find configuration file");
							return ConfigData;
						}
					}
					catch (Exception ex)
					{
						throw ex;
						//MessageBox.Show("Caught Error: " + ex.Message + " " + ex.StackTrace);
						// Base.SendLogMessage("Config", "Load", Errors.LOG_ERROR, "Problem loading config", "", ex.Message, ex.StackTrace);
					}
				}
				/// -----------------------------------------------------------------------------
				/// <summary>
				/// Reload current configuration
				/// </summary>
				/// <remarks>
				/// </remarks>
				/// <history>
				/// 	[Caleb]	12/1/2005	Created
				/// </history>
				/// -----------------------------------------------------------------------------
				public void Reload ()
				{
					//TODO
				}
				
			}
			/// -----------------------------------------------------------------------------
			/// Project	 : BlackLight Services
			/// Class	 : OptionsList
			///
			/// -----------------------------------------------------------------------------
			/// <summary>
			/// A highly typed arraylist that holds config options
			/// </summary>
			/// <remarks>
			/// </remarks>
			/// <history>
			/// 	[Caleb]	12/1/2005	Created
			/// </history>
			/// -----------------------------------------------------------------------------
			public sealed class OptionsList : DictionaryBase
			{
				public string keyed(string key)
				{
					if (char.IsNumber(key[key.Length - 1]))
					{
						return key.ToUpper();
					}
					else
					{
						return key.ToUpper() + "1";
					}
				}
				public void Add (string key, string value)
				{
					Dictionary.Add(key.ToUpper(), value);
				}
				public void Remove (string key)
				{
					Dictionary.Remove(keyed(key));
				}
				public string[] GetSet(string key)
				{
					int tInt = 1;
					ArrayList tReturn = new ArrayList();
					while (Dictionary.Contains(key.ToUpper() + tInt))
					{
						tReturn.Add(Dictionary[key.ToUpper() + tInt]);
						tInt += 1;
					}
					return ((string[]) tReturn.ToArray(typeof(string)));
				}
				public string this[string key]
				{
					get{
						return System.Convert.ToString(Dictionary[keyed(key)]);
					}
					set
					{
						Dictionary[keyed(key)] = value;
					}
				}
				public bool ContainsKey(string key)
				{
					return Dictionary.Contains(keyed(key));
				}
				public System.Collections.ICollection Keys
				{
					get{
						return Dictionary.Keys;
					}
				}
				public System.Collections.ICollection Values
				{
					get{
						return Dictionary.Values;
					}
				}
			}
			//Public NotInheritable Class OptionsList
			//    Implements IList, ICollection, IEnumerable
			//    ReadOnly a As ArrayList
			//    Public Sub New()
			//        a = New ArrayList
			//    End Sub
			//    Public Sub CopyTo(ByVal array As System.Array, ByVal index As Integer) Implements System.Collections.ICollection.CopyTo
			//        a.CopyTo(array, index)
			//    End Sub
			//    Public ReadOnly Property Count() As Integer Implements System.Collections.ICollection.Count
			//        Get
			//            Return a.Count
			//        End Get
			//    End Property
			//    Public ReadOnly Property IsSynchronized() As Boolean Implements System.Collections.ICollection.IsSynchronized
			//        Get
			//            Return False
			//        End Get
			//    End Property
			//    Public ReadOnly Property SyncRoot() As Object Implements System.Collections.ICollection.SyncRoot
			//        Get
			//            Return Me
			//        End Get
			//    End Property
			//    Public Function GetEnumerator() As System.Collections.IEnumerator Implements System.Collections.IEnumerable.GetEnumerator
			//        Return a.GetEnumerator
			//    End Function
			//    Private Function Add2(ByVal value As Object) As Integer Implements System.Collections.IList.Add
			//        Return 0 ' Add(DirectCast(value, ServiceErrorResponse))
			//    End Function
			//    Public Function Add(ByVal key As String, ByVal value As String) As Integer
			//        Return a.Add(New OptionSet(key, value))
			//    End Function
			//    Public Sub Clear() Implements System.Collections.IList.Clear
			//        a.Clear()
			//    End Sub
			//    Private Function Contains2(ByVal value As Object) As Boolean Implements System.Collections.IList.Contains
			//        Return False 'Contains(DirectCast(value, ServiceErrorResponse))
			//    End Function
			//    Public Overloads Function Contains(ByVal key As String) As Boolean
			//        Return IndexOf(key) >= 0
			//    End Function
			//    Private Function IndexOf2(ByVal value As Object) As Integer Implements System.Collections.IList.IndexOf
			//        Return IndexOf(DirectCast(value, String))
			//    End Function
			//    Private Overloads Function IndexOf(ByVal value As String) As Integer
			//        For idx As Integer = 0 To a.Count - 1
			//            If DirectCast(a(idx), OptionSet).key = value.ToUpper Then Return idx
			//        Next
			//        Return -1
			//    End Function
			//    Private Sub Insert2(ByVal index As Integer, ByVal value As Object) Implements System.Collections.IList.Insert
			//        '    Insert(index, DirectCast(value, ServiceErrorResponse))
			//    End Sub
			//    Public ReadOnly Property IsFixedSize() As Boolean Implements System.Collections.IList.IsFixedSize
			//        Get
			//            Return False
			//        End Get
			//    End Property
			//    Public ReadOnly Property IsReadOnly() As Boolean Implements System.Collections.IList.IsReadOnly
			//        Get
			//            Return False
			//        End Get
			//    End Property
			//    Private Property Item2(ByVal index As Integer) As Object Implements System.Collections.IList.Item
			//        Get
			//            Return Item(index)
			//        End Get
			//        Set(ByVal Value As Object)
			//            ' Item(index) = DirectCast(Value, String())
			//        End Set
			//    End Property
			
			//    Default Public Overloads ReadOnly Property Item(ByVal key As String) As String
			//        Get
			//            Dim idx As Integer = IndexOf(key)
			//            If idx < 0 Then Throw New IndexOutOfRangeException("Object not found")
			//            Return DirectCast(Item(idx), OptionSet).value
			//        End Get
			//    End Property
			//    Private Sub Remove2(ByVal value As Object) Implements System.Collections.IList.Remove
			//        'Remove(DirectCast(value, ServiceErrorResponse))
			//    End Sub
			//    Public Sub Remove(ByVal key As String)
			//        a.RemoveAt(IndexOf(key))
			//    End Sub
			//    Private Sub RemoveAt(ByVal item As Integer) Implements IList.RemoveAt
			
			//    End Sub
			//End Class
			/// -----------------------------------------------------------------------------
			/// Project	 : BlackLight Services
			/// Class	 : ConfigurationList
			///
			/// -----------------------------------------------------------------------------
			/// <summary>
			/// A highly typed arraylist that holds config parameters
			/// </summary>
			/// <remarks>
			/// </remarks>
			/// <history>
			/// 	[Caleb]	12/1/2005	Created
			/// </history>
			/// -----------------------------------------------------------------------------
			
			public sealed class ConfigurationList : DictionaryBase
			{
				public string keyed(string key)
				{
					if (char.IsNumber(key[key.Length - 1]))
					{
						return key.ToUpper();
					}
					else
					{
						return key.ToUpper() + "1";
					}
				}
				public void Add (string key, OptionsList value)
				{
					 Dictionary.Add(key.ToUpper(), value);
				}
				public void Remove (string key)
				{
					 Dictionary.Remove(keyed(key));
				}
				public OptionsList this[string key]
				{
					get{
						return ((OptionsList) Dictionary[keyed(key)]);
					}
					set
					{
						
						Dictionary[keyed(key)] = value;
						
						
					}
				}
				
				public bool ContainsKey(string key)
				{
					return  Dictionary.Contains(keyed(key));
				}
				public System.Collections.ICollection Keys
				{
					get{
						return  Dictionary.Keys;
					}
				}
				public System.Collections.ICollection Values
				{
					get{
						return  Dictionary.Values;
					}
				}
				public OptionsList[] GetSet(string key)
				{
					int tInt = 1;
					ArrayList tReturn = new ArrayList();
					while ( Dictionary.Contains(key.ToUpper() + tInt))
					{
						tReturn.Add(Dictionary[key.ToUpper() + tInt]);
						tInt += 1;
					}
					return ((OptionsList[]) tReturn.ToArray(typeof(OptionsList)));
				}
				
			}
			public class OptionSet
			{
				
				public string key;
				public string value;
				public OptionSet(string key, string value) {
					this.key = key.ToUpper();
					this.value = value;
				}
			}
		}
	}
}
