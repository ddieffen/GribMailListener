using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication2
{
    class Program
    {
        static void Main(string[] args)
        {

            try
            {

                foreach (string f in Directory.GetFiles(@"Z:\Music", "* (4).mp3", SearchOption.AllDirectories))
                {
                    //string name = Path.GetFileName(f);
                    //name = "Malcolm In The Middle S03E" + name;
                    //System.IO.File.Move(f, Path.Combine(Path.GetDirectoryName(f), name));
                    if (f.EndsWith("(4).mp3"))
                    {
                        try
                        {
                            File.Delete(f);
                            Console.WriteLine("DELETED: " + f);
                        }
                        catch
                        {
                            Console.WriteLine("Failed to Delete :" + f);
                        }
                    }
                }
                
            }
            catch (System.Exception excpt)
            {
                Console.WriteLine(excpt.Message);
            }
        }

    }
}
