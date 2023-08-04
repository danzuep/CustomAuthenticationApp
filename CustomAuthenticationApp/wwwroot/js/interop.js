window.interop = {
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
        document.cookie = `${name}=${encodedValue};expires=${expiryDate};path=/`;
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
    }
};

window.getDimensions = function () {
    return {
        width: window.innerWidth,
        height: window.innerHeight
    };
};
