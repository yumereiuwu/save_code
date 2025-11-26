using Microsoft.AspNetCore.SignalR;

namespace CodeShowcase.Api;

public sealed class CodeHub : Hub
{
    public const string EventName = "codesUpdated";
}

