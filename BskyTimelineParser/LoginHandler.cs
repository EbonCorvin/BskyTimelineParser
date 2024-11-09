using EbonCorvin.BskyTimelineParser.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace EbonCorvin.BskyTimelineParser
{
    public class LoginHandler
    {
        private static bool CheckTokenObjectAndThrow(BskyToken token)
        {
            if (token == null) throw new ArgumentNullException(nameof(token));
            if (token.accessJwt == null || token.accessJwt == "") throw new ArgumentNullException("Refresh token is null or an empty string!");
            if (token.refreshJwt == null || token.refreshJwt == "") throw new ArgumentNullException("Refresh token is null or an empty string!");
            return true;
        }
        /// <summary>
        /// Check if the token is valid. This method wouldn't refresh the token.
        /// </summary>
        /// <param name="token">The token object. It must have accessJwt and refreshJwt provided</param>
        /// <returns>a boolean value indicate if the token is valid</returns>
        public static async Task<bool> AsyncStartCheckSession(BskyToken token)
        {
            CheckTokenObjectAndThrow(token);
            HttpClient httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.accessJwt);
            try
            {
                BskyToken? userInfo = await httpClient.GetFromJsonAsync<BskyToken>("https://bsky.social/xrpc/com.atproto.server.getSession");
                // Console.WriteLine("Valid token!");
                return true;
            }
            catch (HttpRequestException)
            {
                return false;
            }
        }
        /// <summary>
        /// Get a new token with the refresh token
        /// </summary>
        /// <param name="token">The token object. It must have accessJwt and refreshJwt provided</param>
        /// <returns>A new token. Please save it for next use</returns>
        /// <exception cref="TokenErrorException"></exception>
        public static async Task<BskyToken> AsyncRefreshToken(BskyToken token)
        {
            CheckTokenObjectAndThrow(token);
            HttpClient httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.refreshJwt);
            HttpResponseMessage response = await httpClient.PostAsync("https://oyster.us-east.host.bsky.network/xrpc/com.atproto.server.refreshSession", null);
            if (!response.IsSuccessStatusCode)
            {
                throw new TokenErrorException("Refresh token is expired!");
            }
            BskyToken? newToken = await response.Content.ReadFromJsonAsync<BskyToken>();
            if (newToken != null)
            {
                return newToken;
            }
            else
            {
                throw new TokenErrorException("Unable to convert the response to a Token object!");
            }
        }
        /// <summary>
        /// Try to login with the provided username, password and two factors authentication code. <br/> 
        /// The method will throw <b>BskyErrorException</b> when error returned from Bsky's API. <br/>
        /// Including requiring of the two factor authentication code.
        /// </summary>
        /// <param name="loginParams"></param>
        /// <returns>A new token object if logged in successfully. Please save it for next use</returns>
        /// <exception cref="BskyErrorException"></exception>
        /// <exception cref="TokenErrorException"></exception>
        public static async Task<BskyToken> AsyncLogin(BskyLoginParams loginParams)
        {
            // TODO: Try to use SecureString or other security measure to prevent password from being kept in the memory
            HttpClient httpClient = new HttpClient();
            HttpResponseMessage response = await httpClient.PostAsJsonAsync("https://bsky.social/xrpc/com.atproto.server.createSession", loginParams);
            if (!response.IsSuccessStatusCode)
            {
                BskyError? errorMsg = await response.Content.ReadFromJsonAsync<BskyError>();
                if (errorMsg != null)
                {
                    throw new BskyErrorException(errorMsg.error, errorMsg.message);
                }
                else
                {
                    throw new TokenErrorException("Unable to convert the response to an object!");
                }
            }
            else
            {
                BskyToken? newToken = await response.Content.ReadFromJsonAsync<BskyToken>();
                if (newToken != null)
                {
                    return newToken;
                }
                else
                {
                    throw new TokenErrorException("Unable to convert the response to a Token object!");
                }
            }
        }
    }
}
