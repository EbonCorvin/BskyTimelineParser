using BskyTimelineParser;
using EbonCorvin;
using EbonCorvin.BskyTimelineParser;
using EbonCorvin.BskyTimelineParser.BskyAPIModels;
using EbonCorvin.BskyTimelineParser.Models;
using System.Text;

ConfigLoader config = new ConfigLoader("config.txt");
string accessToken;
string refreshToken;
string did;
BskyToken token = null;
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
    if(!(await LoginHandler.AsyncStartCheckSession(token)))
    {
        token = await LoginHandler.AsyncRefreshToken(token);
        config["accessJwt"] = token.accessJwt;
        config["refreshJwt"] = token.refreshJwt;
        config["did"] = token.did;
    }
}
catch (Exception ex)
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
                Console.Read();
                break;
            }
        }
    } while (retry);
}
BskyParser parser = new BskyParser(token, TimelineTypes.LikedPost);
Post[] posts = await parser.Next();
foreach(var post in posts)
{
    if (post.Medias == null) continue;
    foreach(var media in post.Medias)
    {
        Console.WriteLine(media.MediaType);
        Console.WriteLine(media.Url);
        if(media.MediaType == "videoPlayList")
        {
            await BskyVideoDownloader.StartDownloadVideo(media.Url);
        }
    }
}
Console.WriteLine("Done");
Console.ReadLine();