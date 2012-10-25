using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SaveForWeb
{
    public class Log
    {
        public string Messasge { get; set; }
        public bool Error { get; set; }

        public Log(string message, bool error)
        {
            this.Messasge = message;
            this.Error = error;
        }
    }
}
