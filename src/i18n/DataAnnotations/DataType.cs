namespace i18n.DataAnnotations
{
    /// <summary>
    /// Represents an enumeration of the data types associated with data fields and parameters
    /// </summary>
    public enum DataType
    {
        /// <summary>
        /// Represents a currency value
        /// </summary>
        Currency,
        /// <summary>
        /// Represents a custom data type
        /// </summary>
        Custom,
        /// <summary>
        /// Represents a date value
        /// </summary>
        Date,
        /// <summary>
        /// Represents an instant of time, expressed as a date and time of day
        /// </summary>
        DateTime,
        /// <summary>
        /// Represents a continuous time in which an object exists
        /// </summary>
        Duration,
        /// <summary>
        /// Represents an e-mail address
        /// </summary>
        EmailAddress,
        /// <summary>
        /// Represents an HTML file
        /// </summary>
        Html,
        /// <summary>
        /// Represents a URL to an image
        /// </summary>
        ImageUrl,
        /// <summary>
        /// Represents multi-line test
        /// </summary>
        MultilineText,
        /// <summary>
        /// Represents a password value
        /// </summary>
        Password,
        /// <summary>
        /// Represents a phone number value
        /// </summary>
        PhoneNumber,
        /// <summary>
        /// Represents text that is displayed
        /// </summary>
        Text,
        /// <summary>
        /// Represents a time value
        /// </summary>
        Time,
        /// <summary>
        /// Represents a URL value
        /// </summary>
        Url
    }
}