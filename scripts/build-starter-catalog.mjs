#!/usr/bin/env node
/**
 * Builds data/starter-catalog.pt-BR.json from department product definitions.
 * Run: node scripts/build-starter-catalog.mjs
 */

import { readFileSync, writeFileSync } from 'node:fs';
import { dirname, join } from 'node:path';
import { fileURLToPath } from 'node:url';

const __dirname = dirname(fileURLToPath(import.meta.url));
const root = join(__dirname, '..');
const outputPath = join(root, 'data', 'starter-catalog.pt-BR.json');
const storesPath = join(root, 'data', 'starter-stores.pt-BR.json');

const storesSource = JSON.parse(readFileSync(storesPath, 'utf8'));
const stores = storesSource.stores.map(({ key, name, city, state }) => ({
  key,
  name,
  ...(city ? { city } : {}),
  ...(state ? { state } : {}),
}));

/** @type {Array<{key:string,name:string,category:string,quantityValue:number,quantityUnit:string,defaultStoreKey:string}>} */
const products = [];

function add(dept, cat, sub, key, name, qty, unit, store = 'carrefour') {
  products.push({
    key,
    name,
    category: `${dept} > ${cat} > ${sub}`,
    quantityValue: qty,
    quantityUnit: unit,
    defaultStoreKey: store,
  });
}

// ─── MERCEARIA (80) ───────────────────────────────────────────────────────────
const M = 'Mercearia';

// Grãos e Cereais
add(M, 'Grãos e Cereais', 'Arroz', 'arroz-branco-5kg', 'Arroz Branco', 5, 'kg', 'atacadao');
add(M, 'Grãos e Cereais', 'Arroz', 'arroz-integral-1kg', 'Arroz Integral', 1, 'kg', 'atacadao');
add(M, 'Grãos e Cereais', 'Arroz', 'arroz-parborizado-1kg', 'Arroz Parborizado', 1, 'kg', 'atacadao');
add(M, 'Grãos e Cereais', 'Arroz', 'arroz-japones-1kg', 'Arroz Japonês', 1, 'kg', 'pao-de-acucar');
add(M, 'Grãos e Cereais', 'Feijão', 'feijao-carioca-1kg', 'Feijão Carioca', 1, 'kg', 'atacadao');
add(M, 'Grãos e Cereais', 'Feijão', 'feijao-preto-1kg', 'Feijão Preto', 1, 'kg', 'atacadao');
add(M, 'Grãos e Cereais', 'Feijão', 'feijao-fradinho-500g', 'Feijão Fradinho', 500, 'g', 'atacadao');
add(M, 'Grãos e Cereais', 'Lentilha', 'lentilha-vermelha-500g', 'Lentilha Vermelha', 500, 'g', 'carrefour');
add(M, 'Grãos e Cereais', 'Grão-de-Bico', 'graodebico-500g', 'Grão-de-Bico', 500, 'g', 'carrefour');
add(M, 'Grãos e Cereais', 'Milho', 'milho-pipoca-500g', 'Milho para Pipoca', 500, 'g', 'carrefour');
add(M, 'Grãos e Cereais', 'Quinoa', 'quinoa-branca-250g', 'Quinoa Branca', 250, 'g', 'pao-de-acucar');
add(M, 'Grãos e Cereais', 'Aveia', 'aveia-flocos-500g', 'Aveia em Flocos', 500, 'g', 'carrefour');

// Farinhas e Amidos
add(M, 'Farinhas e Amidos', 'Farinha de Trigo', 'farinha-trigo-1kg', 'Farinha de Trigo', 1, 'kg', 'atacadao');
add(M, 'Farinhas e Amidos', 'Farinha de Mandioca', 'farinha-mandioca-500g', 'Farinha de Mandioca', 500, 'g', 'atacadao');
add(M, 'Farinhas e Amidos', 'Fubá', 'fuba-mimoso-500g', 'Fubá Mimoso', 500, 'g', 'atacadao');
add(M, 'Farinhas e Amidos', 'Polvilho', 'polvilho-doce-500g', 'Polvilho Doce', 500, 'g', 'atacadao');
add(M, 'Farinhas e Amidos', 'Polvilho', 'polvilho-azedo-500g', 'Polvilho Azedo', 500, 'g', 'atacadao');
add(M, 'Farinhas e Amidos', 'Amido de Milho', 'amido-milho-200g', 'Amido de Milho', 200, 'g', 'carrefour');
add(M, 'Farinhas e Amidos', 'Farinha de Roscas', 'farinha-rosca-500g', 'Farinha de Roscas', 500, 'g', 'carrefour');

// Massas
add(M, 'Massas', 'Espaguete', 'macarrao-espaguete-500g', 'Macarrão Espaguete', 500, 'g', 'atacadao');
add(M, 'Massas', 'Parafuso', 'macarrao-parafuso-500g', 'Macarrão Parafuso', 500, 'g', 'atacadao');
add(M, 'Massas', 'Penne', 'macarrao-penne-500g', 'Macarrão Penne', 500, 'g', 'atacadao');
add(M, 'Massas', 'Fusilli', 'macarrao-fusilli-500g', 'Macarrão Fusilli', 500, 'g', 'atacadao');
add(M, 'Massas', 'Talharim', 'macarrao-talharim-500g', 'Macarrão Talharim', 500, 'g', 'atacadao');
add(M, 'Massas', 'Lasanha', 'macarrao-lasanha-500g', 'Massa para Lasanha', 500, 'g', 'carrefour');
add(M, 'Massas', 'Massas Recheadas', 'nhoque-batata-500g', 'Nhoque de Batata', 500, 'g', 'carrefour');
add(M, 'Massas', 'Massas Recheadas', 'capeletti-frango-400g', 'Capeletti de Frango', 400, 'g', 'carrefour');

// Óleos e Azeites
add(M, 'Óleos e Azeites', 'Óleo de Soja', 'oleo-soja-900ml', 'Óleo de Soja', 900, 'ml', 'carrefour');
add(M, 'Óleos e Azeites', 'Óleo de Girassol', 'oleo-girassol-900ml', 'Óleo de Girassol', 900, 'ml', 'carrefour');
add(M, 'Óleos e Azeites', 'Óleo de Canola', 'oleo-canola-900ml', 'Óleo de Canola', 900, 'ml', 'carrefour');
add(M, 'Óleos e Azeites', 'Azeite de Oliva', 'azeite-extra-virgem-500ml', 'Azeite de Oliva Extra Virgem', 500, 'ml', 'pao-de-acucar');
add(M, 'Óleos e Azeites', 'Azeite de Dendê', 'azeite-dende-200ml', 'Azeite de Dendê', 200, 'ml', 'carrefour');

