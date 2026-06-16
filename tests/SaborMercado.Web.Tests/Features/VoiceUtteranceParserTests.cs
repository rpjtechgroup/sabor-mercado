using SaborMercado.Web.Domain.Catalog;
using SaborMercado.Web.Features.Voice;

namespace SaborMercado.Web.Tests.Features;

public class VoiceUtteranceParserTests
{
    [Fact]
    public void Parse_OleoComPrecoPorExtenso_ExtraiNomeEValor()
    {
        var parsed = VoiceUtteranceParser.Parse("óleo de soja Liza oito e noventa");

        Assert.Contains("óleo", parsed.Name, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(8.90m, parsed.UnitPrice);
    }

    [Fact]
    public void Parse_ArrozComPesoEPreco_ExtraiMedidaEValor()
    {
        var parsed = VoiceUtteranceParser.Parse("arroz dois quilos doze reais");

        Assert.Contains("arroz", parsed.Name, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(2m, parsed.QuantityValue);
        Assert.Equal(QuantityUnit.Kg, parsed.QuantityUnit);
        Assert.Equal(12m, parsed.UnitPrice);
    }

    [Fact]
    public void Parse_LeiteComUnidades_ExtraiQuantidade()
    {
        var parsed = VoiceUtteranceParser.Parse("leite três unidades");

        Assert.Contains("leite", parsed.Name, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(3, parsed.Quantity);
    }

    [Fact]
    public void Parse_PrecoComMoeda_ExtraiValorNumerico()
    {
        var parsed = VoiceUtteranceParser.Parse("café R$ 8,99");

        Assert.Contains("café", parsed.Name, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(8.99m, parsed.UnitPrice);
    }

    [Fact]
    public void Parse_TextoVazio_RetornaCamposVazios()
    {
        var parsed = VoiceUtteranceParser.Parse("   ");

        Assert.Equal(string.Empty, parsed.Name);
        Assert.Null(parsed.UnitPrice);
        Assert.Null(parsed.Quantity);
    }
}
