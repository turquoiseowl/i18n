using System;
using System.Web;

namespace i18n
{
    internal class ClonedHttpContext : HttpContextBase
    {
        private readonly HttpContextBase _base;
        private readonly HttpRequestBase _requestBase;

        public ClonedHttpContext(HttpContextBase contextBase, HttpRequestBase requestBase)
        {
            _base = contextBase;
            _requestBase = requestBase;
        }

        public override HttpRequestBase Request
        {
            get { return _requestBase; }
        }

        public override void AddError(Exception errorInfo)
        {
            _base.AddError(errorInfo);
        }

        public override Exception[] AllErrors
        {
            get
            {
                return _base.AllErrors;
            }
        }

        public override HttpApplicationStateBase Application
        {
            get
            {
                return _base.Application;
            }
        }

        public override HttpApplication ApplicationInstance
        {
            get
            {
                return _base.ApplicationInstance;
            }
            set
            {
                _base.ApplicationInstance = value;
            }
        }

        public override System.Web.Caching.Cache Cache
        {
            get
            {
                return _base.Cache;
            }
        }

        public override void ClearError()
        {
            _base.ClearError();
        }

        public override IHttpHandler CurrentHandler
        {
            get
            {
                return _base.CurrentHandler;
            }
        }

        public override RequestNotification CurrentNotification
        {
            get
            {
                return _base.CurrentNotification;
            }
        }

        public override bool Equals(object obj)
        {
            return _base.Equals(obj);
        }

        public override Exception Error
        {
            get
            {
                return _base.Error;
            }
        }

        public override object GetGlobalResourceObject(string classKey, string resourceKey)
        {
            return _base.GetGlobalResourceObject(classKey, resourceKey);
        }

        public override object GetGlobalResourceObject(string classKey, string resourceKey, System.Globalization.CultureInfo culture)
        {
            return _base.GetGlobalResourceObject(classKey, resourceKey, culture);
        }

        public override int GetHashCode()
        {
            return _base.GetHashCode();
        }

        public override object GetLocalResourceObject(string virtualPath, string resourceKey)
        {
            return _base.GetLocalResourceObject(virtualPath, resourceKey);
        }

        public override object GetLocalResourceObject(string virtualPath, string resourceKey, System.Globalization.CultureInfo culture)
        {
            return _base.GetLocalResourceObject(virtualPath, resourceKey, culture);
        }

        public override object GetSection(string sectionName)
        {
            return _base.GetSection(sectionName);
        }

        public override object GetService(Type serviceType)
        {
            return _base.GetService(serviceType);
        }

        public override IHttpHandler Handler
        {
            get
            {
                return _base.Handler;
            }
            set
            {
                _base.Handler = value;
            }
        }

        public override bool IsCustomErrorEnabled
        {
            get
            {
                return _base.IsCustomErrorEnabled;
            }
        }

        public override bool IsDebuggingEnabled
        {
            get
            {
                return _base.IsDebuggingEnabled;
            }
        }

        public override bool IsPostNotification
        {
            get
            {
                return _base.IsPostNotification;
            }
        }

        public override System.Collections.IDictionary Items
        {
            get
            {
                return _base.Items;
            }
        }

        public override IHttpHandler PreviousHandler
        {
            get
            {
                return _base.PreviousHandler;
            }
        }

        public override System.Web.Profile.ProfileBase Profile
        {
            get
            {
                return _base.Profile;
            }
        }

        public override void RemapHandler(IHttpHandler handler)
        {
            _base.RemapHandler(handler);
        }

        public override HttpResponseBase Response
        {
            get
            {
                return _base.Response;
            }
        }

        public override void RewritePath(string filePath, string pathInfo, string queryString)
        {
            _base.RewritePath(filePath, pathInfo, queryString);
        }

        public override void RewritePath(string filePath, string pathInfo, string queryString, bool setClientFilePath)
        {
            _base.RewritePath(filePath, pathInfo, queryString, setClientFilePath);
        }

        public override void RewritePath(string path)
        {
            _base.RewritePath(path);
        }

        public override void RewritePath(string path, bool rebaseClientPath)
        {
            _base.RewritePath(path, rebaseClientPath);
        }

        public override HttpServerUtilityBase Server
        {
            get
            {
                return _base.Server;
            }
        }

        public override HttpSessionStateBase Session
        {
            get
            {
                return _base.Session;
            }
        }

        public override void SetSessionStateBehavior(System.Web.SessionState.SessionStateBehavior sessionStateBehavior)
        {
            _base.SetSessionStateBehavior(sessionStateBehavior);
        }

        public override bool SkipAuthorization
        {
            get
            {
                return _base.SkipAuthorization;
            }
            set
            {
                _base.SkipAuthorization = value;
            }
        }

        public override DateTime Timestamp
        {
            get
            {
                return _base.Timestamp;
            }
        }

        public override string ToString()
        {
            return _base.ToString();
        }

        public override TraceContext Trace
        {
            get
            {
                return _base.Trace;
            }
        }

        public override System.Security.Principal.IPrincipal User
        {
            get
            {
                return _base.User;
            }
            set
            {
                _base.User = value;
            }
        }
    }
}
