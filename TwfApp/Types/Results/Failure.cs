using System;
using System.Collections.Generic;
using System.Text;

namespace TwfApp.Types.Results
{
    public class Failure(string msg)
    {
        public string Message { get; } = msg;
    }
}
