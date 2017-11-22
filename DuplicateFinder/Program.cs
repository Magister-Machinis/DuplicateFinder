using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.IO;
using System.Diagnostics;
using System.Linq;

namespace DuplicateFinder
{
    class container
    {
        public int setting;
        public List<string> filelist;
        public int count;


        public container(int Setting, List<string> Filelist)
        {
            setting = Setting;
            filelist = Filelist;
            count = 0;
            switch (setting)
            {
                case 1:
                    string dest = Path.GetFullPath(@".\Duplicates.txt");
                    using (StreamWriter output = File.CreateText(dest))
                    {
                        output.WriteLine("List of duplicate files, not counting originals");
                    }
                    break;
                case 2:
                    string dest2 = @".\DUPES\";
                    dest2 = Path.GetFullPath(dest2);
                    Directory.CreateDirectory(dest2);
                    break;
            }


        }

    }

    
    class Program
    {
        
        static void Main(string[] args)
        {
            Console.WriteLine("Please select Mode:\r\n 1: List Duplicates \r\n 2: Move Duplicates to folder \r\n 3: Delete Duplicates");
            int setting = Convert.ToInt32(Console.ReadLine());
            Console.WriteLine("Beginning");
            Stopwatch bigtimer = new Stopwatch(); //timer for runtime statistics
            bigtimer.Start();
            
            
            string currentdir = Path.GetFullPath(@".\");
            try
            {

                string[] rawfilelist = Directory.GetFileSystemEntries(currentdir, "*", SearchOption.AllDirectories);
                List<string> filelist = new List<string>(rawfilelist);
                rawfilelist = null;
                Console.WriteLine("List of files and folders obtained, checking for duplicates");
                container contain = new container(setting, filelist);
                DupCheck(contain);


            }
            catch( UnauthorizedAccessException e)
            {
                Console.WriteLine("ERROR: This program does is not authorized to access one or more files in this folder or it's children");
                Console.WriteLine(e.Message);
            }


            bigtimer.Stop();
            TimeSpan rundurationraw = bigtimer.Elapsed;

            Console.WriteLine("Runtime " + rundurationraw);
            Console.WriteLine("Press any key to exit");
            Console.ReadLine();
        }

        static void DupCheck(container contain, int counter = 0)
        {
            string current = contain.filelist[counter];
            byte[] currenthash = GetHash(current);
            counter++;

            Console.WriteLine("Checking " + current);
            if (counter < contain.filelist.Count)
            {
                
                FileAttributes attr = File.GetAttributes(current);
                if (attr.HasFlag(FileAttributes.Directory))
                {
                    contain.filelist.Remove(current);
                }
                else
                {
                    for (int count = counter; count < contain.filelist.Count - 1; count++)
                    {
                        FileAttributes attr2 = File.GetAttributes(contain.filelist[count]);
                        if (attr2.HasFlag(FileAttributes.Directory))
                        {
                            contain.filelist.Remove(contain.filelist[count]);
                        }
                        else
                        {
                            if (currenthash.SequenceEqual(GetHash(contain.filelist[count])))
                                {
                                    switch (contain.setting)
                                {
                                    case 1:
                                        ToFile(contain.filelist[count]);
                                        break;
                                    case 2:
                                        ToFolder(contain.filelist[count]);
                                        break;
                                    case 3:
                                        File.Delete((contain.filelist[count]));
                                        break;
                                }
                                    contain.filelist.Remove(contain.filelist[count]);
                                }
                        }

                    }                    
                }
                Console.WriteLine(((counter / contain.filelist.Count) * 100) + "% Complete");
                DupCheck(contain, counter);

            }
        }

        static void ToFolder(string path)
        {
            string dest = @"/DUPES/";
            
            dest = Path.GetFullPath(dest);
            
            Directory.Move(path, Path.Combine(dest,Path.GetFileName(path)));
            
            
        }
        static void ToFile(string path)
        {
            string dest = @".\Duplicates.txt";
            
                dest = Path.GetFullPath(@".\Duplicates.txt");
                using (StreamWriter output = File.AppendText(dest))
                {
                    output.WriteLine(path);
                }
            
            
        }
        static byte[] GetHash(string path)
        {
            SHA256 thissha = SHA256Managed.Create();
            byte[] Hash = System.IO.File.ReadAllBytes(path);
            Hash = thissha.ComputeHash(Hash);
            Console.WriteLine("Hash of " + path + " is " + (string.Join("",Hash)));
            return Hash;
        }
    }
}
