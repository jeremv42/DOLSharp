using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using DOL.Events;
using DOL.GS.PacketHandler;
using DOL.GS.ServerProperties;


namespace DOL.GS.Scripts;

public class DiscordBot
{
	[ServerProperty("server", "discord_bot_token", "The token to connect the Discord bot", "")]
	public static string DISCORD_BOT_TOKEN = "";
	[ServerProperty("server", "discord_bot_channel_broadcast", "The channel used to connect /b to Discord", "bot-tests")]
	public static string DISCORD_BOT_CHANNEL_BROADCAST = "";
	
	private DiscordSocketClient _client;
	private SocketTextChannel _channelBroadcast;
	private DiscordBot()
	{
		// use Init()
	}

	public void SendMessageBroadcast(GamePlayer author, string message)
	{
		if (_channelBroadcast == null)
			return;
		_channelBroadcast.SendMessageAsync($"[{author.Name}] {message}"); // fire and forget
	}
	private Task _MessageReceived(SocketMessage msg)
	{
		if (msg.Channel.Id != _channelBroadcast?.Id || msg.Author is not SocketGuildUser author)
			return Task.CompletedTask;
		if (author.IsBot)
			return Task.CompletedTask;

		var formattedMessage = $"[Discord] {author.DisplayName}: {msg.Content}";
		if (string.IsNullOrWhiteSpace(msg.Content))
		{
			formattedMessage = $"[Discord] {author.DisplayName} sent a non text message.";
		}
		foreach (var client in WorldMgr.GetAllPlayingClients())
			client.Player?.SendMessage(formattedMessage, eChatType.CT_Broadcast, eChatLoc.CL_ChatWindow);
		return Task.CompletedTask;
	}

	public static DiscordBot Instance { get; private set; }
	public static async Task Init()
	{
		if (Instance != null)
			throw new Exception("DiscordBot is already initialized");
		if (string.IsNullOrWhiteSpace(DISCORD_BOT_TOKEN))
			return;

		var instance = new DiscordBot();
		instance._client = new DiscordSocketClient(new DiscordSocketConfig { GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.MessageContent });
		instance._client.MessageReceived += instance._MessageReceived;
		instance._client.Log += msg => Console.Error.WriteLineAsync($"[{msg.Source}/{msg.Severity}] {msg.Message}");
		instance._client.GuildAvailable += instance._InitDiscordGuild;
		await instance._client.LoginAsync(TokenType.Bot, DISCORD_BOT_TOKEN);
		await instance._client.StartAsync();
		Instance = instance;

		Console.WriteLine("DiscordBot started");
	}

	private async Task _InitDiscordGuild(SocketGuild arg)
	{
		var channelBroadcast = arg.TextChannels.FirstOrDefault(chan => chan.Name == DISCORD_BOT_CHANNEL_BROADCAST);
		if (channelBroadcast == null)
			return;
		_channelBroadcast = channelBroadcast;
		if ((DateTime.Now - Process.GetCurrentProcess().StartTime).TotalMinutes < 5) // 5min after start max
			await _channelBroadcast.SendMessageAsync("Server open!");
	}

	public static async Task Stop()
	{
		if (Instance == null)
			return;
		if (Instance._channelBroadcast != null)
			await Instance._channelBroadcast.SendMessageAsync("Server closed!");
		await Instance._client.LogoutAsync();
		await Instance._client.StopAsync();
		Instance = null;
	}

	[GameServerStartedEvent]
	public static void ServerInit(DOLEvent _e, object _sender, EventArgs _args)
	{
		try
		{
			Init().Wait();
		}
		catch (Exception e)
		{
			Console.Error.WriteLine(e);
			throw;
		}
	}
	[GameServerStoppedEvent]
	public static void ServerStop(DOLEvent _e, object _sender, EventArgs _args)
	{
		try
		{
			Stop().Wait();
		}
		catch (Exception e)
		{
			Console.Error.WriteLine(e);
			throw;
		}
	}
}
