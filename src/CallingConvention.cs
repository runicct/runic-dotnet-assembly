using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Runic.Dotnet
{
    public partial class Assembly
    {
        public enum CallingConvention : byte
        {
            Default = 0x0,
            C = 0x1,
            StdCall = 0x2,
            ThisCall = 0x3,
            FastCall = 0x4,
            VarArgs = 0x5,
        }
    }
}
