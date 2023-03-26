using Microsoft.VisualBasic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Globalization;

namespace TestAntSystem
{
    public enum TypeOfSolution
    {
        Normal,
        Elitist,
        Rank,
        Greedy
    }
    public class Ant
    {
        public int capacity;
        public int currentCity;
        public Stack<int> Path;
        public bool[] visitedCities;
        public int pathLength;

        public Ant()
        {
            capacity = 0;
            currentCity = 0;
            Path = new();
            visitedCities = Array.Empty<bool>();
            pathLength = 0;
        }

        public Ant(int capacity, int startCity, int numberOfCities)
        {
            this.capacity = capacity;
            currentCity = startCity;
            Path = new();
            visitedCities = new bool[numberOfCities];
            visitedCities[startCity] = true;
            visitedCities[0] = true;
            pathLength = 0;
        }

        public Ant(int capacity, int currentCity, Stack<int> path, bool[] visitedCities)
        {
            this.capacity = capacity;
            this.currentCity = currentCity;
            Path = path;
            this.visitedCities = visitedCities;
            pathLength = 0;
        }
    }

    public class AntComparer: IComparer<Ant>
    {
        public int Compare(Ant? antX, Ant? antY)
        {
            if( antX == null || antY == null ) return 0;
            if (antX.pathLength > antY.pathLength)
                return 1;
            else if (antX.pathLength == antY.pathLength)
                return 0;

            return -1;
        }
    }

    public class AntSystem
    {
        private Random random;
        private int maxNumberOfIterations;
        private double pheromonePriority; //alfa
        private double heuristicPriority; //beta
        private double pheromoneDecreaseFactor; //rho
        private double pheromoneIncreaseFactor; //Q
        private readonly double defaultPheromoneStartingValue = 1;
        private readonly double probabilityOfRoulette = 0.2;
        private int[] bestPath;
        private double bestSolutionLength = double.MaxValue;
        private readonly int warehouse = 0;
        private Ant[] ants;
        private int capacity;
        private int numberOfElitistAnts;
        private int numberOfRankAnts;
        private int numberOfAnts;
        private int seed;
        private int optimalValue;
        public double valueWorseThan20Normal=0;
        public double valueWorseThan10Normal=0;
        public double valueWorseThan5Normal=0;
        public double valueWorseThan20Elitist = 0;
        public double valueWorseThan10Elitist = 0;
        public double valueWorseThan5Elitist = 0;
        public double valueWorseThan20Rank = 0;
        public double valueWorseThan10Rank = 0;
        public double valueWorseThan5Rank = 0;
        public double valueNormal=0;
        public double valueElitist=0;
        public double valueRank=0;
        private bool first20Normal = false;
        private bool first10Normal = false;
        private bool first5Normal = false;
        private bool first20Elitist = false;
        private bool first10Elitist = false;
        private bool first5Elitist = false;
        private bool first20Rank = false;
        private bool first10Rank = false;
        private bool first5Rank = false;


        public int sMax;
        public int[] demandsOfCities; // indeksy miast od 1, 0 to indeks magazynu
        public int[,] distances;
        public double[,] pheromones;

        public AntSystem(int numberOfAnts, int[] citiesWithDemands, int[,] distances, double alfa, double beta, double Q, double rho, int maxNumberOfIterations, int capacity, int numberOfElitistAnts, int numberOfRankAnts, int seed,int optimalValue, double[,]? pheromones = null)
        {
            this.optimalValue = optimalValue;
            this.seed = seed;
            random = new(seed);
            this.numberOfAnts = numberOfAnts;
            this.numberOfElitistAnts = numberOfElitistAnts;
            this.numberOfRankAnts = numberOfRankAnts;
            demandsOfCities = citiesWithDemands;
            this.distances = distances;
            pheromonePriority = alfa;
            heuristicPriority = beta;
            pheromoneDecreaseFactor = rho;
            pheromoneIncreaseFactor = Q;
            this.maxNumberOfIterations = maxNumberOfIterations;
            this.capacity = capacity;
            
            if(pheromones != null )
            {
                this.pheromones = pheromones;
            }
            else
            {
                SetStartingPheromones();
            }

            bestPath = Array.Empty<int>();
            ants = new Ant[numberOfAnts];
        }

        private void SetStartingPheromones()
        {
            int numberOfEdges = distances.GetLength(0);
            pheromones = new double[numberOfEdges, numberOfEdges];
            for (int i = 0; i < numberOfEdges; i++)
                for (int j = 0; j < numberOfEdges; j++)
                    pheromones[i, j] = defaultPheromoneStartingValue;
        }

