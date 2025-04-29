public class Process
{
    public int ID { get; set; }
    public int ArrivalTime { get; set; }
    public int BurstTime { get; set; }
    public int RemainingTime { get; set; }
    public int Priority { get; set; }
    public int StartTime { get; set; } = -1;
    public int CompletionTime { get; set; }
    public int WaitingTime { get; set; }
    public int TurnaroundTime { get; set; }
}