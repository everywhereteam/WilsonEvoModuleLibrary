﻿using WilsonEvoModuleLibrary.Entities;

namespace WilsonEvoModuleLibrary.Utility;

public static class SessionDataUtility
{
    public static void Error(this SessionData session, string message)
    {
        session.Output = "error";
        session.Exception = message;
    }
}