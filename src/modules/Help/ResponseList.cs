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
					
					public sealed class ResponseList : IList, ICollection, IEnumerable
					{
						
						readonly ArrayList a;
						public ResponseList() {
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
							return Add((ServiceResponses) value);
						}
						public int Add(ServiceResponses value)
						{
							return a.Add(value);
						}
						public void Clear ()
						{
							a.Clear();
						}
						bool IList.Contains(object value)
						{
							return Contains((ServiceResponses) value);
						}
						public bool Contains(ServiceResponses value)
						{
							return a.Contains(value);
						}
						public bool Contains(string value)
						{
							return IndexOf(value) >= 0;
						}
						int IList.IndexOf(object value)
						{
							return IndexOf((ServiceResponses) value);
						}
						public int IndexOf(ServiceResponses value)
						{
							return a.IndexOf(value);
						}
						public int IndexOf(string value)
						{
							for (int idx = 0; idx <= a.Count - 1; idx++)
							{
								if (((ServiceResponses)(a[idx])).Name.ToLower() == value.ToLower())
								{
									return idx;
								}
							}
							return - 1;
						}
						void IList.Insert (int index, object value)
						{
							Insert(index,((ServiceResponses) value));
						}
						public void Insert (int index, ServiceResponses value)
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
								return ((ServiceResponses)(a[index]));
							}
							set
							{
								a[index] = value;
							}
						}
						public ServiceResponses this[int index]
						{
							get{
								return ((ServiceResponses)(a[index]));
							}
							set
							{
								a[index] = value;
							}
						}
						public ServiceResponses this[string val]
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
							Remove((ServiceResponses) value);
						}
						public void Remove (ServiceResponses value)
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