// Açúcares
add(M, 'Açúcares e Adoçantes', 'Açúcar Refinado', 'acucar-refinado-1kg', 'Açúcar Refinado', 1, 'kg', 'atacadao');
add(M, 'Açúcares e Adoçantes', 'Açúcar Cristal', 'acucar-cristal-1kg', 'Açúcar Cristal', 1, 'kg', 'atacadao');
add(M, 'Açúcares e Adoçantes', 'Açúcar Mascavo', 'acucar-mascavo-500g', 'Açúcar Mascavo', 500, 'g', 'pao-de-acucar');
add(M, 'Açúcares e Adoçantes', 'Açúcar Demerara', 'acucar-demerara-1kg', 'Açúcar Demerara', 1, 'kg', 'atacadao');
add(M, 'Açúcares e Adoçantes', 'Adoçante', 'adocante-liquido-100ml', 'Adoçante Líquido', 100, 'ml', 'carrefour');

// Café e Chás
add(M, 'Café, Chás e Bebidas Quentes', 'Café Torrado', 'cafe-torrado-500g', 'Café Torrado e Moído', 500, 'g', 'pao-de-acucar');
add(M, 'Café, Chás e Bebidas Quentes', 'Café Solúvel', 'cafe-soluvel-200g', 'Café Solúvel', 200, 'g', 'carrefour');
add(M, 'Café, Chás e Bebidas Quentes', 'Café em Cápsula', 'cafe-capsula-10un', 'Café em Cápsula', 10, 'un', 'sams-club');
add(M, 'Café, Chás e Bebidas Quentes', 'Chás', 'cha-preto-20saches', 'Chá Preto', 20, 'un', 'carrefour');
add(M, 'Café, Chás e Bebidas Quentes', 'Chás', 'cha-verde-20saches', 'Chá Verde', 20, 'un', 'carrefour');
add(M, 'Café, Chás e Bebidas Quentes', 'Chás', 'cha-camomila-15saches', 'Chá de Camomila', 15, 'un', 'carrefour');
add(M, 'Café, Chás e Bebidas Quentes', 'Achocolatado', 'achocolatado-po-400g', 'Achocolatado em Pó', 400, 'g', 'carrefour');
add(M, 'Café, Chás e Bebidas Quentes', 'Chocolate em Pó', 'chocolate-po-200g', 'Chocolate em Pó', 200, 'g', 'carrefour');

// Leites em Pó
add(M, 'Leites em Pó e Cremers', 'Leite em Pó', 'leite-po-integral-400g', 'Leite em Pó Integral', 400, 'g', 'carrefour');
add(M, 'Leites em Pó e Cremers', 'Leite em Pó', 'leite-po-desnatado-400g', 'Leite em Pó Desnatado', 400, 'g', 'carrefour');
add(M, 'Leites em Pó e Cremers', 'Leite de Coco', 'leite-coco-200ml', 'Leite de Coco', 200, 'ml', 'carrefour');

// Conservas
add(M, 'Conservas e Enlatados', 'Milho Verde', 'milho-verde-lata-200g', 'Milho Verde em Conserva', 200, 'g', 'carrefour');
add(M, 'Conservas e Enlatados', 'Ervilha', 'ervilha-lata-200g', 'Ervilha em Conserva', 200, 'g', 'carrefour');
add(M, 'Conservas e Enlatados', 'Palmito', 'palmito-conserva-300g', 'Palmito em Conserva', 300, 'g', 'carrefour');
add(M, 'Conservas e Enlatados', 'Azeitona', 'azeitona-verde-200g', 'Azeitona Verde', 200, 'g', 'carrefour');
add(M, 'Conservas e Enlatados', 'Azeitona', 'azeitona-preta-200g', 'Azeitona Preta', 200, 'g', 'carrefour');
add(M, 'Conservas e Enlatados', 'Pepino', 'pepino-conserva-300g', 'Pepino em Conserva', 300, 'g', 'carrefour');
add(M, 'Conservas e Enlatados', 'Peixes Enlatados', 'atum-lata-170g', 'Atum em Conserva', 170, 'g', 'carrefour');
add(M, 'Conservas e Enlatados', 'Peixes Enlatados', 'sardinha-lata-125g', 'Sardinha em Conserva', 125, 'g', 'carrefour');
add(M, 'Conservas e Enlatados', 'Peixes Enlatados', 'salmao-lata-170g', 'Salmão em Conserva', 170, 'g', 'carrefour');
add(M, 'Conservas e Enlatados', 'Seleta de Legumes', 'seleta-legumes-200g', 'Seleta de Legumes', 200, 'g', 'carrefour');

// Molhos
add(M, 'Molhos e Condimentos', 'Molho de Tomate', 'molho-tomate-340g', 'Molho de Tomate', 340, 'g', 'carrefour');
add(M, 'Molhos e Condimentos', 'Extrato de Tomate', 'extrato-tomate-340g', 'Extrato de Tomate', 340, 'g', 'carrefour');
add(M, 'Molhos e Condimentos', 'Shoyu', 'molho-shoyu-150ml', 'Molho Shoyu', 150, 'ml', 'carrefour');
add(M, 'Molhos e Condimentos', 'Mostarda', 'mostarda-255g', 'Mostarda', 255, 'g', 'carrefour');
add(M, 'Molhos e Condimentos', 'Maionese', 'maionese-500g', 'Maionese', 500, 'g', 'carrefour');
add(M, 'Molhos e Condimentos', 'Ketchup', 'ketchup-397g', 'Ketchup', 397, 'g', 'carrefour');
add(M, 'Molhos e Condimentos', 'Vinagre', 'vinagre-alcool-750ml', 'Vinagre de Álcool', 750, 'ml', 'carrefour');
add(M, 'Molhos e Condimentos', 'Vinagre', 'vinagre-balsamico-250ml', 'Vinagre Balsâmico', 250, 'ml', 'pao-de-acucar');

// Temperos
add(M, 'Temperos e Especiarias', 'Sal', 'sal-refinado-1kg', 'Sal Refinado', 1, 'kg', 'atacadao');
add(M, 'Temperos e Especiarias', 'Sal', 'sal-grosso-1kg', 'Sal Grosso', 1, 'kg', 'atacadao');
add(M, 'Temperos e Especiarias', 'Pimenta', 'pimenta-reino-50g', 'Pimenta do Reino', 50, 'g', 'carrefour');
add(M, 'Temperos e Especiarias', 'Páprica', 'paprica-doce-50g', 'Páprica Doce', 50, 'g', 'carrefour');
add(M, 'Temperos e Especiarias', 'Cúrcuma', 'curcuma-po-50g', 'Cúrcuma em Pó', 50, 'g', 'carrefour');
add(M, 'Temperos e Especiarias', 'Canela', 'canela-po-50g', 'Canela em Pó', 50, 'g', 'carrefour');
add(M, 'Temperos e Especiarias', 'Orégano', 'oregano-15g', 'Orégano Desidratado', 15, 'g', 'carrefour');
add(M, 'Temperos e Especiarias', 'Caldo em Pó', 'caldo-galinha-50g', 'Caldo de Galinha em Pó', 50, 'g', 'carrefour');
add(M, 'Temperos e Especiarias', 'Caldo em Pó', 'caldo-carne-50g', 'Caldo de Carne em Pó', 50, 'g', 'carrefour');
add(M, 'Temperos e Especiarias', 'Tempero Completo', 'tempero-completo-60g', 'Tempero Completo', 60, 'g', 'carrefour');

