# Mapeamento do Catálogo Inicial — PT-BR

> Referência de produtos por departamento. Fonte gerada por
> [`scripts/build-starter-catalog.mjs`](../scripts/build-starter-catalog.mjs).

## Convenções

| Campo | Regra |
|-------|-------|
| `key` | kebab-case, única globalmente |
| `category` | `Departamento > Categoria > Subcategoria` |
| `name` | Nome genérico em PT-BR (sem marca obrigatória) |
| `quantityUnit` | `g`, `kg`, `ml`, `l`, `un` |

## Departamentos

### Mercearia (~80 produtos)

- **Grãos e Cereais**: Arroz, Feijão, Lentilha, Grão-de-Bico, Milho, Quinoa, Aveia
- **Farinhas e Amidos**: Farinha de Trigo, Mandioca, Fubá, Polvilho, Amido de Milho
- **Massas**: Espaguete, Parafuso, Penne, Fusilli, Talharim, Lasanha, Recheadas
- **Óleos e Azeites**: Soja, Girassol, Canola, Azeite, Dendê
- **Açúcares e Adoçantes**: Refinado, Cristal, Mascavo, Demerara, Adoçante
- **Café, Chás e Bebidas Quentes**: Café, Chás, Achocolatado, Chocolate em Pó
- **Conservas e Enlatados**: Milho, Ervilha, Palmito, Azeitona, Peixes
- **Molhos e Condimentos**: Tomate, Shoyu, Mostarda, Maionese, Ketchup, Vinagre
- **Temperos e Especiarias**: Sal, Pimenta, Páprica, Cúrcuma, Caldo em Pó
- **Biscoitos e Bolachas**: Cream Cracker, Maisena, Recheado, Wafer, Cookies
- **Cereais e Barras**: Cereal Matinal, Granola, Barra de Cereal, Muesli
- **Sobremesas e Preparos**: Gelatina, Pudim, Mousse, Manjar, Brigadeiro

### Frios e Laticínios (~40 produtos)

- **Leites e Cremes**: UHT, Sem Lactose, Vegetal, Condensado, Creme de Leite
- **Queijos**: Mussarela, Prato, Parmesão, Coalho, Cottage, Cream Cheese, Ricota, Minas
- **Manteigas e Margarinas**: Com/Sem Sal, Margarina, Manteiga de Garrafa
- **Iogurtes**: Natural, Grego, Bebível, Infantil
- **Ovos**: Vermelhos, Brancos, Caipira, Codorna
- **Embutidos**: Presunto, Peru, Mortadela, Salame, Linguiça, Bacon, Patê

### Bebidas (~35 produtos)

- **Refrigerantes**: Cola, Guaraná, Laranja, Limão, Uva, Tônica
- **Sucos e Néctares**: Laranja, Frutas, Caixa, Água de Coco
- **Águas**: Mineral, com Gás, Saborizada
- **Cervejas**: Pilsen, Premium, Sem Álcool, Artesanal
- **Vinhos e Destilados**: Vinhos, Espumante, Cachaça, Vodka, Uísque, Gin
- **Isotônicos e Energéticos**

### Hortifruti (~50 produtos)

- **Frutas**: Banana, Maçã, Laranja, Limão, Manga, Uva, Melancia, Mamão, Morango, etc.
- **Legumes**: Batata, Cenoura, Tomate, Cebola, Alho, Mandioca, Pimentão, etc.
- **Verduras**: Alface, Couve, Brócolis, Repolho, Espinafre, Rúcula, Agrião
- **Ervas Frescas**: Cheiro-Verde, Manjericão, Hortelã, Salsinha, Cebolinha, Coentro

### Açougue (~34 produtos)

- **Carnes Bovinas**: Acém, Alcatra, Contra-Filé, Picanha, Carne Moída, etc.
- **Carnes Suínas**: Pernil, Lombo, Costela, Filé Suíno
- **Aves**: Frango Inteiro, Filé, Sobrecoxa, Asa, Coração, Moela
- **Peixes e Frutos do Mar**: Tilápia, Salmão, Bacalhau, Camarão, Lula
- **Processados**: Almôndegas, Hambúrguer, Kibe, Linguiça, Salsicha

### Padaria e Confeitaria (~21 produtos)

- **Pães**: Forma, Francês, Leite, Queijo, Baguete, Hambúrguer
- **Bolos e Tortas**: Milho, Chocolate, Limão, Brownie, Muffin
- **Doces**: Brigadeiro, Beijinho, Cocada, Pudim
- **Salgados**: Coxinha, Esfiha, Pastel, Empada, Quibe

### Higiene e Beleza (~28 produtos)

- **Cabelo**: Shampoo, Condicionador, Máscara, Creme para Pentear
- **Pele**: Sabonete, Hidratante, Protetor Solar, Desodorante
- **Oral**: Pasta, Escova, Fio Dental, Enxaguante
- **Corporal**: Papel Higiênico, Lenços, Absorvente, Barbear
- **Primeiros Socorros**: Algodão, Curativos, Álcool Gel, Repelente

### Limpeza (~30 produtos)

- **Cozinha**: Detergente, Esponja, Papel Toalha, Alumínio, Filme PVC
- **Lavanderia**: Sabão em Pó, Líquido, Amaciante, Tira-Manchas, Alvejante
- **Banheiro**: Limpador Sanitário, Desinfetante, Água Sanitária, Saco de Lixo
- **Pisos**: Cera, Multiuso, Limpa Vidros, Vassoura, Rodo

## Manutenção

```powershell
# Regenerar catálogo após editar build-starter-catalog.mjs
node scripts/build-starter-catalog.mjs

# Validar
node scripts/catalog-validator.mjs
```
