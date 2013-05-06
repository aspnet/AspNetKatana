
(function ($) {
    var query = {};
    window.location.search.substring(1).split('&').forEach(function (term) {
        var index = term.indexOf('=');
        if (index != -1) {
            query[decodeURIComponent(term.substring(0, index))] = decodeURIComponent(term.substring(index + 1));
        }
    });
    window.opener.$('#Authorize').trigger('signin.' + query['state'], { 'query': query });
    window.close();
})(jQuery);
