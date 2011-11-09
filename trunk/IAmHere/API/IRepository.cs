using System;
using System.Collections.Generic;
using System.Web;

namespace IAmHere.API
{
    public interface IRepository : IDisposable
    {
        void Update(string room, Person person);
        List<Person> AllPersons(string room);
    }
}