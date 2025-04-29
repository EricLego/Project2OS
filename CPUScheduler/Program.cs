using System;
using System.Collections.Generic;
using System.Linq;

namespace CPUScheduler
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Select Test Case:");
            Console.WriteLine("1. Small Test Case (4 processes)");
            Console.WriteLine("2. Large Random Test Case");
            Console.Write("Choice: ");
            int testChoice = int.Parse(Console.ReadLine());

            List<Process> processes;
            
            if (testChoice == 1)
            {
                // Small test case (4 processes)
                processes = new List<Process>
                {
                    new Process { ID = 1, ArrivalTime = 0, BurstTime = 8, RemainingTime = 8 },
                    new Process { ID = 2, ArrivalTime = 1, BurstTime = 4, RemainingTime = 4 },
                    new Process { ID = 3, ArrivalTime = 2, BurstTime = 9, RemainingTime = 9 },
                    new Process { ID = 4, ArrivalTime = 3, BurstTime = 5, RemainingTime = 5 }
                };
            }
            else
            {
                // Large random test case
                Console.Write("Enter number of processes (10-50): ");
                int numProcesses = int.Parse(Console.ReadLine());
                numProcesses = Math.Max(10, Math.Min(50, numProcesses)); // Ensure between 10-50
                
                Random random = new Random();
                processes = new List<Process>();
                
                for (int i = 1; i <= numProcesses; i++)
                {
                    Process p = new Process
                    {
                        ID = i,
                        ArrivalTime = random.Next(0, 20), // Random arrival time between 0-19
                        BurstTime = random.Next(1, 15)    // Random burst time between 1-14
                    };
                    p.RemainingTime = p.BurstTime;
                    processes.Add(p);
                }
            }

            Console.WriteLine("\nSelect Scheduling Algorithm:");
            Console.WriteLine("1. First Come First Serve (FCFS)");
            Console.WriteLine("2. Shortest Remaining Time First (SRTF)");
            Console.WriteLine("3. Highest Response Ratio Next (HRRN)");
            Console.Write("Choice: ");
            int algorithmChoice = int.Parse(Console.ReadLine());

            switch (algorithmChoice)
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

        static void SRTF(List<Process> processes)
        {
            Console.WriteLine("\nRunning SRTF Scheduling...\n");

            // Create a deep copy of processes to avoid modifying the original list
            List<Process> processCopy = processes.Select(p => new Process
            {
                ID = p.ID,
                ArrivalTime = p.ArrivalTime,
                BurstTime = p.BurstTime,
                RemainingTime = p.BurstTime,
                Priority = p.Priority
            }).ToList();

            int currentTime = 0;
            int completedProcesses = 0;
            int totalProcesses = processCopy.Count;
            bool isProcessRunning = false;
            
            // To track execution sequence for possible Gantt chart
            List<KeyValuePair<int, int>> executionSequence = new List<KeyValuePair<int, int>>();
            Process currentProcess = null;

            // Continue until all processes are completed
            while (completedProcesses < totalProcesses)
            {
                // Find the process with the shortest remaining time among arrived processes
                Process shortestProcess = null;
                int shortestRemainingTime = int.MaxValue;

                foreach (var p in processCopy.Where(p => p.ArrivalTime <= currentTime && p.RemainingTime > 0))
                {
                    if (p.RemainingTime < shortestRemainingTime)
                    {
                        shortestRemainingTime = p.RemainingTime;
                        shortestProcess = p;
                    }
                }

                // If no process is available at this time, increment time
                if (shortestProcess == null)
                {
                    currentTime++;
                    continue;
                }

                // If this is the first time the process is being executed, set its start time
                if (shortestProcess.StartTime == -1)
                {
                    shortestProcess.StartTime = currentTime;
                }

                // If a different process is now selected, add the previous execution to the sequence
                if (currentProcess != shortestProcess && isProcessRunning)
                {
                    executionSequence.Add(new KeyValuePair<int, int>(currentProcess.ID, currentTime));
                }

                // Execute the process for one time unit
                shortestProcess.RemainingTime--;
                currentTime++;
                currentProcess = shortestProcess;
                isProcessRunning = true;

                // If the process completes execution
                if (shortestProcess.RemainingTime == 0)
                {
                    completedProcesses++;
                    isProcessRunning = false;
                    
                    // Add the completed process to execution sequence
                    executionSequence.Add(new KeyValuePair<int, int>(shortestProcess.ID, currentTime));
                    
                    // Set completion time and calculate turnaround and waiting times
                    shortestProcess.CompletionTime = currentTime;
                    shortestProcess.TurnaroundTime = shortestProcess.CompletionTime - shortestProcess.ArrivalTime;
                    shortestProcess.WaitingTime = shortestProcess.TurnaroundTime - shortestProcess.BurstTime;
                }
            }

            // Print Gantt Chart
            PrintGanttChart(executionSequence);
            
            // Print detailed results
            PrintResults(processCopy);
        }

        static void HRRN(List<Process> processes)
        {
            Console.WriteLine("\nRunning HRRN Scheduling...\n");

            // Create a deep copy of processes to avoid modifying the original list
            List<Process> processCopy = processes.Select(p => new Process
            {
                ID = p.ID,
                ArrivalTime = p.ArrivalTime,
                BurstTime = p.BurstTime,
                RemainingTime = p.BurstTime,
                Priority = p.Priority
            }).ToList();

            int currentTime = 0;
            int completedProcesses = 0;
            int totalProcesses = processCopy.Count;
            
            // To track execution sequence for Gantt chart
            List<KeyValuePair<int, int>> executionSequence = new List<KeyValuePair<int, int>>();

            // Continue until all processes are completed
            while (completedProcesses < totalProcesses)
            {
                Process selectedProcess = null;
                double highestResponseRatio = -1;

                // Find the process with highest response ratio among arrived processes
                foreach (var p in processCopy.Where(p => p.ArrivalTime <= currentTime && p.RemainingTime > 0))
                {
                    // Calculate response ratio: (Wait Time + Burst Time) / Burst Time
                    double waitingTime = currentTime - p.ArrivalTime;
                    double responseRatio = (waitingTime + p.BurstTime) / p.BurstTime;
                    
                    if (responseRatio > highestResponseRatio)
                    {
                        highestResponseRatio = responseRatio;
                        selectedProcess = p;
                    }
                }

                // If no process is available at this time, increment time
                if (selectedProcess == null)
                {
                    currentTime++;
                    continue;
                }

                // If this is the first time this process is being executed, set its start time
                if (selectedProcess.StartTime == -1)
                {
                    selectedProcess.StartTime = currentTime;
                }

                // Add start of process execution to sequence
                executionSequence.Add(new KeyValuePair<int, int>(selectedProcess.ID, currentTime));
                
                // Execute the process for its full burst time (HRRN is non-preemptive)
                currentTime += selectedProcess.BurstTime;
                selectedProcess.RemainingTime = 0;
                completedProcesses++;
                
                // Set completion time and calculate turnaround and waiting times
                selectedProcess.CompletionTime = currentTime;
                selectedProcess.TurnaroundTime = selectedProcess.CompletionTime - selectedProcess.ArrivalTime;
                selectedProcess.WaitingTime = selectedProcess.TurnaroundTime - selectedProcess.BurstTime;
            }

            // Don't duplicate the last process in the sequence
            // Just use the sequence as is
            
            // Print Gantt Chart
            PrintGanttChart(executionSequence);
            
            // Print detailed results
            PrintResults(processCopy);
        }
        
        static void PrintGanttChart(List<KeyValuePair<int, int>> executionSequence)
        {
            if (executionSequence.Count == 0)
                return;
                
            Console.WriteLine("\nGantt Chart:");
            
            // Print top border
            Console.Write("+");
            int prevTime = 0;
            foreach (var execution in executionSequence)
            {
                int duration = execution.Value - prevTime;
                if (duration > 0) // Skip zero or negative durations
                {
                    Console.Write(new string('-', duration * 2 + 1) + "+");
                    prevTime = execution.Value;
                }
            }
            
            // Print process IDs
            Console.Write("\n|");
            prevTime = 0;
            foreach (var execution in executionSequence)
            {
                int duration = execution.Value - prevTime;
                if (duration > 0) // Skip zero or negative durations
                {
                    // Make sure we don't create negative length strings
                    int spaces = Math.Max(0, (duration - 1) * 2);
                    Console.Write($" P{execution.Key} " + new string(' ', spaces) + "|");
                    prevTime = execution.Value;
                }
            }
            
            // Print bottom border
            Console.Write("\n+");
            prevTime = 0;
            foreach (var execution in executionSequence)
            {
                int duration = execution.Value - prevTime;
                if (duration > 0) // Skip zero or negative durations
                {
                    Console.Write(new string('-', duration * 2 + 1) + "+");
                    prevTime = execution.Value;
                }
            }
            
            // Print time markers
            Console.Write("\n0");
            prevTime = 0;
            foreach (var execution in executionSequence)
            {
                int duration = execution.Value - prevTime;
                if (duration > 0) // Skip zero or negative durations
                {
                    Console.Write(new string(' ', duration * 2) + $"{execution.Value}");
                    prevTime = execution.Value;
                }
            }
            
            Console.WriteLine();
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

            int totalBurstTime = processes.Sum(p => p.BurstTime);
            int totalTime = processes.Max(p => p.CompletionTime);
            double cpuUtilization = (double)totalBurstTime / totalTime * 100;
            double throughput = (double)processes.Count / totalTime;

            Console.WriteLine($"\nAverage Waiting Time: {avgWaiting:F2}");
            Console.WriteLine($"Average Turnaround Time: {avgTurnaround:F2}");
            Console.WriteLine($"CPU Utilization: {cpuUtilization:F2}%");
            Console.WriteLine($"Throughput: {throughput:F2} processes/unit time");
        }
    }
}