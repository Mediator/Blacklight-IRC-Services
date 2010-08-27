using System.Diagnostics;
using Microsoft.VisualBasic;
using System;
using System.Data;
using System.Collections;
using System.IO;
using System.Xml;
using BlackLight;
using BlackLight.Services;
using BlackLight.Services.Modules;
using BlackLight.Services.DB;
using BlackLight.Services.Core;
using BlackLight.Services.Error;

namespace BlackLight
{
	namespace Services
	{
		namespace Modules
		{
			namespace DataDriver
			{
				
				public class XMLDB : BlackLight.Services.DB.DataDriver
				{
					
					
					public XMLDB(ServicesDaemon Base) : base(Base) {
					}
					//Public Overrides Sub ModInit()
					
					//End Sub
					
					public override string DriverName
					{
						get{
							return "XMLDB";
						}
					}
					public override bool DBExists(string filename)
					{
						try
						{
							return File.Exists("data/" + filename);
						}
						catch (Exception)
						{
							MyBase.Core.SendLogMessage("XMLDB", "DBExists", Errors.DEBUG | Errors.WARNING, "Unable to verify database existance", "", "", "");
						}
						return false;
					}
					public override DataBase LoadDB(string filename)
					{
						
						//    Dim DataConnection As String = "Provider=Microsoft.JET.OLEDB.4.0;User ID=Admin;Data Source=data/" & filename & ""
						//    Dim DataCommand As OleDbCommand
						//    Dim TablesList As DataTable
						//    Dim tTable As String
						//    DataCommand = New OleDbCommand
						//    With DataCommand
						//        .Connection = New OleDbConnection(DataConnection)
						//        .Connection.Open()
						//        TablesList = .Connection.GetOleDbSchemaTable(Data.OleDb.OleDbSchemaGuid.Tables, Nothing)
						//        For Each tTable In TablesList.Rows()
						//            MsgBox(tTable.ToString)
						//        Next
						//    End With
						//    'DataReader.
						//    '  Dim Something As DataBase
						//    '  Dim Something2 As SqlClient.SqlDataAdapter
						//    '   Something2.Fill(
						
						
						BlackLight.Services.DB.DataBase tDataBase = new BlackLight.Services.DB.DataBase();
						if (System.IO.File.Exists("data\\" + filename))
						{
							System.Xml.XmlDocument XMLDOC = new System.Xml.XmlDocument();
							XMLDOC.Load("data\\" + filename);
							BlackLight.Services.DB.Table tTable;
							string tName;
							int tRowID;
							int tColumnsCount;
							int tType;
							string tPrimary;
							//BlackLight.Services.DB.Column tColumn;
							foreach (XmlNode tNode in XMLDOC.DocumentElement)
							{
								if (tNode.Name == "table")
								{
									if (!System.Convert.ToBoolean(tNode.Attributes["name"] == null && tNode.Attributes["name"].Value != ""))
									{
										tName = tNode.Attributes["name"].Value;
									}
									else
									{
										MyBase.Core.SendLogMessage("XMLDB", "LoadDB()", Errors.DEBUG | Errors.ERROR, "XML Data Corruption", "", "", "");
										//show("XML Data corruption");
										return null;
									}
									
									if (!System.Convert.ToBoolean(tNode.Attributes["columns"] == null && tNode.Attributes["columns"].Value != ""))
									{
										tColumnsCount = int.Parse(tNode.Attributes["columns"].Value);
									}
									else
									{
										MyBase.Core.SendLogMessage("XMLDB", "LoadDB()", Errors.DEBUG | Errors.ERROR, "XML Data Corruption", "", "", "");
										//show("XML Data corruption");
										return null;
									}
									
									if (!System.Convert.ToBoolean(tNode.Attributes["primary"] == null))
									{
										tPrimary = tNode.Attributes["primary"].Value;
									}
									else
									{
										MyBase.Core.SendLogMessage("XMLDB", "LoadDB()", Errors.DEBUG | Errors.ERROR, "XML Data Corruption", "", "", "");
										//show("XML Data corruption");
										return null;
									}
									tTable = new BlackLight.Services.DB.Table(tName);
									tDataBase.AddTable(tTable);
									foreach (XmlNode tNodeInner in tNode)
									{
										if (tNodeInner.Name == "column")
										{
											if (!System.Convert.ToBoolean(tNodeInner.Attributes["name"] == null && tNodeInner.Attributes["name"].Value != ""))
											{
												tName = tNodeInner.Attributes["name"].Value;
											}
											else
											{
												MyBase.Core.SendLogMessage("XMLDB", "LoadDB()", Errors.DEBUG | Errors.ERROR, "XML Data Corruption", "", "", "");
												//show("XML Data corruption");
												return null;
											}
											if (!System.Convert.ToBoolean(tNodeInner.Attributes["type"] == null && tNodeInner.Attributes["type"].Value != ""))
											{
												tType = int.Parse(tNodeInner.Attributes["type"].Value);
											}
											else
											{
												MyBase.Core.SendLogMessage("XMLDB", "LoadDB()", Errors.DEBUG | Errors.ERROR, "XML Data Corruption", "", "", "");
												//show("XML Data corruption");
												return null;
											}
											tTable.Columns.NewColumn(tName, (BlackLight.Services.DB.ColumnType) tType);
										}
										else if (tNodeInner.Name == "row" && tTable.Columns.Count == tColumnsCount)
										{
											tRowID = tTable.NewRow();
											foreach (BlackLight.Services.DB.Column tTColumn in tTable.Columns)
											{
												if (!System.Convert.ToBoolean(tNodeInner.Attributes[tTColumn.Name] == null && tNodeInner.Attributes[tTColumn.Name].Value != ""))
												{
													switch (tTColumn.Type.FullName)
													{
														case "System.String":
															
															tTable[tRowID][tTColumn.Name] = System.Convert.ToString(tNodeInner.Attributes[tTColumn.Name].Value);
															break;
														case "System.Integer":
															tTable[tRowID][tTColumn.Name] = int.Parse(tNodeInner.Attributes[tTColumn.Name].Value);
															break;
															
														case "System.Int32":
															
															tTable[tRowID][tTColumn.Name] = int.Parse(tNodeInner.Attributes[tTColumn.Name].Value);
															break;
														case "System.Int16":
															tTable[tRowID][tTColumn.Name] = short.Parse(tNodeInner.Attributes[tTColumn.Name].Value);
															break;
															
														case "System.Short":
															
															tTable[tRowID][tTColumn.Name] = short.Parse(tNodeInner.Attributes[tTColumn.Name].Value);
															break;
														default:
															
															throw (new Exception("Invalid Type Casting"));
//															break;
													}
												}
												else if (tNodeInner.Attributes[tTColumn.Name] == null)
												{
													MyBase.Core.SendLogMessage("XMLDB", "LoadDB()", Errors.DEBUG | Errors.ERROR, "XML Data Corruption", "", "", "");
													//show("XML Data corruption");
													return null;
												}
												
											}
										}
										else
										{
											MyBase.Core.SendLogMessage("XMLDB", "LoadDB()", Errors.DEBUG | Errors.ERROR, "XML Data Corruption", "", "", "");
											//show("XML Data corruption");
											return null;
										}
									}
									tTable.PrimaryColumn = tPrimary;
								}
								else
								{
									MyBase.Core.SendLogMessage("XMLDB", "LoadDB()", Errors.DEBUG | Errors.ERROR, "XML Data Corruption", "", "", "");
									//show("XML Data corruption");
									return null;
								}
							}
							
							return tDataBase;
						}
						return null;
						
					}
					public override void SaveDB (string filename, DataBase tDataBase)
					{
						//tDataBase.WriteXml(filename)
						
						System.Xml.XmlDocument XMLDOC = new System.Xml.XmlDocument();
						XMLDOC.LoadXml("<data></data>");
						
						XmlAttribute attr;
						
						XmlNode newTable;
						XmlNode newRow;
						XmlNode newColumn;
						
						foreach (BlackLight.Services.DB.Table tTable in tDataBase)
						{
							newTable = XMLDOC.CreateNode(XmlNodeType.Element, "table", "");
							XMLDOC.DocumentElement.AppendChild(newTable);
							
							attr = XMLDOC.CreateAttribute("name");
							attr.Value = tTable.Name;
							newTable.Attributes.Append(attr);
							
							attr = XMLDOC.CreateAttribute("columns");
							attr.Value = Convert.ToString(tTable.Columns.Count);
							newTable.Attributes.Append(attr);
							
							attr = XMLDOC.CreateAttribute("primary");
							attr.Value = tTable.PrimaryColumn;
							newTable.Attributes.Append(attr);
							
							foreach (BlackLight.Services.DB.Column tColumn in tTable.Columns)
							{
								newColumn = XMLDOC.CreateNode(XmlNodeType.Element, "column", "");
								newTable.AppendChild(newColumn);
								
								attr = XMLDOC.CreateAttribute("name");
								attr.Value = tColumn.Name;
								newColumn.Attributes.Append(attr);
								
								attr = XMLDOC.CreateAttribute("type");
								attr.Value = Convert.ToString(tColumn.DType);
								newColumn.Attributes.Append(attr);
								
							}
							
							foreach (BlackLight.Services.DB.Row tRow in tTable)
							{
								newRow = XMLDOC.CreateNode(XmlNodeType.Element, "row", "");
								newTable.AppendChild(newRow);
								foreach (BlackLight.Services.DB.Column tColumn in tTable.Columns)
								{
									attr = XMLDOC.CreateAttribute(tColumn.Name);
									attr.Value = System.Convert.ToString(tRow[tColumn.Name]);
									newRow.Attributes.Append(attr);
									
								}
							}
							
						}
						
						
						if (System.IO.Directory.Exists("data") == false)
						{
							System.IO.Directory.CreateDirectory("data");
						}
						
						XMLDOC.Save("data\\" + filename);
					}
					
				}
			}
		}
	}
}
