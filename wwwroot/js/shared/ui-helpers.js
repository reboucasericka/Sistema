/* Utilidades compartilhadas de UI/HTTP */
(function (global) {
    "use strict";

    function getCsrfToken() {
        // Busca padrão: hidden input gerado por @Html.AntiForgeryToken()
        var input = document.querySelector('input[name="__RequestVerificationToken"]');
        if (input && input.value) return input.value;
        // Alternativa: meta tag (se adotarmos no layout)
        var meta = document.querySelector('meta[name="csrf-token"]');
        if (meta && meta.content) return meta.content;
        return null;
    }

    function withCsrf(headers) {
        var token = getCsrfToken();
        if (!headers) headers = {};
        if (token) headers["RequestVerificationToken"] = token;
        return headers;
    }

    global.UIHelpers = {
        getCsrfToken: getCsrfToken,
        withCsrf: withCsrf
    };
})(window);


