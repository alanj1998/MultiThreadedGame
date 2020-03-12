using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace ThreadGame
{
    class Program
    {
        static Simulator s = new Simulator();
        static List<int> res = new List<int>();
        static Thread t1 = new Thread(new ThreadStart(() => res.Add(s.RunSimulationForTeam(1))));
        static Thread t2 = new Thread(new ThreadStart(() => res.Add(s.RunSimulationForTeam(2))));
        static void Main(string[] args)
        {
            t1.Start();
            t2.Start();

            t1.Join();
            t2.Join();

            int winningIndex = -1;
            int winningScore = -1;

            for (int i = 0; i < res.Count; i++)
            {
                Console.WriteLine($"Total for team{i + 1}: {res[i]}");

                if (res[i] > winningScore)
                {
                    winningScore = res[i];
                    winningIndex = i;
                }
            }
            Console.WriteLine($"Team {winningIndex + 1} wins!");
            Console.ReadLine();
        }

    }
    class Simulator
    {
        const int PLAYER_COUNT = 2;
        private FileAccess _fileAccess = new FileAccess();

        public int RunSimulationForTeam(int teamNo)
        {
            int total = 0;
            for (int i = 0; i < PLAYER_COUNT; i++)
            {
                lock (this)
                {
                    List<int> shots = SimulateShots(teamNo);
                    int maxVal = shots.Max();
                    total += maxVal;
                    Console.WriteLine($"Best Score: {maxVal}\n");

                    Monitor.Enter(_fileAccess);
                    string s = $"Team_{teamNo} - Team Member {i + 1}\n{maxVal}";
                    _fileAccess.WriteToFile(s);
                    Monitor.Exit(_fileAccess);

                    Monitor.Pulse(this);

                    if(i < PLAYER_COUNT - 1)
                    {
                        Monitor.Wait(this);
                    }
                }  
            }
            
            return total;
        }

        List<int> SimulateShots(int teamNo)
        {
            List<int> l = new List<int>();

            for (int i = 0; i < 3; i++)
            {
                int shot = SimulateShotAmount();
                Console.WriteLine($"Team_{teamNo} - Shot {i + 1} - Number of Points = {shot}");
                l.Add(shot);
            }

            return l;
        }

        int SimulateShotAmount()
        {
            Random r = new Random();
            return r.Next(0, 101);
        }
    }

    class FileAccess
    {
        public void WriteToFile(string s)
        {
            using (StreamWriter sw = new StreamWriter("../../../Results.txt", true))
            {
                sw.WriteLine(s);
            }
        }
    }
}
