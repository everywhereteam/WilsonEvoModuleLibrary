using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WilsonEvoModuleLibrary.Entities;

namespace WilsonEvoModuleLibrary.Utility
{
    public static class SessionDataUtility
    {
        public static void Error(this SessionData session, string message)
        {
            session.Output = "error";
            session.Exception = message;
        }
    }
}
