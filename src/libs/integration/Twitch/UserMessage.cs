namespace LiveStreamManager.Integration.Twitch;
/// <summary>
/// Struct responsible for transforming a system sent message into a composed user message.
/// </summary>
public struct UserMessage
{
    public string Message { get; set; }
    public string User { get; set; }
    public string[] UserBadges { get; set; }
}