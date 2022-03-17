using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;

namespace Modsmith.Utils
{
    public class LaunchArgumentsInput
    {
        [Option('a', "appid", Required = false)]
        public uint AppId { get; set; }
    }
}
