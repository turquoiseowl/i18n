using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace i18n
{
/*
    LocalizingModule can be installed like this:

        IIS7+ Integrated mode:

          <system.webServer>
            <modules>
              <add name="i18n.LocalizingModule" type="i18n.LocalizingModule, i18n" />
            </modules>
          </system.webServer>

        IIS7 Classic mode and II6:

          <system.web>
            <httpModules>
              <add name="i18n.LocalizingModule" type="i18n.LocalizingModule, i18n" /> <!-- #37 -->
            </httpModules>
          </system.web>
*/

    /// <summary>
    /// HTTP module responsible for:
    /// 1. Implementing early URL localization
    /// 2. Installing our ResponseFilter into the ASP.NET pipeline.
    /// </summary>
    public class LocalizingModule : IHttpModule
    {

    #region [IHttpModule]

        public void Init(HttpApplication application)
        {
            DebugHelpers.WriteLine("LocalizingModule::Init -- application: {0}", application);
            
            // Wire up our event handlers into the ASP.NET pipeline.
            if (LocalizedApplication.EarlyUrlLocalizer != null) {
                application.BeginRequest += OnBeginRequest; }
            application.ReleaseRequestState += OnReleaseRequestState;
        }
        public void Dispose() {}

    #endregion

    // Implementation

    // Events handlers

        /// <summary>
        /// Handler for the BeginRequest ASP.NET request pipeline event, where we inject our
        /// Early URL Localization logic.
        /// </summary>
        private void OnBeginRequest(object sender, EventArgs e)
        {
            HttpContextBase context = HttpContext.Current.GetHttpContextBase();
            DebugHelpers.WriteLine("LocalizingModule::OnBeginRequest -- sender: {0}, e:{1}, ContentType: {2},\n+++>Url: {3}\n+++>RawUrl:{4}", sender, e, context.Response.ContentType, context.Request.Url, context.Request.RawUrl);

            if (LocalizedApplication.EarlyUrlLocalizer != null)
            {
                LocalizedApplication.EarlyUrlLocalizer.ProcessIncoming(
                    context, 
                    LocalizedApplication.UrlLocalizer);
            }
        }

        /// <summary>
        /// Handler for the ReleaseRequestState ASP.NET request pipeline event.
        /// This event occurs late on in the pipeline but prior to the response being filtered.
        /// We take the opportunity to inject our i8n post-processing of the response.
        /// </summary>
        private void OnReleaseRequestState(object sender, EventArgs e)
        {
        //
            HttpContextBase context = HttpContext.Current.GetHttpContextBase();
            DebugHelpers.WriteLine("LocalizingModule::OnReleaseRequestState -- sender: {0}, e:{1}, ContentType: {2},\n+++>Url: {3}\n+++>RawUrl:{4}", sender, e, context.Response.ContentType, context.Request.Url, context.Request.RawUrl);

            // If the content type of the entity is eligible for processing...wire up our filter
            // to do the processing. The entity data will be run through the filter a bit later on
            // in the pipeline.
            if (LocalizedApplication.ContentTypesToLocalize != null
                && LocalizedApplication.ContentTypesToLocalize.Match(context.Response.ContentType).Success) {
                DebugHelpers.WriteLine("LocalizingModule::OnReleaseRequestState -- Installing filter");
                context.Response.Filter = new ResponseFilter(context, context.Response.Filter);
            }
        }
    }
}
