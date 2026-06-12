// Módulo fino de acesso ao IndexedDB (único JS do app — frontend-standards §6).
// Consumido exclusivamente por Interop/IndexedDbInterop.cs.

let dbPromise = null;

function upgrade(db, oldVersion) {
  if (oldVersion < 1) {
    db.createObjectStore('shoppingSessions', { keyPath: 'id' });
    const cartItems = db.createObjectStore('cartItems', { keyPath: 'id' });
    cartItems.createIndex('sessionId', 'sessionId', { unique: false });
    db.createObjectStore('products', { keyPath: 'id' });
    const priceRecords = db.createObjectStore('priceRecords', { keyPath: 'id' });
    priceRecords.createIndex('productId', 'productId', { unique: false });
  }
  if (oldVersion < 2) {
    db.createObjectStore('pendingShares', { keyPath: 'id' });
  }
  if (oldVersion < 3) {
    db.createObjectStore('shoppingPatterns', { keyPath: 'id' });
  }
  if (oldVersion < 4) {
    db.createObjectStore('stores', { keyPath: 'id' });
  }
}

export function open(name, version) {
  if (!dbPromise) {
    dbPromise = new Promise((resolve, reject) => {
      const request = indexedDB.open(name, version);
      request.onupgradeneeded = (e) => upgrade(request.result, e.oldVersion);
      request.onsuccess = () => resolve(request.result);
      request.onerror = () => reject(request.error);
    });
  }
  return dbPromise.then(() => undefined);
}

function withStore(mode, storeName, action) {
  return dbPromise.then(
    (db) =>
      new Promise((resolve, reject) => {
        const tx = db.transaction(storeName, mode);
        const result = action(tx.objectStore(storeName));
        tx.oncomplete = () => resolve(result.result ?? result.value);
        tx.onerror = () => reject(tx.error);
        tx.onabort = () => reject(tx.error);
      })
  );
}

export function put(storeName, item) {
  return withStore('readwrite', storeName, (store) => store.put(item));
}

export function get(storeName, key) {
  return withStore('readonly', storeName, (store) => store.get(key));
}

export function getAll(storeName) {
  return withStore('readonly', storeName, (store) => store.getAll());
}

export function getAllByIndex(storeName, indexName, value) {
  return withStore('readonly', storeName, (store) =>
    store.index(indexName).getAll(value)
  );
}

export function remove(storeName, key) {
  return withStore('readwrite', storeName, (store) => store.delete(key));
}

export function removeAllByIndex(storeName, indexName, value) {
  return withStore('readwrite', storeName, (store) => {
    const index = store.index(indexName);
    const request = index.openCursor(IDBKeyRange.only(value));
    request.onsuccess = () => {
      const cursor = request.result;
      if (cursor) {
        cursor.delete();
        cursor.continue();
      }
    };
    return { value: undefined };
  });
}

export function clear(storeName) {
  return withStore('readwrite', storeName, (store) => store.clear());
}
