using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenCvSharp;
using OpenCvSharp.Extensions;

namespace Test
{
    internal class Program
    {
       
        static void Main(string[] args)
        {
            SiftSurfSample siftSurfSample = new SiftSurfSample();
            siftSurfSample.RunTest("D:\\matchImages\\1.bmp", "D:\\matchImages\\2.bmp", "D:\\matchImages\\3.bmp", "D:\\matchImages\\4.bmp");


            Console.ReadLine();

        }
    }
}
