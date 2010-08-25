using System.Diagnostics;
using System;
using System.Data;
using System.Collections;
using BlackLight;
using BlackLight.Services.Modules;

namespace BlackLight
{
	namespace Services
	{
		namespace DB
		{
			
			public class DataBase : IList, ICollection, IEnumerable
			{
				readonly ArrayList a;
				public DataBase() 
				{
					a = new ArrayList();
				}
				void ICollection.CopyTo(System.Array array, int index) { a.CopyTo(array,index); }
				int ICollection.Count {
					get
					{
						return a.Count;
					} }
				
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
				int IList.Add(object o) { return AddTable((Table)o); }
				public int AddTable(Table value)
				{
					return a.Add(value);
				}
				void IList.Clear ()
				{
					a.Clear();
				}
				public void ClearTables ()
				{
					a.Clear();
				}
				bool IList.Contains(object value)
				{
					return ContainsTable((Table) value);
				}
				public bool ContainsTable(Table value)
				{
					return a.Contains(value);
				}
				public bool ContainsTable(string name)
				{
					return IndexOf(name) >= 0;
				}
				int IList.IndexOf(object value)
				{
					return IndexOf((Table)value);
				}
				public int IndexOf(Table value)
				{
					return a.IndexOf(value);
				}
				public int IndexOf(string name)
				{
					for (int idx = 0; idx <= a.Count - 1; idx++)
					{
						if (((Table)(a[idx])).Name.ToLower() == name.ToLower())
						{
							return idx;
						}
					}
					return - 1;
				}
				void IList.Insert (int index, object value)
				{
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
						return ((Table)(a[index]));
					}
					set
					{
						a[index] = (Table)value;
					}
				}

				public Table this[int index]
				{
					get{
						return ((Table)(a[index]));
					}
					set
					{
						a[index] = value;
					}
				}
				public Table this[string name]
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
					RemoveTable((Table) value);
				}
				public void RemoveTable (Table value)
				{
					a.Remove(value);
				}
				public void RemoveAt (int index)
				{
					a.RemoveAt(index);
				}
				public int NewTable(string name)
				{
					return a.Add(new Table(name));
				}
			}
			public class Table : IList, ICollection, IEnumerable
			{
				readonly ArrayList a;
				private string tPrimaryColumn;
				public ColumnsList Columns;
				public string Name;
				public Table(string tname) {
					Columns = new ColumnsList(this);
					Name = "";
					
					a = new ArrayList();
					Name = tname;
				}
				public string PrimaryColumn
				{
					get{
						return tPrimaryColumn;
					}
					set
					{
						if (Columns.ContainsColumn(value))
						{
							tPrimaryColumn = value.ToLower();
						}
						else
						{
							throw (new Exception("Invalid column name"));
						}
					}
				}
				public int Count
				{
					get{
						return a.Count;
					}
				}
				void ICollection.CopyTo(System.Array array, int index) { a.CopyTo(array,index); }
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
				int IList.Add(object value)
				{
					throw new NotImplementedException();
				}
				public void Clear ()
				{
					a.Clear();
				}
				bool IList.Contains(object value)
				{
					return ContainsRow((Row) value);
				}
				public bool ContainsRow(Row value)
				{
					return a.Contains(value);
				}
				public bool ContainsRow(string name)
				{
					return IIndexOf(name) >= 0;
				}
				public int IndexOf(Row value)
				{
					return a.IndexOf(value);
				}
				int IList.IndexOf(object value)
				{
					return IndexOf((Row)value);
				}
				public int IIndexOf(string value)
				{
					for (int idx = 0; idx <= a.Count - 1; idx++)
					{
						if (System.Convert.ToString(((Row)(a[idx]))[tPrimaryColumn]).ToLower() == value.ToLower())
						{
							return idx;
						}
					}
					return - 1;
				}
				void IList.Insert (int index, object value)
				{
					throw new NotImplementedException();
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
						return ((Row)(a[index]));
					}
					set
					{
						a[index] = value;
					}
				}
				public Row this[int index]
				{
					get{
						return ((Row)(a[index]));
					}
					set
					{
						a[index] = value;
					}
				}
				public Row IRow(string primarykey){
					int idx = IIndexOf(primarykey);
					if (idx < 0)
					{
						return null;
					}
					return this[idx];
				}
				
