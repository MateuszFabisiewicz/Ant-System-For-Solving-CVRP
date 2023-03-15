using System;

namespace TestAntSystem
{
	public struct Point
	{
		public int x;
		public int y;

		public Point(int x, int y)
		{
			this.x = x;
			this.y = y;
		}
	}

	public class CVRPFIlesReader
	{
		private int capacity;
		private double[,] distances;
		private int[] citiesWithDemands;
		private string name;
		private double optimalValue;
		private int numberOfTrucks;
		private int dimension;

		public CVRPFIlesReader(string path)
		{
			string[] allText = File.ReadAllLines(path);

			name = allText[0].Split(':')[1];
			dimension = int.Parse(allText[3].Split(':')[1]);
            capacity = int.Parse(allText[5].Split(':')[1]);

			distances = new double[dimension, dimension];
			citiesWithDemands = new int[dimension];
			List<Point> points = new();

			int i = 7;
			while (allText[i] != "DEMAND_SECTION")
			{
				string[] point = allText[i++].Split(" ");
				points.Add(new(int.Parse(point[1]),int.Parse(point[2])));
			}

			i++;
			while (allText[i] != "DEPOT_SECTION")
			{
				string[] demand = allText[i++].Split(" ");
				citiesWithDemands[int.Parse(demand[0]) - 1] = int.Parse(demand[1]);
            }

            for (i = 0; i < dimension; i++)
            {
				Point pointA = points[i];
                for (int j = 0; j < dimension; j++)
                {
					Point pointB = points[j];
                    distances[i, j] = Math.Sqrt(Math.Pow(pointA.x - pointB.x, 2) + Math.Pow(pointA.y - pointB.y, 2));
                }
            }
        }

		public int GetCapacity()
		{
			return capacity;
		}

		public double[,] GetDistances()
		{
			return distances;
		}

		public int[] GetDemands()
		{
			return citiesWithDemands;
		}
	}
}

