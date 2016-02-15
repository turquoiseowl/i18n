using System;
using System.IO;
using System.Text;

namespace i18n.Domain.Helpers
{
    public class PathNormalizer
    {
        private static readonly char[] PathSeparators = { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar };

        public static string MakeRelativePath(string anchorPath, string path)
        {
            if (path == null)
                throw new ArgumentNullException("path");

            if (string.IsNullOrEmpty(anchorPath))
                return path;

            if (path.StartsWith(anchorPath, StringComparison.OrdinalIgnoreCase))
            {
                return path.Substring(anchorPath.Length + 1);
            }

            var anchorComponents = anchorPath.Split(PathSeparators, StringSplitOptions.RemoveEmptyEntries);
            var pathComponents = path.Split(PathSeparators, StringSplitOptions.RemoveEmptyEntries);

            var firstUniqueComponentIndex = FirstUniqueComponentIndex(anchorComponents, pathComponents);

            if (firstUniqueComponentIndex == 0)
                return path;

            var pathBuilder = new StringBuilder();

            var upDirectoryCount = anchorComponents.Length - firstUniqueComponentIndex;
            while (upDirectoryCount > 0)
            {
                pathBuilder.Append("..");
                pathBuilder.Append(Path.DirectorySeparatorChar);

                --upDirectoryCount;
            }

            for (var i = firstUniqueComponentIndex; i < pathComponents.Length; ++i)
            {
                pathBuilder.Append(pathComponents[i]);

                if (i < pathComponents.Length - 1)
                    pathBuilder.Append(Path.DirectorySeparatorChar);
            }

            return pathBuilder.ToString();
        }

        private static int FirstUniqueComponentIndex(string[] anchorComponents, string[] pathComponents)
        {
            var index = 0;
            foreach (var s in anchorComponents)
            {
                if (index >= pathComponents.Length)
                    return 0;

                if (s != pathComponents[index])
                    return index;

                ++index;
            }

            return 0;
        }
    }
}
