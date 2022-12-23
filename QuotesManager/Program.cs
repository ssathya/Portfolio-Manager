// See https://aka.ms/new-console-template for more information
using QuotesManager;

Console.WriteLine("Starting to update pricing");
FunctionHandler functionHandler = new FunctionHandler();
await functionHandler.ExecAsync();
Console.WriteLine("Application Ended");