// EmailJS Helper for Blazor
window.emailJSHelper = {
    isInitialized: false,

    // Initialize EmailJS with public key
    init: function(publicKey) {
        try {
            if (typeof emailjs !== 'undefined' && !this.isInitialized) {
                emailjs.init({
                    publicKey: publicKey
                });
                this.isInitialized = true;
                console.log('EmailJS initialized successfully');
                return true;
            } else if (this.isInitialized) {
                console.log('EmailJS already initialized');
                return true;
            } else {
                console.error('EmailJS library not loaded');
                return false;
            }
        } catch (error) {
            console.error('Failed to initialize EmailJS:', error);
            return false;
        }
    },

    // Send email using EmailJS
    sendEmail: function(serviceId, templateId, templateParams) {
        return new Promise((resolve, reject) => {
            try {
                if (typeof emailjs === 'undefined') {
                    console.error('EmailJS not initialized');
                    resolve(false);
                    return;
                }

                console.log('Sending email with EmailJS...', {
                    serviceId: serviceId,
                    templateId: templateId,
                    templateParams: templateParams
                });

                emailjs.send(serviceId, templateId, templateParams)
                    .then(function(response) {
                        console.log('Email sent successfully:', response.status, response.text);
                        resolve(true);
                    })
                    .catch(function(error) {
                        console.error('EmailJS send failed:', error);
                        resolve(false); // Return false instead of rejecting to avoid unhandled promise rejection
                    });
            } catch (error) {
                console.error('Error in sendEmail:', error);
                resolve(false);
            }
        });
    }
};