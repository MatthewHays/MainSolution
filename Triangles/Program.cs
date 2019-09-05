using CsvHelper;
using CsvHelper.Configuration;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Triangles
{
    /// <summary>
    /// Expected output from given input in DAta.csv
    /// 1990, 4
    /// Comp, 0, 0, 0, 0, 0, 0, 0, 110, 280, 200
    /// Non - Comp, 45.2, 110, 110, 147, 50, 125, 150, 55, 140, 100
    /// </summary>
    class Program
    {
        /// <summary>
        /// Represents a single point in the triangle
        /// ie a Row in the source data
        /// </summary>
        public class TriangleData
        {   
            public string Product          { get; set; }
            public int    OriginYear       { get; set; }
            public int    DevelopmentYear  { get; set; }
            public double IncrementalValue { get; set; }
            public double CumulativeValue  { get; set; } 
        }

        #region Private Methods
        /// <summary>
        /// Load triangle data from the supplied file
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        private static IList<TriangleData> LoadData(string fileName)
        {
            var config = new Configuration();
            config.HeaderValidated = null;
            config.MissingFieldFound = null;
            //match the column name to the field in the Data class, trying to be as 'loose' as possible to ensure a match
            config.PrepareHeaderForMatch += (h, i) =>
            {
                h = h.Trim();
                return h.Replace(" ", string.Empty);
            };
            //TODO how else can we sanity check the csv, is the whole data set considered bad when a single value is bad etc??

            try
            { 
                using (var reader = new StreamReader(fileName))
                {
                    using (var csv = new CsvReader(reader, config))
                    {
                        //materialise the full enumerable into memory as I'm disposing the reader etc afterwards, 
                        //I assume its not a huge collection
                        //Ideally I could stream this as produce a running tally as I read each line is the most efficient
                        return csv.GetRecords<TriangleData>().OrderBy(d => d.Product).ToList();
                    }
                }
            } catch (Exception e)
            {
                //TODO log exception
                //rethrow to make the application crash and die
                throw;
            }
        }

        /// <summary>
        /// Given triangledata for single product, generate the string that represents it
        /// </summary>
        /// <param name="pGroup"></param>
        /// <returns></returns>
        private static string CalculaterProductTriangle(IEnumerable<TriangleData> pGroup)
        {
            var output = string.Empty;

            //make the origin and development years contiguous
            var sorted = pGroup.OrderBy(d => d.OriginYear).ThenBy(d => d.DevelopmentYear).ToList();

            //Fold the data along the origin year using the accumulate function. Because the data is sorted, 
            //I know the development years are incremental, and I can easily detect breaks etc
            sorted.Aggregate(null, (TriangleData prev, TriangleData current) =>
            {
                if (prev == null)
                {
                    //this is the first item in the list of value, we need to treat it 'specially'
                    current.CumulativeValue = current.IncrementalValue;
                    output += current.CumulativeValue + ",";
                }
                else if (prev.OriginYear == current.OriginYear)
                {
                    current.CumulativeValue = prev.CumulativeValue + current.IncrementalValue;

                    //if there's a break in the development years, then assume the incremental value is 0
                    //therefore the cumulative will be unchanged, and we can 'auto' pad
                    for (int i = prev.DevelopmentYear; i < current.DevelopmentYear - 1; ++i)
                        output += prev.CumulativeValue + ",";
                    output += current.CumulativeValue + ",";
                }
                else
                {
                    //new origin year, reset the cumulative amount
                    current.CumulativeValue = current.IncrementalValue;
                    output += current.CumulativeValue + ",";
                }

                return current;
            });

            return output;
        }
        #endregion

        /// <summary>
        /// TODO
        /// This needs to be broken out into a class that can be mocked (ie SOLID etc). 
        /// I need to be able to pass test data into the class to be able to test edge cases/scenarios without involving files etc
        /// For now I just focused on solving the main problem due to time constraints.
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            var listOfData = LoadData("Data.csv");

            //figure out the earliest origin year, and the latest development period 
            var earliestOrigin           = listOfData.Min(d => d.OriginYear);
            var largestDevelopmentPeriod = listOfData.Max(d => d.DevelopmentYear - d.OriginYear) + 1;
            var latestDevelopmentPeriod  = listOfData.Max(d => d.DevelopmentYear);

            //get the distinct set of products, I have to materialise them now 
            //as I need to modify the collection below. I realise that I'm traversing the list mutiple times here,
            //when I could be doing this once.
            var allProducts = listOfData.Select(d => d.Product).Distinct().ToList();

            //Fill in any missing ranges, perhaps not the most efficient way
            foreach (var product in allProducts)
            {
                for (int currentOrigin = earliestOrigin; currentOrigin <= latestDevelopmentPeriod; ++currentOrigin)
                    for (int currentDevelopmentPeriod = currentOrigin; currentDevelopmentPeriod <= latestDevelopmentPeriod; ++currentDevelopmentPeriod)
                    {
                        if (!listOfData.Any(d => d.Product == product && d.OriginYear == currentOrigin && d.DevelopmentYear == currentDevelopmentPeriod))
                        {
                            listOfData.Add(new TriangleData() { Product = product, OriginYear = currentOrigin, DevelopmentYear = currentDevelopmentPeriod, IncrementalValue = 0.0 });
                            //Console.WriteLine($"Adding {product}-{currentOrigin}-{currentDevelopmentPeriod}");
                        }
                    }
            }

            //output the data, no real need for a csv specific helper class here.
            //TODO ensure the file is available to write (not locked etc)
            using (var writer = new StreamWriter("Output.csv"))
            {
                writer.WriteLine($"{earliestOrigin}, {largestDevelopmentPeriod}");

                //I could to toLookup here, or just iterate through as we've sorted by product, but groups make this simple to 
                //isolate each product set and make it a bit more readable
                foreach (var pGroup in listOfData.GroupBy(d => d.Product))
                {
                    string output = pGroup.Key + "," + CalculaterProductTriangle(pGroup);
                    
                    writer.WriteLine(output);
                    
                    //just to make my debugging easier 
                    Debug.WriteLine(output);
                    Console.WriteLine(output);
                }
            } //using
            
            Console.WriteLine("Data processing complete, press enter");
            Console.ReadLine();
        } //Main
    } //class
} //namespace
