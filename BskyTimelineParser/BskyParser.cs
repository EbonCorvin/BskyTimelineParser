using EbonCorvin.BskyTimelineParser.BskyAPIModels;
using EbonCorvin.BskyTimelineParser.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace EbonCorvin.BskyTimelineParser
{
    public enum TimelineTypes
    {
        LikedPost, Following, Feed
    }
    public class BskyParser
    {
        private static readonly string[] BSKY_API_ENDPOINT =
        {
            "https://oyster.us-east.host.bsky.network/xrpc/app.bsky.feed.getActorLikes?actor={0}&limit=30",
            "https://oyster.us-east.host.bsky.network/xrpc/app.bsky.feed.getTimeline",
            "https://bsky.social/xrpc/app.bsky.feed.getFeed?feed={0}"
        };
        private const string BSKY_POST_URL = "https://bsky.app/profile/{0}/post/{1}";
        private string apiEndPoint;
        private TimelineTypes timelineType = default;
        public BskyToken Token { get; set; }
        public string Target
        {
            set
            {
                apiEndPoint = String.Format(BSKY_API_ENDPOINT[(int)timelineType], value);
            }
        }
        private string NextCursor { get; set; }
        private HttpClient httpClient = null;
        /// <summary>
        /// Create a new Bsky post parser.
        /// </summary>
        /// <param name="token">A valid Bsky user token</param>
        /// <param name="timelineType">The timeline to fetch and parse.</param>
        /// <param name="target"><para>The person you want to grab liked post from, or the feed you want to get post from</para><para>If not supplied, the parser will grab your own liked post, or your discover feed</para></param>
        public BskyParser(BskyToken token, TimelineTypes timelineType, string? target = "")
        {
            Token = token;
            this.timelineType = timelineType;
            if (timelineType == TimelineTypes.LikedPost || timelineType == TimelineTypes.Feed)
            {
                if (target == "" || target == null)
                {
                    if (timelineType == TimelineTypes.Feed)
                        Target = "at://did:plc:z72i7hdynmk6r22z27h6tvur/app.bsky.feed.generator/whats-hot";
                    else
                        Target = token.did;
                }
                else
                    Target = target;
            }
            else
            {
                apiEndPoint = BSKY_API_ENDPOINT[(int)timelineType];
            }
            httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.accessJwt);
        }
        /// <summary>
        /// Start to fetch and parse the posts of the next page each time it's called.
        /// </summary>
        /// <returns></returns>
        public async Task<Post[]> Next()
        {
            HttpResponseMessage response = await httpClient.GetAsync(apiEndPoint);
            string json = await response.Content.ReadAsStringAsync();
            JsonDocument doc = JsonDocument.Parse(json);
            NextCursor = doc.RootElement.GetProperty("cursor").GetString();
            //Console.WriteLine("Next cursor: " + NextCursor);
            JsonElement posts = doc.RootElement.GetProperty("feed");
            int postCount = posts.GetArrayLength();
            //Console.WriteLine(postCount + " post(s) fetched");
            Post[] returnPost = new Post[postCount];
            for (var i = 0; i < postCount; i++)
            {
                JsonElement post = posts[i].GetProperty("post");
                string rawUri = post.GetProperty("uri").ToString();
                string postId = rawUri.Substring(rawUri.LastIndexOf("/") + 1);
                JsonElement authorJson = post.GetProperty("author");
                string author = "";
                string handle = authorJson.GetProperty("handle").GetString();
                // displayName will not present if user doesn't have a displayName
                if (!authorJson.TryGetProperty("displayName", out var displayName))
                {
                    author = handle;
                }
                else
                {
                    author = displayName.GetString();
                }
                JsonElement postRecord = post.GetProperty("record");
                string text = postRecord.GetProperty("text").GetString();
                string createdAt = postRecord.GetProperty("createdAt").GetString();
                Media[] mediaList = null;
                if (post.TryGetProperty("embed", out var embedded))
                {
                    string mediaType = embedded.GetProperty("$type").GetString();
                    // recordWithMedia is post that has both embedded media and embedded post, mostly happen when user is replying a post with media.
                    if (mediaType == "app.bsky.embed.recordWithMedia#view")
                    {
                        embedded = embedded.GetProperty("media");
                        mediaType = embedded.GetProperty("$type").GetString();
                        //Console.WriteLine("Post with embedded post and media " + i);
                    }

                    if (mediaType == "app.bsky.embed.images#view")
                    {
                        var images = embedded.GetProperty("images");
                        int imgCount = images.GetArrayLength();
                        mediaList = new Media[imgCount];
                        for (var j = 0; j < imgCount; j++)
                        {
                            var url = images[j].GetProperty("fullsize").GetString();
                            mediaList[j] = new Media() { MediaType = "image", Url = url };
                        }
                    }
                    else if(mediaType == "app.bsky.embed.video#view")
                    {
                        // playlist is playlist that contain playlists for different resolution of the same video
                        // And each resolution of video is splitted into multiple short video clips
                        mediaList = new Media[]
                        {
                            new Media()
                            {
                                MediaType = "videoPlayList",
                                Url = embedded.GetProperty("playlist").GetString()
                            }
                        };
                    }
                }
                else
                {
                    //Console.WriteLine("This post has no embedded media");
                }
                returnPost[i] = new Post()
                {
                    Medias = mediaList,
                    // MediaJoined = mediaList != null ? string.Join(',', mediaList.Select(new Func<Media, int, string>((m,c)=>m.Url))) : "",
                    PostId = postId,
                    PostUrl = String.Format(BSKY_POST_URL, handle, postId),
                    Author = author,
                    CreateDate = createdAt,
                    Content = text
                };
            }
            return returnPost;
        }
    }
}
