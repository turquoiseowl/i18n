//[assembly: PreApplicationStartMethod(typeof(PreApplicationStart), "Start")]

namespace i18n
{
    // Using this approach doesn't work reliably; many times, no methods are found

    public static class PreApplicationStart
    {
        private static bool _started;

        public static void Start()
        {
            if (_started)
            {
                return;
            }

            I18N.BuildDatabase();

            _started = true;
        }
    }
}