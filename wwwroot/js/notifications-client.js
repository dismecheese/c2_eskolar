// Minimal SignalR client to receive notifications and notify Blazor
(function () {
    if (typeof window === 'undefined') return;

    // Load SignalR from CDN for reliability
    const script = document.createElement('script');
    script.src = 'https://cdn.jsdelivr.net/npm/@microsoft/signalr@7.0.7/dist/browser/signalr.min.js';
    script.crossOrigin = 'anonymous';
    script.onload = () => {
        // expose a connect function
        window.eskolarNotifications = {
            connection: null,
            _dotNetRef: null,
            registerDotNet: function (dotNetRef) {
                this._dotNetRef = dotNetRef;
            },
            unregisterDotNet: function () {
                this._dotNetRef = null;
            },
            connect: function (hubUrl) {
                if (!hubUrl) hubUrl = '/hubs/notifications';
                if (this.connection) return;
                this.connection = new signalR.HubConnectionBuilder()
                    .withUrl(hubUrl)
                    .withAutomaticReconnect()
                    .build();

                this.connection.on('ReceiveNotification', (payload) => {
                    try {
                        // Dispatch DOM event for flexibility
                        const event = new CustomEvent('eskolar:notification', { detail: payload });
                        window.dispatchEvent(event);

                        // If a DotNet reference is registered, call its method
                        if (this._dotNetRef) {
                            try {
                                this._dotNetRef.invokeMethodAsync('ReceiveNotificationFromJs', payload);
                            }
                            catch (err) {
                                console.warn('Error invoking DotNet method:', err);
                            }
                        }
                    }
                    catch (e) {
                        console.error('Error handling incoming notification:', e);
                    }
                });

                this.connection.start().catch(err => console.error('SignalR connection error:', err));
            },
            disconnect: function () {
                if (this.connection) {
                    this.connection.stop();
                    this.connection = null;
                }
                this.unregisterDotNet();
            }
        };
    };
    document.head.appendChild(script);
})();
