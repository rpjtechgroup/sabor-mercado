using SaborMercado.Web.Domain.Catalog;
using SaborMercado.Web.Features.Voice;

namespace SaborMercado.Web.Tests.Features;

public class VoiceModelOutputParserTests
{
    [Fact]
    public void Parse_InvalidModelOutput_FallsBackToDeterministicParser()
    {
        var result = VoiceModelOutputParser.Parse("óleo de soja oito e noventa", "lixo sem json");

        Assert.Equal(VoiceExtractionSource.DeterministicFallback, result.Source);
        Assert.Equal("Óleo de soja", result.Fields.Name);
        Assert.Equal(8.90m, result.Fields.UnitPrice);
    }

    [Fact]
    public void Parse_ValidModelJson_MergesWithDeterministicRules()
    {
        const string modelJson = """
            {"name":"Óleo de Soja","brand":"Liza","unitPrice":8.99,"quantity":2,"quantityValue":900,"quantityUnit":"ml"}
            """;

        var result = VoiceModelOutputParser.Parse("óleo", modelJson);

        Assert.Equal(VoiceExtractionSource.LocalModel, result.Source);
        Assert.Equal("Óleo de Soja", result.Fields.Name);
        Assert.Equal("Liza", result.Fields.Brand);
        Assert.Equal(8.99m, result.Fields.UnitPrice);
        Assert.Equal(2, result.Fields.Quantity);
        Assert.Equal(900m, result.Fields.QuantityValue);
        Assert.Equal(QuantityUnit.Ml, result.Fields.QuantityUnit);
    }

    [Fact]
    public void Parse_ModelJsonMissingPrice_UsesDeterministicPrice()
    {
        const string modelJson = """{"name":"Arroz","brand":null,"unitPrice":null,"quantity":null,"quantityValue":null,"quantityUnit":null}""";

        var result = VoiceModelOutputParser.Parse("arroz doze reais", modelJson);

        Assert.Equal(VoiceExtractionSource.LocalModel, result.Source);
        Assert.Equal("Arroz", result.Fields.Name);
        Assert.Equal(12m, result.Fields.UnitPrice);
    }
}
