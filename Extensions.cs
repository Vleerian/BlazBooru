using System; 
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;

namespace BlazBooruAPI
{
    public static class Extensions
    {
        /// <summary>
        /// SQLCast runs a SqliteQuery and using reflection, maps the retrived values to data model objects.
        /// The object field names MUST match 1:1 with the database column names.
        /// </summary>
        /// <author>Atagait Denral</author>
        /// <typeparam name="T">The data model type</typeparam>
        /// <param name="command">The SQLite command to be executed</param>
        /// <returns>An enumerable containing all populated data objects</returns>
        /// <exception cref="InvalidCastException">Thrown if the field count is not equal to the number of columns in the database.</exception>
        /// <exception cref="MissingFieldException">Thrown if a class does not contain a field the database contains</exception>
        public static IEnumerable<T> SQLCast<T>(this SQLiteCommand command) where T : new()
        {
            // Get type info
            var type = typeof(T);
            var Fields_Unsorted = type.GetRuntimeFields().ToArray();
            
            List<T> ReturnList = new List<T>();
            using(var reader = command.ExecuteReader())
            {
                // Check that the field count == member count
                var Fieldcount = reader.FieldCount;
                if(Fields_Unsorted.Length != Fieldcount)
                    throw new InvalidCastException("Field count does not match class member count.");

                // Optimization to ensure things get assigned to the right fields
                bool first = true;
                FieldInfo[] Fields = new FieldInfo[Fieldcount];
                while(reader.Read())
                {
                    // Create a temporary object
                    T tmp = new T();
                    for(int i = 0; i < Fieldcount; i++)
                    {
                        // If this is the first run-through, sort the field list
                        if(first)
                        {
                            var colName = reader.GetName(i);
                            var Field = Fields_Unsorted.Where(F=>F.Name == colName).FirstOrDefault();
                            if(Field == default)
                                throw new MissingFieldException($"Field {colName} not found.");
                            Fields[i] = Field;
                        }
                        
                        // Get the property type, and convert the value from the database to that type.
                        object ValueToConvert = reader.GetValue(i);
                        Type FieldType = Fields[i].FieldType;
                        dynamic convertedValue = Convert.ChangeType(ValueToConvert, FieldType);

                        // Assign to the temporary object
                        Fields[i].SetValue(tmp, convertedValue);
                    }
                    first = false;
                    ReturnList.Add(tmp);
                }
            }
            return ReturnList;
        }

        /// <summary>
        /// SQLCast runs a SqliteQuery and using reflection, maps the retrived values to data model objects.
        /// The object field names MUST match 1:1 with the database column names.
        /// </summary>
        /// <author>Atagait Denral</author>
        /// <typeparam name="T">The data model type</typeparam>
        /// <param name="command">The SQLite command to be executed</param>
        /// <returns>An enumerable containing all populated data objects</returns>
        /// <exception cref="InvalidCastException">Thrown if the field count is not equal to the number of columns in the database.</exception>
        /// <exception cref="MissingFieldException">Thrown if a class does not contain a field the database contains</exception>
        public static async Task<IEnumerable<T>> SQLCastAsync<T>(this SQLiteCommand command) where T : new()
        {
            // Get type info
            var type = typeof(T);
            var Fields_Unsorted = type.GetRuntimeFields().ToArray();

            List<T> ReturnList = new List<T>();
            using(var reader = await command.ExecuteReaderAsync())
            {
                // Check that the field count == member count
                var Fieldcount = reader.FieldCount;
                if(Fields_Unsorted.Length != Fieldcount)
                    throw new InvalidCastException("Field count does not match class member count.");

                // Optimization to ensure things get assigned to the right fields
                bool first = true;
                FieldInfo[] Fields = new FieldInfo[Fieldcount];
                while(await reader.ReadAsync())
                {
                    // Create a temporary object
                    T tmp = new T();
                    for(int i = 0; i < Fieldcount; i++)
                    {
                        // If this is the first run-through, sort the field list
                        if(first)
                        {
                            var colName = reader.GetName(i);
                            var Field = Fields_Unsorted.Where(F=>F.Name == colName || F.Name.StartsWith($"<{colName}>")).FirstOrDefault();
                            if(Field == default)
                                throw new MissingFieldException($"Field {colName} not found.");
                            Fields[i] = Field;
                        }
                        
                        // Get the property type, and convert the value from the database to that type.
                        object ValueToConvert = reader.GetValue(i);
                        Type FieldType = Fields[i].FieldType;
                        dynamic convertedValue = Convert.ChangeType(ValueToConvert, FieldType);

                        // Assign to the temporary object
                        Fields[i].SetValue(tmp, convertedValue);
                    }
                    first = false;
                    ReturnList.Add(tmp);
                }
            }
            return ReturnList;
        }

        public static SQLiteCommand CreateCommand(this SQLiteConnection connection, string SQL)
        {
            var cmd = connection.CreateCommand();
            cmd.CommandText = SQL;
            return cmd;
        }

        public static SQLiteCommand CreateCommand(this SQLiteConnection connection, string SQL, KeyValuePair<string, object>[] Parameters)
        {
            var cmd = connection.CreateCommand();
            cmd.CommandText = SQL;
            foreach(var Param in Parameters)
                cmd.Parameters.AddWithValue(Param.Key, Param.Value);
            return cmd;
        }

        public static SQLiteConnection SetupDatabase(string database_name)
        {
            var First_Init = false;
            if(!File.Exists($"{database_name}.db"))
                First_Init = true;
            var connection = new SQLiteConnection($"Data Source={database_name}.db; Verison=3").OpenAndReturn();
            if(First_Init)
            {
                foreach(var DDL in File.ReadAllText($"{database_name}.ddl").Split("|||"))
                {
                    var cmd = connection.CreateCommand();
                    cmd.CommandText = DDL;
                    cmd.ExecuteNonQuery();
                }
            }

            return connection;
        }

        public static void ForEach<T>(this T[] collection, Action<T> action)
        {
            foreach(T i in collection)
                action(i);
        }
    }
}