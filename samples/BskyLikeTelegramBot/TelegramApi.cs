using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using TwitterLike_Telegram_bot.Model;
using System.IO;
using System.Text.Json;
using System.Text.RegularExpressions;
using EbonCorvin.BskyTimelineParser.Models;
using System.Net.Http.Json;
using EbonCorvin.Utils;
using System.Net.Http.Headers;

namespace TwitterLike_Telegram_bot
{
    public class TelegramApi
    {
        private static HttpClient httpClient = null;
        private const String TELEGRAM_API_URL = "https://api.telegram.org/bot{0}/{1}";
        //private const String TELEGRAM_API_URL = "https://posttestserver.dev/p/00tb5a1wszdyjctd/post/{0}/{1}";
        private static String apikey = null;

        public static void SetApiKey(String key)
        {
            apikey = key;
        }

        public static async Task SendMessage(String target, String content, SendTextBody options = null)
        {
            if (options == null)
                options = new SendTextBody();
            options.chat_id = target;
            options.text = EscapeText(content);
            String reqBody = JsonSerializer.Serialize(options, new JsonSerializerOptions()
            {
                PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
            });
            await SendRequest("sendMessage", reqBody);
        }

        private static String EscapeText(String text)
        {
            Regex regex = new Regex("([_*\\[\\]()~`>#+-=|{}.!])", RegexOptions.Compiled);
            return regex.Replace(text, new MatchEvaluator((m)=>"\\"+m.Value));
        }

        public static async Task SendPhoto(String target, String imageUrl, String caption, SendPhotoBody options = null)
        {
            if (options == null)
                options = new SendPhotoBody();
            options.chat_id = target;
            options.caption = EscapeText(caption);
            options.photo = imageUrl;
            String reqBody = JsonSerializer.Serialize(options, new JsonSerializerOptions()
            {
                PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
            });
            await SendRequest("sendPhoto", reqBody);
        }

        public static async Task SendVideo(String target, String videoUrl, String caption, SendMessageBodyBase options = null)
        {
            if (options == null)
                options = new SendMessageBodyBase();
            string videoLocalFileName = Guid.NewGuid().ToString();
            string videoLocalPath = Path.GetTempPath() + "TelegramBot";
            await BskyVideoDownloader.StartDownloadVideo(videoUrl, videoLocalFileName, videoLocalPath);
           
            options.chat_id = target;
            options.caption = caption;
            //await SendRequest("sendVideo", options, File.OpenRead(videoLocalPath + "/" + videoLocalFileName + ".ts"));
            // Send the video as a document for now because we need to convert the video to mp4 in order to play on Telegram directly
            await SendRequest("sendDocument", options, File.OpenRead(videoLocalPath + "/" + videoLocalFileName + ".ts"));
        }

        public static async Task SendGroupMedia(String target, GroupMediaItem[] items, String caption, SendGroupMediaBody options = null)
        {
            if (options == null)
                options = new SendGroupMediaBody();
            options.chat_id = target;
            options.media = items;
            String reqBody = JsonSerializer.Serialize(options, new JsonSerializerOptions()
            {
                PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
            });
            await SendRequest("sendMediaGroup", reqBody);
        }

        public static async Task SendText(String target, String text, SendTextBody options = null)
        {
            if (options == null)
                options = new SendTextBody();
            options.chat_id = target;
            options.text = EscapeText(text);
            String reqBody = JsonSerializer.Serialize(options, new JsonSerializerOptions()
            {
                PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
            });
            await SendRequest("sendMessage", reqBody);
        }

        public static async Task<string> SendRequest(string endpoint, string json)
        {
            if (httpClient == null)
            {
                httpClient = new HttpClient();
            }
            var jsonByte = Encoding.UTF8.GetBytes(json);
            var content = new ByteArrayContent(jsonByte);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            var response = await httpClient.PostAsync(String.Format(TELEGRAM_API_URL, apikey, endpoint), content);
            string responseAsString = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
                throw new WebException(responseAsString);
            return await response.Content.ReadAsStringAsync();
        }

        public static async Task<string> SendRequest(string endpoint, SendMessageBodyBase json, FileStream file)
        {
            if (httpClient == null)
            {
                httpClient = new HttpClient();
            }
            var content = new MultipartFormDataContent("--------UPLAD" + DateTime.Now.Ticks);

            var jsonBytes = Encoding.UTF8.GetBytes(json.caption);
            var jsonContent = new ByteArrayContent(jsonBytes, 0, jsonBytes.Length);
            jsonContent.Headers.ContentType = MediaTypeHeaderValue.Parse("text/plain");
            content.Add(jsonContent, "caption");

            var videoContent = new StreamContent(file);
            videoContent.Headers.ContentType = MediaTypeHeaderValue.Parse("video/mp4");
            content.Add(videoContent, "document", "video.ts");

            var response = await httpClient.PostAsync(String.Format(TELEGRAM_API_URL, apikey, endpoint) + "?chat_id=" + json.chat_id, content);
            string responseAsString = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
                throw new WebException(responseAsString);
            return await response.Content.ReadAsStringAsync();
        }
    }
}