// Biscoitos
add(M, 'Biscoitos e Bolachas', 'Cream Cracker', 'biscoito-cream-cracker-400g', 'Biscoito Cream Cracker', 400, 'g', 'pao-de-acucar');
add(M, 'Biscoitos e Bolachas', 'Maisena', 'biscoito-maisena-400g', 'Biscoito Maisena', 400, 'g', 'pao-de-acucar');
add(M, 'Biscoitos e Bolachas', 'Recheado', 'biscoito-recheado-chocolate-130g', 'Biscoito Recheado Chocolate', 130, 'g', 'carrefour');
add(M, 'Biscoitos e Bolachas', 'Recheado', 'biscoito-recheado-morango-126g', 'Biscoito Recheado Morango', 126, 'g', 'carrefour');
add(M, 'Biscoitos e Bolachas', 'Wafer', 'biscoito-wafer-140g', 'Biscoito Wafer', 140, 'g', 'carrefour');
add(M, 'Biscoitos e Bolachas', 'Rosquinha', 'biscoito-rosquinha-350g', 'Biscoito Rosquinha', 350, 'g', 'carrefour');
add(M, 'Biscoitos e Bolachas', 'Água e Sal', 'biscoito-agua-sal-350g', 'Biscoito Água e Sal', 350, 'g', 'carrefour');
add(M, 'Biscoitos e Bolachas', 'Cookies', 'cookies-chocolate-150g', 'Cookies com Chocolate', 150, 'g', 'pao-de-acucar');

// Cereais
add(M, 'Cereais e Barras', 'Cereal Matinal', 'cereal-milho-flakes-250g', 'Cereal de Milho', 250, 'g', 'carrefour');
add(M, 'Cereais e Barras', 'Granola', 'granola-300g', 'Granola', 300, 'g', 'pao-de-acucar');
add(M, 'Cereais e Barras', 'Barra de Cereal', 'barra-cereal-3un', 'Barra de Cereal', 3, 'un', 'carrefour');
add(M, 'Cereais e Barras', 'Muesli', 'muesli-300g', 'Muesli', 300, 'g', 'pao-de-acucar');

// Sobremesas
add(M, 'Sobremesas e Preparos', 'Gelatina', 'gelatina-morango-20g', 'Gelatina Morango', 20, 'g', 'carrefour');
add(M, 'Sobremesas e Preparos', 'Pudim', 'pudim-chocolate-100g', 'Pudim de Chocolate', 100, 'g', 'carrefour');
add(M, 'Sobremesas e Preparos', 'Mousse', 'mousse-chocolate-50g', 'Mousse de Chocolate', 50, 'g', 'carrefour');
add(M, 'Sobremesas e Preparos', 'Manjar', 'manjar-po-100g', 'Manjar em Pó', 100, 'g', 'carrefour');
add(M, 'Sobremesas e Preparos', 'Brigadeiro', 'brigadeiro-po-100g', 'Brigadeiro em Pó', 100, 'g', 'carrefour');

// ─── FRIOS E LATICÍNIOS (40) ─────────────────────────────────────────────────
const L = 'Frios e Laticínios';

add(L, 'Leites e Cremes', 'Leite UHT Integral', 'leite-uht-integral-1l', 'Leite UHT Integral', 1, 'l', 'carrefour');
add(L, 'Leites e Cremes', 'Leite UHT Desnatado', 'leite-uht-desnatado-1l', 'Leite UHT Desnatado', 1, 'l', 'carrefour');
add(L, 'Leites e Cremes', 'Leite UHT Integral', 'leite-uht-semi-1l', 'Leite UHT Semi-Desnatado', 1, 'l', 'carrefour');
add(L, 'Leites e Cremes', 'Leite Sem Lactose', 'leite-sem-lactose-1l', 'Leite Sem Lactose', 1, 'l', 'carrefour');
add(L, 'Leites e Cremes', 'Leite Vegetal', 'leite-amendoa-1l', 'Leite de Amêndoa', 1, 'l', 'pao-de-acucar');
add(L, 'Leites e Cremes', 'Leite Vegetal', 'leite-soja-1l', 'Leite de Soja', 1, 'l', 'carrefour');
add(L, 'Leites e Cremes', 'Leite Condensado', 'leite-condensado-395g', 'Leite Condensado', 395, 'g', 'carrefour');
add(L, 'Leites e Cremes', 'Creme de Leite', 'creme-leite-200g', 'Creme de Leite', 200, 'g', 'carrefour');
add(L, 'Leites e Cremes', 'Creme de Leite', 'creme-leite-light-200g', 'Creme de Leite Light', 200, 'g', 'carrefour');
add(L, 'Leites e Cremes', 'Chantilly', 'chantilly-lata-200g', 'Chantilly em Lata', 200, 'g', 'carrefour');

add(L, 'Queijos', 'Mussarela', 'queijo-mussarela-500g', 'Queijo Mussarela', 500, 'g', 'carrefour');
add(L, 'Queijos', 'Prato', 'queijo-prato-200g', 'Queijo Prato', 200, 'g', 'carrefour');
add(L, 'Queijos', 'Parmesão', 'queijo-parmesao-100g', 'Queijo Parmesão Ralado', 100, 'g', 'carrefour');
add(L, 'Queijos', 'Coalho', 'queijo-coalho-500g', 'Queijo Coalho', 500, 'g', 'assai');
add(L, 'Queijos', 'Cottage', 'queijo-cottage-200g', 'Queijo Cottage', 200, 'g', 'carrefour');
add(L, 'Queijos', 'Cream Cheese', 'cream-cheese-150g', 'Cream Cheese', 150, 'g', 'carrefour');
add(L, 'Queijos', 'Ricota', 'queijo-ricota-300g', 'Ricota Fresca', 300, 'g', 'carrefour');
add(L, 'Queijos', 'Minas', 'queijo-minas-frescal-500g', 'Queijo Minas Frescal', 500, 'g', 'carrefour');
add(L, 'Queijos', 'Requeijão', 'requeijao-cremoso-200g', 'Requeijão Cremoso', 200, 'g', 'carrefour');

