
function getRecognitionCtor() {
  return window.SpeechRecognition || window.webkitSpeechRecognition;
}

function toMessage(err) {
  if (!err) {
    return 'Não foi possível reconhecer a fala.';
  }
  if (err.error === 'not-allowed' || err.error === 'service-not-allowed') {
    return 'Permissão de microfone negada. Libere o acesso nas configurações do navegador.';
  }
  if (err.error === 'network') {
    return 'Reconhecimento de voz precisa de internet neste dispositivo.';
  }
  if (err.error === 'no-speech') {
    return 'Nenhuma fala detectada. Tente novamente.';
  }
  if (err.error === 'audio-capture') {
    return 'Microfone indisponível neste dispositivo.';
  }
  return err.message || 'Não foi possível reconhecer a fala.';
}

let recognition = null;

export function isSupported() {
  return !!getRecognitionCtor();
}

export function start(dotNetRef, options) {
  const SpeechRecognition = getRecognitionCtor();
  if (!SpeechRecognition) {
    throw new Error('Microfone não suportado neste navegador.');
  }

  if (!window.isSecureContext) {
    throw new Error('Reconhecimento de voz exige conexão segura (HTTPS).');
  }

  stop();

  recognition = new SpeechRecognition();
  recognition.lang = options?.lang || 'pt-BR';
  recognition.continuous = options?.continuous ?? false;
  recognition.interimResults = options?.interimResults ?? true;
  recognition.maxAlternatives = 1;

  recognition.onresult = (event) => {
    const last = event.results[event.results.length - 1];
    if (!last?.[0]) {
      return;
    }
    dotNetRef.invokeMethodAsync('OnSpeechResult', last[0].transcript, last.isFinal);
  };

  recognition.onerror = (event) => {
    dotNetRef.invokeMethodAsync('OnSpeechError', toMessage(event));
  };

  recognition.onend = () => {
    dotNetRef.invokeMethodAsync('OnSpeechEnd');
  };

  recognition.start();
}

export function stop() {
  if (recognition) {
    try {
      recognition.stop();
    } catch {
      
    }
    recognition = null;
  }
}
