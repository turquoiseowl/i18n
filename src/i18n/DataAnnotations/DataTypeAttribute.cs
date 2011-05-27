using System;
using System.Web;

namespace i18n.DataAnnotations
{
    public enum DataType
    {
        Currency,
        Custom,
        Date,
        DateTime,
        Duration,
        EmailAddress,
        Html,
        ImageUrl,
        MultilineText,
        Password,
        PhoneNumber,
        Text,
        Time,
        Url
    }

    public class DataTypeAttribute : System.ComponentModel.DataAnnotations.DataTypeAttribute, ILocalizing
    {
        private readonly I18NSession _session;

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

        public DataTypeAttribute(string customDataType) : base(customDataType)
        {
            _session = new I18NSession();   
        }

        public virtual string _(string text)
        {
            return _session.GetText(HttpContext.Current, text);
        }

        public override string FormatErrorMessage(string name)
        {
            var formatted = base.FormatErrorMessage(name);
            return _(formatted);
        }
    }
}