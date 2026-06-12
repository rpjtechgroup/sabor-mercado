using SaborMercado.Web.Contracts.Recognition;
using SaborMercado.Web.Features.Recognition;

namespace SaborMercado.Web.Tests.Features;

public class RecognitionNormalizerTests
{
    [Fact]
    public void Normalize_ParsesBrazilianPriceFromRawText()
    {
        var raw = new RecognitionResultDto(
            "óleo de soja liza",
            "liza",
            900m,
            "ML",
            null,
            "7891234567890",
            0.91m,
            "ÓLEO DE SOJA LIZA 900ML R$ 8,99");

        var result = RecognitionNormalizer.Normalize(raw);

        Assert.Equal("Óleo De Soja Liza", result.ProductName);
        Assert.Equal("ml", result.QuantityUnit);
        Assert.Equal(8.99m, result.Price);
    }
}
