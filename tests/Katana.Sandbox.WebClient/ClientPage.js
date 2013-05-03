
(function ($) {

    var authorizeUri = 'http://localhost:18001/Katana.Sandbox.WebServer/Authorize';
    var returnUri = 'http://localhost:18002/Katana.Sandbox.WebClient/ClientPageSignIn.html';

    $('#Authorize').click(function () {
        var uri = addQueryString(authorizeUri, {
            'client_id': '7890ab',
            'redirect_uri': returnUri,
            'state': 'my-nonce',
            'scope': 'bio notes',
            'response_type': 'code',
        });

        var authorizeWindow = window.open(uri, 'Authorize', 'width=640,height=480');

        window.setTimeout(monitorPopup, 250);

        function monitorPopup() {
            if (authorizeWindow.closed) {
                return;
            }

            var location = authorizeWindow.location;
            if (location.uri.indexOf(returnUri) == 0) {
                authorizeWindow.close();
                return;
            }

            window.setTimeout(monitorPopup, 250);
        }
    });

    function addQueryString(uri, parameters) {
        var delimiter = (uri.indexOf('?') == -1) ? '?' : '&';
        for (var parameterName in parameters) {
            var parameterValue = parameters[parameterName];
            uri += delimiter + encodeURIComponent(parameterName) + '=' + encodeURIComponent(parameterValue);
            delimiter = '&';
        }
        return uri;
    }
})(jQuery);
