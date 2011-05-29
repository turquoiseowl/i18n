using System;

namespace i18n.PostBuild
{
    class Program
    {
        static void Main(string[] args)
        {
            if(args.Length != 1)
            {
                Console.WriteLine("This post build task requires passing in the $(ProjectDirectory) path");
                return;
            }

            var path = args[0];
            path = path.Trim(new[] {'\"'});
            
            new PostBuildTask().Execute(path);
        }
    }
}
