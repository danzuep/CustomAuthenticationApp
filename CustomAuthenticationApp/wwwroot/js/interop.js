﻿window.interop = {
    getElementByName: (name) => {
        var elements = document.getElementsByName(name);
        if (elements.length) {
            return elements[0].value;
        } else {
            return "";
        }
    },
    SetFocus: (id) => { document.getElementById(id).focus(); },
    submitForm: (path, fields) => {
        const form = document.createElement('form');
        form.method = 'post';
        form.action = path;

        for (const key in fields) {
            if (fields.hasOwnProperty(key)) {
                const hiddenField = document.createElement('input');
                hiddenField.type = 'hidden';
                hiddenField.name = key;
                hiddenField.value = fields[key];
                form.appendChild(hiddenField);
            }
        }

        document.body.appendChild(form);
        form.submit();
    }
};

window.triggerFileDownload = (fileName, url) => {
    const anchorElement = document.createElement('a');
    anchorElement.href = url;
    anchorElement.download = fileName ?? '';
    anchorElement.click();
    anchorElement.remove();
};

window.downloadFileFromStream = async (fileName, contentStreamReference) => {
    const arrayBuffer = await contentStreamReference.arrayBuffer();
    const blob = new Blob([arrayBuffer]);
    const url = URL.createObjectURL(blob);
    const anchorElement = document.createElement('a');
    anchorElement.href = url;
    anchorElement.download = fileName ?? '';
    anchorElement.click();
    anchorElement.remove();
    URL.revokeObjectURL(url);
};

window.cookieStorage = {
    get: (name) => {
        const cookieMatch = document.cookie.match(`(^|;)\\s*${name}\\s*=\\s*([^;]+)`);
        if (cookieMatch) {
            return decodeURIComponent(cookieMatch[2]);
        }
        return "";
    },
    set: (name, value, absoluteExpiryMs, relativeExpiryMs) => {
        const expiryTime = absoluteExpiryMs ? absoluteExpiryMs :
            new Date().getTime() + (relativeExpiryMs || 0);
        const expiryDate = new Date(expiryTime).toUTCString();
        const encodedValue = encodeURIComponent(value);
        document.cookie = `${name}=${encodedValue};expires=${expiryDate};samesite=lax;path=/`;
    },
    delete: (name) => {
        document.cookie = `${name}=;expires=Thu, 01 Jan 1970 00:00:00 UTC;path=/`;
    },
    clear: () => {
        const cookies = document.cookie.split(";");
        for (let i = 0; i < cookies.length; i++) {
            const cookie = cookies[i];
            const eqPos = cookie.indexOf("=");
            const name = eqPos > -1 ? cookie.substring(0, eqPos) : cookie;
            document.cookie = `${name}=;expires=Thu, 01 Jan 1970 00:00:00 UTC;path=/`;
        }
    },
    eventListener: (componentRef) => {
        const handleCookieChange = async (event) => {
            const cookieKey = event.detail.name;
            await componentRef.invokeMethodAsync("OnStorageUpdated", cookieKey);
        };
        document.addEventListener("cookiechange", handleCookieChange);
        document.addEventListener("cookieupdate", handleCookieChange);
    }
};

window.storeLocally = {
    get: (key) => {
        return window.localStorage.getItem(key);
    },
    set: (key, value, absoluteExpiryMs, relativeExpiryMs) => {
        const expiryTime = absoluteExpiryMs ? absoluteExpiryMs :
            new Date().getTime() + (relativeExpiryMs || 0);
        const cookie = { value: value, expires: expiryTime };
        const encodedValue = JSON.stringify(cookie);
        window.localStorage.setItem(key, encodedValue);
    },
    delete: (key) => {
        window.localStorage.removeItem(key);
    },
    clear: () => {
        window.localStorage.clear();
    },
    eventListener: (componentRef) => {
        window.addEventListener("storage", async e => {
            await componentRef.invokeMethodAsync("OnStorageUpdated", e.key);
        });
    }
};

window.dimensions = {
    throttleResizeHandlerId: -1,
    get: function () {
        return {
            width: window.innerWidth,
            height: window.innerHeight
        };
    },
    getElement: function (elementId) {
        // Get element by ID, e.g. "nav", "footer"
        const element = document.querySelector(elementId);
        // Return null if element ID is not found
        return element ?
            {
                width: element.offsetWidth,
                height: element.offsetHeight
            } : null;
    },
    getOffset: function (headerId, footerId, sideId) {
        const header = document.querySelector(headerId);
        const footer = document.querySelector(footerId);
        const side = document.querySelector(sideId);
        const win = this.getWindow();
        var pageWidth = win.width;
        var pageHeight = win.height;
        if (header && footer) {
            pageHeight -= footer.offsetHeight + header.offsetHeight;
        } else if (header) {
            pageHeight -= header.offsetHeight;
        } else if (footer) {
            pageHeight -= footer.offsetHeight;
        }
        if (side) {
            pageWidth -= side.offsetWidth;
        }
        return {
            width: pageWidth,
            height: pageHeight
        };
    },
    eventListener: function (componentRef, reportRate) {
        const throttledResize = async () => {
            const handleResize = async () => {
                await componentRef.invokeMethodAsync("OnWindowDimensionsUpdated", this.get());
            };
            clearTimeout(this.throttleResizeHandlerId);
            this.throttleResizeHandlerId = window.setTimeout(handleResize, reportRate);
        };
        window.addEventListener("resize", throttledResize);
    },
    cancelListener: function () {
        clearTimeout(this.throttleResizeHandlerId);
        window.removeEventListener("resize", this.eventListener);
    },
    matchMedia: function (query) {
        var m = window.matchMedia(query).matches;
        return m;
    },
};