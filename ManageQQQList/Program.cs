// See https://aka.ms/new-console-template for more information
using ManageQQQList;

Console.WriteLine("Starting Manage NASDAQ-100 securities");
Function function = new Function();
await function.FunctionHandler();
Console.WriteLine("Application ended");