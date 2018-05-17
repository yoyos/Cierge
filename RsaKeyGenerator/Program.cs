using System;
using System.Security.Cryptography;

namespace RsaKeyGenerator
{
    class Program
    {
        private static string _key { get; set; }
        private static RSACryptoServiceProvider _rsa;


        static void Main(string[] args)
        {
            try
            {
                MakePrivateKey();

                if (args.Length > 1)
                {
                    SaveKey(args[0]);
                }
                else
                {
                    SaveKey(AskSavePath());
                }

                if (AskPrintKey())
                {
                    Console.WriteLine(_key);
                    Console.ReadKey();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error:\n{ex.Message}\n");
                Main(args);
            }
        }

        private static void MakePrivateKey()
        {
            _rsa = new RSACryptoServiceProvider(2048);
            _key = _rsa.ToJsonString(true);
        }

        private static void SaveKey(string path)
        {
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(System.IO.File.Create(path)))
            {
                file.WriteLine(_key);
            }
        }

        private static string AskSavePath()
        {
            Console.WriteLine("Path to save key to (including filename)? >>>");
            return Console.ReadLine();
        }

        private static bool AskPrintKey()
        {
            Console.WriteLine("Would you like to print the generated key to terminal now (includes private key)? [y/N] >>>");
            return Console.ReadLine() == "y";
        }
    }
}
