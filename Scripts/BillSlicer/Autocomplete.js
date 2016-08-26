var BillSlicer = (function () {

    this.autocomplete = function (settings) {

        if (!settings.hasOwnProperty('minLen'))
            settings.linLen = 3;

        var source;
        if (typeof settings.source == 'string') {
            source = function (request, callback) {
                $.get({
                    url: settings.source,
                    data: {
                        SearchTerm: request.term
                    },
                    success: function (data) {
                        if (settings.hasOwnProperty('filter'))
                            callback(data.data.filter(settings.filter));
                        else
                            callback(data.data);
                    },
                    error: function () {
                        callback([]);
                    },

                })
            }
        }
        else if (typeof settings.source == 'function') {
            source = settings.source;
        }

        var acSettings = {
            source: source,
            minLength: settings.minLen,
        }

        for (var event in settings.listeners) {
            acSettings[event] = settings.listeners[event];
        }

        var ac = $(settings.selector).autocomplete(acSettings);

    }

    return this;

}).call(BillSlicer || {})