using System;

namespace i18n.PostBuild
{
    class Program
    {
        static void Main(string[] args)
        {
            if(args.Length != 1)
            {
                Console.WriteLine("This post build task requires passing in the $(SolutionDirectory) path");
                return;
            }
            
            new PostBuildTask().Execute(args[0]);
        }
    }
}
