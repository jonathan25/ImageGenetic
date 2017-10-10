using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace ImageGenetic.ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            /*var x = ImageGenetic.Core.ImageProcessing.FromRGBToHSL(255, 0, 0);
            var y = ImageGenetic.Core.ImageProcessing.FromRGBToHSL(255, 255, 0);
            var z = ImageGenetic.Core.ImageProcessing.FromRGBToHSL(0, 0, 255);*/
            //Console.WriteLine("{0} {1} {2}", x.Item1, y.Item1, z.Item1);
            //int resolution = 1024;
            
            var proc = new Core.ImageGenetic(10, 100);
            proc.Start();
            int i = 0;
            foreach (var image in proc.PopulationBitmaps)
            {
                image.Save("test-" + proc.Aptitudes[i] + "-" +  image.GetHashCode() + ".png");
                i++;
            }
            Console.WriteLine("done");
        }
        public static double StandardDeviation(IEnumerable<int> values)
        {
            double avg = values.Average();
            return Math.Sqrt(values.Average(v => Math.Pow(v - avg, 2)));
        }
    }


}
