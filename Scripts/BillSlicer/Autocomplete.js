var BillSlicer = (function () {

    this.autocomplete = function (settings) {

        if (!settings.hasOwnProperty('minLen'))
            settings.linLen = 3;

        var acSettings = {
            source: function (request, callback) {
                $.get({
                    url: settings.url,
                    data: {
                        SearchTerm: request.term
                    },
                    success: function (data) {
                        callback(data.data);
                    },
                    error: function () {
                        callback([]);
                    },

                })
            },
            minLength: settings.minLen,
        }

        for (var event in settings.listeners) {
            acSettings[event] = settings.listeners[event];
        }

        var ac = $(settings.selector).autocomplete(acSettings);

    }

    return this;

}).call(BillSlicer || {})