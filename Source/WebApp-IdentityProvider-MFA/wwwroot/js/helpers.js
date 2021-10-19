﻿// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

function Base64UrlToUint8Array(str) {
    escaped = str.replace(/-/g, "+").replace(/_/g, "/")
    return Uint8Array.from(atob(escaped), c => c.charCodeAt(0));
}

function ToBase64Url(object) {
    if (object instanceof Uint8Array) {
        var str = "";
        var len = object.byteLength;

        for (var i = 0; i < len; i++) {
            str += String.fromCharCode(object[i]);
        }
        object = window.btoa(str);
    }
    if (typeof object !== "string") {
        throw new Error("could not convert object to string");
    }
    // base64 to base64url
    // NOTE: "=" at the end of challenge is optional, strip it off here
    return object.replace(/\+/g, "-").replace(/\//g, "_").replace(/=*$/g, "");
};