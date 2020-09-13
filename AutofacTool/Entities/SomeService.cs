using System;
using System.Collections.Generic;
using System.Text;

namespace AutofacTool.Entities
{
    class SomeService : ISomeService
    {
        public void DoSomething()
        {
            Console.WriteLine("Service doing this!!!");
        }
    }
}
