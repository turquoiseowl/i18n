# i18n
## Smart internationalization for .NET web apps

    PM> Package-Install I18N

### Introduction


### Features
- Globally recognized interface; localize like the big kids
- Localizes everything; views, controllers, validation attributes, and even routes!
- SEO-friendly; language selection varies the URL, and `Content-Language` is set appropriately
- Automatic; no routing changes required, just use an alias method where you want localization
- Smart; knows when to hold them, fold them, walk away, or run, based on i18n best practices

### Usage
To localize text in your application, use the `_("text")` alias method wherever required. That's it.
Here's an example of localizing text in a Razor view:

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

And here's an example in a controller:

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

#### Installing a base WebViewPage for Razor
In the view example above, the `_("text")` alias is called on the base class of the Razor view page.
Depending on whether you're using the provided base classes or your own base class with `ILocalizing` (see below),
you'll want to change the `~/Views/web.config` file to point Razor to the base class containing the alias.
Here is how you'd set up the alias using the provided `I18NWebViewPage` class:

     <system.web.webPages.razor>
        <!-- ... -->
        <pages pageBaseType="i18n.I18NWebViewPage">
          <!-- ... -->
        </pages>
      </system.web.webPages.razor>

#### Using base classes vs. interfaces
The central service is `ILocalizingService`; anywhere you need localization, implement the `ILocalizing` interface.
The package comes with default base classes for convenience, including `I18NController`, `I18NWebViewPage`, and
`I18NWebViewPage<T>`. If your project needs prevent you from using a base class, implement `ILocalizing` and defer
to `ILocalizingService`; here is what implementing `ILocalizing` on a `Controller` might look like as a reference:

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

#### Building PO databases
At application start, if you've called `I18N.RebuildDatabase()`, I18N rips through your source code, 
finding everywhere you've used the `ILocalizing._("text")` alias, and uses this to build or update a master .PO 
localization database. You don't have to do anything special, all PO data is stored in the `/locale` folder relative
to your web application.

From here, you use any of the widely available PO editing tools (like [POEdit](http://www.poedit.net))
to provide locale-specific text and place them in your `/locale` folder relative to the provided language, i.e. `locale/fr`. 
If you change a PO file on the fly, I18N will update accordingly; you do _not_ need to redeploy your application.

#### What if I don't want all of this reflection happening at boot time?
A large web application may start up slowly due to analyzing the IL looking for localization text. You can optionally
avoid this performance hit by passing false to `I18N.RebuildDatabase(bool compileViews)` command. It's up to you, then, to
either ensure you're using precompiled Razor views, or that you've set `MvcBuildViews` to `true` in your project
properties.

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
most part, you should be able to substitute `System.ComponentModel.DataAnnotations` with `i18n.DataAnnotations` namespaces
cleanly.

### Contributing
There's lot of room for further enhancements and features to this library, and you are encouraged to fork it and
contribute back anything new. Specifically, these would be great places to add more functionality:

* Better parsing and handling of PO files for more general purposes / outside editors
* Additional validation attributes
* Supporting additional view engines beyond Razor (dynamic building)
* Support for GNU gettext extraction tools (static building)
* Support for additional storage mechanisms beyond ASP.NET Session (i.e. cookies, closures, etc.)
* Support for medium trust environments