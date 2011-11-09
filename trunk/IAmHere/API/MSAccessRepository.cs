using System;
using System.Collections.Generic;
using System.Web;
using System.Data.OleDb;

namespace IAmHere.API
{
    public class MSAccessRepository : IRepository
    {
        string _dataFilePath = string.Empty;
        public MSAccessRepository(string dataFilePath)
        {
            _dataFilePath = dataFilePath;
        }

        public void Update(string room, Person person)
        {
            if (string.IsNullOrEmpty(room) || string.IsNullOrEmpty(person.Name))
            {
                return;
            }

            updatePerson(room, person);
        }

        public List<Person> AllPersons(string room)
        {
            return dbSelect(room);
        }

        public void Dispose() { }

#region Logic helpers

        const double _precision = 0.000001;
        static bool needUpdate(Person person, Person oldPerson)
        {
            return Math.Abs(person.X - oldPerson.X) > _precision || Math.Abs(person.Y - oldPerson.Y) > _precision;
        }

        static object _addPersonLock = new object();
        void updatePerson(string room, Person person)
        {
            lock (_addPersonLock)
            {
                Person oldPerson = dbGet(room, person.Name);
                if (oldPerson != null)
                {
                    if (needUpdate(person, oldPerson))
                    {
                        dbUpdate(room, person);
                    }
                }
                else
                {
                    dbInsert(room, person);
                }
            }
        }

#endregion


#region Database helpers

        /// <summary>
        /// Get a person from database. If the person does not exist, return null.
        /// </summary>
        Person dbGet(string room, string personName)
        {
            OleDbDataReader reader = dbExecute("select X, Y from Persons where Room = ? and Name = ?", room, personName);
            if (reader.HasRows)
            {
                reader.Read();
                return new Person() 
                {
                    X = dbParseDouble(reader["X"]),
                    Y = dbParseDouble(reader["Y"])
                };
            }
            return null;
        }

        bool dbExist(string room, string personName)
        {
            OleDbDataReader reader = dbExecute("select ID from Persons where Room = ? and Name = ?", room, personName);
            return reader.HasRows;
        }
        void dbUpdate(string room, Person person)
        {
            dbExecute("update Persons set X = ?, Y = ? where Room = ? and Name = ?"
                , person.X, person.Y, room, person.Name);
        }
        void dbInsert(string room, Person person)
        {
            dbExecute("insert into Persons(Room, Name, X, Y) values(?, ?, ?, ?)"
                , room, person.Name, person.X, person.Y);
        }

        List<Person> dbSelect(string room)
        {
            List<Person> personList = new List<Person>();
            OleDbDataReader reader = dbExecute("select Name, X, Y from Persons where Room = ?", room);
            if (reader.HasRows)
            {
                while (reader.Read())
                {
                    personList.Add(new Person()
                    {
                        Name = (string)reader["Name"],
                        X = dbParseDouble(reader["X"]),
                        Y = dbParseDouble(reader["Y"])
                    });
                }
            }
            return personList;
        }
        double dbParseDouble(object field)
        {
            return (field.GetType() == typeof(DBNull) ? 0.0 : (double)field);
        }
        static object executeLock = new object();
        OleDbDataReader dbExecute(string sqlCommand, params object[] parameters)
        {
            OleDbCommand command = new OleDbCommand(sqlCommand);
            foreach (object value in parameters)
            {
                command.Parameters.AddWithValue("?", value);
            }

            lock (executeLock)
            {
                if (_connection == null)
                {
                    _connection = newConnection();
                }
                command.Connection = _connection;
                if (sqlCommand[0] == 's') // select command
                {
                    return command.ExecuteReader();
                }
                else // update, delete, insert commands
                {
                    command.ExecuteNonQuery();
                    return null;
                }
            }
        }
        static OleDbConnection _connection = null;
        
        OleDbConnection newConnection()
        {
            string connectionString = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + _dataFilePath;
            OleDbConnection connection = new OleDbConnection(connectionString);
            connection.Open();
            return connection;
        }
#endregion
    }
}