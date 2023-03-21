using System.IO;

namespace TestAntSystem
{
    internal class Program
    {
        static void Main(string[] args)
        {
            
            
            string path = Path.Combine(Directory.GetCurrentDirectory(), @"../../../A-n32-k5.vrp.txt");
            CVRPFIlesReader reader = new(path);

            string text = "";

            AntSystem antSystem = new(62, reader.GetDemands(), reader.GetDistances(), 3,2,2,0.1,1500,reader.GetCapacity(),30,5,1);
            //for (int i = 2; i < 12; i++)
            //{
                Console.WriteLine("Ant System:");
                text += "Ant System: \n" + antSystem.AntSystemSoultion(TypeOfSolution.Normal);
                Console.WriteLine("Elitist Ant System:");
                text += "\n Elitist Ant System: \n" + antSystem.AntSystemSoultion(TypeOfSolution.Elitist);
                Console.WriteLine("Rank Ant System:");
                text += "\n Rank Ant System: \n" + antSystem.AntSystemSoultion(TypeOfSolution.Rank);
                Console.WriteLine("Greedy Ant:");
                text += "\n Greedy Ant: \n" + antSystem.AntSystemSoultion(TypeOfSolution.Greedy);
            //antSystem.ChangeSeed(i);
            //}
            path = Path.Combine(Directory.GetCurrentDirectory(), @"../../../test.txt");
            File.AppendAllText(path,text);
            /*
            string path = Path.Combine(Directory.GetCurrentDirectory(), @"../../../P-n23-k8.vrp.txt");
            CVRPFIlesReader reader = new(path);

            AntSystem antSystem = new(44, reader.GetDemands(), reader.GetDistances(), 3, 2, 2, 0.1, 1500, reader.GetCapacity());
            antSystem.AntSystemSoultion();
            */
            /*
            string path = Path.Combine(Directory.GetCurrentDirectory(), @"../../../P-n40-k5.vrp.txt");
            CVRPFIlesReader reader = new(path);

            AntSystem antSystem = new(80, reader.GetDemands(), reader.GetDistances(), 3, 2, 2, 0.1, 1000, reader.GetCapacity());
            antSystem.AntSystemSoultion();
            */
            /*
            string path = Path.Combine(Directory.GetCurrentDirectory(), @"../../../P-n16-k8.vrp.txt");
            CVRPFIlesReader reader = new(path);

            AntSystem antSystem = new(30, reader.GetDemands(), reader.GetDistances(), 3, 2, 2, 0.1, 100, reader.GetCapacity());
            antSystem.AntSystemSoultion();
            */
        }

    }
}