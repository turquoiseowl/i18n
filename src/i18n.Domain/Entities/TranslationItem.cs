using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace i18n.Domain.Entities
{
    /// <summary>
    /// All the data that one translation one item can contain. Apart from Id (which is the string to translate) and Message (which is the translation) it contains some meta data. This is linked from Translation that ties many of these items together for a complete language
    /// </summary>
    public class TranslationItem
    {
        //todo: The po specification actually says that if you want to multiline the message (and possibly the id) it should looks like this
        //"this is line 1 of comment \n""
        //"this is line two"
        //this of course means that messages needs to be a collection and the POTranslationRepository's parse and save functions needs to be updated.
        //this should preferably be decided upon and fixed before anyone writes a database repository
    
        public string MsgKey { get; set; }
        public string MsgId { get; set; }
        public string Message { get; set; }
        public IEnumerable<ReferenceContext> References { get; set; }
        public IEnumerable<string> ExtractedComments { get; set; }
        public IEnumerable<string> TranslatorComments { get; set; }
        public IEnumerable<string> Flags { get; set; }

        public override string ToString()
        {
            return MsgKey;
        }
    }
}
