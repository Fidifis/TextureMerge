using System;

namespace TextureMerge
{
    public class Program
    {
        public static string[] cmdArgs = null;

        [STAThread()]
        public static void Main(string[] args)
        {
            if (args.Length > 0)
            {
                cmdArgs = args;
            }
            App.Main();
        }
    }
}
