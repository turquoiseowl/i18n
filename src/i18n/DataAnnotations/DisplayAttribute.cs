namespace i18n.DataAnnotations
{
    public class DisplayAttribute : ValidationAttribute
    {
        private string _name;
        private string _prompt;
        private string _description;

        public string Name
        {
            get { return _(_name); }
            set { _name = value; }
        }

        public string Prompt
        {
            get { return _(_prompt); }
            set { _prompt = value; }
        }

        public string Description
        {
            get { return _(_description); }
            set { _description = value; }
        }
    }
}