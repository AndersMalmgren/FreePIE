(function () {
    $("a.ga-track").click(function () {
        var anchor = this;
        try {
            _gaq.push(["_trackEvent", "External links", anchor.href]);
        } catch (err) {
            console.log(err);
        }
        
        if ($(this).attr("target") !== "_blank") {
            setTimeout(function () {
                document.location.href = anchor.href;
            }, 100);
            return false;
        }

        return true;
    });
})();

