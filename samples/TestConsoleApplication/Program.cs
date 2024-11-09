// See https://aka.ms/new-console-template for more information
using EbonCorvin;
using EbonCorvin.BskyTimelineParser;
using EbonCorvin.BskyTimelineParser.Models;
using System.Runtime.Intrinsics.Arm;

ConfigLoader config = new ConfigLoader("config.txt");
BskyToken token = new BskyToken()
{
    did = config["did"],
    accessJwt = config["accessJwt"],
    refreshJwt = config["refreshJwt"]
};
bool istokenValid = (token.accessJwt != null && token.refreshJwt != null) && await LoginHandler.AsyncStartCheckSession(token);
if (!istokenValid)
{
    try
    {
        Console.WriteLine("Token is expired");
        token = await LoginHandler.AsyncRefreshToken(token);
    }
    catch (Exception ex2)
    {
        Console.WriteLine("Failed to refresh token, please login:");
        bool retry = false;
        do
        {
            Console.Write("Identifier: ");
            string? identifier = Console.ReadLine();
            Console.Write("Password: ");
            string? password = Console.ReadLine();
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
                    return;
                }
            }
        } while (retry);

    }
    config["did"] = token.did;
    config["accessJwt"] = token.accessJwt;
    config["refreshJwt"] = token.refreshJwt;
}
BskyParser parser = new BskyParser(token, TimelineTypes.Following);
var posts = await parser.Next();
Console.Read();