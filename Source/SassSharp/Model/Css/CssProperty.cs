﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crondale.SassSharp.Model.Css
{
    class CssProperty
    {
        public string Name { get; set; }
        public string Value { get; set; }

        public int Level { get; set; }

        public CssProperty(string name, string value, int level)
        {
            Level = level;
            Name = name;
            Value = value;
        }
    }
}
