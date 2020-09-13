using System;
using System.Collections.Generic;
using System.Text;

namespace AutofacTool.Entities
{
    class MainClass
    {
        private readonly ISomeService _someService;

        public MainClass(string some, ISomeService someService)
        {
            _someService = someService;
        }

        public void Do()
        {
            _someService.DoSomething();
        }
    }
}