add(L, 'Manteigas e Margarinas', 'Manteiga com Sal', 'manteiga-com-sal-200g', 'Manteiga com Sal', 200, 'g', 'carrefour');
add(L, 'Manteigas e Margarinas', 'Manteiga sem Sal', 'manteiga-sem-sal-200g', 'Manteiga sem Sal', 200, 'g', 'carrefour');
add(L, 'Manteigas e Margarinas', 'Margarina', 'margarina-500g', 'Margarina', 500, 'g', 'carrefour');
add(L, 'Manteigas e Margarinas', 'Margarina', 'margarina-light-500g', 'Margarina Light', 500, 'g', 'carrefour');
add(L, 'Manteigas e Margarinas', 'Manteiga de Garrafa', 'manteiga-garrafa-200ml', 'Manteiga de Garrafa', 200, 'ml', 'assai');

add(L, 'Iogurtes', 'Iogurte Natural', 'iogurte-natural-170g', 'Iogurte Natural', 170, 'g', 'carrefour');
add(L, 'Iogurtes', 'Iogurte Natural', 'iogurte-desnatado-170g', 'Iogurte Natural Desnatado', 170, 'g', 'carrefour');
add(L, 'Iogurtes', 'Iogurte Grego', 'iogurte-grego-100g', 'Iogurte Grego', 100, 'g', 'carrefour');
add(L, 'Iogurtes', 'Iogurte Grego', 'iogurte-grego-light-100g', 'Iogurte Grego Light', 100, 'g', 'carrefour');
add(L, 'Iogurtes', 'Iogurte Bebível', 'iogurte-bebivel-morango-170g', 'Iogurte Bebível Morango', 170, 'g', 'carrefour');
add(L, 'Iogurtes', 'Iogurte Infantil', 'iogurte-infantil-100g', 'Iogurte Infantil', 100, 'g', 'carrefour');

add(L, 'Ovos', 'Ovos Vermelhos', 'ovos-vermelhos-dz', 'Ovos Vermelhos', 12, 'un', 'carrefour');
add(L, 'Ovos', 'Ovos Brancos', 'ovos-brancos-dz', 'Ovos Brancos', 12, 'un', 'carrefour');
add(L, 'Ovos', 'Ovos Caipira', 'ovos-caipira-dz', 'Ovos Caipira', 12, 'un', 'pao-de-acucar');
add(L, 'Ovos', 'Ovos de Codorna', 'ovos-codorna-18un', 'Ovos de Codorna', 18, 'un', 'pao-de-acucar');

add(L, 'Embutidos', 'Presunto', 'presunto-cozido-200g', 'Presunto Cozido', 200, 'g', 'carrefour');
add(L, 'Embutidos', 'Presunto', 'presunto-cru-100g', 'Presunto Cru', 100, 'g', 'pao-de-acucar');
add(L, 'Embutidos', 'Peito de Peru', 'peito-peru-200g', 'Peito de Peru', 200, 'g', 'carrefour');
add(L, 'Embutidos', 'Mortadela', 'mortadela-200g', 'Mortadela', 200, 'g', 'carrefour');
add(L, 'Embutidos', 'Salame', 'salame-100g', 'Salame', 100, 'g', 'carrefour');
add(L, 'Embutidos', 'Linguiça', 'linguica-calabresa-500g', 'Linguiça Calabresa', 500, 'g', 'assai');
add(L, 'Embutidos', 'Bacon', 'bacon-fatias-150g', 'Bacon em Fatias', 150, 'g', 'carrefour');
add(L, 'Embutidos', 'Patê', 'pate-frango-150g', 'Patê de Frango', 150, 'g', 'carrefour');

// ─── BEBIDAS (35) ─────────────────────────────────────────────────────────────
const B = 'Bebidas';

add(B, 'Refrigerantes', 'Cola', 'refrigerante-cola-2l', 'Refrigerante Cola', 2, 'l', 'extra');
add(B, 'Refrigerantes', 'Cola', 'refrigerante-cola-zero-2l', 'Refrigerante Cola Zero', 2, 'l', 'extra');
add(B, 'Refrigerantes', 'Guaraná', 'refrigerante-guarana-2l', 'Refrigerante Guaraná', 2, 'l', 'extra');
add(B, 'Refrigerantes', 'Guaraná', 'refrigerante-guarana-diet-2l', 'Refrigerante Guaraná Diet', 2, 'l', 'extra');
add(B, 'Refrigerantes', 'Laranja', 'refrigerante-laranja-2l', 'Refrigerante Laranja', 2, 'l', 'extra');
add(B, 'Refrigerantes', 'Limão', 'refrigerante-limao-2l', 'Refrigerante Limão', 2, 'l', 'extra');
add(B, 'Refrigerantes', 'Uva', 'refrigerante-uva-2l', 'Refrigerante Uva', 2, 'l', 'extra');
add(B, 'Refrigerantes', 'Tônica', 'refrigerante-tonica-350ml', 'Refrigerante Tônica', 350, 'ml', 'extra');
add(B, 'Refrigerantes', 'Citrus', 'refrigerante-citrus-500ml', 'Refrigerante Citrus', 500, 'ml', 'extra');

add(B, 'Sucos e Néctares', 'Suco de Laranja', 'suco-laranja-integral-1l', 'Suco de Laranja Integral', 1, 'l', 'extra');
add(B, 'Sucos e Néctares', 'Néctar de Frutas', 'nectar-laranja-1l', 'Néctar de Laranja', 1, 'l', 'extra');
add(B, 'Sucos e Néctares', 'Néctar de Frutas', 'nectar-uva-1l', 'Néctar de Uva', 1, 'l', 'extra');
add(B, 'Sucos e Néctares', 'Néctar de Frutas', 'nectar-maca-1l', 'Néctar de Maçã', 1, 'l', 'extra');
add(B, 'Sucos e Néctares', 'Néctar de Frutas', 'nectar-maracuja-1l', 'Néctar de Maracujá', 1, 'l', 'extra');
add(B, 'Sucos e Néctares', 'Suco de Caixa', 'suco-caixa-200ml', 'Suco de Caixa', 200, 'ml', 'extra');
add(B, 'Sucos e Néctares', 'Água de Coco', 'agua-coco-1l', 'Água de Coco', 1, 'l', 'extra');

add(B, 'Águas', 'Água Mineral sem Gás', 'agua-mineral-1-5l', 'Água Mineral', 1.5, 'l', 'extra');
add(B, 'Águas', 'Água Mineral com Gás', 'agua-com-gas-500ml', 'Água Mineral com Gás', 500, 'ml', 'extra');
add(B, 'Águas', 'Água Saborizada', 'agua-saborizada-limao-500ml', 'Água Saborizada Limão', 500, 'ml', 'extra');
add(B, 'Águas', 'Água Saborizada', 'agua-saborizada-frutas-500ml', 'Água Saborizada Frutas', 500, 'ml', 'extra');

