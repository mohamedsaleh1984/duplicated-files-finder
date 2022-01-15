using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Linq;

namespace FindDublicateFiles
{
    internal class Program
    {
        private static readonly MD5 Md5 = MD5.Create();
        private static string location = "";
        private static string extensions = "";
        static void Main(string[] args)
        {
            if (CheckUserParameters(args))
            {
                Console.WriteLine("The program processing is depending on the number of files Or the size of the files.");
                Console.WriteLine("Duplication Finding Process Started.");

                List<string> files = GetAllFiles(location, GetExtensions(extensions));

                Dictionary<string, List<string>> result = FindDuplicate(files);

                WriteDuplicationResult(result);
            }
        }
        private static bool CheckUserParameters(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("You must enter the location");
                Console.WriteLine("File extensions must be comma separated for example to filter mp4 And txt files you must type mp4,txt AFTER directory");
                return false;
            }
            else if (args.Length == 1)
            {
                location = args[0];
                return IsValidDirectory(location);
            }
            else if (args.Length == 2)
            {
                location = args[0];
                if (IsValidDirectory(location))
                {
                    extensions = args[1];
                    return IsValidExtensions(extensions);
                }
            }
            return false;
        }
        private static Dictionary<string, List<string>> FindDuplicate(List<string> files)
        {
            Dictionary<string, string> table = new Dictionary<string, string>();
            Dictionary<string, List<string>> result = new Dictionary<string, List<string>>();
            foreach (string file in files)
            {
                string hash = GetFileHash(GetHashMD5(file));

                Console.WriteLine("Checking " + Path.GetFileName(file) + " dupliaction...");

                if (!table.ContainsKey(hash))
                {
                    table.Add(hash, file);
                    List<string> list = new List<string>();
                    list.Add(file);
                    result.Add(hash, list);
                }
                else
                {
                    result[hash].Add(file);
                }
            }
            return result;
        }
        private static void WriteDuplicationResult(Dictionary<string, List<string>> result)
        {
            Console.WriteLine("Writing Duplication Results...");

            if (File.Exists("Duplication Result.txt"))
                File.Delete("Duplication Result.txt");

            StreamWriter sw = File.CreateText("Duplication Result.txt");

            foreach (string key in result.Keys)
            {
                string s = Path.GetFileName(result[key][0]);
                if (result[key].Count > 1)
                {
                    sw.WriteLine(s);
                    foreach (string value in result[key])
                        sw.WriteLine("\t\t" + value);
                }
            }
            sw.Close();

            Console.WriteLine("Duplication Finding Process has finished, Please check 'Duplication Result.txt' file.");
        }
        private static List<string> GetExtensions(string extensions)
        {
            if (string.IsNullOrEmpty(extensions))
                return new List<string>();
            return extensions.Split(new char[] { ',' }).ToList();
        }
        private static List<string> GetAllFiles(string directory, List<string> ext)
        {
            List<string> files = null;
            try
            {
               File.SetAttributes(directory, FileAttributes.Normal);
                files = Directory.GetFiles(directory, "", SearchOption.AllDirectories).ToList();
                if (ext.Count > 0)
                    files = FilterFiles(files, ext);
            }
            catch (System.UnauthorizedAccessException ex)
            {
                Console.WriteLine("Given directory is not accessable.");
                Environment.Exit(0);
            }
            return files;
        }
        private static List<string> FilterFiles(List<string> files, List<string> acceptedExtensions)
        {
            List<string> filesToReturn = new List<string>();
            foreach (var ext in acceptedExtensions)
            {
                filesToReturn.AddRange(files.Where(x => x.EndsWith(ext)).ToList());
            }
            return filesToReturn;
        }
        private static byte[] GetHashMD5(string filename)
        {
            using (FileStream stream = File.OpenRead(filename))
                return Md5.ComputeHash(stream);
        }
        public static string GetFileHash(byte[] bytes)
        {
            string result = "";
            foreach (byte b in bytes) result += b.ToString("x2");
            return result;
        }
        public static bool IsValidDirectory(string directory)
        {
            if (!Directory.Exists(directory))
            {
                Console.WriteLine("You provided invalid directory.");
                return false;
            }
            return true;
        }
        private static bool IsValidExtensions(string extensions)
        {
            if (string.IsNullOrEmpty(extensions))
            {
                Console.WriteLine("You provided invalid extensions.");
                return false;
            }
            return true;
        }
    }
}
