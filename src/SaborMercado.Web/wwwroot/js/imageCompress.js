const MAX_EDGE = 1024;
const JPEG_QUALITY = 0.85;

export async function compressImageBytes(bytes) {
  const blob = new Blob([bytes]);
  const bitmap = await createImageBitmap(blob);
  const scale = Math.min(1, MAX_EDGE / Math.max(bitmap.width, bitmap.height));
  const width = Math.max(1, Math.round(bitmap.width * scale));
  const height = Math.max(1, Math.round(bitmap.height * scale));

  const canvas = new OffscreenCanvas(width, height);
  const ctx = canvas.getContext('2d');
  ctx.drawImage(bitmap, 0, 0, width, height);
  bitmap.close();

  const jpegBlob = await canvas.convertToBlob({ type: 'image/jpeg', quality: JPEG_QUALITY });
  const buffer = await jpegBlob.arrayBuffer();
  return Array.from(new Uint8Array(buffer));
}
