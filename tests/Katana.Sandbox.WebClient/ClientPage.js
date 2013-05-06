
(function ($) {

    var authorizeUri = 'http://localhost:18001/Katana.Sandbox.WebServer/Authorize';
    var tokenUri = 'http://localhost:18001/Katana.Sandbox.WebServer/Token';
    var apiUri = 'http://localhost:18001/Katana.Sandbox.WebServer/api/me';
    var returnUri = 'http://localhost:18002/Katana.Sandbox.WebClient/ClientPageSignIn.html';

    $('#Authorize').click(function () {
        var nonce = 'my-nonce';
        
        var uri = addQueryString(authorizeUri, {
            'client_id': '7890ab',
            'redirect_uri': returnUri,
            'state': nonce,
            'scope': 'bio notes',
            'response_type': 'code',
        });

        $(this).bind('signin.' + nonce, function(ev, data) {
            $(this).unbind(ev);
            console.log(data);
            authorizationReturned(data.query);
        });

        function authorizationReturned(query) {
            $.post(tokenUri, {
                "grant_type": "authorization_code",
                "code": query["code"],
                "redirect_uri": returnUri,
                "client_id": "7890ab",
            }, function (data, textStatus, xhr) {
                $('#AccessToken').val(data.access_token);
            }, "json");
        }

        window.open(uri, 'Authorize', 'width=640,height=480');
    });

    $('#CallWebApi').click(function () {
        $.ajax(apiUri, {
            beforeSend: function (xhr) {
                xhr.setRequestHeader('Authorization', 'Bearer ' + $('#AccessToken').val());
            },
            dataType: 'text',
            success: function (data) {
                console.log(data);
                $('#AllClaims').text(data);
            }
        });
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