        private double CalculateSumOfEdgeWeightsFromCity(int currentCity, Ant ant)
        {
            double sumOfEdgeWeights = 0;
            for (int nextCity = 1; nextCity < demandsOfCities.Length; nextCity++)
            {
                if (CanAntGoToTheCity(ant,nextCity))
                {
                    sumOfEdgeWeights += ProbabilityOfMovingToTheCity(currentCity, nextCity);
                }
            }
            sumOfEdgeWeights = sumOfEdgeWeights < 0.000000001 ? 0.000000001 : sumOfEdgeWeights;
            sumOfEdgeWeights = sumOfEdgeWeights > 100000000 ? 100000000: sumOfEdgeWeights;
            return sumOfEdgeWeights;
        }

        public double[] MakeRouletteArray(Ant ant)
        {
            double[] roulettePercents = new double[demandsOfCities.Length + 1];
            roulettePercents[warehouse] = 0;
            double sumOfEdgeWeightsFromCity = CalculateSumOfEdgeWeightsFromCity(ant.currentCity, ant);
            
            for (int nextCity = 1; nextCity < demandsOfCities.Length; nextCity++)
            {
                if (CanAntGoToTheCity(ant,nextCity))
                {
                    roulettePercents[nextCity] = ProbabilityOfMovingToTheCity(ant.currentCity,nextCity) / sumOfEdgeWeightsFromCity;
                }
                else // Mamy 0% na wylosowanie miasta, w którym aktualnie znajduje się mrówka lub już w nim była lub nie może do niego pójść
                {
                    roulettePercents[nextCity] = 0;
                }
            }
            
            for (int index = 1; index < demandsOfCities.Length; index++)
                roulettePercents[index] = roulettePercents[index] + roulettePercents[index - 1];

            return roulettePercents;
        }
        
        public int ChooseNextCityByRoulette(Ant ant)
        {
            double[] roulette = MakeRouletteArray(ant);
            double makeDraw = random.NextDouble();
            
            for(int i = 0; i < roulette.Length; i++)
            {
                if (ant.currentCity != i && roulette[i] > makeDraw)
                {
                    return i;
                }
            }
            return -1;
        }

        public int ChooseNextCityByPseudoRoulette(Ant ant)
        {
            double makeDraw = random.NextDouble();
            if (makeDraw > probabilityOfRoulette)
            {
                return ChooseNextCityByRoulette(ant);
            }
            else
            {
                return ChooseNearestCity(ant);
            }
        }

        private double ProbabilityOfMovingToTheCity(int currentCity,int nextCity)
        {
            return Math.Pow(pheromones[currentCity, nextCity], pheromonePriority) * Math.Pow(1.0 / distances[currentCity, nextCity], heuristicPriority);
        }

        private bool CanAntGoToTheCity(Ant ant, int nextCity)
        {
            return nextCity != ant.currentCity && !ant.visitedCities[nextCity] && ant.capacity - demandsOfCities[nextCity] >= 0;
        }

        private void InitAnts()
        {
            ants = new Ant[numberOfAnts];
            for( int i = 1; i < ants.Length; i++)
            {
                int city;
                do
                {
                    city = random.Next(1, demandsOfCities.Length);
                } while (capacity - demandsOfCities[city] < 0);
                
                ants[i] = new Ant(capacity, city, demandsOfCities.Length);
                ants[i].capacity -= demandsOfCities[city];
                ants[i].Path.Push(warehouse);
                ants[i].Path.Push(city);
                ants[i].pathLength += distances[warehouse, city];
            }
        }

