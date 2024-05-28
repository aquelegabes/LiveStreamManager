namespace LiveStreamManager.Integration.Twitch;
/// <summary>
/// Class responsible for handling messages.
/// </summary>
public class MessageHandler
{
    /// <summary>
    /// Twitch service.
    /// </summary>
    private readonly TwitchService service;
    private Credentials credentials { get; }
    private readonly TimeSpan RespondToAtSpan = new TimeSpan(0, 0, 15);
    /// <summary>
    /// Last time someone @me.
    /// </summary>
    private DateTime LastAtMe { get; set; }

    public MessageHandler(
        TwitchService service,
        Credentials credentials)
    {
        this.service = service;
        this.credentials = credentials;
    }

    

    /// <summary>
    /// Handles that when people @me I respond.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public async Task AtBotMessageReceivedHandler(object sender, MessageReceivedEventArgs e)
    {
        if (DateTime.Now.ToUniversalTime() > LastAtMe.Add(RespondToAtSpan))
        {
            if (e.IsUserMessage
                && e?.UsrMessage.Message.Contains(credentials.UserName, StringComparison.OrdinalIgnoreCase) == true)
            {
                await this.service.SendMessageAsync("Don't @me bro! I'm trying to working here. DansGame", e.Channel);
                LastAtMe = DateTime.Now.ToUniversalTime();
            }
        }
    }
}
