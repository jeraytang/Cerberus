namespace IdentityServer4.SecurityTokenService.Models.Account
{
    public static class ResultProvider
    {
        public static ResultJson Success(string msg = "")
        {
            var message = string.IsNullOrWhiteSpace(msg) ? "成功！" : msg;
            return new ResultJson {Code = 200, Msg = message};
        }

        public static ResultJson Fail(string msg = "")
        {
            var message = string.IsNullOrWhiteSpace(msg) ? "失败！" : msg;
            return new ResultJson {Code = 500, Msg = message};
        }
    }

    public class ResultJson
    {
        public int Code { get; set; }

        public string Msg { get; set; }
    }
}