add(B, 'Cervejas', 'Pilsen', 'cerveja-pilsen-350ml', 'Cerveja Pilsen', 350, 'ml', 'extra');
add(B, 'Cervejas', 'Premium', 'cerveja-premium-330ml', 'Cerveja Premium', 330, 'ml', 'extra');
add(B, 'Cervejas', 'Sem Álcool', 'cerveja-sem-alcool-350ml', 'Cerveja Sem Álcool', 350, 'ml', 'extra');
add(B, 'Cervejas', 'Artesanal', 'cerveja-artesanal-500ml', 'Cerveja Artesanal', 500, 'ml', 'pao-de-acucar');

add(B, 'Vinhos e Destilados', 'Vinho Tinto', 'vinho-tinto-seco-750ml', 'Vinho Tinto Seco', 750, 'ml', 'pao-de-acucar');
add(B, 'Vinhos e Destilados', 'Vinho Tinto', 'vinho-tinto-suave-750ml', 'Vinho Tinto Suave', 750, 'ml', 'pao-de-acucar');
add(B, 'Vinhos e Destilados', 'Vinho Branco', 'vinho-branco-seco-750ml', 'Vinho Branco Seco', 750, 'ml', 'pao-de-acucar');
add(B, 'Vinhos e Destilados', 'Vinho Rosé', 'vinho-rose-750ml', 'Vinho Rosé', 750, 'ml', 'pao-de-acucar');
add(B, 'Vinhos e Destilados', 'Espumante', 'espumante-750ml', 'Espumante', 750, 'ml', 'pao-de-acucar');
add(B, 'Vinhos e Destilados', 'Cachaça', 'cachaca-700ml', 'Cachaça', 700, 'ml', 'extra');
add(B, 'Vinhos e Destilados', 'Vodka', 'vodka-700ml', 'Vodka', 700, 'ml', 'extra');
add(B, 'Vinhos e Destilados', 'Uísque', 'uisque-1l', 'Uísque', 1, 'l', 'sams-club');
add(B, 'Vinhos e Destilados', 'Gin', 'gin-750ml', 'Gin', 750, 'ml', 'pao-de-acucar');

add(B, 'Isotônicos e Energéticos', 'Isotônico', 'isotonico-500ml', 'Isotônico', 500, 'ml', 'extra');
add(B, 'Isotônicos e Energéticos', 'Energético', 'energetico-250ml', 'Energético', 250, 'ml', 'extra');

// ─── HORTIFRUTI (35) ─────────────────────────────────────────────────────────
const H = 'Hortifruti';

add(H, 'Frutas', 'Banana', 'banana-prata-kg', 'Banana Prata', 1, 'kg', 'extra');
add(H, 'Frutas', 'Banana', 'banana-nanica-kg', 'Banana Nanica', 1, 'kg', 'extra');
add(H, 'Frutas', 'Maçã', 'maca-gala-kg', 'Maçã Gala', 1, 'kg', 'extra');
add(H, 'Frutas', 'Maçã', 'maca-fuji-kg', 'Maçã Fuji', 1, 'kg', 'extra');
add(H, 'Frutas', 'Laranja', 'laranja-pera-kg', 'Laranja Pera', 1, 'kg', 'extra');
add(H, 'Frutas', 'Limão', 'limao-tahiti-kg', 'Limão Tahiti', 1, 'kg', 'extra');
add(H, 'Frutas', 'Manga', 'manga-tommy-kg', 'Manga Tommy', 1, 'kg', 'extra');
add(H, 'Frutas', 'Uva', 'uva-verde-500g', 'Uva Verde', 500, 'g', 'extra');
add(H, 'Frutas', 'Uva', 'uva-roxa-500g', 'Uva Roxa', 500, 'g', 'extra');
add(H, 'Frutas', 'Melancia', 'melancia-fatia', 'Melancia', 3, 'kg', 'extra');
add(H, 'Frutas', 'Melão', 'melao-amarelo-un', 'Melão Amarelo', 1, 'un', 'extra');
add(H, 'Frutas', 'Mamão', 'mamao-formosa-kg', 'Mamão Formosa', 1, 'kg', 'extra');
add(H, 'Frutas', 'Morango', 'morango-bandeja-250g', 'Morango', 250, 'g', 'extra');
add(H, 'Frutas', 'Abacaxi', 'abacaxi-perola-un', 'Abacaxi Pérola', 1, 'un', 'extra');
add(H, 'Frutas', 'Abacate', 'abacate-kg', 'Abacate', 1, 'kg', 'extra');
add(H, 'Frutas', 'Maracujá', 'maracuja-kg', 'Maracujá', 1, 'kg', 'extra');
add(H, 'Frutas', 'Kiwi', 'kiwi-bandeja-4un', 'Kiwi', 4, 'un', 'extra');

add(H, 'Legumes', 'Batata', 'batata-inglesa-kg', 'Batata Inglesa', 1, 'kg', 'extra');
add(H, 'Legumes', 'Batata', 'batata-doce-kg', 'Batata Doce', 1, 'kg', 'extra');
add(H, 'Legumes', 'Cenoura', 'cenoura-kg', 'Cenoura', 1, 'kg', 'extra');
add(H, 'Legumes', 'Tomate', 'tomate-salada-kg', 'Tomate', 1, 'kg', 'extra');
add(H, 'Legumes', 'Tomate', 'tomate-cereja-250g', 'Tomate Cereja', 250, 'g', 'extra');
add(H, 'Legumes', 'Cebola', 'cebola-kg', 'Cebola', 1, 'kg', 'extra');
add(H, 'Legumes', 'Alho', 'alho-kg', 'Alho', 1, 'kg', 'extra');
add(H, 'Legumes', 'Mandioca', 'mandioca-kg', 'Mandioca', 1, 'kg', 'extra');
add(H, 'Legumes', 'Berinjela', 'berinjela-kg', 'Berinjela', 1, 'kg', 'extra');
add(H, 'Legumes', 'Pepino', 'pepino-japones-kg', 'Pepino Japonês', 1, 'kg', 'extra');
add(H, 'Legumes', 'Pimentão', 'pimentao-verde-kg', 'Pimentão Verde', 1, 'kg', 'extra');
add(H, 'Legumes', 'Pimentão', 'pimentao-vermelho-kg', 'Pimentão Vermelho', 1, 'kg', 'extra');
add(H, 'Legumes', 'Abobrinha', 'abobrinha-italiana-kg', 'Abobrinha Italiana', 1, 'kg', 'extra');
add(H, 'Legumes', 'Vagem', 'vagem-kg', 'Vagem', 1, 'kg', 'extra');
add(H, 'Legumes', 'Milho Verde', 'milho-verde-espiga-un', 'Milho Verde', 1, 'un', 'extra');

