using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CQRS.Light.Tutorial.Commands
{
    public class OpenTab
    {
        public Guid TabId;
        public int TableNumber;
        public string Waiter;
    }
}
