using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;

namespace Database
{
    [Obsolete("Use List<T> directly instead of ListWrapper<T>")]
    [JsonConverter(typeof(ListWrapperConverter))]
    [System.Serializable]
    public class ListWrapper<T> : IList<T>, IReadOnlyList<T>
    {
        public List<T> Items = new List<T>();
         
        public T this[int index] { get => Items[index]; set => Items[index] = value; }
        public int Count => Items.Count;
        public bool IsReadOnly => false;
        public void Add(T item) => Items.Add(item);
        public void Clear() => Items.Clear();
        public bool Contains(T item) => Items.Contains(item);
        public void CopyTo(T[] array, int arrayIndex) => Items.CopyTo(array, arrayIndex);
        public IEnumerator<T> GetEnumerator() => Items.GetEnumerator();
        public int IndexOf(T item) => Items.IndexOf(item);
        public void Insert(int index, T item) => Items.Insert(index, item);
        public bool Remove(T item) => Items.Remove(item);
        public void RemoveAt(int index) => Items.RemoveAt(index);
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => Items.GetEnumerator();
    }
    
    
    public class ListWrapperConverter : CustomCreationConverter<object>
    {
        public override object Create(Type objectType)
        {
            return Activator.CreateInstance(objectType);
        }

        public override bool CanWrite => false; 
        
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {

            var innerType = objectType.GetGenericArguments()[0];
            var listType = typeof(List<>).MakeGenericType(innerType);
            
            var temporaryList = serializer.Deserialize(reader, listType) as System.Collections.IList;
            
            var listWrapperInstance = Activator.CreateInstance(objectType) as System.Collections.IList;
            
            if (temporaryList != null && listWrapperInstance != null)
            {
                foreach (var item in temporaryList)
                {
                    listWrapperInstance.Add(item);
                }
            }
            
            return listWrapperInstance;
        }
    }
}