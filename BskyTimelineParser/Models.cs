using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EbonCorvin.BskyTimelineParser.Models
{
    public class BskyToken
    {
        public string did { get; set; } 
        public string accessJwt { get; set; } 
        public string refreshJwt { get; set; }
    }

    public class BskyLoginParams
    {
        public string Identifier { get; set; }
        public string Password { get; set; }
        public string AuthFactorToken { get; set; } = "";
    }

    internal class BskyError
    {
        public string error { get; set; }
        public string message { get; set; }
    }

    /// <summary>
    /// Represent an error returned from the Bsky's API. <br/>
    /// Check <b>Message</b> for error code and <b>BskyDetailMessage</b> for the detail message returned by the API.
    /// </summary>
    public class BskyErrorException : Exception
    {
        public string BskyDetailMessage { get; set; }
        public BskyErrorException(string message, string detailMessage) : base(message)
        {
            BskyDetailMessage = detailMessage;
        }
    }
    public class TokenErrorException : Exception
    {
        public TokenErrorException(String message) : base(message)
        {}
    }

    public class Post
    {
        public string Author { get; set; }
        public string PostId { get; set; }
        public string Content { get; set; }
        public string CreateDate { get; set; }
        public string PostUrl { get; set; }
        public Media[] Medias { get; set; }
        public string MediaJoined { get; set; }
    }

    public class Media
    {
        public string MediaType { get; set; }
        public string Url { get; set; }
    }

    public struct TestStructure
    {
        public string name { get; set; }
        public string password { get; set; }
    }
}
