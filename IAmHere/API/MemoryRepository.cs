using System;
using System.Collections.Generic;
using System.Web;

namespace IAmHere.API
{
    public class MemoryRepository : IRepository
    {
        static Dictionary<string, List<Person>> rooms = new Dictionary<string, List<Person>>();

        #region Logic helpers

        static object _addRoomLock = new object();
        static void ensureRoomIsExist(string room)
        {
            lock (_addRoomLock)
            {
                if (!rooms.ContainsKey(room))
                {
                    rooms.Add(room, new List<Person>());
                }
            }
        }

        static object _addPersonLock = new object();
        static void updatePerson(string room, Person person)
        {
            lock (_addPersonLock)
            {
                // Try to update the old one. 
                for (int i = 0; i < rooms[room].Count; i++)
                {
                    if (rooms[room][i].Name == person.Name)
                    {
                        rooms[room][i].X = person.X;
                        rooms[room][i].Y = person.Y;
                        rooms[room][i].LastRefresh = person.LastRefresh;
                        return;
                    }
                }

                rooms[room].Add(person);
            }
        }

        #endregion

        public void Update(string room, Person person)
        {
            if (string.IsNullOrEmpty(room) || string.IsNullOrEmpty(person.Name))
            {
                return;
            }
            person.LastRefresh = DateTime.Now;
            ensureRoomIsExist(room);
            updatePerson(room, person);
        }
        public List<Person> AllPersons(string room)
        {
            if (!string.IsNullOrEmpty(room) && rooms.ContainsKey(room))
            {
                List<Person> result = new List<Person>();

                // Only return the latest  users.
                foreach (Person person in rooms[room])
                {
                    if ((DateTime.Now - person.LastRefresh) < TimeSpan.FromHours(1))
                    {
                        result.Add(person);
                    }
                }

                return result;
            }
            return new List<Person>();
        }

        public void Dispose() { }
    }
}