add(H, 'Verduras', 'Alface', 'alface-crespa-un', 'Alface Crespa', 1, 'un', 'extra');
add(H, 'Verduras', 'Alface', 'alface-romana-un', 'Alface Romana', 1, 'un', 'extra');
add(H, 'Verduras', 'Couve', 'couve-manteiga-maco', 'Couve Manteiga', 1, 'un', 'extra');
add(H, 'Verduras', 'Brócolis', 'brocolis-un', 'Brócolis', 1, 'un', 'extra');
add(H, 'Verduras', 'Couve-Flor', 'couve-flor-un', 'Couve-Flor', 1, 'un', 'extra');
add(H, 'Verduras', 'Repolho', 'repolho-verde-un', 'Repolho Verde', 1, 'un', 'extra');
add(H, 'Verduras', 'Espinafre', 'espinafre-maco', 'Espinafre', 1, 'un', 'extra');
add(H, 'Verduras', 'Rúcula', 'rucula-maco', 'Rúcula', 1, 'un', 'extra');
add(H, 'Verduras', 'Agrião', 'agriao-maco', 'Agrião', 1, 'un', 'extra');

add(H, 'Ervas e Temperos Frescos', 'Cheiro-Verde', 'cheiro-verde-maco', 'Cheiro-Verde', 1, 'un', 'extra');
add(H, 'Ervas e Temperos Frescos', 'Manjericão', 'manjericao-fresco-maco', 'Manjericão Fresco', 1, 'un', 'extra');
add(H, 'Ervas e Temperos Frescos', 'Hortelã', 'hortela-fresca-maco', 'Hortelã Fresca', 1, 'un', 'extra');
add(H, 'Ervas e Temperos Frescos', 'Salsinha', 'salsinha-maco', 'Salsinha', 1, 'un', 'extra');
add(H, 'Ervas e Temperos Frescos', 'Cebolinha', 'cebolinha-maco', 'Cebolinha', 1, 'un', 'extra');
add(H, 'Ervas e Temperos Frescos', 'Coentro', 'coentro-maco', 'Coentro', 1, 'un', 'extra');
add(H, 'Ervas e Temperos Frescos', 'Gengibre', 'gengibre-kg', 'Gengibre', 1, 'kg', 'extra');

// ─── AÇOUGUE (30) ────────────────────────────────────────────────────────────
const A = 'Açougue';

add(A, 'Carnes Bovinas', 'Acém', 'acem-bovino-kg', 'Acém Bovino', 1, 'kg', 'assai');
add(A, 'Carnes Bovinas', 'Alcatra', 'alcatra-bovina-kg', 'Alcatra Bovina', 1, 'kg', 'assai');
add(A, 'Carnes Bovinas', 'Contra-Filé', 'contrafile-bovino-kg', 'Contra-Filé Bovino', 1, 'kg', 'assai');
add(A, 'Carnes Bovinas', 'Coxão Mole', 'coxao-mole-kg', 'Coxão Mole', 1, 'kg', 'assai');
add(A, 'Carnes Bovinas', 'Cupim', 'cupim-kg', 'Cupim', 1, 'kg', 'assai');
add(A, 'Carnes Bovinas', 'Filé Mignon', 'file-mignon-kg', 'Filé Mignon', 1, 'kg', 'assai');
add(A, 'Carnes Bovinas', 'Fraldinha', 'fraldinha-kg', 'Fraldinha', 1, 'kg', 'assai');
add(A, 'Carnes Bovinas', 'Lagarto', 'lagarto-bovino-kg', 'Lagarto Bovino', 1, 'kg', 'assai');
add(A, 'Carnes Bovinas', 'Maminha', 'maminha-kg', 'Maminha', 1, 'kg', 'assai');
add(A, 'Carnes Bovinas', 'Patinho', 'patinho-kg', 'Patinho', 1, 'kg', 'assai');
add(A, 'Carnes Bovinas', 'Picanha', 'picanha-kg', 'Picanha', 1, 'kg', 'assai');
add(A, 'Carnes Bovinas', 'Carne Moída', 'carne-moida-kg', 'Carne Moída', 1, 'kg', 'assai');

add(A, 'Carnes Suínas', 'Pernil', 'pernil-suino-kg', 'Pernil Suíno', 1, 'kg', 'assai');
add(A, 'Carnes Suínas', 'Lombo', 'lombo-suino-kg', 'Lombo Suíno', 1, 'kg', 'assai');
add(A, 'Carnes Suínas', 'Costela', 'costela-suina-kg', 'Costela Suína', 1, 'kg', 'assai');
add(A, 'Carnes Suínas', 'Filé Suíno', 'file-suino-kg', 'Filé Suíno', 1, 'kg', 'assai');

add(A, 'Aves', 'Frango Inteiro', 'frango-inteiro-kg', 'Frango Inteiro', 1, 'kg', 'assai');
add(A, 'Aves', 'Filé de Peito', 'file-peito-frango-kg', 'Filé de Peito de Frango', 1, 'kg', 'assai');
add(A, 'Aves', 'Sobrecoxa', 'sobrecoxa-frango-kg', 'Sobrecoxa de Frango', 1, 'kg', 'assai');
add(A, 'Aves', 'Coxinha da Asa', 'coxinha-asa-frango-kg', 'Coxinha da Asa', 1, 'kg', 'assai');
add(A, 'Aves', 'Asa', 'asa-frango-kg', 'Asa de Frango', 1, 'kg', 'assai');
add(A, 'Aves', 'Coração', 'coracao-frango-kg', 'Coração de Frango', 1, 'kg', 'assai');
add(A, 'Aves', 'Moela', 'moela-frango-kg', 'Moela de Frango', 1, 'kg', 'assai');

add(A, 'Peixes e Frutos do Mar', 'Tilápia', 'file-tilapia-kg', 'Filé de Tilápia', 1, 'kg', 'assai');
add(A, 'Peixes e Frutos do Mar', 'Salmão', 'file-salmao-kg', 'Filé de Salmão', 1, 'kg', 'pao-de-acucar');
add(A, 'Peixes e Frutos do Mar', 'Bacalhau', 'bacalhau-postas-kg', 'Bacalhau', 1, 'kg', 'pao-de-acucar');
add(A, 'Peixes e Frutos do Mar', 'Sardinha Fresca', 'sardinha-fresca-kg', 'Sardinha Fresca', 1, 'kg', 'assai');
add(A, 'Peixes e Frutos do Mar', 'Camarão', 'camarao-7barbas-kg', 'Camarão 7 Barbas', 1, 'kg', 'assai');
add(A, 'Peixes e Frutos do Mar', 'Lula', 'lula-kg', 'Lula', 1, 'kg', 'assai');

