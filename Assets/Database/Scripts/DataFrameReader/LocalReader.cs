using System.Collections.Generic;

namespace Database.DataReader
{
    public interface IDataReader
    {
        public abstract List<DataFrame> Read(string path);
    }
}