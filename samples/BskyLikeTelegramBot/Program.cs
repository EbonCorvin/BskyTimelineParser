using EbonCorvin.BskyTimelineParser.BskyAPIModels;
using EbonCorvin.BskyTimelineParser.Models;
using EbonCorvin.BskyTimelineParser;
using EbonCorvin.Utils;
using System.Text;
using TwitterLike_Telegram_bot;

ConfigLoader config = new ConfigLoader("config.txt");
string accessToken;
string refreshToken;
string did;
BskyToken token = null;
bool requireLogin = false;
if (!config.IsFileExists)
{
    Console.WriteLine("Please provide your Bluesky login, Telegram API and channel for the first time");
    Console.Write("Telegram API key: ");
    string? apiKey = Console.ReadLine();
    Console.Write("Telegram channel handle: ");
    string? channel = Console.ReadLine();
    config["telegram_bot_apikey"] = apiKey;
    config["telegram_channel"] = channel;
    config["check_interval"] = "120";
    requireLogin = true;
}
else
{
    try
    {
        accessToken = config["accessJwt"];
        refreshToken = config["refreshJwt"];
        did = config["did"];
        token = new BskyToken()
        {
            did = config["did"],
            accessJwt = config["accessJwt"],
            refreshJwt = config["refreshJwt"]
        };
        if (!(await LoginHandler.AsyncStartCheckSession(token)))
        {

            token = await LoginHandler.AsyncRefreshToken(token);

            config["accessJwt"] = token.accessJwt;
            config["refreshJwt"] = token.refreshJwt;
            config["did"] = token.did;
        }
    }
    catch
    {
        requireLogin = true;
    }
}
if (requireLogin)
{
    Console.WriteLine("Please login for the first time. You only have to do it once.");
    Console.Write("Identifier: ");
    string? identifier = Console.ReadLine();
    StringBuilder sb = new StringBuilder();
    Console.Write("Password: ");
    do
    {
        char key = Console.ReadKey().KeyChar;
        if (key == '\r') break;
        sb.Append((char)key);
        Console.Write("\b");
        Console.Write("*");
    } while (true);
    Console.WriteLine();
    string password = sb.ToString();
    bool retry = false;
    do
    {
        string? authToken = "";
        if (retry)
        {
            Console.Write("Auth Token: ");
            authToken = Console.ReadLine();
        }
        BskyLoginParams loginInfo = new BskyLoginParams()
        {
            Identifier = identifier,
            Password = password,
            AuthFactorToken = authToken
        };
        try
        {
            token = await LoginHandler.AsyncLogin(loginInfo);
            config["accessJwt"] = token.accessJwt;
            config["refreshJwt"] = token.refreshJwt;
            config["did"] = token.did;
            break;
        }
        catch (BskyErrorException ex3)
        {
            if (ex3.Message == "AuthFactorTokenRequired")
            {
                Console.WriteLine("Your account enabled two factors authentication, please check your email box!");
                retry = true;
            }
            else
            {
                Console.WriteLine("Unable to login!");
                Console.WriteLine(ex3.Message);
                Console.WriteLine(ex3.BskyDetailMessage);
                token = null;
                Console.Read();
                break;
            }
        }
    } while (retry);
}

if (token == null) return;

TelegramBot bot = new TelegramBot(token, config);
await bot.Start();
Console.Read();