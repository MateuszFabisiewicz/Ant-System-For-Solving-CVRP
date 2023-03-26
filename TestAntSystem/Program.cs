using System.IO;
using System.Reflection.PortableExecutable;

namespace TestAntSystem
{
    public struct AlfaBeta
    {
        public double alfa;
        public double beta;

        public AlfaBeta(double alfa, double beta)
        {
            this.alfa = alfa;
            this.beta = beta;
        }
    }
    
    internal class Program
    {
        static void Main(string[] args)
        {

            string[] fileNames = {"P-n70-k10.vrp","P-n51-k10.vrp","P-n23-k8.vrp", "P-n40-k5.vrp","E-n33-k4.vrp", "P-n16-k8.vrp","A-n37-k5.vrp","A-n45-k7.vrp","A-n32-k5.vrp"};
            
            CVRPFIlesReader[] readers = new CVRPFIlesReader[8];
            for (int i = 0; i < readers.Length; i++)
                readers[i] = new(Path.Combine(Directory.GetCurrentDirectory(), @"../../../" + fileNames[i] + ".txt"));

            AlfaBeta[] alfaBetas = {
                new(0,1),
                new(1,0),
                new(2,3),
                new(3,2),
                new(1,1),
                new(2,2),
                new(3,3),
                new(3,1),
                new(1,3),
                new(4,4),
            };

            int[] seeds = { 1,2,3,4,5,6,7,8,9,10};
            /*
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
            */
            /*
            int[] demands = readers[6].GetDemands();
            int numberOfAnts = 2 * demands.Length - 2;
            AntSystem antSystem = new(numberOfAnts, demands, readers[6].GetDistances(), 3, 2, 2, 0.1, 2500, readers[6].GetCapacity(), numberOfAnts / 3, numberOfAnts / 12, 1, readers[6].GetOptimalValue());
            antSystem.AntSystemSoultion(TypeOfSolution.Normal);
            antSystem.AntSystemSoultion(TypeOfSolution.Elitist);
            antSystem.AntSystemSoultion(TypeOfSolution.Rank);

            antSystem = new(numberOfAnts, demands, readers[6].GetDistances(), 2, 3, 2, 0.1, 2500, readers[6].GetCapacity(), numberOfAnts / 3, numberOfAnts / 12, 1, readers[6].GetOptimalValue());
            antSystem.AntSystemSoultion(TypeOfSolution.Normal);
            antSystem.AntSystemSoultion(TypeOfSolution.Elitist);
            antSystem.AntSystemSoultion(TypeOfSolution.Rank);
            */
            Parallel.ForEach(readers,
                reader =>
                {
                    int[] demands = reader.GetDemands();
                    int numberOfAnts = 2 * demands.Length - 2;
                    string pathNormal = Path.Combine(Directory.GetCurrentDirectory(), @"../../../" + reader.GetName() + ".Normal6.txt");
                    string pathElitist = Path.Combine(Directory.GetCurrentDirectory(), @"../../../" + reader.GetName() + ".Elitist6.txt");
                    string pathRank = Path.Combine(Directory.GetCurrentDirectory(), @"../../../" + reader.GetName() + ".Rank6.txt");
                    string textNormal = $"Liczba mrówek: {numberOfAnts}\nLiczba ciężarówek: {reader.GetNumberOfTrucks()}\n";
                    string textElitist = $"Liczba mrówek: {numberOfAnts}\nLiczba ciężarówek: {reader.GetNumberOfTrucks()}\nLiczba elitystycznych mrówek: {numberOfAnts / 3}\n";
                    string textRank = $"Liczba mrówek: {numberOfAnts}\nLiczba ciężarówek: {reader.GetNumberOfTrucks()}\nLiczba rangowych mrówek: {numberOfAnts / 12}\n"; ;
                    string textGreedy = $"Liczba mrówek: {numberOfAnts}\nLiczba ciężarówek: {reader.GetNumberOfTrucks()}\n";
                    File.AppendAllText(pathNormal, textNormal);
                    File.AppendAllText(pathElitist, textElitist);
                    File.AppendAllText(pathRank, textRank);
                    Parallel.ForEach(alfaBetas,
                        alfaBeta =>
                        {
                            Console.WriteLine("Alfa: "+alfaBeta.alfa);
                            Console.WriteLine("Beta: " + alfaBeta.beta);
                            AntSystem antSystem = new(numberOfAnts, demands, reader.GetDistances(), alfaBeta.alfa, alfaBeta.beta, 2, 0.1, 2500, reader.GetCapacity(), numberOfAnts / 3, numberOfAnts / 12, 1, reader.GetOptimalValue());
                            Console.WriteLine("Ant System:");
                            string textNormal = "Ant System: \n" + "Alfa: " + alfaBeta.alfa + " Beta: " + alfaBeta.beta + '\n';
                            Console.WriteLine("Elitist Ant System:");
                            string textElitist = "Elitist Ant System: \n" + "Alfa: " + alfaBeta.alfa + " Beta: " + alfaBeta.beta + '\n';
                            Console.WriteLine("Rank Ant System:");
                            string textRank = "Rank Ant System: \n" + "Alfa: " + alfaBeta.alfa + " Beta: " + alfaBeta.beta + '\n';
                            for(int i=2;i<12;i++)//Parallel.ForEach(seeds, seed =>
                            {
                                antSystem.AntSystemSoultion(TypeOfSolution.Normal);
                                antSystem.AntSystemSoultion(TypeOfSolution.Elitist);
                                antSystem.AntSystemSoultion(TypeOfSolution.Rank);
                                antSystem.ChangeSeed(i);
                            };
                            textNormal += "Wartość średnia: "+antSystem.valueNormal/10;
                            textNormal += "\nLiczba iteracji dla której wynik jest gorszy o co najwyżej 20%: " + antSystem.valueWorseThan20Normal / 10;
                            textNormal += "\nLiczba iteracji dla której wynik jest gorszy o co najwyżej 10%: " + antSystem.valueWorseThan10Normal / 10;
                            textNormal += "\nLiczba iteracji dla której wynik jest gorszy o co najwyżej 5%: " + antSystem.valueWorseThan5Normal / 10;

                            textElitist += "Wartość średnia: " + antSystem.valueElitist / 10;
                            textElitist += "\nLiczba iteracji dla której wynik jest gorszy o co najwyżej 20%: " + antSystem.valueWorseThan20Elitist / 10;
                            textElitist += "\nLiczba iteracji dla której wynik jest gorszy o co najwyżej 10%: " + antSystem.valueWorseThan10Elitist / 10;
                            textElitist += "\nLiczba iteracji dla której wynik jest gorszy o co najwyżej 5%: " + antSystem.valueWorseThan5Elitist / 10;

                            textRank += "Wartość średnia: " + antSystem.valueRank / 10;
                            textRank += "\nLiczba iteracji dla której wynik jest gorszy o co najwyżej 20%: " + antSystem.valueWorseThan20Rank / 10;
                            textRank += "\nLiczba iteracji dla której wynik jest gorszy o co najwyżej 10%: " + antSystem.valueWorseThan10Rank / 10;
                            textRank += "\nLiczba iteracji dla której wynik jest gorszy o co najwyżej 5%: " + antSystem.valueWorseThan5Rank / 10;
                            File.AppendAllText(pathNormal, textNormal);
                            File.AppendAllText(pathElitist, textElitist);
                            File.AppendAllText(pathRank, textRank);
                        });
                });
        }

    }
}