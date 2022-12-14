// See https://aka.ms/new-console-template for more information
using EarningsCalendar.Processing;

Console.WriteLine("Starting Earnings Calendar maintainer");
Function function = new();
await function.ExecuteAsync();