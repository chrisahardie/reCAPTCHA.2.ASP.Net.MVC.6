# reCAPTCHA 2 for MVC 6
Implementation of Google's reCAPTCHA for ASP.Net 5/MVC 6 (dnxcore and dnx451)

## Getting started

Once you have addded the project to your solution, you will need to add a dependency reference to it from your ASP.Net 5 project. You can do this via the package manager console:

>Install-Package Recaptcha2-MVC6 -IncludePrerelease

You will now need to configure the `reCAPTCHA`. If you haven't done so already, visit https://www.google.com/recaptcha/intro/index.html and sign up for a `reCAPTCHA` account. Once you have, you will be provided with a site key and secret. Add those to an `appsettings.json` file off the root of your ASP.Net 5 application. Your file should look like this (the following keys are invalid):

      {
        "RecaptchaOptions": {
          "SiteKey": "6LcQfA8TAAAAAP2eIEy5wRsAdsTa99rDSI3Qwf_o",
          "Secret": "6LcQfA8TAAAAALj6jHM6z60M9DgMWoJYHIatXXXW"
        },
        "Data": {
          "DefaultConnection": {
            "ConnectionString": "Server=(localdb)\\mssqllocaldb;Database=aspnet5-WebApplication3-XXXXX;Trusted_Connection=True;MultipleActiveResultSets=true"
          }
        },
        
  The `reCAPTCHA` relies on a Javascript file, so somewhere in your `View` or `Layout`, reference the following:
  
          <script src='https://www.google.com/recaptcha/api.js'></script>
       
You will need to notify your application that you have added a custom tag helper from this project. In `/Views/_ViewImports.cshtml`, add this line:

> @addTagHelper "*, Recaptcha2-MVC6"

 To use the `reCAPTCHA`, identify a controller action to which it is being applied and add this action filter:
 
        [HttpPost, ValidateRecaptcha]
        public async Task<IActionResult> AddComment(AddCommentViewModel viewmodel)

Note this will emit a hard-coded `"We could not verify that you are a human."` `ModelState` error message if verification fails. If you want to change that message or add localization, add the filter like this:

        [HttpPost, ValidateRecaptcha(ErrorMessage = Resource.RecaptchaError)]
        
Finally, in any form where you would like to add the `Recaptcha`, add this tag helper:
  
          <form asp-action="AddComment" asp-controller="Home">
            <recaptcha multi-site="false"></recaptcha>
             ...
          </form>
      
## Supporting multiple sites with the same tokens

As described at https://developers.google.com/recaptcha/docs/secure_token, you can use the same key and secret tokens across multiple domains. To enable this behaviour, simply add the `multi-site` attribute to the `recaptcha` tag helper and set it to true:

        <form asp-action="AddComment" asp-controller="Home">
          <recaptcha multi-site="true"></recaptcha>
          ...
        </form>
