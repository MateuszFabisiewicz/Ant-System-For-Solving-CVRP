using System.IO;

namespace TestAntSystem
{
    internal class Program
    {
        static void Main(string[] args)
        {

            string[] fileNames = { "A-n37-k5.vrp","A-n45-k7.vrp","P-n70-k10.vrp","P-n51-k10.vrp","P-n23-k8.vrp", "P-n40-k5.vrp", "A-n32-k5.vrp", "E-n33-k4.vrp", "P-n16-k8.vrp"};

            /*
            string[] paths = new string[10];
            for(int i=0;i<paths.Length;i++)
                paths[i] = Path.Combine(Directory.GetCurrentDirectory(), @"../../../" + fileNames[i] +".txt");
            */
            CVRPFIlesReader[] readers = new CVRPFIlesReader[7];
            for (int i = 0; i < readers.Length; i++)
                readers[i] = new(Path.Combine(Directory.GetCurrentDirectory(), @"../../../" + fileNames[i] + ".txt"));

            Parallel.ForEach(readers,
                reader =>
                {
                    int[] demands = reader.GetDemands();
                    int numberOfAnts = 2 * demands.Length - 2;
                    AntSystem antSystem = new(numberOfAnts, demands, reader.GetDistances(), 3, 2, 2, 0.1, 2500, reader.GetCapacity(), numberOfAnts / 3, numberOfAnts / 12, 1, reader.GetOptimalValue());
                    string textNormal = $"Liczba mrówek: {numberOfAnts}\nLiczba ciężarówek: {reader.GetNumberOfTrucks()}\n";
                    string textElitist = $"Liczba mrówek: {numberOfAnts}\nLiczba ciężarówek: {reader.GetNumberOfTrucks()}\nLiczba elitystycznych mrówek: {numberOfAnts/3}\n";
                    string textRank = $"Liczba mrówek: {numberOfAnts}\nLiczba ciężarówek: {reader.GetNumberOfTrucks()}\nLiczba rangowych mrówek: {numberOfAnts / 12}\n"; ;
                    string textGreedy = $"Liczba mrówek: {numberOfAnts}\nLiczba ciężarówek: {reader.GetNumberOfTrucks()}\n";
                    for (int i = 2; i < 12; i++)
                    {
                    Console.WriteLine("Ant System:");
                    textNormal += "Ant System: \n" + antSystem.AntSystemSoultion(TypeOfSolution.Normal);
                    Console.WriteLine("Elitist Ant System:");
                    textElitist += "Elitist Ant System: \n" + antSystem.AntSystemSoultion(TypeOfSolution.Elitist);
                    Console.WriteLine("Rank Ant System:");
                    textRank += "Rank Ant System: \n" + antSystem.AntSystemSoultion(TypeOfSolution.Rank);
                    
                    antSystem.ChangeSeed(i);
                    }
                    Console.WriteLine("Greedy Ant:");
                    textGreedy += "\nGreedy Ant: \n" + antSystem.AntSystemSoultion(TypeOfSolution.Greedy);

                    string pathNormal = Path.Combine(Directory.GetCurrentDirectory(), @"../../../" + reader.GetName() + ".Normal.txt");
                    string pathElitist = Path.Combine(Directory.GetCurrentDirectory(), @"../../../" + reader.GetName() + ".Elitist.txt");
                    string pathRank = Path.Combine(Directory.GetCurrentDirectory(), @"../../../" + reader.GetName() + ".Rank.txt");
                    string pathGreedy = Path.Combine(Directory.GetCurrentDirectory(), @"../../../" + reader.GetName() + ".Greedy.txt");
                    File.AppendAllText(pathNormal,textNormal);
                    File.AppendAllText(pathElitist, textElitist);
                    File.AppendAllText(pathRank, textRank);
                    File.AppendAllText(pathGreedy, textGreedy);
                });
        }

    }
}