using System.Collections.Immutable;
using System.Diagnostics;
using System.Globalization;

namespace TestAntSystem
{
    public class Ant
    {
        public int capacity;
        public int currentCity;
        public Stack<int> Path;
        public bool[] visitedCities;
        public double pathLength;
        public int usedTrucks;

        public Ant()
        {
            capacity = 0;
            currentCity = 0;
            Path = new();
            visitedCities = Array.Empty<bool>();
            pathLength = 0;
            usedTrucks = 0;
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
            usedTrucks = 0;
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
        private readonly Random random = new(1); //69 123456789 123
        private int maxNumberOfIterations = 1000;
        private double pheromonePriority = 3; //alfa
        private double heuristicPriority = 2; //beta
        private double pheromoneDecreaseFactor = 0.1; //rho
        private double pheromoneIncreaseFactor = 2.0; //Q
        private double defaultPheromoneStartingValue = 1;
        private double probabilityOfRoulette = 0.2;
        private int[] bestPath;
        private double bestSolution = double.MaxValue;
        private int warehouse = 0;
        private Ant[] ants;
        private int capacity = 35;
        private int numberOfElitistAnts = 3;
        private int numberOfRankAnts = 5;
        private int numberOfTrucks = 5;


        public int sMax;
        public int[] demandsOfCities; // indeksy miast od 1, 0 to indeks magazynu
        public double[,] distances;
        public double[,] pheromones;
        
        public AntSystem(int numberOfAnts, int[] citiesWithDemands, double[,] distances, double[,]? pheromones = null)
        { 
            this.demandsOfCities = citiesWithDemands;
            this.distances = distances;
            
            if(pheromones != null )
            {
                this.pheromones = pheromones;
            }
            else
            {
                int numberOfEdges = distances.GetLength(0);
                this.pheromones = new double[numberOfEdges, numberOfEdges];
                for (int i = 0; i < numberOfEdges; i++)
                    for (int j = 0; j < numberOfEdges; j++)
                        this.pheromones[i, j] = defaultPheromoneStartingValue;
            }

            bestPath = Array.Empty<int>();
            ants = new Ant[numberOfAnts];
            /*
            Console.WriteLine("Dystanse międzi miastami: ");
            for (int i = 0; i < distances.GetLength(0); i++)
                for (int j = 0; j < distances.GetLength(1); j++)
                    Console.WriteLine($"Dystans między {i}-{j}: {distances[i,j]}");
            */
        }

        private double CalculateSumOfEdgeWeightsFromCity(int currentCity, Ant ant)
        {
            double sumOfEdgeWeights = 0;
            for (int nextCity = 1; nextCity < demandsOfCities.Length; nextCity++)
            {
                if (CanAntGoToTheCity(ant,nextCity))
                {
                    sumOfEdgeWeights += ProbabilityOfMovingToTheCity(currentCity, nextCity);
                    //Console.WriteLine(sumOfEdgeWeights);
                }
            }
            sumOfEdgeWeights = sumOfEdgeWeights < 0.0000000001 ? 0.0000000001 : sumOfEdgeWeights;
            sumOfEdgeWeights = sumOfEdgeWeights > 1000000000 ? 1000000000 : sumOfEdgeWeights;
            return sumOfEdgeWeights;
        }

        public double[] MakeRouletteArray(Ant ant)
        {
            double[] roulettePercents = new double[demandsOfCities.Length+1];
            roulettePercents[warehouse] = 0;
            double sumOfEdgeWeightsFromCity = CalculateSumOfEdgeWeightsFromCity(ant.currentCity, ant);
            //Console.WriteLine("Suma: "+sumOfEdgeWeightsFromCity);
            for (int nextCity = 1; nextCity < demandsOfCities.Length; nextCity++)
            {
                if (CanAntGoToTheCity(ant,nextCity))
                {
                    //Console.WriteLine("Miasto mrówki: " + ant.currentCity);
                    //Console.WriteLine("Następne miasto: " + nextCity);
                    //Console.WriteLine(ProbabilityOfMovingToTheCity(ant.currentCity, nextCity));
                    //Console.WriteLine(sumOfEdgeWeightsFromCity);
                    roulettePercents[nextCity] = ProbabilityOfMovingToTheCity(ant.currentCity,nextCity) / sumOfEdgeWeightsFromCity;
                    //Console.WriteLine("Procenty: "+roulettePercents[nextCity]);
                }
                else // Mamy 0% na wylosowanie miasta, w którym aktualnie znajduje się mrówka lub już w nim była lub nie może do niego pójść
                {
                    roulettePercents[nextCity] = 0;
                    //Console.WriteLine("Procenty "+roulettePercents[nextCity]);
                }
            }

            for (int index = 1; index < demandsOfCities.Length; index++)
                roulettePercents[index] = roulettePercents[index] + roulettePercents[index - 1];

            return roulettePercents;
        }
        
        public int ChooseNextCityByRoulette(Ant ant)
        {
            //Console.WriteLine();
            double[] roulette = MakeRouletteArray(ant);
            double makeDraw = random.NextDouble();
            //Console.WriteLine("Draw: "+makeDraw);
            for(int i = 0; i < roulette.Length; i++)
            {
                //Console.WriteLine(roulette[i]);
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
                double min = double.MaxValue;
                
                int index = -1;

                for (int nextCity = 1; nextCity < demandsOfCities.Length; nextCity++)
                {
                    if (CanAntGoToTheCity(ant, nextCity) && min > distances[ant.currentCity,nextCity])
                    {
                        min = distances[ant.currentCity,nextCity];
                        index = nextCity;
                    }
                }
                return index;
            }
        }

        private double ProbabilityOfMovingToTheCity(int currentCity,int nextCity)
        {
            //Console.WriteLine("Feromony: "+pheromones[currentCity, nextCity]);
            //Console.WriteLine("Dystans: " + distances[currentCity, nextCity]); 
            double pom =  Math.Pow(pheromones[currentCity, nextCity], pheromonePriority) * Math.Pow(1 / distances[currentCity, nextCity], heuristicPriority);
            return pom;
        }

        private bool CanAntGoToTheCity(Ant ant, int nextCity)
        {
            return nextCity != ant.currentCity && !ant.visitedCities[nextCity] && ant.capacity - demandsOfCities[nextCity] >= 0;
        }

        private void InitAnts()
        {
            ants = new Ant[demandsOfCities.Length];
            for( int i = 1; i < ants.Length; i++)
            {
                int city = i % demandsOfCities.Length;
                /*
                do
                {
                    city = random.Next(1, demandsOfCities.Length);
                } while (capacity - demandsOfCities[city] < 0);
                */
                ants[i] = new Ant(capacity, city, demandsOfCities.Length);
                ants[i].capacity -= demandsOfCities[city];
                ants[i].Path.Push(warehouse);
                ants[i].Path.Push(city);
                ants[i].pathLength += distances[warehouse, city];
                //Console.WriteLine("Mrówka numer: "+i);
                //Console.WriteLine("Ładowność: " + capacity);
                //Console.WriteLine("Miasto startowe: " + (i+1));

            }
        }

        private void FindSolution(Ant ant, int id, int iteration)
        {
            
            int startCapacity = capacity;
            
            while (!ant.visitedCities.All(x => x)) // dopóki mrówka nie odwiedziła wszystkich miast
            {
                int nextCity = ChooseNextCityByRoulette(ant);
                //Console.WriteLine("Następne miasto: "+nextCity);
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
                    ant.usedTrucks++;
                }
                /*
                for(int i=0;i<ant.visitedCities.Length;i++)
                {
                    if (!ant.visitedCities[i])
                        Console.WriteLine("Mrówka nie odiwedziła jeszcze miasta: "+i);
                }
                Console.WriteLine();
                */
            }
            // dodaj jeszcze połączenie do magazynu
            ant.pathLength += distances[ant.currentCity, warehouse];
            ant.Path.Push(warehouse);
            //Console.WriteLine(ant.visitedCities.All(x => x));
            //Console.WriteLine(solution);
            if (ant.pathLength < bestSolution)
            {
                //Stack<int> stack = new(ant.Path);
                Console.WriteLine($"Mrówka {id} znalazła rozwiązanie: " + ant.pathLength + " w iteracji: "+iteration);
                bestPath = ant.Path.ToArray();
                /*
                while (stack.Count > 0)
                {
                    Console.Write(stack.Pop()+"<-");
                }
                Console.WriteLine();
                */
           }
            bestSolution = ant.pathLength < bestSolution ? ant.pathLength : bestSolution;
            
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

        private void UpdatePheromoneWithRank()
        {

            Array.Sort(ants, new AntComparer());

            for (int i = 1; i < numberOfRankAnts; i++)
            {
                //Console.WriteLine("Rozwiązanie mrówki: " + i);
                while (ants[i].Path.Count > 1)
                {
                    int secondCity = ants[i].Path.Pop();
                    int firstCity = ants[i].Path.Peek();
                    //Console.Write(secondCity + "<-" + firstCity+"<-");
                    double length = ants[i].pathLength;
                    
                    double increase = (numberOfRankAnts-i-1)*pheromoneIncreaseFactor / length;

                    pheromones[firstCity, secondCity] += increase;
                    pheromones[secondCity, firstCity] += increase;
                }
            }
        }

        private void UpdatePheromone()
        {
            for (int i = 1; i < ants.Length; i++)
            {
                //Console.WriteLine("Rozwiązanie mrówki: " + i);
                while (ants[i].Path.Count > 1)
                {
                    int secondCity = ants[i].Path.Pop();
                    int firstCity = ants[i].Path.Peek();
                    //Console.Write(secondCity + "<-" + firstCity+"<-");
                    double length = ants[i].pathLength;

                    double increase = pheromoneIncreaseFactor / length;

                    //increase = ants[i].usedTrucks > 5 ? increase/100 : increase;

                    pheromones[firstCity, secondCity] += increase;
                    pheromones[secondCity, firstCity] += increase;
                }
            }
        }

        private void UpdatePheromoneOnTheBestSolution()
        {
            for(int i = 0;i < bestPath.Length - 1;i++)
            {
                int secondCity = bestPath[i];
                int firstCity = bestPath[i + 1];

                double pom = pheromones[firstCity, secondCity] * numberOfElitistAnts;

                pheromones[firstCity, secondCity] = pom;
                pheromones[secondCity, firstCity] = pom;
            }
        }

        public void AntSystemSoultion()
        {
            for(int i = 0; i < maxNumberOfIterations; i++)
            {
                // stwórz mrówki
                InitAnts();
                
                // dla każdej mrówki
                for(int j = 1; j < ants.Length; j++)
                {
                    // znajdź rozwiązanie
                    FindSolution(ants[j],j,i);
                }
                // wyparuj feromon
                EvaporatePheromone();
                // dla każdej mrówki rozłóż feromon (z parowaniem)
                UpdatePheromoneWithRank();
                //elitaryzm
                //UpdatePheromoneOnTheBestSolution();

                

                //Console.WriteLine("Iteracja: "+i);

            }

            Console.WriteLine("Rozwiązanie: "+bestSolution);
            int routeNumber = 0;
            for(int i = 0;i < bestPath.Length;i++)
            {
                if (bestPath[i] == 0 && i!=bestPath.Length-1)
                {
                    Console.WriteLine();
                    Console.Write("Route #" + ++routeNumber);
                }
                else
                    Console.Write(" "+bestPath[i]);
            }
        }
        
    }
}
