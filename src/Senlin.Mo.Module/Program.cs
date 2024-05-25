Console.WriteLine("Hello, World!");

public interface IModule
{
    string Name { get; }
    
    IEnumerable<(Type Abstraction, Type Implementation)> GetRepositories();
    
    Type DbContextType { get; }
}