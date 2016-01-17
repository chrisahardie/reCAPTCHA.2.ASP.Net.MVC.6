using Microsoft.AspNet.Razor.TagHelpers;
using Microsoft.Extensions.Configuration;

namespace Recaptcha
{
    [HtmlTargetElement("recaptcha", Attributes = multiSite)]
    public class RecaptchaTagHelper : TagHelper
    {
        private readonly string secret;
        private readonly string siteKey;
        private const string multiSite = "multi-site";

        [HtmlAttributeName(multiSite)]
        public bool MultiSite { get; set; }
        public RecaptchaTagHelper() : base()
        {
            var builder = new ConfigurationBuilder();
            builder.AddJsonFile("appsettings.json");
            builder.AddEnvironmentVariables();
            var config = builder.Build();
            secret = config["RecaptchaOptions:Secret"];
            siteKey = config["RecaptchaOptions:SiteKey"];
        }
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            var token = CryptUtils.CreateToken(secret);
            output.TagName = "div";
            output.Attributes["class"] = "g-recaptcha";
            output.Attributes["data-sitekey"] = siteKey;
            if (MultiSite == true)
            {
                output.Attributes["data-stoken"] = token;
            }
            base.Process(context, output);       
        }
    }
}
