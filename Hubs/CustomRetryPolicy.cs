using System;
using Microsoft.AspNetCore.SignalR.Client;

namespace WilsonEvoModuleLibrary.Hubs;

public class CustomRetryPolicy : IRetryPolicy
{
    public TimeSpan? NextRetryDelay(RetryContext retryContext)
    {
        return TimeSpan.FromSeconds(5);
    }
}