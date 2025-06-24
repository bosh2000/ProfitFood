using ProfitFood.Model.DBModel;

namespace ProfitFood.Tests.BaseUnitTests;

public class BaseUnitProductsTests
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void ProductCreateEmptyCollection()
    {
        var baseUnit = BaseUnit.Create("Test").Value;
        Assert.IsNotNull(baseUnit.Products);
        Assert.IsEmpty(baseUnit.Products);
    }
}
