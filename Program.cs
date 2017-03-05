using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PIT_Look_Batch_Tool
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length > 0)
            {
                Console.WriteLine(args);
                for (int id = 0; id < args.Length; id++)
                {
                    Console.WriteLine(args[id]);
                    if ((File.Exists(args[id])) && (args[id].Contains(".pit")))
                    {
                        string homedir = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
                        string filename = args[id];
                        string looklist =  homedir + "\\look_list.csv";
                        string filedest1 = "header.txt";
                        string filedest2 = "base_look.txt";
                        string filedest3 = "def_look.txt";
                        string filedest4 = "trailer.txt";
                        //string[] headertype;
                        //string[] headervalue;
                        //string[] globaltype;
                        //int[] globalvalue;
                        int lookcnt = 1;
                        int baselook_id = 0;
                        //int variantcnt = 1;
                        //int matcnt = 1;
                        //int partcnt = 1;
                        bool isheader = true;
                        bool islook = false;
                        bool isbaselook = true;
                        bool istrailer = false;

                        string[] look_name;
                        string[] texture_tag;
                        string[] texture_path;

                        using (var fs = File.OpenRead(looklist))
                        using (var reader = new StreamReader(fs))
                        {
                            List<string> listA = new List<string>();
                            List<string> listB = new List<string>();
                            List<string> listC = new List<string>();
                            while (!reader.EndOfStream)
                            {
                                var line = reader.ReadLine();
                                var values = line.Split(',');

                                listA.Add(values[0]);
                                listB.Add(values[1]);
                                listC.Add(values[2]);
                            }
                            look_name = listA.ToArray();
                            texture_tag = listB.ToArray();
                            texture_path = listC.ToArray();
                            lookcnt = look_name.Length;
                        }
                        System.IO.StreamWriter filewrite1 = new System.IO.StreamWriter(filedest1);
                        System.IO.StreamWriter filewrite2 = new System.IO.StreamWriter(filedest2);
                        System.IO.StreamWriter filewrite3 = new System.IO.StreamWriter(filedest3);
                        System.IO.StreamWriter filewrite4 = new System.IO.StreamWriter(filedest4);
                        if (File.Exists(filename))
                        {

                            string line;
                            string line2;

                            // Read the file and display it line by line.
                            System.IO.StreamReader file = new System.IO.StreamReader(filename);

                            while ((line = file.ReadLine()) != null)
                            {

                                // Screw comments!
                                if (line.StartsWith("#")) continue;

                                while (isheader == true)
                                {
                                    string linetrim = NoWhiteSpace(line);
                                    if (linetrim.StartsWith("Look {"))
                                    {
                                        isheader = false;
                                        islook = true;
                                        filewrite1.Close();
                                        filewrite2.WriteLine(line);
                                        break;
                                    }
                                    if (linetrim.StartsWith("Source"))
                                    {
                                        int denominator = line.IndexOf(":");
                                        filewrite1.WriteLine("{0}: \"PIT Look Addition Batch Tool by MandelSoft\"", line.Substring(0, denominator));
                                        //Console.WriteLine("{0} : \"PIT Look Addition Batch Tool by MandelSoft\"", line.Substring(0, denominator));
                                    }
                                    else if (linetrim.StartsWith("LookCount"))
                                    {
                                        int denominator = line.IndexOf(":");
                                        filewrite1.WriteLine("{0}: {1}", line.Substring(0, denominator), lookcnt);
                                        //Console.WriteLine("{0} : {1}", line.Substring(0, denominator), lookcnt);
                                    }
                                    else
                                    {
                                        filewrite1.WriteLine(line);
                                        //Console.WriteLine(line);
                                    }
                                    line = file.ReadLine();
                                }


                                while (islook == true)
                                {
                                    if (line.StartsWith("Variant {"))
                                    {
                                        islook = false;
                                        istrailer = true;
                                        break;
                                    }
                                    line = file.ReadLine();
                                    if (isbaselook == true)
                                    {
                                        if (line.StartsWith("Look {"))
                                        {
                                            isbaselook = false;
                                            filewrite2.Close();
                                            System.IO.StreamReader file_baselook = new System.IO.StreamReader(filedest2);

                                            while ((line2 = file_baselook.ReadLine()) != null)
                                            {
                                                string linetrim = NoWhiteSpace(line2);
                                                if (linetrim.StartsWith("Name"))
                                                {
                                                    int denominator = line2.IndexOf(":");
                                                    int name_endpoint = line2.Length - (denominator + 3);
                                                    string baselookname = line2.Substring(denominator + 2, name_endpoint);
                                                    for (int i = 0; i < lookcnt; i++)
                                                    {
                                                        if (baselookname.Contains(look_name[i]))
                                                        {
                                                            //Console.WriteLine("Look found: {0}", look_name[i]);
                                                            baselook_id = i;
                                                        }
                                                    }
                                                }
                                            }
                                            for (int i = 0; i < lookcnt; i++)
                                            {
                                                file_baselook.BaseStream.Position = 0;
                                                file_baselook.DiscardBufferedData();
                                                while ((line2 = file_baselook.ReadLine()) != null)
                                                {
                                                    string linetrim = NoWhiteSpace(line2);
                                                    if (linetrim.StartsWith("Name"))
                                                    {
                                                        int denominator = line2.IndexOf(":");
                                                        filewrite3.WriteLine("{0}: \"{1}\"", line2.Substring(0, denominator), look_name[i]);
                                                        //Console.WriteLine("{0} : {1}", line2.Substring(0, denominator), look_name[i]);
                                                    }
                                                    else if (linetrim.StartsWith("Texture {"))
                                                    {
                                                        filewrite3.WriteLine(line2);
                                                        //Console.WriteLine(line2);
                                                        line2 = file_baselook.ReadLine();
                                                        filewrite3.WriteLine(line2);
                                                        //Console.WriteLine(line2);
                                                        if (line2.Contains(texture_tag[i]))
                                                        {
                                                            line2 = file_baselook.ReadLine();
                                                            int denominator = line2.IndexOf(":");
                                                            int name_endpoint = line2.Length - (denominator + 3);
                                                            string texturename = line2.Substring(denominator + 2, name_endpoint);
                                                            if (texturename.Contains(texture_path[baselook_id]))
                                                            {
                                                                filewrite3.WriteLine("{0}: \"{1}\"", line2.Substring(0, denominator), texture_path[i]);
                                                                //Console.WriteLine("{0} : \"{1}\"", line2.Substring(0, denominator), texture_path[i]);
                                                            }
                                                            else
                                                            {
                                                                filewrite3.WriteLine(line2);
                                                                //Console.WriteLine(line2);
                                                            }
                                                        }
                                                    }
                                                    else
                                                    {
                                                        filewrite3.WriteLine(line2);
                                                        //Console.WriteLine(line2);
                                                    }
                                                }
                                            }
                                            filewrite3.Close();
                                            file_baselook.Close();
                                        }
                                        else
                                        {
                                            filewrite2.WriteLine(line);
                                        }
                                    }
                                    //Console.WriteLine(line);

                                    //{
                                    //    int denominator = line.IndexOf(":");
                                    //    filewrite2.WriteLine("{0} : {1}", line.Substring(0, denominator), lookcnt);
                                    //    Console.WriteLine("{0} : {1}", line.Substring(0, denominator), lookcnt);
                                    //}
                                    //else
                                    //{
                                    //    filewrite2.WriteLine(line);
                                    //    Console.WriteLine(line);
                                    //}

                                }
                                while (istrailer == true)
                                {
                                    if (line == null)
                                    {
                                        islook = false;
                                        istrailer = true;
                                        filewrite4.Close();
                                        break;
                                    }
                                    filewrite4.WriteLine(line);
                                    //Console.WriteLine(line);
                                    line = file.ReadLine();
                                }

                            }

                            file.Close();
                        }
                        using (var output = File.Create(filename))
                        {
                            foreach (var fileoutput in new[] { filedest1, filedest3, filedest4 })
                            {
                                using (var input = File.OpenRead(fileoutput))
                                {
                                    input.CopyTo(output);
                                }
                            }
                        }
                        // Clean-up
                        try
                        {
                            System.IO.File.Delete(filedest1);
                        }
                        catch (System.IO.IOException e)
                        {
                            Console.WriteLine(e.Message);
                            Console.ReadLine();
                            return;
                        }
                        try
                        {
                            System.IO.File.Delete(filedest2);
                        }
                        catch (System.IO.IOException e)
                        {
                            Console.WriteLine(e.Message);
                            Console.ReadLine();
                            return;
                        }
                        try
                        {
                            System.IO.File.Delete(filedest3);
                        }
                        catch (System.IO.IOException e)
                        {
                            Console.WriteLine(e.Message);
                            Console.ReadLine();
                            return;
                        }
                        try
                        {
                            System.IO.File.Delete(filedest4);
                        }
                        catch (System.IO.IOException e)
                        {
                            Console.WriteLine(e.Message);
                            Console.ReadLine();
                            return;
                        }
                    }
                }
            }
        }
        public static string NoWhiteSpace(string line)
        {
            string linetrim = line.Trim();
            return linetrim;
        }
        public static void PrintParams(string type, string value)
        {
            Console.WriteLine("{0} : {1}", type, value);
        }
    }
}
