using LiveStreamManager.Integration.Twitch;

string user = Environment.GetEnvironmentVariable("TWITCH_USER", EnvironmentVariableTarget.Machine)!;
string passw = Environment.GetEnvironmentVariable("TWITCH_OAUTH_PASSWORD", EnvironmentVariableTarget.Machine)!;
string channel = Environment.GetEnvironmentVariable("TWITCH_CHANNEL", EnvironmentVariableTarget.Machine)!;

var settings = new TwitchSettings
{
    Channel = channel.ToLower(),
    UserName = user.ToLower(),
    Password = passw
};

var service = new TwitchService(settings);
service.ConnectAsync().Wait();

service.MessageReceived += (sender ,e) => {
    Console.WriteLine($"[{settings.Channel}] [{DateTime.Now}]: {e}");
};

while (true) {
    service.ReadMessagesAsync().Wait();
}