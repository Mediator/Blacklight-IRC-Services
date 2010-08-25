using System.Diagnostics;
using Microsoft.VisualBasic;
using System;
using System.Data;
using System.Collections;

namespace BlackLight
{
	namespace Services
	{
		namespace Modules
		{
			namespace Help
			{
				namespace Lists
				{
					
					public sealed class CommandList : IList, ICollection, IEnumerable
					{
						
						readonly ArrayList a;
						public CommandList() {
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
							return Add((ServiceCommandResponse) value);
						}
						public int Add(ServiceCommandResponse value)
						{
							return a.Add(value);
						}
						public void Clear ()
						{
							a.Clear();
						}
						bool IList.Contains(object value)
						{
							return Contains((ServiceCommandResponse) value);
						}
						public bool Contains(ServiceCommandResponse value)
						{
							return a.Contains(value);
						}
						public bool Contains(string value)
						{
							return IndexOf(value) >= 0;
						}
						int IList.IndexOf(object value)
						{
							return IndexOf((ServiceCommandResponse) value);
						}
						public int IndexOf(ServiceCommandResponse value)
						{
							return a.IndexOf(value);
						}
						public int IndexOf(string value)
						{
							for (int idx = 0; idx <= a.Count - 1; idx++)
							{
								if (((ServiceCommandResponse)(a[idx])).Name.ToLower() == value.ToLower())
								{
									return idx;
								}
							}
							return - 1;
						}
						void IList.Insert (int index, object value)
						{
							Insert(index,((ServiceCommandResponse) value));
						}
						public void Insert (int index, ServiceCommandResponse value)
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
								return ((ServiceCommandResponse)(a[index]));
							}
							set
							{
								a[index] = value;
							}
						}
						public ServiceCommandResponse this[int index]
						{
							get{
								return ((ServiceCommandResponse)(a[index]));
							}
							set
							{
								a[index] = value;
							}
						}
						public ServiceCommandResponse this[string val]
						{
							get{
								int idx = IndexOf(val);
								if (idx < 0)
								{
									throw (new IndexOutOfRangeException("Object not found"));
								}
								return this[idx];
							}
						}
						void IList.Remove (object value)
						{
							Remove((ServiceCommandResponse) value);
						}
						public void Remove (ServiceCommandResponse value)
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
	}
}