add(A, 'Processados e Embutidos', 'Almôndegas', 'almondegas-bovinas-500g', 'Almôndegas Bovinas', 500, 'g', 'assai');
add(A, 'Processados e Embutidos', 'Hambúrguer', 'hamburguer-bovino-4un', 'Hambúrguer Bovino', 4, 'un', 'assai');
add(A, 'Processados e Embutidos', 'Kibe', 'kibe-500g', 'Kibe', 500, 'g', 'assai');
add(A, 'Processados e Embutidos', 'Linguiça', 'linguica-toscana-kg', 'Linguiça Toscana', 1, 'kg', 'assai');
add(A, 'Processados e Embutidos', 'Salsicha', 'salsicha-fresca-kg', 'Salsicha Fresca', 1, 'kg', 'assai');

// ─── PADARIA (15) ────────────────────────────────────────────────────────────
const P = 'Padaria e Confeitaria';

add(P, 'Pães', 'Pão de Forma', 'pao-forma-branco-500g', 'Pão de Forma Branco', 500, 'g', 'pao-de-acucar');
add(P, 'Pães', 'Pão Integral', 'pao-forma-integral-500g', 'Pão de Forma Integral', 500, 'g', 'pao-de-acucar');
add(P, 'Pães', 'Pão Francês', 'pao-frances-un', 'Pão Francês', 1, 'un', 'extra');
add(P, 'Pães', 'Pão de Leite', 'pao-leite-6un', 'Pão de Leite', 6, 'un', 'extra');
add(P, 'Pães', 'Pão de Queijo', 'pao-queijo-congelado-400g', 'Pão de Queijo Congelado', 400, 'g', 'carrefour');
add(P, 'Pães', 'Baguete', 'baguete-un', 'Baguete', 1, 'un', 'pao-de-acucar');
add(P, 'Pães', 'Pão de Hambúrguer', 'pao-hamburguer-6un', 'Pão de Hambúrguer', 6, 'un', 'carrefour');

add(P, 'Bolos e Tortas', 'Bolo Simples', 'bolo-milho-400g', 'Bolo de Milho', 400, 'g', 'extra');
add(P, 'Bolos e Tortas', 'Bolo Simples', 'bolo-chocolate-400g', 'Bolo de Chocolate', 400, 'g', 'extra');
add(P, 'Bolos e Tortas', 'Torta', 'torta-limao-un', 'Torta de Limão', 1, 'un', 'extra');
add(P, 'Bolos e Tortas', 'Brownie', 'brownie-un', 'Brownie', 1, 'un', 'extra');
add(P, 'Bolos e Tortas', 'Muffin', 'muffin-chocolate-4un', 'Muffin de Chocolate', 4, 'un', 'extra');

add(P, 'Doces e Confeitos', 'Brigadeiro', 'brigadeiro-un', 'Brigadeiro', 1, 'un', 'extra');
add(P, 'Doces e Confeitos', 'Beijinho', 'beijinho-un', 'Beijinho', 1, 'un', 'extra');
add(P, 'Doces e Confeitos', 'Cocada', 'cocada-un', 'Cocada', 1, 'un', 'extra');
add(P, 'Doces e Confeitos', 'Pudim Pronto', 'pudim-leite-un', 'Pudim de Leite', 1, 'un', 'extra');

add(P, 'Salgados', 'Coxinha', 'coxinha-un', 'Coxinha', 1, 'un', 'extra');
add(P, 'Salgados', 'Esfiha', 'esfiha-un', 'Esfiha', 1, 'un', 'extra');
add(P, 'Salgados', 'Pastel', 'pastel-un', 'Pastel', 1, 'un', 'extra');
add(P, 'Salgados', 'Empada', 'empada-frango-un', 'Empada de Frango', 1, 'un', 'extra');
add(P, 'Salgados', 'Quibe Frito', 'quibe-frito-un', 'Quibe Frito', 1, 'un', 'extra');

// ─── HIGIENE (30) ────────────────────────────────────────────────────────────
const HY = 'Higiene e Beleza';

add(HY, 'Cuidados com o Cabelo', 'Shampoo', 'shampoo-350ml', 'Shampoo', 350, 'ml', 'carrefour');
add(HY, 'Cuidados com o Cabelo', 'Shampoo', 'shampoo-anticaspa-350ml', 'Shampoo Anticaspa', 350, 'ml', 'carrefour');
add(HY, 'Cuidados com o Cabelo', 'Condicionador', 'condicionador-350ml', 'Condicionador', 350, 'ml', 'carrefour');
add(HY, 'Cuidados com o Cabelo', 'Máscara Capilar', 'mascara-capilar-300g', 'Máscara Capilar', 300, 'g', 'carrefour');
add(HY, 'Cuidados com o Cabelo', 'Creme para Pentear', 'creme-pentear-300ml', 'Creme para Pentear', 300, 'ml', 'carrefour');
add(HY, 'Cuidados com o Cabelo', 'Finalizador', 'finalizador-capilar-200ml', 'Finalizador Capilar', 200, 'ml', 'carrefour');

add(HY, 'Cuidados com a Pele', 'Sabonete', 'sabonete-90g', 'Sabonete', 90, 'g', 'carrefour');
add(HY, 'Cuidados com a Pele', 'Sabonete Líquido', 'sabonete-liquido-200ml', 'Sabonete Líquido', 200, 'ml', 'carrefour');
add(HY, 'Cuidados com a Pele', 'Hidratante Corporal', 'hidratante-corporal-400ml', 'Hidratante Corporal', 400, 'ml', 'carrefour');
add(HY, 'Cuidados com a Pele', 'Protetor Solar', 'protetor-solar-fps50-200ml', 'Protetor Solar FPS 50', 200, 'ml', 'carrefour');
add(HY, 'Cuidados com a Pele', 'Desodorante', 'desodorante-aerosol-150ml', 'Desodorante Aerosol', 150, 'ml', 'carrefour');
add(HY, 'Cuidados com a Pele', 'Desodorante', 'desodorante-rollon-50ml', 'Desodorante Roll-on', 50, 'ml', 'carrefour');

add(HY, 'Higiene Oral', 'Pasta de Dente', 'pasta-dente-90g', 'Pasta de Dente', 90, 'g', 'carrefour');
add(HY, 'Higiene Oral', 'Pasta de Dente', 'pasta-dente-sensibilidade-90g', 'Pasta de Dente Sensibilidade', 90, 'g', 'carrefour');
add(HY, 'Higiene Oral', 'Escova de Dente', 'escova-dente-un', 'Escova de Dente', 1, 'un', 'carrefour');
add(HY, 'Higiene Oral', 'Fio Dental', 'fio-dental-50m', 'Fio Dental', 50, 'un', 'carrefour');
add(HY, 'Higiene Oral', 'Enxaguante Bucal', 'enxaguante-bucal-500ml', 'Enxaguante Bucal', 500, 'ml', 'carrefour');

