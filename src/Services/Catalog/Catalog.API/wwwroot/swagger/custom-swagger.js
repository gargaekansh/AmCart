window.onload = function () {
    const ui = SwaggerUIBundle({
        url: "./swagger/v1/swagger.json",
        dom_id: "#swagger-ui",
        deepLinking: true,
        oauth2RedirectUrl: window.location.origin + "/swagger/oauth2-redirect.html",
        presets: [SwaggerUIBundle.presets.apis, SwaggerUIStandalonePreset],
        plugins: [SwaggerUIBundle.plugins.DownloadUrl],
        requestInterceptor: async function (req) {
            if (req.url.includes("/connect/authorize")) {
                // 🔹 Add PKCE code challenge parameters
                const codeVerifier = generateCodeVerifier();
                const codeChallenge = await generateCodeChallenge(codeVerifier);
                sessionStorage.setItem("pkce_code_verifier", codeVerifier);

                const url = new URL(req.url);
                url.searchParams.set("code_challenge", codeChallenge);
                url.searchParams.set("code_challenge_method", "S256");

                req.url = url.toString();
            }
            return req;
        },
    });

    window.ui = ui;
};

// 🔹 Function to generate PKCE code verifier (random string)
function generateCodeVerifier() {
    const array = new Uint8Array(32);
    window.crypto.getRandomValues(array);
    return btoa(String.fromCharCode.apply(null, array))
        .replace(/\+/g, "-")
        .replace(/\//g, "_")
        .replace(/=/g, "");
}

// 🔹 Function to generate PKCE code challenge (hashed version of verifier)
async function generateCodeChallenge(verifier) {
    const encoder = new TextEncoder();
    const data = encoder.encode(verifier);
    const digest = await window.crypto.subtle.digest("SHA-256", data);
    return btoa(String.fromCharCode(...new Uint8Array(digest)))
        .replace(/\+/g, "-")
        .replace(/\//g, "_")
        .replace(/=/g, "");
}
