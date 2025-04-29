using System;
using System.Collections.Generic;
using System.Linq;

namespace CPUScheduler
{
    class Program
    {
        static void Main(string[] args)
        {
            // Step 1: Create a list of processes (hardcoded for now)
            List<Process> processes = new List<Process>
            {
                new Process { ID = 1, ArrivalTime = 0, BurstTime = 8, RemainingTime = 8 },
                new Process { ID = 2, ArrivalTime = 1, BurstTime = 4, RemainingTime = 4 },
                new Process { ID = 3, ArrivalTime = 2, BurstTime = 9, RemainingTime = 9 },
                new Process { ID = 4, ArrivalTime = 3, BurstTime = 5, RemainingTime = 5 }
            };

            Console.WriteLine("Select Scheduling Algorithm:");
            Console.WriteLine("1. First Come First Serve (FCFS)");
            Console.WriteLine("2. Shortest Remaining Time First (SRTF)");
            Console.WriteLine("3. Highest Response Ratio Next (HRRN)");
            Console.Write("Choice: ");
            int choice = int.Parse(Console.ReadLine());

            switch (choice)
            {
                case 1:
                    FCFS(processes);
                    break;
                case 2:
                    SRTF(processes);
                    break;
                case 3:
                    HRRN(processes);
                    break;
                default:
                    Console.WriteLine("Invalid Choice.");
                    break;
            }
        }

        static void FCFS(List<Process> processes)
        {
            Console.WriteLine("\nRunning FCFS Scheduling...\n");

            var sorted = processes.OrderBy(p => p.ArrivalTime).ToList();
            int currentTime = 0;
            foreach (var p in sorted)
            {
                if (currentTime < p.ArrivalTime)
                    currentTime = p.ArrivalTime;

                p.StartTime = currentTime;
                p.CompletionTime = p.StartTime + p.BurstTime;
                p.TurnaroundTime = p.CompletionTime - p.ArrivalTime;
                p.WaitingTime = p.TurnaroundTime - p.BurstTime;
                currentTime += p.BurstTime;
            }

            PrintResults(sorted);
        }

        static void PrintResults(List<Process> processes)
        {
            Console.WriteLine("\nProcess\tArrival\tBurst\tStart\tCompletion\tTurnaround\tWaiting");

            foreach (var p in processes)
            {
                Console.WriteLine($"{p.ID}\t{p.ArrivalTime}\t{p.BurstTime}\t{p.StartTime}\t{p.CompletionTime}\t\t{p.TurnaroundTime}\t\t{p.WaitingTime}");
            }

            double avgWaiting = processes.Average(p => p.WaitingTime);
            double avgTurnaround = processes.Average(p => p.TurnaroundTime);

            Console.WriteLine($"\nAverage Waiting Time: {avgWaiting:F2}");
            Console.WriteLine($"Average Turnaround Time: {avgTurnaround:F2}");
        }
    }
}