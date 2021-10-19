// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

let mfaLoginForm = document.getElementById('login-mfa-fido2')
mfaLoginForm.addEventListener('submit', handleMfaSignInSubmit);

async function handleMfaSignInSubmit(event) {
    event.preventDefault();

    //TODO Animate an element so that the user knows something is happening

    fido2AssertionOptions = await parseAssertionOptions(document.getElementById('cred-options').value)

    if (fido2AssertionOptions == null) { return;}

    let credential;
    try {
        credential = await navigator.credentials.get({ publicKey: fido2AssertionOptions })
    } catch (err) {
        alert(err.message ? err.message : err);
        document.getElementById("cred-message").innerText = "Erreur lors de l'interaction avec la clé de sécurité."
        return;
    }

    try {
        document.getElementById("asserted-cred").value = await serializeAssertionResponse(credential);
        mfaLoginForm.submit();
    } catch (e) {
        alert("Impossible de vérifier l'assertion", e);
        return;
    }
}
async function parseAssertionOptions(serializedAssertion) {

    assertionOptions = JSON.parse(serializedAssertion)
    if (assertionOptions.status !== "ok") {
        alert(assertionOptions.errorMessage);
        return;
    }

    assertionOptions.challenge = Base64UrlToUint8Array(assertionOptions.challenge);
    assertionOptions.allowCredentials.forEach(listItem => listItem.id = Base64UrlToUint8Array(listItem.id));
    return assertionOptions;
}

async function serializeAssertionResponse(assertedCredential) {

    return JSON.stringify({
        id: assertedCredential.id,
        rawId: ToBase64Url(new Uint8Array(assertedCredential.rawId)),
        type: assertedCredential.type,
        extensions: assertedCredential.getClientExtensionResults(),
        response: {
            authenticatorData: ToBase64Url(new Uint8Array(assertedCredential.response.authenticatorData)),
            clientDataJson: ToBase64Url(new Uint8Array(assertedCredential.response.clientDataJSON)),
            signature: ToBase64Url(new Uint8Array(assertedCredential.response.signature))
        }
    });
}