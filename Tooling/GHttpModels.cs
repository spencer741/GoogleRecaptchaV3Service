using System;

namespace AspNetCoreRecaptchaV3ValidationDemo.Tooling
{

    public class GRequestModel
    {
        public string path;
        public string secret { get; set; }
        public string response { get; set; }
        public string remoteip { get; set; }

        public GRequestModel(string res, string remip)
        {
            response = res;
            remoteip = remip;
            secret = Startup.Configuration["Secrets:GoogleRecaptchaV3"];
            path = "https://www.google.com/recaptcha/api/siteverify";
        }
    }

    public class GResponseModel
    {
        public bool success { get; set; }
        public DateTime challenge_ts { get; set; }
        public string hostname { get; set; }
        public string error_codes { get; set; }
    }
}