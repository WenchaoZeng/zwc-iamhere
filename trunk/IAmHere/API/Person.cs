using System;
using System.Collections.Generic;
using System.Web;

namespace IAmHere.API
{
    public class Person
    {
        public string ID { get; set; }
        public string Name { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public DateTime LastRefresh { get; set; }
    }
}