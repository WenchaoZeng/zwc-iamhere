using System;
using System.Collections.Generic;
using System.Web;
using System.Text;

namespace IAmHere.API
{
    /// <summary>
    /// Summary description for submit
    /// </summary>
    public class submit : IHttpHandler
    {
        public void ProcessRequest(HttpContext context)
        {
            // parameters: room, name, x, y
            string room = context.Request.QueryString["room"];
            Person me = new Person();
            me.Name = context.Request.QueryString["name"];
            {
                double value = 0.0;
                double.TryParse(context.Request.QueryString["x"], out value);
                me.X = value;
            }
            {
                double value = 0.0;
                double.TryParse(context.Request.QueryString["y"], out value);
                me.Y = value;
            }
            List<Person> allPersons = new List<Person>();
            IRepository repository = null;
            if (!string.IsNullOrEmpty(context.Request.QueryString["persist"]) && context.Request.QueryString["persist"].ToLower() == "true")
            {
                repository = new MSAccessRepository(HttpContext.Current.Server.MapPath("~/App_Data/IAmHere.mdb"));
            }
            else
            {
                repository = new MemoryRepository();
            }

            using (repository)
            {
                repository.Update(room, me);
                allPersons = repository.AllPersons(room);
            }

            // return: a list of name, x, y
            context.Response.ContentType = "text/plain";
            StringBuilder builder = new StringBuilder();
            foreach (Person person in allPersons)
            {
                builder.AppendFormat("{0},{1},{2}\n", person.Name, person.X, person.Y);
            }
            if (builder.Length > 0)
            {
                builder.Remove(builder.Length - 1, 1);
            }
            context.Response.Write(builder.ToString());
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }
}