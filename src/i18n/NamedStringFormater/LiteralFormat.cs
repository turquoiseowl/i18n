namespace i18n.NamedStringFormater
{
    public class LiteralFormat : ITextExpression
    {
        public LiteralFormat(string literalText)
        {
            LiteralText = literalText;
        }

        public string LiteralText
        {
            get;
            private set;
        }

        public string Eval(object o)
        {
            string literalText = LiteralText
                .Replace("{{", "{")
                .Replace("}}", "}");
            return literalText;
        }
    }
}
