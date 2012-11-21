# i18n
## Smart internationalization for .NET web apps
```
    PM> Install-Package I18N
```
_Note: NuGet is currently not having it when it comes to nested folders with executables, which this project requires. For now, I suggest you download the lib from this site if you're experiencing problems, and add the gettext folder as content in your web application root folder. I'm working on a better solution._

### Introduction

The i18n library is designed to replace the use of .NET resources in favor of an easier, globally recognized standard for localizing web applications. Using this library simplifies localization by making it a first class citizen of views, controllers, and validation attributes.

### Features
- Globally recognized interface; localize like the big kids
- Localizes everything; views, controllers, validation attributes, and even routes!
- SEO-friendly; language selection varies the URL, and `Content-Language` is set appropriately
- Automatic; no routing changes required, just use an alias method where you want localization
- Smart; knows when to hold them, fold them, walk away, or run, based on i18n best practices

### Usage
To localize text in your application, use the `_("text")` alias method wherever required. That's it.
Here's an example of localizing text in a Razor view:

```html
    <div id="content">
        <h2>@_("Welcome to my web app!")</h2>
        <h3><span>@_("Amazing slogan here")</span></h3>
        <p>@_("Ad copy that would make Hiten Shah fall off his chair!")</p>
        <span class="button">
            <a href="@Url.Action("Plans", "Home", new { area = "" })">
                <strong>@_("SEE PLANS & PRICING")</strong>
                <span>@_("Free unicorn with all plans!")</span>
            </a>
        </span>
    </div>
```

And here's an example in a controller:

```csharp
    using i18n;
    
    namespace MyApplication
    {
        public class HomeController : I18NController
        {
            public ActionResult Index()
            {
                ViewBag.Message = _("Welcome to ASP.NET MVC!");

                return View();
            }
        }
    }
```

For use in URL-Helpers or other functions that require a plain string, you can use the `__("text")` alias:

```html
@Html.LabelFor(m => m.Name, __("First Name"))
```

#### Installing a base WebViewPage for Razor
In the view example above, the `_("text")` alias is called on the base class of the Razor view page.
Depending on whether you're using the provided base classes or your own base class with `ILocalizing` (see below),
you'll want to change the `~/Views/web.config` file to point Razor to the base class containing the alias.
Here is how you'd set up the alias using the provided `I18NWebViewPage` class:

```xml
     <system.web.webPages.razor>
        <!-- ... -->
        <pages pageBaseType="i18n.I18NWebViewPage">
          <!-- ... -->
        </pages>
      </system.web.webPages.razor>
```

#### Using base classes vs. interfaces
The central service is `ILocalizingService`; anywhere you need localization, implement the `ILocalizing` interface.
The package comes with default base classes for convenience, including `I18NController`, `I18NWebViewPage`, and
`I18NWebViewPage<T>`. If your project needs prevent you from using a base class, implement `ILocalizing` and defer
to `ILocalizingService`; here is what implementing `ILocalizing` on a `Controller` might look like as a reference:

```csharp
    using System.Web.Mvc;
    using i18n;

    namespace MyApplication
    {
        public abstract class MyController : Controller, ILocalizing
        {
            private static ILocalizingService _service;

            protected MyController()
            {
                _service = new LocalizingService();
            }
            
            public string _(string text)
            {
                // Prefer a stored value to browser-supplied preferences
                var stored = LanguageSession.GetLanguageFromSession(ControllerContext.HttpContext);
                if (stored != null)
                {
                    return _service.GetText(text, new[] { stored });
                }

                // Find the most appropriate fit from the user's browser settings
                var languages = HttpContext.Request.UserLanguages;                
                return _service.GetText(text, languages);
            }
        }
    }
```

#### Building PO databases

To set up automatic PO database building, add the following post-build task to your project, after
adding `i18n.PostBuild.exe` as a project reference:

```
    "$(TargetDir)i18n.PostBuild.exe" "$(ProjectDir)"
```
    
After a successful build, this task will rip through your source code, finding everywhere you've used the `ILocalizing._("text")` alias, 
and uses this to build a master .PO template file located at `/locale/messages.pot` relative to your web application folder. After the
new template is constructed, any locales that exist inside the `/locale` folder are automatically merged with the template, so that
new strings can be flagged for further translation.

From here, you use any of the widely available PO editing tools (like [POEdit](http://www.poedit.net))
to provide locale-specific text and place them in your `/locale` folder relative to the provided language, i.e. `locale/fr`. 
If you change a PO file on the fly, i18n will update accordingly; you do _not_ need to redeploy your application.

#### Automatic routing
To participate in the automatic routing features of this library, call `I18N.Register()` in your startup code;
this will register a global filter and route decorator to provide the feature.

I18N comes with the ability to build on top of your existing routes to automatically redirect language choice to
an appropriate URL suffix. When a regular route is accessed, I18N will inspect the browser's `Accept-Language` header
to find the most appropriate choice of language from those you've prepared from PO files; for example, if the user's
most preferred language is `fr-CA` followed by `en-US`, and your application has PO files for `en-US` and `fr`, I18N 
will select `fr` as the best language choice. At this point, the regular route is then redirected to the language
specific URL, and the `Content-Language` header is set accordingly.

If your user is surfing in the `fr` locale, but then explicitly asks for a route with `en` appended, their preference
is persisted, and all subsequent requests will redirect to the `en` locale, making it simple to change language choice
for the user. You may also optionally use `/?language=fr` style query string parameters to elicit the same redirection
behavior. Route requests for languages you do not have resources for will _not_ redirect to a default resource, they
will 404 as expected.

#### Validation attributes

Generally speaking, the stock validation attributes in ASP.NET MVC are closed to change, which makes it difficult to
use them with a framework like this. Still, I18N contains replacements for several `ValidationAttributes`, including
a base class for extending to build your own. These replacements function similarly to the originals, but use the
`ILocalizing` interface; just use normally and any derived text will pass through the localization process. Any custom
interactions that occur using these attributes elsewhere in the framework, however, will not work as expected. For the
most part, you should be able to swap `System.ComponentModel.DataAnnotations` and `i18n.DataAnnotations` namespaces
cleanly.

#### A reminder about folders in a web application

Your `locale` folder is exposed to HTTP requests as-is, just like a typical log directory, so remember to block all requests
to this folder by adding a `Web.config` file. 

```xml
    <?xml version="1.0"?>
    <configuration>    
        <system.web>
            <httpHandlers>
                <add path="*" verb="*" type="System.Web.HttpNotFoundHandler"/>
            </httpHandlers>
        </system.web>
        <system.webServer>
            <handlers>
                <remove name="BlockViewHandler"/>
                <add name="BlockViewHandler" path="*" verb="*" preCondition="integratedMode" type="System.Web.HttpNotFoundHandler"/>
            </handlers>
        </system.webServer>
    </configuration>
```

### Contributing
There's lot of room for further enhancements and features to this library, and you are encouraged to fork it and
contribute back anything new. Specifically, these would be great places to add more functionality:

* Better parsing and handling of PO files for more general purposes / outside editors
* Additional validation attributes
* Support for additional storage mechanisms beyond ASP.NET Session (i.e. cookies, closures, etc.)
* Support for medium trust environments