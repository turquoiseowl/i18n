namespace i18n.DataAnnotations
{
    public class DisplayAttribute : LocalizingAttribute
    {
        private string _name;
        private string _prompt;
        private string _description;

        public string Name
        {
            get { return _(_name).ToHtmlString(); }
            set { _name = value; }
        }

        public string Prompt
        {
            get { return _(_prompt).ToHtmlString(); }
            set { _prompt = value; }
        }

        public string Description
        {
            get { return _(_description).ToHtmlString(); }
            set { _description = value; }
        }
    }
}