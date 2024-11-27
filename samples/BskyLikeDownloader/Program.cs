using EbonCorvin.Utils;
using EbonCorvin.BskyTimelineParser;
using EbonCorvin.BskyTimelineParser.BskyAPIModels;
using EbonCorvin.BskyTimelineParser.Models;
using System.Text;

ConfigLoader config = new ConfigLoader("config.txt");
string accessToken;
string refreshToken;
string did;
BskyToken token = null;
bool requireLogin = false;
if (!config.IsFileExists)
{
    config["savePath"] = "saved";
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

BskyParser parser = new BskyParser(token, TimelineTypes.LikedPost);
string savePath = config["savePath"];
savePath = savePath ?? "";
Directory.CreateDirectory(savePath);
HttpClient client = new HttpClient();
while (true)
{
    Post[] posts = await parser.Next();
    if (posts.Length == 0) break;
    foreach(var post in posts)
    {
        if (post.Medias == null) continue;
        foreach(var media in post.Medias)
        {
            //Console.WriteLine(media.MediaType);
            //Console.WriteLine(media.Url);
            string url = media.Url;
            string fileName = "";
            if (media.MediaType == MediaTypes.VideoPlayList)
            {
                int lastSlashIdx = 0;
                lastSlashIdx = url.LastIndexOf("/");
                int secondLastSlashIdx = url.LastIndexOf("/", lastSlashIdx - 1);
                fileName = url.Substring(secondLastSlashIdx + 2, lastSlashIdx - secondLastSlashIdx - 2) + ".ts";
            }
            else if (media.MediaType == MediaTypes.Image)
            {
                int lastSlashIdx = url.LastIndexOf("/") + 1;
                int lastAtIdx = url.LastIndexOf("@");
                fileName = url.Substring(lastSlashIdx, lastAtIdx - lastSlashIdx) + "." + url.Substring(lastAtIdx + 1);
            }
            else continue;
            if (File.Exists(Path.Combine(savePath, fileName))) continue;
            Console.WriteLine(fileName);
            switch (media.MediaType)
            {
                case MediaTypes.VideoPlayList:
                    await BskyVideoDownloader.StartDownloadVideo(url, fileName, savePath);
                    break;
                case MediaTypes.Image:
                    var response = await client.GetAsync(url);
                    File.WriteAllBytes(Path.Combine(savePath, fileName), await response.Content.ReadAsByteArrayAsync());
                    break;
            }
        }
    }
    Console.WriteLine("Next Batch!");
}
Console.WriteLine("Done fetching feed!");
Console.ReadLine();