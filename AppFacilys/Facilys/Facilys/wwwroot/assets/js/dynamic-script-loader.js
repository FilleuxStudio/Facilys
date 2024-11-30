window.dynamicScriptLoader = {
    load: function (urls) {
        urls.forEach(url => {
            if (!document.querySelector(`script[src="${url}"]`)) {
                const script = document.createElement("script");
                script.src = url;
                script.defer = true;
                script.type = "text/javascript";
                document.body.appendChild(script);
                console.log(`Script loaded: ${url}`);
            } else {
                console.log(`Script already loaded: ${url}`);
            }
        });
    }
};
