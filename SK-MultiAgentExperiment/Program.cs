namespace SK_MultiAgentExperiment
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Hello, World!");

            await new SoftwareDevTeam().Execute();
        }
    }
}
