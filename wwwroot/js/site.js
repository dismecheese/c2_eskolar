window.benefactorProfile = {
    loadProfileInterop: function () {
        // Call the C# method via JS interop
        if (window.DotNet && window.DotNet.invokeMethodAsync) {
            window.DotNet.invokeMethodAsync('c2_eskolar', 'LoadProfileViaJsInterop');
        } else if (window.Blazor && window.Blazor.invokeMethodAsync) {
            window.Blazor.invokeMethodAsync('c2_eskolar', 'LoadProfileViaJsInterop');
        } else {
            console.warn('Blazor JS interop not available.');
        }
    }
};