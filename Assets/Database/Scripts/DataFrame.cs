using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Text;
using UnityEngine.Scripting;

namespace Database
{
    [Serializable]
    [JsonConverter(typeof(DataFrameConverter))]
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
    
    public class DataFrameConverter : CustomCreationConverter<DataFrame>
    {

        [Preserve]
        public DataFrameConverter()
        {
            
        }
        public override DataFrame Create(Type objectType)
        {
            return new DataFrame("Unnamed");
        }

        public override bool CanWrite => true;

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var df = value as DataFrame;
            writer.WriteStartObject();

            writer.WritePropertyName("name");
            writer.WriteValue(df.name);

            writer.WritePropertyName("varNames");
            serializer.Serialize(writer, df.varNames);

            writer.WritePropertyName("types");
            serializer.Serialize(writer, df.types);

            writer.WritePropertyName("comments");
            serializer.Serialize(writer, df.comments);

            writer.WritePropertyName("data");
            serializer.Serialize(writer, df.data);

            writer.WriteEndObject();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var df = new DataFrame("Unnamed");

            while (reader.Read())
            {
                if (reader.TokenType == JsonToken.EndObject)
                    break;

                if (reader.TokenType == JsonToken.PropertyName)
                {
                    string propertyName = (string)reader.Value;
                    reader.Read();

                    switch (propertyName)
                    {
                        case "name":
                            df.name = (string)reader.Value;
                            break;
                        case "varNames":
                            df.varNames = serializer.Deserialize<string[]>(reader);
                            break;
                        case "types":
                            df.types = serializer.Deserialize<string[]>(reader);
                            break;
                        case "comments":
                            df.comments = serializer.Deserialize<string[]>(reader);
                            break;
                        case "data":
                            df.data = serializer.Deserialize<string[][]>(reader);
                            break;
                    }
                }
            }

            return df;
        }
    }
}