﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SassSharp.Model.Nodes
{
    class ImportNode:CodeNode
    {
        public string Path { get; set; }

        public ImportNode(string path)
        {
            Path = path;
        }
    }
}
