// See https://aka.ms/new-console-template for more information
using EarningsReport;

Console.WriteLine("Retrieving Earnings report....");
Function function = new Function();
await function.ExecuteAsync();