using Microsoft.AspNetCore.Mvc;
using System.Net;
using TweetSharp;

namespace TeamProjectTest.Twitter
{
    [Route("api/[controller]")]
    [ApiController]
    public class TwittersController : ControllerBase
    {

        private static string _consumer_key = "meMixAylqWVR0dE52DKHAb315";
        private static string _consumer_key_secret = "L2Pa1lgfU5bHClBe6WYdLzPSukjWd78xPEJIB6F3FhjnFbqXZ9";
        private static string _access_token = "4856603181-6DUI4iSoU95O69hvaheuPztcIpWuPVjjUYQtBFR";
        private static string _access_token_secret = "ggHXGbVuage4PwIUyd4GG13Pv4ST7L1TRIR6wsEihCdv0";
        public static TwitterService twitterService = new TwitterService(_consumer_key, _consumer_key_secret, _access_token, _access_token_secret);

        [HttpPost]
        [Route("Post-Tweet")]
        public IActionResult PostTweet(Twitter model)
        {
            var tweet = model.TweetText;

            SendTweet(tweet);

            static void SendTweet(string _status)
            {
                twitterService.SendTweet(new SendTweetOptions { Status = _status }, (tweet, response) => { });
            }

            return Ok(tweet);
        }
    }
}
