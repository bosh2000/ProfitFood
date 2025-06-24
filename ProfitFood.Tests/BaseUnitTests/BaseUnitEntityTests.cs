using ProfitFood.Model.DBModel;

namespace ProfitFood.Tests.BaseUnitTests;

public class BaseUnitEntityTests
{
    [SetUp]
    public void Setup()
    {
    }

    /// <summary>
    /// Успешное создание BaseUnit с валидным именем.
    /// </summary>
    [Test]
    public void CreateWithValidNameReturnSuccess()
    {
        string validName = "Килограм";
        var result = BaseUnit.Create(validName);
        Assert.IsTrue(result.IsSuccess);
        Assert.IsEmpty(result.Errors);
        Assert.AreEqual(validName, result.Value.Name);
    }

    [Test]
    public void CreateWithEmptyNamewReturnValidationError()
    {
        string emptyName = "";
        var result = BaseUnit.Create(emptyName);
        Assert.IsFalse(result.IsSuccess);
        Assert.AreEqual(1, result.Errors.Count);
        StringAssert.Contains("Наименование обязательно", result.Errors.First().Message);
    }
}