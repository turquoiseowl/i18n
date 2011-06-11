using System;
using System.Web;
using System.Web.Mvc;

namespace i18n.DataAnnotations
{
    /// <summary>
    /// Specifies an additional type to associate with a data field
    /// </summary>
    public class DataTypeAttribute : System.ComponentModel.DataAnnotations.DataTypeAttribute, ILocalizing
    {
        private readonly I18NSession _session;

        ///<summary>
        /// Initializes a new instance of the <see cref="DataTypeAttribute"/> class by using the specified type name
        ///</summary>
        ///<param name="dataType"></param>
        public DataTypeAttribute(DataType dataType) : base(Convert(dataType))
        {
            _session = new I18NSession();
        }

        private static System.ComponentModel.DataAnnotations.DataType Convert(DataType dataType)
        {
            switch(dataType)
            {
                case DataAnnotations.DataType.Currency:
                    return System.ComponentModel.DataAnnotations.DataType.Currency;
                case DataAnnotations.DataType.Custom:
                    return System.ComponentModel.DataAnnotations.DataType.Custom;
                case DataAnnotations.DataType.Date:
                    return System.ComponentModel.DataAnnotations.DataType.Date;
                case DataAnnotations.DataType.DateTime:
                    return System.ComponentModel.DataAnnotations.DataType.DateTime;
                case DataAnnotations.DataType.Duration:
                    return System.ComponentModel.DataAnnotations.DataType.Duration;
                case DataAnnotations.DataType.EmailAddress:
                    return System.ComponentModel.DataAnnotations.DataType.EmailAddress;
                case DataAnnotations.DataType.Html:
                    return System.ComponentModel.DataAnnotations.DataType.Html;
                case DataAnnotations.DataType.ImageUrl:
                    return System.ComponentModel.DataAnnotations.DataType.ImageUrl;
                case DataAnnotations.DataType.MultilineText:
                    return System.ComponentModel.DataAnnotations.DataType.MultilineText;
                case DataAnnotations.DataType.Password:
                    return System.ComponentModel.DataAnnotations.DataType.Password;
                case DataAnnotations.DataType.PhoneNumber:
                    return System.ComponentModel.DataAnnotations.DataType.PhoneNumber;
                case DataAnnotations.DataType.Text:
                    return System.ComponentModel.DataAnnotations.DataType.Text;
                case DataAnnotations.DataType.Time:
                    return System.ComponentModel.DataAnnotations.DataType.Time;
                case DataAnnotations.DataType.Url:
                    return System.ComponentModel.DataAnnotations.DataType.Url;
                default:
                    throw new ArgumentOutOfRangeException("dataType");
            }
        }

        ///<summary>
        /// Initializes a new instance of the <see cref="DataTypeAttribute"/> class by using the specified field template name
        ///</summary>
        ///<param name="customDataType"></param>
        public DataTypeAttribute(string customDataType) : base(customDataType)
        {
            _session = new I18NSession();   
        }

        /// <summary>
        /// Returns localized text for the given key, if available
        /// </summary>
        /// <param name="text">The text to localize</param>
        public virtual IHtmlString _(string text)
        {
            return new MvcHtmlString(_session.GetText(HttpContext.Current, text));
        }

        /// <summary>
        /// Applies formatting to an error message, based on the data field where the error occurred. 
        /// </summary>
        /// <returns>
        /// An instance of the formatted error message.
        /// </returns>
        /// <param name="name">The name to include in the formatted message.</param>
        public override string FormatErrorMessage(string name)
        {
            var formatted = base.FormatErrorMessage(name);
            return _(formatted).ToHtmlString();
        }
    }
}