// See https://aka.ms/new-console-template for more information
using ManageSimFinRatings;

Console.WriteLine("Retrieving SimFin Values....");
ProcessHandler processHandler = new();
await processHandler.ExecuteAsync();