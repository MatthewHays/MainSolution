using CSharpTest.Net.Collections;
using CSharpTest.Net.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scratch
{
    class Program
    {
        static void Main(string[] args)
        {
            var options = new BPlusTree<string, string>.OptionsV2(PrimitiveSerializer.String, PrimitiveSerializer.String);
            options.CalcBTreeOrder(16, 24);
            options.CreateFile = CreatePolicy.Always;
            options.StoragePerformance = StoragePerformance.Fastest;
            options.FileName = "c:\\temp\\dict.txt";
            options.ExistingLogAction = ExistingLogAction.Truncate;
            options.TransactionLogFileName = null;

            var a =new BPlusTree<string, string>(options);
            a.EnableCount();

            /*var start = DateTime.Now;
            for (var i=0; i<1000000; ++i)
            {
                a.Add(i.ToString(), i.ToString() + i.ToString() + i.ToString() + i.ToString()+ i.ToString()+ i.ToString()+ i.ToString()+ i.ToString()+ i.ToString());
                //a.Commit();
            }
            Console.WriteLine(DateTime.Now-start);*/


            var b = new Dictionary<string, string>();
            var start = DateTime.Now;
            for (var i = 0; i < 1000000; ++i)
            {
                b.Add(i.ToString(), i.ToString() + i.ToString() + i.ToString() + i.ToString() + i.ToString() + i.ToString() + i.ToString() + i.ToString() + i.ToString());
                //a.Commit();
            }
            Console.WriteLine(DateTime.Now - start);

            /*start = DateTime.Now;
            var a1 = a["50000"];
            Console.WriteLine(DateTime.Now - start);*/
            start = DateTime.Now;
            for (var count=0;count<1000;++count)
            var b1 = b["50000"];
            Console.WriteLine(DateTime.Now - start);
            
            Console.WriteLine("done");
        }
    }
}
