// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

let mfaRegisterForm = document.getElementById('register-mfa-fido2')
mfaRegisterForm.addEventListener('submit', handleMfaSignInSubmit);


async function handleMfaSignInSubmit(event) {
    event.preventDefault();

    //TODO Animate something so that the user knows something is happening

    fido2AttestationOptions = await parseAttestationOptions(document.getElementById('cred-options').value)

    if (fido2AttestationOptions == null) { return; }

    // ask browser for credentials (browser will ask connected authenticators)

    let newCredential;
    try {
        newCredential = await navigator.credentials.create({ publicKey: fido2AttestationOptions })
    } catch (err) {
        alert(err.message ? err.message : err);
        document.getElementById("cred-message").innerText = "Erreur lors de l'interaction avec la clé de sécurité. Vous avez peut-être déjà enregistré cette clé avec ce compte."
        return;
    }

    try {
        document.getElementById("attested-cred").value = await serializeAttestationResponse(newCredential);
        mfaRegisterForm.submit();
    } catch (e) {
        alert("Impossible de vérifier l'assertion", e);
        return;
    }
}
async function parseAttestationOptions(serializedAttestation) {

    options = JSON.parse(serializedAttestation)
    // show options error to user
    if (options.status !== "ok") {
        alert(options.errorMessage);
        return null;
    }

    // Turn the challenge back into the accepted format of padded base64
    options.challenge = Base64UrlToUint8Array(options.challenge);
    options.user.id = Base64UrlToUint8Array(options.user.id);
    options.excludeCredentials.forEach(listItem => listItem.id = Base64UrlToUint8Array(listItem.id));

    if (options.authenticatorSelection.authenticatorAttachment === null) options.authenticatorSelection.authenticatorAttachment = undefined;

    return options;
}

async function serializeAttestationResponse(generatedCredential) {

    return JSON.stringify({
        id: generatedCredential.id,
        rawId: ToBase64Url(new Uint8Array(generatedCredential.rawId)),
        type: generatedCredential.type,
        extensions: generatedCredential.getClientExtensionResults(),
        response: {
            attestationObject: ToBase64Url(new Uint8Array(generatedCredential.response.attestationObject)),
            clientDataJson: ToBase64Url(new Uint8Array(generatedCredential.response.clientDataJSON))
        }
    });
}