        private void FindSolution(Ant ant, int id, int iteration,ref string text, TypeOfSolution type)
        {
            
            int startCapacity = capacity;
            
            while (!ant.visitedCities.All(x => x)) // dopóki mrówka nie odwiedziła wszystkich miast
            {
                int nextCity = type == TypeOfSolution.Greedy ? ChooseNearestCity(ant) : ChooseNextCityByPseudoRoulette(ant);
                
                if (nextCity != -1)
                {
                    ant.pathLength += distances[ant.currentCity, nextCity];
                    ant.currentCity = nextCity;
                    ant.capacity -= demandsOfCities[nextCity];
                    ant.Path.Push(nextCity);
                    ant.visitedCities[nextCity] = true;
                }
                else // jeśli nie mogę iść dalej, to wracam do magazynu
                {
                    ant.pathLength += distances[ant.currentCity, warehouse];
                    ant.currentCity = warehouse;
                    ant.Path.Push(warehouse);
                    ant.capacity = startCapacity;
                }
            }

            // dodaj jeszcze połączenie do magazynu
            ant.pathLength += distances[ant.currentCity, warehouse];
            ant.Path.Push(warehouse);
            
            if (ant.pathLength < bestSolutionLength)
            {
                Console.WriteLine($"Mrówka {id} znalazła rozwiązanie: " + ant.pathLength + " w iteracji: "+iteration + $"  Wynik gorszy od optymalnego o {Math.Round(((double)ant.pathLength/optimalValue - 1)*100,3)}%");
                text += $"Mrówka {id} znalazła rozwiązanie: " + ant.pathLength + " w iteracji: " + iteration + $"  Wynik gorszy od optymalnego o {Math.Round(((double)ant.pathLength / optimalValue - 1) * 100, 3)}%\n";
                bestPath = ant.Path.ToArray();
                bestSolutionLength = ant.pathLength;

                if(Math.Round(((double)ant.pathLength / optimalValue - 1) * 100, 3) <= 20)
                {
                    if (TypeOfSolution.Normal == type && !first20Normal)
                    {
                        valueWorseThan20Normal += iteration;
                        first20Normal = true;
                    }
                    if (TypeOfSolution.Elitist == type && !first20Elitist)
                    {
                        valueWorseThan20Elitist += iteration;
                        first20Elitist = true;
                    }
                    if (TypeOfSolution.Rank == type && !first20Rank)
                    {
                        valueWorseThan20Rank += iteration;
                        first20Rank = true;
                    }
                }
                if (Math.Round(((double)ant.pathLength / optimalValue - 1) * 100, 3) <= 10)
                {
                    if (TypeOfSolution.Normal == type && !first10Normal)
                    {
                        valueWorseThan10Normal += iteration;
                        first10Normal = true;
                    }
                    if (TypeOfSolution.Elitist == type && !first10Elitist)
                    {
                        valueWorseThan10Elitist += iteration;
                        first10Elitist = true;
                    }
                    if (TypeOfSolution.Rank == type && !first10Rank)
                    {
                        valueWorseThan10Rank += iteration;
                        first10Rank = true;
                    }
                }
                if (Math.Round(((double)ant.pathLength / optimalValue - 1) * 100, 3) <= 5)
                {
                    if (TypeOfSolution.Normal == type && !first5Normal)
                    {
                        valueWorseThan5Normal += iteration;
                        first5Normal = true;
                    }
                    if (TypeOfSolution.Elitist == type && !first5Elitist)
                    {
                        valueWorseThan5Elitist += iteration;
                        first5Elitist = true;
                    }
                    if (TypeOfSolution.Rank == type && !first5Rank)
                    {
                        valueWorseThan5Rank += iteration;
                        first5Rank = true;
                    }
                }
            }   
        }

        private void EvaporatePheromone()
        {
            for (int i = 0; i < pheromones.GetLength(0); i++)
            {
                for (int j = 0; j < pheromones.GetLength(1); j++)
                {
                    double decrease = (1.0 - pheromoneDecreaseFactor) * pheromones[i, j];
                    pheromones[i, j] = decrease;
                    pheromones[j, i] = decrease;
                }
            }
        }

        private void UpdatePheromone(TypeOfSolution type)
        {
            int n = ants.Length;
            if (type == TypeOfSolution.Rank)
            {
                Array.Sort(ants, new AntComparer());
                n = numberOfRankAnts;
            }
            /*
            else if(type == TypeOfSolution.Elitist)
            {
                Array.Sort(ants, new AntComparer());
                n = numberOfElitistAnts;
            }
            */
            for (int i = 1; i < n; i++)
            {
                while (ants[i].Path.Count > 1)
                {
                    int secondCity = ants[i].Path.Pop();
                    int firstCity = ants[i].Path.Peek();
                    
                    double length = ants[i].pathLength;

                    int rank = type == TypeOfSolution.Rank ? n - i - 1 : 1;
                    double increase = rank * pheromoneIncreaseFactor / length;

                    pheromones[firstCity, secondCity] += increase;
                    pheromones[secondCity, firstCity] += increase;
                }
            }

            UpdatePheromoneOnTheBestSolution(type);
        }

