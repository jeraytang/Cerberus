namespace IdentityServer4.SecurityTokenService.Models
{
    public static class JsonMessageResult
    {
        public static JsonMessage Success(string message)
        {
            return new JsonMessage {code = 200, msg = message};
        }

        public static JsonMessage Fail(string message, object data = null)
        {
            return new JsonMessage {code = 500, msg = message};
        }
    }

    public class JsonMessage
    {
        public int code { get; set; }

        public string msg { get; set; }
    }
}