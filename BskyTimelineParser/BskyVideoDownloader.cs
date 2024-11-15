using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace BskyTimelineParser
{
    /// <summary>
    /// This class contains a static method that download a video media from Bsky API
    /// </summary>
    public class BskyVideoDownloader
    {
        /// <summary>
        /// This method download the video of the highest resolution of a video playlist and merge the video clip to a single video file
        /// </summary>
        /// <param name="playlistUrl">The URL of the playlist</param>
        /// <returns>a Task</returns>
        /// <exception cref="IOException">Occurred if 404 is returned from the server</exception>
        public static async Task StartDownloadVideo(string playlistUrl)
        {
            var client = new HttpClient();
            var httpResponse = await client.GetAsync(playlistUrl);
            if (httpResponse.StatusCode == HttpStatusCode.NotFound)
                throw new IOException("404 File not found");
            StreamReader reader = new StreamReader(httpResponse.Content.ReadAsStream());
            string line = null;
            string videoPlayListUrl = null;
            while ((line = reader.ReadLine()) != null)
            {
                if (line.StartsWith("#")) continue;
                if (line != "") videoPlayListUrl = line;
            }
            reader.Close();

            httpResponse.Dispose();

            string baseUrl = playlistUrl.Substring(0, playlistUrl.LastIndexOf("/") + 1);

            httpResponse = await client.GetAsync(baseUrl + videoPlayListUrl);
            if (httpResponse.StatusCode == HttpStatusCode.NotFound)
                throw new IOException("404 File not found");
            reader = new StreamReader(httpResponse.Content.ReadAsStream());
            List<string> tsList = new List<string>();
            while ((line = reader.ReadLine()) != null)
            {
                if (line.StartsWith("#")) continue;
                if (line != "") tsList.Add(line);
            }
            reader.Close();

            httpResponse.Dispose();

            baseUrl = baseUrl + videoPlayListUrl;
            baseUrl = baseUrl.Substring(0, baseUrl.LastIndexOf("/") + 1);

            FileStream videoOutput = File.Create("testvideooutput_" + DateTime.Now.ToFileTime() + ".ts");
            bool isFirst = true;
            foreach(string videoClip in tsList)
            {
                httpResponse = await client.GetAsync(baseUrl + videoClip);
                byte[] data = await httpResponse.Content.ReadAsByteArrayAsync();
                int adaptationFieldSize = 0;
                // Check if the file has the adpatation fields header
                if((data[3] & 0x10) != 0x10)
                {
                    Console.WriteLine("This file contains adapatation field");
                    adaptationFieldSize = data[4] + 1;
                }
                Console.WriteLine("The sequence of this file: " + (data[3] & 0x0F));
                videoOutput.Write(data, isFirst ? 0 : 4 + adaptationFieldSize, data.Length - 4 - adaptationFieldSize);
                isFirst = false;
                httpResponse.Dispose();
            }
            videoOutput.Close();
        } 
    }
}
