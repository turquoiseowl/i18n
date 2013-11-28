using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using i18n;

namespace i18n.Domain.Entities
{
	/// <summary>
	/// Template items are only used to keep track of the strings needing translation in any given project and for then updating the translations and translationItems with this data.
	/// You should never need to work with TemplateItem unless you work with finding nuggets and updating the template file.
	/// </summary>
	public class TemplateItem
	{
		public string MsgKey;
        public string MsgId;
		public IEnumerable<string> References { get; set; }
		public IEnumerable<string> Comments { get; set; }

        public override string ToString()
        {
            return MsgKey;
        }

        public static string KeyFromMsgidAndComment(
            string msgid, 
            string comment,
            bool MessageContextFromComment)
        {
            if (MessageContextFromComment && !string.IsNullOrEmpty(comment)) {
                return string.Format("{0}:£#£#£:{1}", msgid, comment); }
            return msgid;
        }

	}
}
