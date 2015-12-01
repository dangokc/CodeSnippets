function LoadPartialView() {
    var mUrl = $('#co_list').data('url');
    $.ajax({
        url: mUrl,
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
