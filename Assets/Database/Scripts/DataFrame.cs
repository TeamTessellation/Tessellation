using System;
using System.Text;

namespace Database
{
    [Serializable]
    public class DataFrame
    {
        public bool IsEmpty => data == null || data.Length == 0;
        public int RowCount => data.Length;
        public int MaxColumn => varNames.Length;
        public string name;
        public string[] varNames;
        public string[] types;
        public string[] comments;
        public string[][] data;
        
        public DataFrame(string name)
        {
            this.name = name;
        }
        
        public string GetVarName(int index)
        {
            var tmp = varNames[index].Split('_');
            if(tmp[0].StartsWith("arr"))
            {
                return tmp[1];
            }

            return tmp[0];
        }


        public override string ToString()
        {
            if (IsEmpty)
                return "Empty DataFrame";
            StringBuilder sb = new StringBuilder();
            void AppendArray(string[] array)
            {
                for (int i = 0; i < array.Length; i++)
                {
                    sb.Append(array[i]);
                    if (i < array.Length - 1)
                        sb.Append(", ");
                }
                sb.AppendLine();
            }
            sb.AppendFormat("DataFrame: {0}", name);
            AppendArray(varNames);
            AppendArray(types);
            AppendArray(comments);
            for (int i = 0; i < data.Length; i++)
            {
                var row = data[i];
                for (int j = 0; j < row.Length - 1; j++)
                {
                    sb.Append(row[j]);
                    sb.Append(", ");
                }
                sb.Append(row[^1]);
                sb.AppendLine();
            }
            return sb.ToString();
        }
    }
}