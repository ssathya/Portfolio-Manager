// See https://aka.ms/new-console-template for more information
using ManageDowList.Processing;

Console.WriteLine("Starting Manage Dow-30 securities");
Function function = new();
await function.ExecuteAsync();
Console.WriteLine("Application ended");