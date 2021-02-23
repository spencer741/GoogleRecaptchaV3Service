using System;
using System.Runtime.Serialization;

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
            secret = Startup.Configuration["GoogleRecaptchaV3:Secret"];
            path = Startup.Configuration["GoogleRecaptchaV3:ApiUrl"];
            if(String.IsNullOrWhiteSpace(secret) || String.IsNullOrWhiteSpace(path))
            {
                throw new Exception("Invalid 'Secret' and 'Path' properties in appsettings.json. Parent: GoogleRecaptchaV3.");
            }
        }
    }

    //Google's response property naming is 
    //embarassingly inconsistent, that's why we have to 
    //use DataContract and DataMember attributes,
    //so we can bind the class from properties that have 
    //naming where a C# variable by that name would be
    //against the language specifications... (i.e., '-').
    [DataContract]
    public class GResponseModel
    {
        [DataMember]
        public bool success { get; set; }
        [DataMember]
        public DateTime challenge_ts { get; set; }
        [DataMember]
        public string hostname { get; set; }

        //Could create a child object for 
        //error-codes
        [DataMember(Name = "error-codes")]
        public string[] error_codes { get; set; }
    }
}