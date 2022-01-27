using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;
using Torch;

namespace N1ShittyCommands
{
    
    public class Core:TorchPluginBase
    {
        public static readonly Logger Log = LogManager.GetLogger("N1ShittyCommands");
    }
}
