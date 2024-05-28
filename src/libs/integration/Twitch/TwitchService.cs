using System.Net.Sockets;

namespace LiveStreamManager.Integration.Twitch;
public static class Extensions
{
    public async static Task WriteLineAndFlushAsync(this StreamWriter sw, string message)
    {
        await sw.WriteLineAsync(message);
        await sw.FlushAsync();
    }
}

/// <summary>
/// Class responsible for handling functionalities related to the twitch service.
/// </summary>
public class TwitchService
{
    private TcpClient? _client { get; set; }
    private StreamReader? _reader { get; set; }
    private StreamWriter? _writer { get; set; }

    /// <summary>
    /// Twitch service host.
    /// </summary>
    public const string TWITCH_HOST = "irc.chat.twitch.tv";
    /// <summary>
    /// Twitch service port.
    /// </summary>
    public const int TWITCH_PORT = 6667;

    private readonly TwitchSettings _credentials;

    /// <summary>
    /// Handlers received messages. Must use  custom implementation.
    /// </summary>
    public event EventHandler<MessageReceivedEventArgs> MessageReceived;

    /// <summary>
    /// Constructor to initialize threads.
    /// </summary>
    public TwitchService(
        TwitchSettings credentials)
    {
        this._credentials = credentials;
        this.MessageReceived += PongMessageReceivedHandler!;
    }

    /// <summary>
    /// Reads incoming messages from the chat.
    /// </summary>
    protected virtual void OnMessageReceived(MessageReceivedEventArgs e)
    {
        MessageReceived?.Invoke(this, e);
    }

    /// <summary>
    /// Connect to twitch service with provided username and password.
    /// </summary>
    /// <param name="channel">Channel for the bot to write messages and/or moderate.</param>
    /// <remarks>Passwords meaning your provided token.</remarks>
    /// <returns>True whether could connect otherwise false.</returns>
    public async Task ConnectAsync()
    {
        try
        {
            _client = new TcpClient(TWITCH_HOST, TWITCH_PORT);
            _reader = new StreamReader(_client.GetStream());
            _writer = new StreamWriter(_client.GetStream());

            // logs in
            await _writer.WriteLineAndFlushAsync(
                    "PASS " + _credentials.Password + Environment.NewLine +
                    "NICK " + _credentials.UserName + Environment.NewLine +
                    "USER " + _credentials.UserName + " 8 * :" + _credentials.UserName
                );

            // shows how many users are logged on chat, shows online mods
            await _writer.WriteLineAndFlushAsync(
                "CAP REQ :twitch.tv/membership"
            );
            // join chatroom
            await _writer.WriteLineAndFlushAsync(
                $"JOIN #{_credentials.Channel.ToLower()}"
            );

        }
        catch (Exception)
        {
            throw;
        }
    }

    /// <summary>
    /// Reconnect to twitch service with provided username/password/channel.
    /// </summary>
    /// <param name="channel">Channel for the bot to write messages and/or moderate.</param>
    /// <remarks>Passwords meaning your provided token.</remarks>
    public async Task ReconnectAsync(string channel)
    {
        Disconnect();
        await ConnectAsync();
    }

    /// <summary>
    /// Disconnects bot from a twitch channel.
    /// </summary>
    /// <param name="channel">Channel name.</param>
    public void Disconnect()
    {
        _reader!.Dispose();
        _writer!.Dispose();
        _client!.Dispose();
    }

    /// <summary>
    /// Write a message to twitch chat.
    /// </summary>
    /// <param name="message">Message to send.</param>
    public async Task SendMessageAsync(string message, string channel)
    {
        if (!string.IsNullOrWhiteSpace(message))
        {
            var msgFormat =
                $":{_credentials.UserName.ToLower()}!{_credentials.UserName.ToLower()}@{_credentials.UserName.ToLower()}.tmi.twitch.tv PRIVMSG #{channel.ToLower()} :{message}";

            await _writer!.WriteLineAndFlushAsync(msgFormat);
        }
    }

    public async Task ReadMessagesAsync()
    {
        if (_client!.Available > 0 || _reader!.Peek() >= 0)
        {
            var message = await _reader!.ReadLineAsync();
            OnMessageReceived(new MessageReceivedEventArgs(message!, _credentials.Channel));
        }
    }

    /// <summary>
    /// Handles server ping request.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="message"></param>
    private void PongMessageReceivedHandler(object sender, MessageReceivedEventArgs e)
    {
        if (e.FromSystemFormat.Contains("ping", StringComparison.OrdinalIgnoreCase))
            this.SendMessageAsync("PONG :tmi.twitch.tv", e.Channel).Wait();
    }
}
