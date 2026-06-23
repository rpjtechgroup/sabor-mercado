namespace SaborMercado.Web.Features.Voice;

internal static class VoiceUtterancePrompt
{
    public static string Build(VoiceExtractionTarget target, string transcript) =>
        target switch
        {
            VoiceExtractionTarget.CartItem => BuildCartItemPrompt(transcript),
            _ => BuildProductCatalogPrompt(transcript),
        };

    public static object BuildSchema(VoiceExtractionTarget target) =>
        target switch
        {
            VoiceExtractionTarget.CartItem => CartItemSchema,
            _ => ProductCatalogSchema,
        };

    private static string BuildProductCatalogPrompt(string transcript) =>
        """
        Você extrai campos para o formulário de CADASTRO DE PRODUTO de supermercado no Brasil.
        A entrada é fala ou texto livre em português do Brasil. Responda somente com JSON válido no schema.

        Campos do formulário (use null quando o usuário não mencionar ou não houver certeza):
        - name (obrigatório quando identificável): nome genérico do produto, sem marca. Ex.: "Óleo de Soja", "Arroz Branco".
        - brand: marca ou fabricante. Ex.: "Liza", "Tio João".
        - quantityValue: número do peso ou volume do pacote/embalagem. Ex.: 900 para "novecentos", 1 para "um litro".
        - quantityUnit: exatamente uma de "g", "kg", "ml", "l", "un".
        - ean: código de barras EAN/GTIN somente dígitos (8 a 14), sem espaços.
        - category: categoria de mercado. Ex.: "Mercearia", "Bebidas", "Limpeza".
        - notes: observações livres que não caibam nos outros campos.

        Regras:
        1. Separe nome e marca quando possível ("óleo Liza" → name "Óleo", brand "Liza" ou name "Óleo de Soja" se o tipo estiver explícito).
        2. Peso/volume da embalagem: "900 ml", "novecentos mililitros", "meio quilo" → quantityValue 500, quantityUnit "g" OU 0.5/"kg" conforme o mais natural.
        3. Não inclua preço, quantidade de unidades compradas nem nome de loja neste modo.
        4. Não invente dados. Prefira null a chute.
        5. Capitalize nomes de produto e marcas de forma natural em português.

        Exemplos:
        Entrada: "óleo de soja Liza novecentos ml mercearia"
        Saída: {"name":"Óleo de Soja","brand":"Liza","quantityValue":900,"quantityUnit":"ml","category":"Mercearia","ean":null,"notes":null}

        Entrada: "arroz branco tio joão pacote de 5 quilos"
        Saída: {"name":"Arroz Branco","brand":"Tio João","quantityValue":5,"quantityUnit":"kg","category":null,"ean":null,"notes":null}

        Entrada: "detergente ypê limão unidade"
        Saída: {"name":"Detergente","brand":"Ypê","quantityValue":1,"quantityUnit":"un","category":"Limpeza","ean":null,"notes":"limão"}

        Texto do usuário:
        """ + transcript.Trim();

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

    private static readonly object ProductCatalogSchema = new
    {
        type = "object",
        properties = new
        {
            name = new { type = "string", description = "Nome genérico do produto, sem marca." },
            brand = new { type = "string", nullable = true, description = "Marca ou fabricante." },
            quantityValue = new { type = "number", nullable = true, description = "Peso ou volume numérico da embalagem." },
            quantityUnit = new
            {
                type = "string",
                nullable = true,
                @enum = new[] { "g", "kg", "ml", "l", "un" },
                description = "Unidade de peso/volume.",
            },
            ean = new { type = "string", nullable = true, description = "Código EAN/GTIN somente dígitos." },
            category = new { type = "string", nullable = true, description = "Categoria de mercado." },
            notes = new { type = "string", nullable = true, description = "Observações adicionais." },
        },
    };

    private static readonly object CartItemSchema = new
    {
        type = "object",
        properties = new
        {
            name = new { type = "string", description = "Nome do produto." },
            brand = new { type = "string", nullable = true },
            quantityValue = new { type = "number", nullable = true },
            quantityUnit = new
            {
                type = "string",
                nullable = true,
                @enum = new[] { "g", "kg", "ml", "l", "un" },
            },
            unitPrice = new { type = "number", nullable = true, description = "Preço unitário em reais." },
            quantity = new { type = "integer", nullable = true, description = "Quantidade de unidades compradas." },
        },
    };
}
