namespace SaborMercado.Web.Features.Voice;

internal static class VoiceUtterancePrompt
{
    public static string Build(VoiceExtractionTarget target, string transcript) =>
        BuildCartItemPrompt(transcript);

    public static object BuildSchema(VoiceExtractionTarget target) => CartItemSchema;

    private static string BuildCartItemPrompt(string transcript) =>
        """
        Você extrai campos para ADICIONAR ITEM AO CARRINHO de compras no Brasil.
        A entrada é fala ou texto livre em português do Brasil. Responda somente com JSON válido no schema.

        Campos do formulário (use null quando o usuário não mencionar ou não houver certeza):
        - name: nome do produto, preferencialmente sem marca.
        - brand: marca, se mencionada.
        - quantityValue: peso/volume da embalagem (número).
        - quantityUnit: exatamente uma de "g", "kg", "ml", "l", "un".
        - unitPrice: preço unitário em reais (número decimal com ponto). Ex.: "nove e noventa" → 9.90, "oito reais e cinquenta" → 8.50.
        - quantity: quantidade de unidades/pacotes comprados (inteiro ≥ 1). Ex.: "dois pacotes" → 2. Padrão 1 se não mencionado mas houver produto.

        Regras:
        1. Preço falado em reais brasileiros; ignore "reais" e "centavos" ao converter.
        2. "três vezes" ou "3 unidades" → quantity 3.
        3. Não confunda peso da embalagem (quantityValue) com quantidade comprada (quantity).
        4. Não invente dados. Prefira null a chute.

        Exemplos:
        Entrada: "óleo de soja Liza novecentos ml nove e noventa dois pacotes"
        Saída: {"name":"Óleo de Soja","brand":"Liza","quantityValue":900,"quantityUnit":"ml","unitPrice":9.90,"quantity":2}

        Entrada: "leite integral três e vinte"
        Saída: {"name":"Leite Integral","brand":null,"quantityValue":null,"quantityUnit":null,"unitPrice":3.20,"quantity":1}

        Texto do usuário:
        """ + transcript.Trim();

    private static readonly object CartItemSchema = new
    {
        type = "object",
        properties = new
        {
            name = new { type = "string" },
            brand = new { type = "string", nullable = true },
            quantityValue = new { type = "number", nullable = true },
            quantityUnit = new { type = "string", nullable = true },
            unitPrice = new { type = "number", nullable = true },
            quantity = new { type = "number", nullable = true },
        },
        required = new[] { "name" },
    };
}