        private void UpdatePheromoneOnTheBestSolution(TypeOfSolution type)
        {
            if (type == TypeOfSolution.Rank || type == TypeOfSolution.Elitist)
            {
                for (int i = 0; i < bestPath.Length - 1; i++)
                {
                    int secondCity = bestPath[i];
                    int firstCity = bestPath[i + 1];
                    double increase = (type == TypeOfSolution.Rank ? numberOfRankAnts : numberOfElitistAnts) * pheromoneIncreaseFactor / bestSolutionLength;

                    pheromones[firstCity, secondCity] += increase;
                    pheromones[secondCity, firstCity] += increase;
                }
            }
        }

        private void PrintBestSolution(ref string text)
        {
            Console.WriteLine($"Rozwiązanie dla ziarna {seed}: " + bestSolutionLength);
            text += $"Rozwiązanie dla ziarna {seed}: " + bestSolutionLength + '\n';

            int routeNumber = 0;
            for (int i = 0; i < bestPath.Length; i++)
            {
                if (bestPath[i] == 0 && i != bestPath.Length - 1)
                {
                    Console.WriteLine();
                    text += '\n';
                    Console.Write("Route #" + ++routeNumber);
                    text += "Route #" + routeNumber;
                }
                else if (bestPath[i] != 0)
                {
                    Console.Write(" " + bestPath[i]);
                    text += " " + bestPath[i];
                }
            }
            Console.WriteLine();
            text += '\n';
        }

        public string AntSystemSoultion(TypeOfSolution type)
        {
            ResetData();
            string text = "Optymalne rozwiązanie: "+optimalValue+"\n";
            if (type == TypeOfSolution.Elitist)
            {
                Console.WriteLine("Liczba elitarnych mrówek: " + numberOfElitistAnts);
                text += "Liczba elitarnych mrówek: " + numberOfElitistAnts + '\n';
            }
            if (type == TypeOfSolution.Rank)
            {
                Console.WriteLine("Liczba rangowych mrówek: " + numberOfRankAnts);
                text += "Liczba rangowych mrówek: " + numberOfRankAnts + '\n';
            }
            
            if (type != TypeOfSolution.Greedy)
            {
                for (int i = 0; i < maxNumberOfIterations; i++)
                {
                    // stwórz mrówki
                    InitAnts();

                    // dla każdej mrówki
                    for (int j = 1; j < ants.Length; j++)
                    {
                        // znajdź rozwiązanie
                        FindSolution(ants[j], j, i, ref text, type);
                    }
                    // wyparuj feromon
                    EvaporatePheromone();

                    // rozłóż feromon
                    UpdatePheromone(type);
                }
            }
            else
            {
                Ant ant = new(capacity, warehouse, demandsOfCities.Length);
                FindSolution(ant, 0, 0, ref text, type);
            }
            if (TypeOfSolution.Elitist == type)
                valueElitist += bestSolutionLength;
            if (TypeOfSolution.Rank == type)
                valueRank += bestSolutionLength;
            if (TypeOfSolution.Normal == type)
                valueNormal += bestSolutionLength;
            PrintBestSolution(ref text);
            return text;
        }

        private int ChooseNearestCity(Ant ant)
        {
            int min = int.MaxValue;

            int index = -1;

            for (int nextCity = 1; nextCity < demandsOfCities.Length; nextCity++)
            {
                if (CanAntGoToTheCity(ant, nextCity) && min > distances[ant.currentCity, nextCity])
                {
                    min = distances[ant.currentCity, nextCity];
                    index = nextCity;
                }
            }
            return index;
        }

        private void ResetData()
        {
            bestSolutionLength = double.MaxValue;
            bestPath = Array.Empty<int>();
            random = new(seed);
            int numberOfEdges = distances.GetLength(0);
            this.pheromones = new double[numberOfEdges, numberOfEdges];
            for (int i = 0; i < numberOfEdges; i++)
                for (int j = 0; j < numberOfEdges; j++)
                    this.pheromones[i, j] = defaultPheromoneStartingValue;

            first20Normal = false;
            first20Elitist = false;
            first20Rank = false;
            first10Normal = false;
            first10Elitist = false;
            first10Rank = false;
            first5Normal = false;
            first5Elitist = false;
            first5Rank = false;

        }

        public void ChangeSeed(int newSeed)
        {
            seed = newSeed;
        }

        public void ChangeAlfa(double newAlfa)
        {
            pheromonePriority = newAlfa;
        }

        public void ChangeBeta(double newBeta)
        {
            heuristicPriority = newBeta;
        }
    }
}
