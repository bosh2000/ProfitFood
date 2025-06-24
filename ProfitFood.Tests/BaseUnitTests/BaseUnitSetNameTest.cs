using ProfitFood.Model.DBModel;

namespace ProfitFood.Tests.BaseUnitTests;

public class BaseUnitSetNameTest
{
    private BaseUnit _baseUnit;

    [SetUp]
    public void Setup()
    {
        _baseUnit = BaseUnit.Create("Метр").Value;
    }

    [Test]
    public void SetNameWithValidName_UpdateNameReturnSuccess()
    {
        string newName = "Квадратный метр";
        var result= _baseUnit.SetName(newName);
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(newName,_baseUnit.Name);
    }
    [Test]
    public void SetNameWithEmptyName_ReturnError()
    {
        string originalName=_baseUnit.Name;
        string emptyName = "";
        var result=_baseUnit.SetName(emptyName);
        Assert.IsFalse(result.IsSuccess);
        Assert.AreEqual(1,result.Errors.Count);
        Assert.AreEqual(originalName,_baseUnit.Name);
    }
}
