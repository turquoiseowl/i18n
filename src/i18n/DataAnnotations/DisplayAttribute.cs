namespace i18n.DataAnnotations
{
    /// <summary>
    /// 
    /// </summary>
    public class DisplayAttribute : LocalizingAttribute
    {
        private string _name;
        private string _prompt;
        private string _description;

        /// <summary>
        /// 
        /// </summary>
        public string Name
        {
            get { return _(_name).ToHtmlString(); }
            set { _name = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        public string Prompt
        {
            get { return _(_prompt).ToHtmlString(); }
            set { _prompt = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        public string Description
        {
            get { return _(_description).ToHtmlString(); }
            set { _description = value; }
        }
    }
}