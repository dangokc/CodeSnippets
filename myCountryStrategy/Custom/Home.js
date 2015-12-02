function LoadPartialView() {
    $.ajax({
        url: $('#co_list').data('url');,
        method: 'POST',
        success: function (data) {
            $('#tableListId').html(data);
            loadAllXeditable();
        }
    })
};

function LoadAFMView() {
    var mUrl = $('#col-xs-1').data('url');
    $.ajax({
        url: mUrl,
        method: 'POST',
        success: function (data) {
            $('#tableListId').html(data);
            loadAllXeditable();
        }
    })
};

function LoadDirectorView() {
    $.ajax({
        url: $('#col-xs-1').data('url'),
        method: 'POST',
        success: function (data) {
            $('#tableListId').html(data);
            loadAllXeditable();
        }
    })
};

function LoadCorpDevView() {
    var mUrl = $('#col-xs-1').data('url');
    $.ajax({
        url: mUrl,
        method: 'POST',
        success: function (data) {
            $('#tableListId').html(data);
            loadAllXeditable();
        }
    })
};
