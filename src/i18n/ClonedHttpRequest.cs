using System;
using System.IO;
using System.Web;

namespace i18n
{
    internal class ClonedHttpRequest : HttpRequestBase
    {
        private readonly HttpRequestBase _base;
        private readonly string _url;

        public ClonedHttpRequest(HttpRequestBase requestBase, string url)
        {
            _base = requestBase;
            _url = url;
        }

        public override string RawUrl
        {
            get { return _url; }
        }
        public override string FilePath
        {
            get { return _url; }
        }
        public override string Path
        {
            get { return _url; }
        }
        public override string AppRelativeCurrentExecutionFilePath
        {
            get { return VirtualPathUtility.ToAppRelative(_url); }
        }
        public override string CurrentExecutionFilePath
        {
            get { return _url; }
        }

        public override string[] AcceptTypes
        {
            get { return _base.AcceptTypes; }
        }

        public override string AnonymousID
        {
            get { return _base.AnonymousID; }
        }

        public override string ApplicationPath
        {
            get { return _base.ApplicationPath; }
        }
        
        public override byte[] BinaryRead(int count)
        {
            return _base.BinaryRead(count);
        }

        public override HttpBrowserCapabilitiesBase Browser
        {
            get { return _base.Browser; }
        }

        public override HttpClientCertificate ClientCertificate
        {
            get { return _base.ClientCertificate; }
        }

        public override System.Text.Encoding ContentEncoding
        {
            get { return _base.ContentEncoding; }
            set { _base.ContentEncoding = value; }
        }

        public override int ContentLength
        {
            get { return _base.ContentLength; }
        }

        public override HttpCookieCollection Cookies
        {
            get { return _base.Cookies; }
        }

        public override string ContentType
        {
            get { return base.ContentType; }
            set { _base.ContentType = value; }
        }

        public override bool Equals(object obj)
        {
            return _base.Equals(obj);
        }
        
        public override HttpFileCollectionBase Files
        {
            get { return _base.Files; }
        }

        public override Stream Filter
        {
            get { return _base.Filter; }
            set { _base.Filter = value; }
        }

        public override System.Collections.Specialized.NameValueCollection Form
        {
            get { return _base.Form; }
        }

        public override int GetHashCode()
        {
            return _base.GetHashCode();
        }

        public override System.Collections.Specialized.NameValueCollection Headers
        {
            get
            {
                return _base.Headers;
            }
        }

        public override System.Security.Authentication.ExtendedProtection.ChannelBinding HttpChannelBinding
        {
            get
            {
                return _base.HttpChannelBinding;
            }
        }

        public override string HttpMethod
        {
            get
            {
                return _base.HttpMethod;
            }
        }

        public override Stream InputStream
        {
            get
            {
                return _base.InputStream;
            }
        }

        public override bool IsAuthenticated
        {
            get
            {
                return _base.IsAuthenticated;
            }
        }

        public override bool IsLocal
        {
            get
            {
                return _base.IsLocal;
            }
        }

        public override bool IsSecureConnection
        {
            get
            {
                return _base.IsSecureConnection;
            }
        }

        public override System.Security.Principal.WindowsIdentity LogonUserIdentity
        {
            get
            {
                return _base.LogonUserIdentity;
            }
        }

        public override int[] MapImageCoordinates(string imageFieldName)
        {
            return _base.MapImageCoordinates(imageFieldName);
        }

        public override string MapPath(string virtualPath)
        {
            return _base.MapPath(virtualPath);
        }

        public override string MapPath(string virtualPath, string baseVirtualDir, bool allowCrossAppMapping)
        {
            return _base.MapPath(virtualPath, baseVirtualDir, allowCrossAppMapping);
        }

        public override System.Collections.Specialized.NameValueCollection Params
        {
            get
            {
                return _base.Params;
            }
        }

        public override string PathInfo
        {
            get
            {
                return _base.PathInfo;
            }
        }

        public override string PhysicalApplicationPath
        {
            get
            {
                return _base.PhysicalApplicationPath;
            }
        }

        public override string PhysicalPath
        {
            get
            {
                return _base.PhysicalPath;
            }
        }

        public override System.Collections.Specialized.NameValueCollection QueryString
        {
            get { return HttpUtility.ParseQueryString(new Uri(_url).Query); }
        }

        public override System.Web.Routing.RequestContext RequestContext
        {
            get
            {
                return _base.RequestContext;
            }
        }

        public override string RequestType
        {
            get
            {
                return _base.RequestType;
            }
            set
            {
                _base.RequestType = value;
            }
        }

        public override void SaveAs(string filename, bool includeHeaders)
        {
            _base.SaveAs(filename, includeHeaders);
        }

        public override System.Collections.Specialized.NameValueCollection ServerVariables
        {
            get
            {
                return _base.ServerVariables;
            }
        }

        public override string this[string key]
        {
            get
            {
                return _base[key];
            }
        }

        public override string ToString()
        {
            return _base.ToString();
        }

        public override int TotalBytes
        {
            get
            {
                return _base.TotalBytes;
            }
        }

        public override Uri Url
        {
            get
            {
                return new Uri(_url);
            }
        }

        public override Uri UrlReferrer
        {
            get
            {
                return _base.UrlReferrer;
            }
        }

        public override string UserAgent
        {
            get
            {
                return _base.UserAgent;
            }
        }

        public override string UserHostAddress
        {
            get
            {
                return _base.UserHostAddress;
            }
        }

        public override string UserHostName
        {
            get
            {
                return _base.UserHostName;
            }
        }

        public override string[] UserLanguages
        {
            get
            {
                return _base.UserLanguages;
            }
        }

        public override void ValidateInput()
        {
            _base.ValidateInput();
        }
    }
}