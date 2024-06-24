using Senlin.Mo.Excel.Abstractions;

namespace Senlin.Mo.Excel.Test;

public class Class1
{
    [Fact]
    public void Test()
    {
        var a = new TestExcel();
        
        a.A();
        
        Assert.Equal(1, 1);
    }
}