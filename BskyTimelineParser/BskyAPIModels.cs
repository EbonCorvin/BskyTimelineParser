namespace EbonCorvin.BskyTimelineParser.BskyAPIModels
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

	public class BskyFeed
	{
		public string uri { get; set; }
		public string did { get; set; }
		public string displayName { get; set; }
		public string description { get; set; }
	}
}