				void IList.Remove (object value)
				{
					RemoveRow((Row) value);
				}
				public void RemoveRow (Row value)
				{
					a.Remove(value);
				}
				public void RemoveAt (int index)
				{
					a.RemoveAt(index);
				}
				public int NewRow()
				{
					return a.Add(new Row((ArrayList) Columns.shema.Clone()));
				}
				public Row MakeRow()
				{
					return new Row((ArrayList) Columns.shema.Clone());
				}
				public void AddRow (Row tRow)
				{
					if (this.Columns.shema.Count != tRow.Shema.Count)
					{
						throw (new Exception("Row field mismatch"));
					}
					for (int tInt = 0; tInt <= tRow.Shema.Count - 1; tInt++)
					{
						if (this.Columns.shema.GetName(tInt).ToLower() != tRow.Shema.GetName(tInt).ToLower())
						{
							throw (new Exception("Row field mismatch"));
						}
					}
					
					a.Add(tRow);
				}
				
				
			}
			public class Column
			{
				private string tName;
				private Type pType;
				public Column(ColumnType tType) {
					tName = "";
					
					if ((int)tType < 1 || (int)tType > 3)
					{
						tType = ColumnType.STRING_TYPE;
					}
					switch (tType)
					{
						case ColumnType.INT_TYPE:
							
							pType = typeof(int);
							break;
						case ColumnType.SMALLINT_TYPE:
							
							pType = typeof(short);
							break;
						case ColumnType.STRING_TYPE:
							
							pType = typeof(string);
							break;
					}
					
				}
				public Type Type
				{
					get{
						return pType;
					}
				}
				public int DType
				{
					get{
						if (pType == typeof(int))
						{
							return (int)ColumnType.INT_TYPE;
						}
						else if (pType == typeof(short))
						{
							return (int)ColumnType.SMALLINT_TYPE;
						}
						else
						{
							return (int)ColumnType.STRING_TYPE;
						}
					}
				}
				public Column(string name, ColumnType tType) {
					tName = "";
					
					if ((int)tType < 1 || (int)tType > 3)
					{
						tType = ColumnType.STRING_TYPE;
					}
					switch (tType)
					{
						case ColumnType.INT_TYPE:
							
							pType = typeof(int);
							break;
						case ColumnType.SMALLINT_TYPE:
							
							pType = typeof(short);
							break;
						case ColumnType.STRING_TYPE:
							
							pType = typeof(string);
							break;
					}
					tName = name.ToLower();
				}
				public string Name
				{
					get{
						return tName;
					}
					set
					{
						tName = value.ToLower();
					}
				}
			}
			public enum ColumnType
			{
				STRING_TYPE = 1,
				SMALLINT_TYPE = 2,
				INT_TYPE = 3
			}
			public class ColumnsList : CollectionBase
			{
				
				//readonly ArrayList a;
				private Table pTable;
				private FieldList fieldshema;
				public FieldList shema
				{
					get{
						return fieldshema;
					}
				}
				
				public ColumnsList(Table tTable) {
					fieldshema = new FieldList();
					//a = new ArrayList();
					pTable = tTable;
				}
				
				/*private int AddColumn(Column value)
				{
					if (value.Name != "" || value.Name != null)
					{
						fieldshema.CreateField(value);
						return a.Add(value);
//						foreach (Row tRow in pTable)
//						{
//							tRow.Shema_AddColumn(value);
//							}
						}
						else
						{
							return - 1;
						}
					}*/
					public new void Clear ()
					{
						List.Clear();
						fieldshema.Clear();
					}
					public bool ContainsColumn(Column value)
					{
						return List.Contains(value);
					}
					public bool ContainsColumn(string name)
					{
						return IndexOf(name) >= 0;
					}
					public int IndexOf(Column value)
					{
						return List.IndexOf(value);
					}
					public int IndexOf(string name)
					{
						for (int idx = 0; idx <= List.Count - 1; idx++)
						{
							if (((Column)(List[idx])).Name == name.ToLower())
							{
								return idx;
							}
						}
						return - 1;
					}
					private void Insert (int index, object value)
					{
					}
					private bool IsFixedSize
					{
						get{
							return false;
						}
					}
					private bool IsReadOnly
					{
						get{
							return false;
						}
					}
					public Column this[int index]
					{
						get{
							return ((Column)(List[index]));
						}
						set
						{
							List[index] = value;
						}
					}
					public Column this[string name]
					{
						get{
							int idx = IndexOf(name);
							if (idx < 0)
							{
								throw (new IndexOutOfRangeException("Object not found"));
							}
							return (Column)List[idx];
						}
						set
						{
							int idx = IndexOf(name);
							if (idx < 0)
							{
								throw (new IndexOutOfRangeException("Object not found"));
							}
							List[idx] = value;
						}
					}
					private void Remove (object value)
					{
						RemoveColumn((Column) value);
					}
					public void RemoveColumn (Column value)
					{
						List.Remove(value);
						fieldshema.Remove(value.Name);
						foreach (Row tRow in pTable)
						{
							tRow.Shema_DelColumn(value.Name);
						}
					}
					public new void RemoveAt (int index)
					{
						List.RemoveAt(index);
					}
					public int NewColumn(string name, ColumnType type)
					{
						Column tColumn = new Column(name, type);
						fieldshema.CreateField(tColumn);
						return List.Add(tColumn);
//						foreach (Row tRow in pTable)
//						{
//							tRow.Shema_AddColumn(tColumn);
//							}
						}
					}
					public class FieldList : IList, ICollection, IEnumerable, ICloneable
					{
						
						readonly ArrayList a;
						public FieldList() {
							a = new ArrayList();
						}
						public FieldList(ArrayList tList) {
							a = tList;
						}
						void ICollection.CopyTo(System.Array array, int index) { a.CopyTo(array,index); }
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
						public object Clone()
						{
							return a.Clone();
						}
						public int Count
						{
							get{
								return a.Count;
							}
						}
						public System.Collections.IEnumerator GetEnumerator()
						{
							return a.GetEnumerator();
						}
int IList.Add(object o) { throw new NotImplementedException(); }
						public int CreateField(Column Column)
						{
							if (Column.Name != null&& Column.Name != "")
							{
								return a.Add(new Field(Column));
							}
							else
							{
								return - 1;
							}
						}
						public int CreateField(string name, ColumnType type)
						{
							if (name != null&& name != "")
							{
								return a.Add(new Field(name, type));
							}
							else
							{
								return - 1;
							}
						}
						public string GetName(int index)
						{
							return ((Field)(a[index])).Name;
						}
						public void Clear ()
						{
							a.Clear();
						}
						bool IList.Contains(object value)
						{
							throw new NotImplementedException();
						}
						int IList.IndexOf(object value)
						{
							throw new NotImplementedException();
						}
						public int IndexOf(string name)
						{
							for (int idx = 0; idx <= a.Count - 1; idx++)
							{
								//    MsgBox(DirectCast(a(idx), Field).Name & " | " & LCase(name))
								if (((Field)(a[idx])).Name == name.ToLower())
								{
									return idx;
								}
							}
							return - 1;
						}
						void IList.Insert (int index, object value)
						{
							throw new NotImplementedException();
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
						public object this[int index]
						{
							get{
								return ((Field)(a[index])).Value;
							}
							set
							{
								if (((Field)(a[index])).Type == value.GetType())
								{
									a[index] = new Field(((Field)(a[index])).Name,((Field)(a[index])).Type, value);
								}
							}
						}
						public object this[string name]
						{
							get{
								try
								{
									int idx = IndexOf(name);
									
									if (idx < 0)
									{
										throw (new IndexOutOfRangeException("Object not found"));
									}
									
									return ((Field)(a[idx])).Value;
								}
								catch (Exception ex)
								{
									throw ex;
									//MessageBox.Show("Get Column: " + ex.Message + " " + ex.StackTrace);
								}
							}
							set
							{
								try
								{
									int idx = IndexOf(name);
									if (idx < 0)
									{
										throw (new IndexOutOfRangeException("Object not found"));
									}
									
									if (value.GetType() ==((Field)(a[idx])).Type)
									{
										a[idx] = new Field(((Field)(a[idx])).Name,((Field)(a[idx])).Type, value);
									}
									else
									{
										throw (new ArgumentException("Type of value does not match column type"));
									}
								}
								catch (Exception ex)
								{
									throw ex;
									//MessageBox.Show("Set Column: " + ex.Message + " " + ex.StackTrace);
								}
								
								
							}
						}
						void IList.Remove (object o)
						{
							throw new NotImplementedException();
						}
						public void Remove (string name)
						{
							int idx = IndexOf(name);
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
						
						
						private class Field
						{
							public Field()
							{
								tName = "";
								
							}
							private string tName;
							private Type pType;
							public object Value;
							public Type Type
							{
								get{
									return pType;
								}
							}
							public Field(string name, ColumnType tType) {
								tName = "";
								
								if ((int)tType < 1 || (int)tType > 3)
								{
									tType = ColumnType.STRING_TYPE;
								}
								switch (tType)
								{
									case ColumnType.INT_TYPE:
										
										pType = typeof(int);
										break;
									case ColumnType.SMALLINT_TYPE:
										
										pType = typeof(short);
										break;
									case ColumnType.STRING_TYPE:
										
										pType = typeof(string);
										break;
								}
								tName = name.ToLower();
							}
							public Field(string name, Type tType, object tvalue) {
								tName = "";
								
								
								pType = tType;
								tName = name.ToLower();
								Value = tvalue;
							}
							public Field(Column column) {
								tName = "";
								
								pType = column.Type;
								tName = column.Name.ToLower();
							}
							public string Name
							{
								get{
									return tName;
								}
								set
								{
									tName = value.ToLower();
								}
							}
						}
						
					}
					public class Row
					{
						private FieldList pColumns;
						
						
						public Row(FieldList Columns) {
							pColumns = new FieldList();
							
							try
							{
								if (Columns != null&& Columns.Count > 0)
								{
									pColumns = Columns;
								}
								else
								{
									throw (new ArgumentNullException("Columns", "Row cannot be created with no columns"));
								}
							}
							catch (Exception ex)
							{
								throw (ex);
							}
						}
						public Row(ArrayList Columns) {
							pColumns = new FieldList();
							
							try
							{
								if (Columns != null&& Columns.Count > 0)
								{
									pColumns = new FieldList(Columns);
								}
								else
								{
									throw (new ArgumentNullException("Columns", "Row cannot be created with no columns"));
								}
							}
							catch (Exception ex)
							{
								throw (ex);
							}
						}
						public object this[string tColumn]
						{
							
							get{
								try
								{
									return pColumns[tColumn.ToLower()];
								}
								catch (Exception ex)
								{
									throw ex;
									//MessageBox.Show("Error " + ex.Message + " " + ex.StackTrace);
								}
							}
							set
							{
								try
								{
									if (value is string || value is short || value is int)
									{
										pColumns[tColumn] = value;
									}
								}
								catch (Exception ex)
								{
									throw ex;
									//MessageBox.Show("Error " + ex.Message + " " + ex.StackTrace);
								}
							}
						}
						public object this[int tColumn]
						{
							
							get{
								try
								{
									return pColumns[tColumn];
								}
								catch (Exception ex)
								{
									throw ex;
									//MessageBox.Show("Error " + ex.Message + " " + ex.StackTrace);
								}
							}
							set
							{
								try
								{
									if (value is string || value is short || value is int)
									{
										pColumns[tColumn] = value;
									}
								}
								catch (Exception ex)
								{
									throw ex;
									//MessageBox.Show("Error " + ex.Message + " " + ex.StackTrace);
								}
							}
						}
						
						public void Shema_DelColumn (string tColumn)
						{
							pColumns.Remove(tColumn);
						}
						public void Shema_AddColumn (Column tColumn)
						{
							pColumns.CreateField(tColumn);
						}
						public FieldList Shema
						{
							get{
								return pColumns;
							}
						}
					}
					
					public abstract class DataDriver : BlackLight.Services.Modules.BlackLightModule
					{
						
						public DataDriver(ServicesDeamon tBase) : base(tBase) {
						}
						public abstract bool DBExists(string filename);
						public abstract DataBase LoadDB(string filename);
						public abstract void SaveDB (string filename, DataBase tDataBase);
						public abstract string DriverName{
							get;
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
						public override string Description
						{
							get{
								return "";
							}
						}
						
						public override bool ModLoad()
						{
							return true;
						}
						
						public override void ModUnload ()
						{
						}
						
						public override string Name
						{
							get{
								return DriverName;
							}
						}
						public override bool NeedsDBDriver
						{
							get{
								return false;
							}
						}
					}
					
				}
			}
		}
