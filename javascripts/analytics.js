(function () {
    $("a.ga-track").click(function () {
        var anchor = this;
        try {
            _gaq.push(["_trackEvent", "External links", anchor.hostname]);
        } catch (err) {
            console.log(err);
        }

        setTimeout(function () {
            document.location.href = anchor.href;
        }, 100);
        return false;
    });
})();