add(HY, 'Higiene Corporal', 'Papel Higiênico', 'papel-higienico-12rolos', 'Papel Higiênico', 12, 'un', 'atacadao');
add(HY, 'Higiene Corporal', 'Papel Higiênico', 'papel-higienico-4rolos', 'Papel Higiênico Folha Dupla', 4, 'un', 'atacadao');
add(HY, 'Higiene Corporal', 'Lenços Umedecidos', 'lencos-umedecidos-48un', 'Lenços Umedecidos', 48, 'un', 'carrefour');
add(HY, 'Higiene Corporal', 'Absorvente', 'absorvente-noturno-8un', 'Absorvente Noturno', 8, 'un', 'carrefour');
add(HY, 'Higiene Corporal', 'Barbear', 'creme-barbear-75g', 'Creme de Barbear', 75, 'g', 'carrefour');
add(HY, 'Higiene Corporal', 'Barbear', 'lamina-barbear-4un', 'Lâmina de Barbear', 4, 'un', 'carrefour');

add(HY, 'Primeiros Socorros', 'Algodão', 'algodao-50g', 'Algodão', 50, 'g', 'carrefour');
add(HY, 'Primeiros Socorros', 'Curativos', 'curativos-20un', 'Curativos', 20, 'un', 'carrefour');
add(HY, 'Primeiros Socorros', 'Álcool em Gel', 'alcool-gel-500ml', 'Álcool em Gel', 500, 'ml', 'carrefour');
add(HY, 'Primeiros Socorros', 'Repelente', 'repelente-200ml', 'Repelente de Insetos', 200, 'ml', 'carrefour');

// ─── LIMPEZA (35) ────────────────────────────────────────────────────────────
const CL = 'Limpeza';

add(CL, 'Cozinha', 'Detergente', 'detergente-louca-500ml', 'Detergente para Louça', 500, 'ml', 'atacadao');
add(CL, 'Cozinha', 'Detergente', 'detergente-louca-coco-500ml', 'Detergente Coco', 500, 'ml', 'atacadao');
add(CL, 'Cozinha', 'Esponja', 'esponja-louca-un', 'Esponja de Louça', 1, 'un', 'atacadao');
add(CL, 'Cozinha', 'Esponja', 'esponja-magica-2un', 'Esponja Mágica', 2, 'un', 'atacadao');
add(CL, 'Cozinha', 'Papel Toalha', 'papel-toalha-2rolos', 'Papel Toalha', 2, 'un', 'atacadao');
add(CL, 'Cozinha', 'Guardanapo', 'guardanapo-50un', 'Guardanapo', 50, 'un', 'atacadao');
add(CL, 'Cozinha', 'Papel Alumínio', 'papel-aluminio-un', 'Papel Alumínio', 1, 'un', 'sams-club');
add(CL, 'Cozinha', 'Filme PVC', 'filme-pvc-un', 'Filme PVC', 1, 'un', 'sams-club');
add(CL, 'Cozinha', 'Sacos Plásticos', 'sacos-freezer-100un', 'Sacos para Freezer', 100, 'un', 'atacadao');

add(CL, 'Lavanderia', 'Sabão em Pó', 'sabao-po-1kg', 'Sabão em Pó', 1, 'kg', 'atacadao');
add(CL, 'Lavanderia', 'Sabão em Pó', 'sabao-po-multiacao-1kg', 'Sabão em Pó Multiação', 1, 'kg', 'atacadao');
add(CL, 'Lavanderia', 'Sabão Líquido', 'sabao-liquido-3l', 'Sabão Líquido', 3, 'l', 'atacadao');
add(CL, 'Lavanderia', 'Amaciante', 'amaciante-2l', 'Amaciante', 2, 'l', 'atacadao');
add(CL, 'Lavanderia', 'Amaciante', 'amaciante-concentrado-1l', 'Amaciante Concentrado', 1, 'l', 'atacadao');
add(CL, 'Lavanderia', 'Tira-Manchas', 'tira-manchas-500ml', 'Tira-Manchas', 500, 'ml', 'atacadao');
add(CL, 'Lavanderia', 'Alvejante', 'alvejante-sem-cloro-1l', 'Alvejante sem Cloro', 1, 'l', 'atacadao');

add(CL, 'Banheiro', 'Limpador Sanitário', 'limpador-sanitario-750ml', 'Limpador Sanitário', 750, 'ml', 'atacadao');
add(CL, 'Banheiro', 'Desinfetante', 'desinfetante-2l', 'Desinfetante', 2, 'l', 'atacadao');
add(CL, 'Banheiro', 'Água Sanitária', 'agua-sanitaria-1l', 'Água Sanitária', 1, 'l', 'atacadao');
add(CL, 'Banheiro', 'Aromatizador', 'aromatizador-spray-360ml', 'Aromatizador', 360, 'ml', 'atacadao');
add(CL, 'Banheiro', 'Saco de Lixo', 'saco-lixo-30un', 'Saco de Lixo', 30, 'un', 'atacadao');
add(CL, 'Banheiro', 'Saco de Lixo', 'saco-lixo-50l-30un', 'Saco de Lixo 50L', 30, 'un', 'atacadao');

add(CL, 'Pisos e Superfícies', 'Cera', 'cera-incolor-750ml', 'Cera Líquida Incolor', 750, 'ml', 'atacadao');
add(CL, 'Pisos e Superfícies', 'Cera', 'cera-amarela-750ml', 'Cera Líquida Amarela', 750, 'ml', 'atacadao');
add(CL, 'Pisos e Superfícies', 'Multiuso', 'limpador-multiuso-500ml', 'Limpador Multiuso', 500, 'ml', 'atacadao');
add(CL, 'Pisos e Superfícies', 'Limpa Vidros', 'limpa-vidros-500ml', 'Limpa Vidros', 500, 'ml', 'atacadao');
add(CL, 'Pisos e Superfícies', 'Vassoura', 'vassoura-un', 'Vassoura', 1, 'un', 'atacadao');
add(CL, 'Pisos e Superfícies', 'Rodo', 'rodo-un', 'Rodo', 1, 'un', 'atacadao');
add(CL, 'Pisos e Superfícies', 'Pano de Limpeza', 'pano-limpeza-3un', 'Pano de Limpeza', 3, 'un', 'atacadao');

// Validate uniqueness
const keys = new Set();
for (const p of products) {
  if (keys.has(p.key)) {
    console.error(`Duplicate key: ${p.key}`);
    process.exit(1);
  }
  keys.add(p.key);
}

const catalog = {
  version: 3,
  locale: 'pt-BR',
  stores,
  products,
};

writeFileSync(outputPath, JSON.stringify(catalog, null, 2) + '\n', 'utf8');
console.log(`Built ${products.length} products → ${outputPath}`);
