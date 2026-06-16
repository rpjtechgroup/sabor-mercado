let scriptPromise;

function loadScript() {
    if (window.google?.accounts?.id) {
        return Promise.resolve();
    }

    if (!scriptPromise) {
        scriptPromise = new Promise((resolve, reject) => {
            const existing = document.querySelector('script[data-google-gsi]');
            if (existing) {
                existing.addEventListener('load', () => resolve());
                existing.addEventListener('error', () => reject(new Error('GIS load failed')));
                return;
            }

            const script = document.createElement('script');
            script.src = 'https://accounts.google.com/gsi/client';
            script.async = true;
            script.defer = true;
            script.dataset.googleGsi = 'true';
            script.onload = () => resolve();
            script.onerror = () => reject(new Error('GIS load failed'));
            document.head.appendChild(script);
        });
    }

    return scriptPromise;
}

export async function renderButton(container, clientId, dotNetRef) {
    if (!clientId) {
        return false;
    }

    await loadScript();

    const element = typeof container === 'string'
        ? document.getElementById(container)
        : container;

    if (!element) {
        return false;
    }

    element.innerHTML = '';

    google.accounts.id.initialize({
        client_id: clientId,
        callback: (response) => {
            if (response?.credential) {
                dotNetRef.invokeMethodAsync('OnGoogleCredential', response.credential);
            }
        },
        auto_select: false,
        cancel_on_tap_outside: true,
    });

    const paintButton = () => {
        const width = Math.floor(
            element.getBoundingClientRect().width
            || element.parentElement?.clientWidth
            || 320);

        google.accounts.id.renderButton(element, {
            type: 'standard',
            theme: 'outline',
            size: 'large',
            text: 'signin_with',
            shape: 'pill',
            logo_alignment: 'left',
            width: Math.max(240, width),
        });
    };

    requestAnimationFrame(() => requestAnimationFrame(paintButton));

    return true;
}

export function getClientEnvironment() {
    return {
        userAgent: navigator.userAgent,
        language: navigator.language,
        viewportWidth: window.innerWidth,
        viewportHeight: window.innerHeight,
        online: navigator.onLine,
        serviceWorkerSupported: 'serviceWorker' in navigator,
    };
}
