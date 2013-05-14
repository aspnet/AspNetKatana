
(function ($) {
    $('.collapsable').hide();
    $('.frame:first-child .collapsable').show();
    $('.page').hide();
    $('#stackpage').show();

    $('.frame').click(function () {
        $(this).children('.source').children('.collapsable').toggle('fast');
    });
    
    $('#headers li').click(function () {

        var unselected = $('#headers .selected').removeClass('selected').attr('id');        
        var selected = $(this).addClass('selected').attr('id');
        
        $('#' + unselected + 'page').hide();
        $('#' + selected + 'page').show('fast');
    });
    
})(jQuery);
