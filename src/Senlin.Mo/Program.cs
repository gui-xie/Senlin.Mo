using Senlin.Mo;
using Senlin.Mo.Localization.Abstractions;

Console.WriteLine("Hello, Senlin.Mo!");
var resolve = new LocalizationResolver("en", L.Directory).Resolve;
var t = L.AgeIs("3").Resolve(resolve);
Console.WriteLine(t);