namespace LiveStreamManager.Integration.Twitch;
/// <summary>
/// Message received event arguments.
/// </summary>
public class MessageReceivedEventArgs : EventArgs
{
    /// <summary>
    /// Constructor responsible for initializing a <see cref="MessageReceivedEventArgs"/>.
    /// </summary>
    /// <param name="msg">System received message.</param>
    public MessageReceivedEventArgs(string msg, string channel)
    {
        this.Channel = channel;
        this.FromSystemFormat = msg;

        if (msg.Contains("PRIVMSG"))
        {
            IsUserMessage = true;
            int userLength = Math.Abs(msg.IndexOf('@') + 1 - (msg.IndexOf(".tmi") + 1));
            UserMessage = new UserMessage
            {
                User = msg.Substring(msg.IndexOf('@'), userLength),
                // +3 cause (space, ':' and length +1)
                Message = msg.Substring(msg.IndexOf('#') + Channel.Length + 3)
            };
        }
    }

    public string Channel { get; private set; } = string.Empty;
    public string FromSystemFormat { get; private set; } = string.Empty;
    public bool IsUserMessage { get; private set; } = false;
    public UserMessage UserMessage { get; private set; } = new();

    /// <summary>
    /// Returns a string that represents current object.
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return this.FromSystemFormat;
    }
}
