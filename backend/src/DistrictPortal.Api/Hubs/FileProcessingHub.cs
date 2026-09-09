using Microsoft.AspNetCore.SignalR;

namespace DistrictPortal.Api.Hubs;

/// <summary>
/// Real-time channel that replaces client-side polling: once a submission finishes
/// validating, the server pushes the result straight to whoever is looking at that LEA's
/// dashboard, plus a bell notification to everyone connected.
/// </summary>
public sealed class FileProcessingHub : Hub
{
    /// <summary>Client method invoked with a <c>SubmissionStatusChangedMessage</c> once processing finishes.</summary>
    public const string SubmissionStatusChangedMethod = "SubmissionStatusChanged";

    /// <summary>Client method invoked with a <c>NotificationDto</c> whenever a new notification is created.</summary>
    public const string NotificationCreatedMethod = "NotificationCreated";

    private const string GlobalGroup = "global";

    public override async Task OnConnectedAsync()
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, GlobalGroup);
        await base.OnConnectedAsync();
    }

    /// <summary>Called by the client when it opens a given LEA's dashboard.</summary>
    public Task JoinLeaGroup(string leaId) => Groups.AddToGroupAsync(Context.ConnectionId, LeaGroupName(leaId));

    /// <summary>Called by the client when it navigates away from a given LEA's dashboard.</summary>
    public Task LeaveLeaGroup(string leaId) => Groups.RemoveFromGroupAsync(Context.ConnectionId, LeaGroupName(leaId));

    public static string LeaGroupName(string leaId) => $"lea:{leaId}";

    public static string GlobalGroupName() => GlobalGroup;
}
