

using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Agents.Chat;
using Microsoft.SemanticKernel.ChatCompletion;

namespace SK_MultiAgentExperiment;

public class SoftwareDevTeam
{
    public async Task Execute()
    {
        Kernel kernel = Kernel.CreateBuilder()
           .AddOpenAIChatCompletion(
            "gpt-4o",   //"gpt-3.5-turbo",
            Environment.GetEnvironmentVariable("OPENAI_API_KEY")!,
            "")
           .Build();

        //1: Create 3 Agents

        string programManager = """
            You are a program manager which will take the requirement and 
            create a plan for creating app. Program Manager understands the 
            user requirements and form the detail documents with requirements and costing. 
            """;

        string softwareEngineer = """
            You are Software Engieer, and your goal is develop web app using HTML and JavaScript (JS) by taking into consideration all
            the requirements given by Program Manager. Produce the final Html code.
            """;

        string manager = """
            You are manager which will review software engineer code, and make sure all client requirements are completed.
            Once all client requirements are completed, you can approve the request by just responding "approve"
            """;

        #pragma warning disable SKEXP0110 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
        
        ChatCompletionAgent programManagerAgent = new()
        {
            Instructions = programManager,
            Name = "ProgramManagerAgent",
            Kernel = kernel
        };

        ChatCompletionAgent softwareEnginerAgent = new()
        {
            Instructions= softwareEngineer,
            Name = "SoftwareEngineerAgent",
            Kernel = kernel
        };

        ChatCompletionAgent projectManagerAgent = new()
        {
            Instructions = manager,
            Name = "ProjectManagerAgent",
            Kernel = kernel
        };

        //2: Create Termination Strategy
        //(ApprovalTerminationStrategy class created below)
        //What it does: it terminates the agent operation when Project Manager approves the final output.


        //3: Create Agent Chat Group for allowing these agents to interact with each other


        AgentGroupChat chat = new(programManagerAgent, softwareEnginerAgent, projectManagerAgent)
        {
            ExecutionSettings = new()
            {
                TerminationStrategy = new ApprovalTerminationStrategy()
                {
                    Agents = [projectManagerAgent],
                    MaximumIterations = 99,
                },
            }
        };


        //4: Let's see it in action :)

        string input = """
            I want to develop a calculator app.
            keep it very simple. Get final approval from manager.
            """;

        chat.AddChatMessage(new ChatMessageContent(AuthorRole.User, input));

        await foreach (var content in chat.InvokeAsync())
        {
            #pragma warning disable SKEXP0001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
            
            Console.WriteLine($"# {content.Role} - {content.AuthorName ?? "*"}: '{content.Content}'");
            
            #pragma warning restore SKEXP0001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
        }

        
        #pragma warning restore SKEXP0110 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

    }

    
}

#pragma warning disable SKEXP0110 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
public sealed class ApprovalTerminationStrategy : TerminationStrategy
{
    protected override Task<bool> ShouldAgentTerminateAsync(Agent agent, IReadOnlyList<ChatMessageContent> history, CancellationToken cancellationToken)
        => Task.FromResult(history[history.Count - 1].Content?.Contains("approve", StringComparison.OrdinalIgnoreCase) ?? false);
}
#pragma warning restore SKEXP0110 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
