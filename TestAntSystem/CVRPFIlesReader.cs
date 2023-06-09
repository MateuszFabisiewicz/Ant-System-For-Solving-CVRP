﻿using System;
using System.Diagnostics;

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
		private int[,] distances;
		private int[] citiesWithDemands;
		private string name;
		private int optimalValue;
		private int numberOfTrucks;
		private int dimension;

		public CVRPFIlesReader(string path)
		{
			string[] allText = File.ReadAllLines(path);

			name = allText[0].Split(':')[1];
			Console.WriteLine("Wczytano plik: "+name);

			string[] commentSection = allText[1].Split(":");

            numberOfTrucks = int.Parse(commentSection[2].Split(',')[0]);
			Console.WriteLine("Liczba ciężarówek: " + numberOfTrucks);
			string optValSection = commentSection[3];
            optimalValue = int.Parse(optValSection.Remove(optValSection.Length - 1));
            Console.WriteLine("Optymalna wartość: " + optimalValue);
            dimension = int.Parse(allText[3].Split(':')[1]);
            capacity = int.Parse(allText[5].Split(':')[1]);

			distances = new int[dimension, dimension];
			citiesWithDemands = new int[dimension];
			List<Point> points = new();

			int i = 7;
			int pom = i + dimension;
			while (i < pom)//allText[i] != "DEMAND_SECTION")
			{
				string[] point = allText[i++].Split(" ");
				//Console.WriteLine(point[1] + point[2]);
				points.Add(new(int.Parse(point[^2]),int.Parse(point[^1])));
			}

			i++;
			pom = i + dimension;
			while (i < pom)//allText[i] != "DEPOT_SECTION")
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
                    distances[i, j] = (int)Math.Round(Math.Sqrt(Math.Pow(pointA.x - pointB.x, 2) + Math.Pow(pointA.y - pointB.y, 2)));
                }
            }
        }

		public int GetCapacity()
		{
			return capacity;
		}

		public int[,] GetDistances()
		{
			return distances;
		}

		public int[] GetDemands()
		{
			return citiesWithDemands;
		}

		public int GetOptimalValue()
		{
			return optimalValue;
		}

		public string GetName()
		{ 
			return name; 
		}

		public int GetNumberOfTrucks()
		{ 
			return numberOfTrucks; 
		}
	}
}

