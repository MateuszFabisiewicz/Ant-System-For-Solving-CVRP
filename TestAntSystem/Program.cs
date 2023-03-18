using System.IO;

namespace TestAntSystem
{
    internal class Program
    {
        static void Main(string[] args)
        {
            /*
            string[] points = File.ReadAllLines("points3.txt");
            string[] demands = File.ReadAllLines("demands3.txt");
            
            int[] citiesWithDemands = new int[demands.Length];
            //citiesWithDemands[0] = citiesWithDemands[1] = citiesWithDemands[2] = citiesWithDemands[3] = 1;
            double[,] distances = new double[points.Length, points.Length];

            for (int i = 0; i < citiesWithDemands.Length; i++)
            {
                citiesWithDemands[i] = int.Parse(demands[i]);
                //Console.WriteLine(demands[i]);
            }

            for (int i = 0; i < points.Length; i++)
            {
                string[] pointA = points[i].Split(" ");
                //Console.WriteLine(pointA[0] + ";" + pointA[1]);
                for (int j = 0; j < points.Length; j++)
                {
                    string[] pointB = points[j].Split(" ");
                    //Console.WriteLine(pointB[0] + ";" + pointB[1]);
                    distances[i, j] = Math.Sqrt(Math.Pow(int.Parse(pointA[0])- int.Parse(pointB[0]),2) + Math.Pow(int.Parse(pointA[1]) - int.Parse(pointB[1]), 2));
                }
            }
            */
            
            string path = Path.Combine(Directory.GetCurrentDirectory(), @"..\..\..\A-n32-k5.vrp.txt");
            CVRPFIlesReader reader = new(path);

            AntSystem antSystem = new(62, reader.GetDemands(), reader.GetDistances(), 3,2,2,0.1,1500,reader.GetCapacity());
            antSystem.AntSystemSoultion();
            
            /*
            string path = Path.Combine(Directory.GetCurrentDirectory(), @"..\..\..\P-n23-k8.vrp.txt");
            CVRPFIlesReader reader = new(path);

            AntSystem antSystem = new(44, reader.GetDemands(), reader.GetDistances(), 3, 2, 2, 0.1, 1500, reader.GetCapacity());
            antSystem.AntSystemSoultion();
            */
            /*
            string path = Path.Combine(Directory.GetCurrentDirectory(), @"..\..\..\P-n40-k5.vrp.txt");
            CVRPFIlesReader reader = new(path);

            AntSystem antSystem = new(80, reader.GetDemands(), reader.GetDistances(), 3, 2, 2, 0.1, 1000, reader.GetCapacity());
            antSystem.AntSystemSoultion();
            */
            /*
            string path = Path.Combine(Directory.GetCurrentDirectory(), @"..\..\..\P-n16-k8.vrp.txt");
            CVRPFIlesReader reader = new(path);

            AntSystem antSystem = new(30, reader.GetDemands(), reader.GetDistances(), 3, 2, 2, 0.1, 100, reader.GetCapacity());
            antSystem.AntSystemSoultion();
            */
        }

    }
}