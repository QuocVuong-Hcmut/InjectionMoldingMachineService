using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InjectionMoldingMachineDataAcquisitionService.Communication.Messages
{
    public class DaMessage
    {
        public string Name { get; private set; }
        public object Value { get; private set; }

        public DaMessage (string name,object value)
        {
            Name=name;
            Value=value;
        }
    }
}
