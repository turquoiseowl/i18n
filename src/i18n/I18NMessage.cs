using System.Text;

namespace i18n
{
    /// <summary>
    /// A localized message residing in a PO resource file
    /// </summary>
    public class I18NMessage
    {
        /// <summary>
        /// 
        /// </summary>
        public string Comment { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string MsgId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string MsgStr { get; set; }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("# ").Append(Comment).AppendLine();
            sb.Append("msgid \"").Append(MsgId).Append("\"").AppendLine();
            sb.Append("msgstr \"").Append(MsgStr).Append("\"").AppendLine();
            return sb.ToString();
        }
    }
}

