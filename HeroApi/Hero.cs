using System;
using System.Collections.Generic;
using System.Text;

namespace HeroApi
{
    class Hero
    {
        public Hero(int _id, string _name, string _role)
        {
            id = _id;
            name = _name;
            role = _role;
        }
        public int id { get; set; }
        public string name { get; set; }
        public string role { get; set; }
    }
}
