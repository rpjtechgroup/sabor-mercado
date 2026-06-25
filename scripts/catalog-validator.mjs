#!/usr/bin/env node
/**
 * Validates data/starter-catalog.pt-BR.json against project rules.
 * Run: node scripts/catalog-validator.mjs
 */

import { readFileSync } from 'node:fs';
import { dirname, join } from 'node:path';
import { fileURLToPath } from 'node:url';

const __dirname = dirname(fileURLToPath(import.meta.url));
const catalogPath = join(__dirname, '..', 'data', 'starter-catalog.pt-BR.json');

const VALID_UNITS = new Set(['g', 'kg', 'ml', 'l', 'un']);
const MIN_PRODUCTS = 300;
const MIN_PRODUCTS_PER_SUBCATEGORY = 3;
const CATEGORY_SEPARATOR = ' > ';

/** @type {{ errors: string[], warnings: string[] }} */
const result = { errors: [], warnings: [] };

function error(msg) {
  result.errors.push(msg);
}

function warn(msg) {
  result.warnings.push(msg);
}

let catalog;
try {
  catalog = JSON.parse(readFileSync(catalogPath, 'utf8'));
} catch (e) {
  console.error(`Failed to read catalog: ${e.message}`);
  process.exit(1);
}

if (typeof catalog.version !== 'number') {
  error('Missing or invalid version');
}

if (catalog.locale !== 'pt-BR') {
  error(`Expected locale pt-BR, got ${catalog.locale}`);
}

if (!Array.isArray(catalog.stores) || catalog.stores.length === 0) {
  error('stores must be a non-empty array');
}

if (!Array.isArray(catalog.products) || catalog.products.length === 0) {
  error('products must be a non-empty array');
}

const storeKeys = new Set(catalog.stores?.map((s) => s.key) ?? []);
const productKeys = new Set();
/** @type {Map<string, number>} */
const subcategoryCounts = new Map();
/** @type {Set<string>} */
const departments = new Set();

for (const store of catalog.stores ?? []) {
  if (!store.key || !store.name) {
    error(`Store missing key or name: ${JSON.stringify(store)}`);
  }
}

for (const product of catalog.products ?? []) {
  if (!product.key) {
    error('Product missing key');
    continue;
  }

  if (productKeys.has(product.key)) {
    error(`Duplicate product key: ${product.key}`);
  }
  productKeys.add(product.key);

  if (!product.name) {
    error(`Product ${product.key} missing name`);
  }

  if (!product.defaultStoreKey) {
    error(`Product ${product.key} missing defaultStoreKey`);
  } else if (!storeKeys.has(product.defaultStoreKey)) {
    error(`Product ${product.key} references unknown store: ${product.defaultStoreKey}`);
  }

  if (product.quantityUnit && !VALID_UNITS.has(product.quantityUnit)) {
    error(`Product ${product.key} has invalid quantityUnit: ${product.quantityUnit}`);
  }

  if (product.quantityValue !== undefined && product.quantityValue <= 0) {
    error(`Product ${product.key} has invalid quantityValue: ${product.quantityValue}`);
  }

  if (!product.category) {
    warn(`Product ${product.key} has no category`);
  } else {
    const parts = product.category.split(CATEGORY_SEPARATOR);
    if (parts.length !== 3) {
      warn(`Product ${product.key} category not hierarchical (expected 3 levels): ${product.category}`);
    } else {
      departments.add(parts[0]);
      const subKey = product.category;
      subcategoryCounts.set(subKey, (subcategoryCounts.get(subKey) ?? 0) + 1);
    }
  }

  if (!/^[a-z0-9]+(-[a-z0-9]+)*$/.test(product.key)) {
    warn(`Product ${product.key} key is not kebab-case`);
  }
}

if ((catalog.products?.length ?? 0) < MIN_PRODUCTS) {
  error(`Expected at least ${MIN_PRODUCTS} products, got ${catalog.products?.length ?? 0}`);
}

if (departments.size < 8) {
  error(`Expected at least 8 departments, got ${departments.size}`);
}

for (const [sub, count] of subcategoryCounts) {
  if (count < MIN_PRODUCTS_PER_SUBCATEGORY) {
    warn(`Subcategory "${sub}" has only ${count} product(s) (minimum recommended: ${MIN_PRODUCTS_PER_SUBCATEGORY})`);
  }
}

// Report
console.log('Catalog Validation Report');
console.log('========================');
console.log(`File: ${catalogPath}`);
console.log(`Version: ${catalog.version}`);
console.log(`Stores: ${catalog.stores?.length ?? 0}`);
console.log(`Products: ${catalog.products?.length ?? 0}`);
console.log(`Departments: ${departments.size}`);
console.log(`Subcategories: ${subcategoryCounts.size}`);
console.log('');

if (result.warnings.length > 0) {
  console.log(`Warnings (${result.warnings.length}):`);
  for (const w of result.warnings) {
    console.log(`  ⚠ ${w}`);
  }
  console.log('');
}

if (result.errors.length > 0) {
  console.log(`Errors (${result.errors.length}):`);
  for (const e of result.errors) {
    console.log(`  ✗ ${e}`);
  }
  process.exit(1);
}

console.log('✓ Catalog validation passed');
if (result.warnings.length > 0) {
  process.exit(0);
}
