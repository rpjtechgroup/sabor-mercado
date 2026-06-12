
function toMessage(err) {
  if (err.code === 1) {
    return 'Permissão de localização negada. Libere o acesso nas configurações do navegador.';
  }
  if (err.code === 2) {
    return 'Localização indisponível neste dispositivo.';
  }
  if (err.code === 3) {
    return 'Não foi possível obter a localização a tempo. Ative o GPS, tente novamente ou deixe em branco — é opcional.';
  }
  return err.message || 'Não foi possível obter a localização.';
}

function requestPosition(options) {
  return new Promise((resolve, reject) => {
    navigator.geolocation.getCurrentPosition(resolve, reject, options);
  });
}

export async function getCurrentPosition() {
  if (!window.isSecureContext) {
    throw new Error(
      'Geolocalização exige conexão segura (HTTPS). Use o endereço https do site ou informe as coordenadas manualmente.'
    );
  }

  if (!navigator.geolocation) {
    throw new Error('Geolocalização não suportada neste navegador.');
  }

  const attempts = [
    { enableHighAccuracy: false, timeout: 20000, maximumAge: 300000 },
    { enableHighAccuracy: true, timeout: 45000, maximumAge: 0 },
  ];

  let lastError = null;

  for (const options of attempts) {
    try {
      const pos = await requestPosition(options);
      return {
        latitude: pos.coords.latitude,
        longitude: pos.coords.longitude,
      };
    } catch (err) {
      lastError = err;
      if (err.code === 1 || err.code === 2) {
        break;
      }
    }
  }

  throw new Error(toMessage(lastError ?? { code: 3, message: '' }));
}
