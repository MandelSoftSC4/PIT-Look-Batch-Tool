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
                        // File paths
                        string homedir = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
                        string filename = args[id];
                        string looklist =  homedir + "\\look_list.csv";
                        string filedest1 = "header.txt";
                        string filedest2 = "base_look.txt";
                        string filedest3 = "def_look.txt";
                        string filedest4 = "trailer.txt";

                        // Initialisation of global parameters
                        int lookcnt = 1;
                        int baselook_id = 0;
                        bool isheader = true;
                        bool islook = false;
                        bool isbaselook = true;
                        bool istrailer = false;

                        string[] look_name;
                        string[] texture_tag;
                        string[] texture_path;

                        // Read and store the look list
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

                        // Initiate temporary files for the program to dump data
                        System.IO.StreamWriter filewrite1 = new System.IO.StreamWriter(filedest1);
                        System.IO.StreamWriter filewrite2 = new System.IO.StreamWriter(filedest2);
                        System.IO.StreamWriter filewrite3 = new System.IO.StreamWriter(filedest3);
                        System.IO.StreamWriter filewrite4 = new System.IO.StreamWriter(filedest4);

                        // Start work!
                        if (File.Exists(filename))
                        {

                            // Our strings that we use to read things from
                            string line;
                            string line2;

                            // Read the file and display it line by line.
                            System.IO.StreamReader file = new System.IO.StreamReader(filename);

                            while ((line = file.ReadLine()) != null)
                            {

                                // Screw comments!
                                if (line.StartsWith("#")) continue;

                                // Copy the header lines
                                while (isheader == true)
                                {
                                    string linetrim = NoWhiteSpace(line);
                                    // Let's check if we are still in the header range...
                                    if (linetrim.StartsWith("Look {"))
                                    {
                                        // kthnxbai
                                        isheader = false;
                                        islook = true;
                                        filewrite1.Close();
                                        filewrite2.WriteLine(line);
                                        break;
                                    }
                                    // Change source name to identify this tool has processed this file...
                                    if (linetrim.StartsWith("Source"))
                                    {
                                        int denominator = line.IndexOf(":");
                                        filewrite1.WriteLine("{0}: \"PIT Look Addition Batch Tool by MandelSoft\"", line.Substring(0, denominator));
                                        //Console.WriteLine("{0} : \"PIT Look Addition Batch Tool by MandelSoft\"", line.Substring(0, denominator));
                                    }
                                    // Change the look count to match with the input look count of the look list...
                                    else if (linetrim.StartsWith("LookCount"))
                                    {
                                        int denominator = line.IndexOf(":");
                                        filewrite1.WriteLine("{0}: {1}", line.Substring(0, denominator), lookcnt);
                                        //Console.WriteLine("{0} : {1}", line.Substring(0, denominator), lookcnt);
                                    }
                                    // Nothing to see here, just copy-paste...
                                    else
                                    {
                                        filewrite1.WriteLine(line);
                                        //Console.WriteLine(line);
                                    }
                                    line = file.ReadLine();
                                }


                                while (islook == true)
                                {
                                    // We only want the first look, so if we have found it, we continue to the trailer.
                                    if (line.StartsWith("Variant {"))
                                    {
                                        // kthnxbai
                                        islook = false;
                                        istrailer = true;
                                        break;
                                    }
                                    line = file.ReadLine();
                                    if (isbaselook == true)
                                    {
                                        if (line.StartsWith("Look {"))
                                        {
                                            // Well, we have reached the second look. We don't need you!
                                            isbaselook = false;
                                            // kthnxbai, we don't need to write to you anymore.
                                            filewrite2.Close();

                                            // Let's read the file we just created...
                                            System.IO.StreamReader file_baselook = new System.IO.StreamReader(filedest2);

                                            while ((line2 = file_baselook.ReadLine()) != null)
                                            {
                                                string linetrim = NoWhiteSpace(line2);
                                                // So, does this line contain the look name?
                                                if (linetrim.StartsWith("Name"))
                                                {
                                                    int denominator = line2.IndexOf(":");
                                                    int name_endpoint = line2.Length - (denominator + 3);
                                                    string baselookname = line2.Substring(denominator + 2, name_endpoint);
                                                    for (int i = 0; i < lookcnt; i++)
                                                    {
                                                        // Check if this look is in the look list...
                                                        if (baselookname.Contains(look_name[i]))
                                                        {
                                                            // Awesome! Let's write it down!
                                                            //Console.WriteLine("Look found: {0}", look_name[i]);
                                                            baselook_id = i;
                                                        }
                                                    }
                                                }
                                            }
                                            // OK, let's generate some new looks!
                                            for (int i = 0; i < lookcnt; i++)
                                            {
                                                // We start to read from the beginning...
                                                file_baselook.BaseStream.Position = 0;
                                                file_baselook.DiscardBufferedData();

                                                // Just continue minding your business until the file ends...
                                                while ((line2 = file_baselook.ReadLine()) != null)
                                                {
                                                    string linetrim = NoWhiteSpace(line2);
                                                    // Is this the look name?
                                                    if (linetrim.StartsWith("Name"))
                                                    {
                                                        // Let's rename it!
                                                        int denominator = line2.IndexOf(":");
                                                        filewrite3.WriteLine("{0}: \"{1}\"", line2.Substring(0, denominator), look_name[i]);
                                                        //Console.WriteLine("{0} : {1}", line2.Substring(0, denominator), look_name[i]);
                                                    }
                                                    // We're going to change the texture! Check if it's the texture we're looking for
                                                    else if (linetrim.StartsWith("Texture {"))
                                                    {
                                                        filewrite3.WriteLine(line2);
                                                        //Console.WriteLine(line2);
                                                        line2 = file_baselook.ReadLine();
                                                        filewrite3.WriteLine(line2);
                                                        //Console.WriteLine(line2);

                                                        // Is this the right texture tag? Or does it matter anyway?
                                                        if ((line2.Contains(texture_tag[i])) || (texture_tag[i]=="any"))
                                                        {
                                                            line2 = file_baselook.ReadLine();
                                                            int denominator = line2.IndexOf(":");
                                                            int name_endpoint = line2.Length - (denominator + 3);
                                                            string texturename = line2.Substring(denominator + 2, name_endpoint);
                                                            if (texturename.Contains(texture_path[baselook_id]))
                                                            {
                                                                // Awesome! We can replace the texture!
                                                                filewrite3.WriteLine("{0}: \"{1}\"", line2.Substring(0, denominator), texture_path[i]);
                                                                //Console.WriteLine("{0} : \"{1}\"", line2.Substring(0, denominator), texture_path[i]);
                                                            }
                                                            // These are not the textures are looking for...
                                                            else
                                                            {
                                                                filewrite3.WriteLine(line2);
                                                                //Console.WriteLine(line2);
                                                            }
                                                        }
                                                    }
                                                    // Nothing to see here, just copy-paste...
                                                    else
                                                    {
                                                        filewrite3.WriteLine(line2);
                                                        //Console.WriteLine(line2);
                                                    }
                                                }
                                            }
                                            // kthnxbai, we don't need to write to you anymore.
                                            filewrite3.Close();
                                            file_baselook.Close();
                                        }
                                        // Write down the base look!
                                        else
                                        {
                                            filewrite2.WriteLine(line);
                                        }
                                    }
                                }
                                // Let's write down the trailer lines.
                                while (istrailer == true)
                                {
                                    // Are we at the end of the file?
                                    if (line == null)
                                    {
                                        // kthnxbai
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
                            // tl;dr, kthnxbai
                            file.Close();
                        }

                        // Entries, assemble!
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
                        // Hey, cleaning lady! We need to clean-up some files!
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
        // So, here we define some functions...
        public static string NoWhiteSpace(string line)
        {
            string linetrim = line.Trim();
            return linetrim;
        }
    }
}
