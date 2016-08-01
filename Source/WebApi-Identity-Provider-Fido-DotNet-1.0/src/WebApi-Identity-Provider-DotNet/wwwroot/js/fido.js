"use strict";

if (typeof Fido == "undefined") {
    var Fido = {

        isAvailable: typeof window.msCredentials != "undefined",

        isEnable: localStorage.getItem("fido_isEnable") ? true : false,

        makeCredential: function (signInId, userId) {
            try {
                var accountInfo = {
                    rpDisplayName: 'FranceConnect',
                    userDisplayName: userId
                };

                var cryptoParameters = [
                    {
                        type: 'FIDO_2_0',
                        algorithm: 'RSASSA-PKCS1-v1_5'
                    }
                ];

                window.msCredentials.makeCredential(accountInfo, cryptoParameters)
                    .then(function (cred) {
                        var publicKeyHash = CryptoJS.SHA256(cred.publicKey);
                        var publicKeyHint = publicKeyHash.toString(CryptoJS.enc.Base64);
                        var xhr = new XMLHttpRequest();
                        xhr.onreadystatechange = function () {
                            if (xhr.readyState == 4 && xhr.status == 200)
                                if (xhr.responseText == "true") {
                                    localStorage.setItem("fido_isEnable", true);
                                    localStorage.setItem("fido_publicKeyHint", publicKeyHint);
                                    localStorage.setItem("fido_userId", userId);
                                    localStorage.setItem("fido_credentialId", cred.id);
                                    alert("Windows Hello est configuré pour FranceConnect !");
                                    window.location = "/ui/finalize_credential?id=" + signInId;
                                } else {
                                    alert("Impossible de configurer Windows Hello pour FranceConnect.");
                                }
                        }
                        xhr.open("POST", "/ui/register_credential", true);
                        xhr.setRequestHeader("Content-type", "application/x-www-form-urlencoded");
                        var str = "DeviceName=" + navigator.userAgent + "&UserId=" + userId + "&PublicKey=" + encodeURIComponent(cred.publicKey);
                        xhr.send(str);
                    }).catch(function (reason) {
                        if (reason.message === 'NotSupportedError') {
                            alert("Vous devez activer Windows Hello sur votre appareil pour l'utiliser sur ce site.");
                        }
                        console.log("FIDO error: " + reason.message);
                    });
            } catch (ex) {
                console.log("FIDO error: " + ex);
            }
        },

        getAssertion: function (signInId) {
            try {
                var userId = localStorage.getItem("fido_userId");
                var publicKeyHint = localStorage.getItem("fido_publicKeyHint");
                var credentialId = localStorage.getItem("fido_credentialId");

                var xhrChallenge = new XMLHttpRequest();
                xhrChallenge.onreadystatechange = function () {
                    if (xhrChallenge.readyState == 4 && xhrChallenge.status == 200) {
                        var challenge = xhrChallenge.responseText;
                        var filters = {
                            accept:
                                [
                                    {
                                        type: 'FIDO_2_0',
                                        id: credentialId
                                    }
                                ]
                        };
                        window.msCredentials.getAssertion(challenge, filters)
                            .then(function (attestation) {
                                var xhrSignature = new XMLHttpRequest();
                                xhrSignature.onreadystatechange = function () {
                                    if (xhrSignature.readyState == 4 && xhrSignature.status == 200)
                                        if (xhrSignature.responseText == "true") {
                                            window.location = "/ui/finalize_credential?id=" + signInId;
                                        } else {
                                            alert("Tentative de connexion incorrecte.");
                                        }
                                }
                                xhrSignature.open("POST", "/ui/submit_response", true);
                                xhrSignature.setRequestHeader("Content-type", "application/x-www-form-urlencoded");
                                var signatureParams = "UserId=" + encodeURIComponent(userId)
                                    + "&Signature=" + encodeURIComponent(attestation.signature.signature)
                                    + "&PublicKeyHint=" + encodeURIComponent(publicKeyHint)
                                    + "&ClientData=" + encodeURIComponent(attestation.signature.clientData)
                                    + "&AuthnrData=" + encodeURIComponent(attestation.signature.authnrData);
                                xhrSignature.send(signatureParams);
                            });
                    }
                }
                xhrChallenge.open("POST", "/ui/request_challenge", true);
                xhrChallenge.setRequestHeader("Content-type", "application/x-www-form-urlencoded");
                var challengeParams = "DeviceName=" + navigator.userAgent
                    + "&UserId=" + encodeURIComponent(userId)
                    + "&PublicKeyHint=" + encodeURIComponent(publicKeyHint);
                xhrChallenge.send(challengeParams);
            } catch (ex) {
                console.log("FIDO error: " + ex);
            }
        },

        removeCredential: function (callback) {
            try {
                var userId = localStorage.getItem("fido_userId");
                var publicKeyHint = localStorage.getItem("fido_publicKeyHint");

                var xhrDelete = new XMLHttpRequest();
                xhrDelete.onreadystatechange = function () {
                    if (xhrDelete.readyState == 4 && xhrDelete.status == 200)
                        if (xhrDelete.responseText == "true") {
                            localStorage.removeItem("fido_isEnable");
                            localStorage.removeItem("fido_publicKeyHint");
                            localStorage.removeItem("fido_userId");
                            localStorage.removeItem("fido_credentialId");
                            callback();
                            alert("Vos informations d'ientification ont été supprimé.");
                        } else {
                            alert("Une erreur est survenue lors de la suppression de vos informations d'identification. Veuillez réessayer ultérieurement.");
                        }
                }
                xhrDelete.open("POST", "/ui/remove_credential", true);
                xhrDelete.setRequestHeader("Content-type", "application/x-www-form-urlencoded");
                var deleteParams = "UserId=" + encodeURIComponent(userId)
                    + "&PublicKeyHint=" + encodeURIComponent(publicKeyHint);
                xhrDelete.send(deleteParams);
            } catch (ex) {
                console.log("FIDO error: " + ex);
            }
        },

        later: function (signInId) {
            window.location = "/ui/finalize_credential?id=" + signInId;
        }
    }
}