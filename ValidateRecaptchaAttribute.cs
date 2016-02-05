using Microsoft.AspNet.Mvc.Filters;
using Microsoft.CSharp.RuntimeBinder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Net.Http;
using System.Threading.Tasks;

namespace reCAPTCHA
{
    public class ValidateRecaptchaAttribute : ActionFilterAttribute
    {
        private const string verificationUrl = "https://www.google.com/recaptcha/api/siteverify";
        public string ErrorMessage { get; set; } = "We could not verify that you are a human.";
        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if(context.HttpContext.Request.Method != HttpMethod.Post.ToString())
            {
                throw new reCAPTCHAException("ValidateRecaptchaAttribute must only decorate POST actions.");
            }

            StringValues recaptchaResponse;
            try {
                recaptchaResponse = context.HttpContext.Request.Form["g-recaptcha-response"];
            }
            catch(InvalidOperationException exc)
            {
                throw new reCAPTCHAException("reCAPTCHA failure, likely due to ValidateRecaptchaAttribute improperly decorating an action to which an unintended request has been posted.", exc);
            }

            if (String.IsNullOrWhiteSpace(recaptchaResponse))
            {
                context.ModelState.AddModelError("reCAPTCHAFailure", ErrorMessage);
                await next();
            }

            var builder = new ConfigurationBuilder();
            builder.AddJsonFile("appsettings.json");
            builder.AddEnvironmentVariables();
            var config = builder.Build();
            var siteSecret = config["RecaptchaOptions:Secret"];

            if (String.IsNullOrWhiteSpace(siteSecret))
            {
                throw new reCAPTCHAException("Could not find value for reCAPTCHA Secret in appsettings.json.");
            }

            using (var client = new HttpClient())
            {
                var values = new Dictionary<string, string>
                {
                   { "secret", siteSecret },
                   { "response", context.HttpContext.Request.Form["g-recaptcha-response"] },
                   { "remoteip", GetRemoteIp(context) }
                };
                var content = new FormUrlEncodedContent(values);

                HttpResponseMessage response;
                try {
                    response = await client.PostAsync(verificationUrl, content);
                }
                catch(HttpRequestException exc)
                {
                    throw new reCAPTCHAException("Could not reach Google's reCAPTCHA service for verification.", exc);
                }
                var responseString = await response.Content.ReadAsStringAsync();

                var converter = new ExpandoObjectConverter();
                try {
                    dynamic obj = JsonConvert.DeserializeObject<ExpandoObject>(responseString, converter);
                    bool isHuman = obj.success;

                    if (!isHuman)
                    {
                        context.ModelState.AddModelError("reCAPTCHAFailure", ErrorMessage);
                    }
                }
                catch(RuntimeBinderException exc)
                {
                    throw new reCAPTCHAException("Response from Google's verification service was in an unexpected format:" + responseString, exc);
                }
            }

            await next();
        }
        private string GetRemoteIp(ActionExecutingContext context)
        {
            string ip = context.HttpContext.Request.Headers["HTTP_X_FORWARDED_FOR"];

            if (String.IsNullOrEmpty(ip))
            {
                ip = context.HttpContext.Request.Headers["REMOTE_ADDR"];
            }

            return ip;
        }
    }
}
