using System;
using System.Threading.Tasks;
using System.Text.Json;
using System.Web;
using System.Net.Http;
using System.IO;
using System.Text;
using System.Runtime.Serialization.Json;

namespace AspNetCoreRecaptchaV3ValidationDemo.Tooling
{
    public class CaptchaRequestException : Exception
    {
        public CaptchaRequestException()
        {
        }
        public CaptchaRequestException(string message)
            : base(message)
        {
        }
        public CaptchaRequestException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }

    public interface IGoogleRecaptchaV3Service
    {
        HttpClient _httpClient { get; set; }
        GRequestModel Request { get; set; }
        GResponseModel Response { get; set; }
        void InitializeRequest(GRequestModel request);
        Task<bool> Execute();
    }

    public class GoogleRecaptchaV3Service : IGoogleRecaptchaV3Service
    {
        public HttpClient _httpClient { get; set; }

        public GRequestModel Request { get; set; }

        public GResponseModel Response { get; set; }

        public HttpRequestException HttpReqException { get; set; }

        public Exception GeneralException { get; set; }

        public GoogleRecaptchaV3Service(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public void InitializeRequest(GRequestModel request)
        {
            Request = request;
        }

        public async Task<bool> Execute()
        {
            // Notes on error handling:
            // Google will pass back a 200 Status Ok response if no network or server errors occur.
            // If there are errors in on the "business" level, they will be coded in an array;
            // CaptchaRequestException is for these types of errors.

            // CaptchaRequestException and multiple catches are used to help seperate the concerns of 
            //  a) an HttpRequest 400+ status code 
            //  b) an error at the "business" level 
            //  c) an unpredicted error that can only be handled generically.

            // It might be worthwhile to implement a "user error message" property in this class so the
            // calling procedure can decide what, if anything besides a server error, to return to the 
            // client and any client handling from there on.
            try
            {

                //Don't to forget to invoke any loggers in the logic below.

                //formulate request
                string builtURL = Request.path + '?' + HttpUtility.UrlPathEncode($"secret={Request.secret}&response={Request.response}&remoteip={Request.remoteip}");
                StringContent content = new StringContent(builtURL);

                Console.WriteLine($"Sent Request {builtURL}");

                //send request, await.
                HttpResponseMessage response = await _httpClient.PostAsync(builtURL, null);
                response.EnsureSuccessStatusCode();

                //read response
                byte[] res = await response.Content.ReadAsByteArrayAsync();

                string logres = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Retrieved Response: {logres}");

                //Serialize into GReponse type
                using (MemoryStream ms = new MemoryStream(res))
                {
                    DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(GResponseModel));
                    Response = (GResponseModel)serializer.ReadObject(ms);
                }

                //check if business success
                if (!Response.success)
                {
                    throw new CaptchaRequestException();
                }

                //return bool.
                return true; //response.IsSuccessStatusCode; <- don't need this. EnsureSuccessStatusCode is now in play.
            }
            catch (HttpRequestException hre)
            {
                //handle http error code.
                HttpReqException = hre;

                //invoke logger accordingly

                //only returning bool. It is ultimately up to the calling procedure
                //to decide what data it wants from the Service.
                return false;
            }
            catch (CaptchaRequestException ex)
            {

                //Business-level error... values are accessible in error-codes array.
                //this catch block mainly serves for logging purposes. 

                /*  Here are the possible "business" level codes:
                    missing-input-secret 	The secret parameter is missing.
                    invalid-input-secret 	The secret parameter is invalid or malformed.
                    missing-input-response 	The response parameter is missing.
                    invalid-input-response 	The response parameter is invalid or malformed.
                    bad-request 	        The request is invalid or malformed.
                    timeout-or-duplicate 	The response is no longer valid: either is too old or has been used previously.
                */

                //invoke logger accordingly 

                //only returning bool. It is ultimately up to the calling procedure 
                //to decide what data it wants from the Service.
                return false;
            }
            catch (Exception ex)
            {
                // Generic unpredictable error
                GeneralException = ex;

                // invoke logger accordingly

                //only returning bool. It is ultimately up to the calling procedure 
                //to decide what data it wants from the Service.
                return false;
            }
        }
    }
}