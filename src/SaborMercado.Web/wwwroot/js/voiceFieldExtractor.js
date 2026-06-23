import { pipeline, env } from 'https://cdn.jsdelivr.net/npm/@huggingface/transformers@3.7.2';

env.allowLocalModels = false;
env.useBrowserCache = true;

const MODEL_ID = 'onnx-community/Qwen2.5-0.5B-Instruct';

let generator = null;
let loadingPromise = null;

const SYSTEM_PROMPT =
  'Extraia campos de item de supermercado em português. Responda APENAS um JSON válido com as chaves: ' +
  '"name" (string), "brand" (string ou null), "unitPrice" (número ou null), "quantity" (inteiro ou null), ' +
  '"quantityValue" (número ou null), "quantityUnit" ("g"|"kg"|"ml"|"l"|"un" ou null).';

async function ensureGenerator() {
  if (generator) {
    return generator;
  }

  if (!loadingPromise) {
    loadingPromise = pipeline('text-generation', MODEL_ID, { dtype: 'q4' })
      .then((pipe) => {
        generator = pipe;
        return pipe;
      })
      .catch((error) => {
        loadingPromise = null;
        throw error;
      });
  }

  return loadingPromise;
}

function buildPrompt(text) {
  const utterance = text.trim().replace(/"/g, "'");
  return (
    `<|im_start|>system\n${SYSTEM_PROMPT}\n` +
    `<|im_start|>user\n${utterance}\n` +
    '<|im_start|>assistant\n'
  );
}

export async function isModelSupported() {
  return typeof window !== 'undefined' && typeof WebAssembly !== 'undefined';
}

export async function extractProductFields(text) {
  if (!text?.trim()) {
    return '';
  }

  const pipe = await ensureGenerator();
  const prompt = buildPrompt(text);
  const result = await pipe(prompt, {
    max_new_tokens: 160,
    temperature: 0.1,
    do_sample: false,
    return_full_text: false,
  });

  const generated = result?.[0]?.generated_text ?? '';
  return generated.trim();
}

export async function disposeModel() {
  if (generator) {
    await generator.dispose();
    generator = null;
  }

  loadingPromise = null;
}
