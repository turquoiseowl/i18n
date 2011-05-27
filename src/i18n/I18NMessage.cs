using System.Text;

namespace i18n
{
    /// <summary>
    /// A localized message residing in a PO resource file
    /// </summary>
    public class I18NMessage
    {
        public string Comment { get; set; }
        public string MsgId { get; set; }
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

