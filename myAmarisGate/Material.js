function createNewCategory(data) {
    if (data.status == "error") {
        showError("", data.message);
    }
    else {
        showInfo(data.message);
        $('#modal').modal('hide');
    }
}

$(document).ready(function () {
    //customized delete functionality for Materials
    $('.btn.btn-danger.deleteMaterial').click(function () {
        var materialId = $(this).attr('data-val');

        $.ajax({
            url: $(this).data('url'),
            type: 'GET',
            cache: false,
            success: function (result) {
                var modal = $('#deleteMaterialDiv').html(result).find('.modal');
                modal.find("#deleteConfirm").attr('data-val', materialId);
                modal.modal({
                    show: true
                });
            }
        });
        return false; //prevent browser defualt behavior
    });

    //show notification when submiting a new material
    $("#sendCreatedMaterial").click(function () {
        infoNotification("Redirecting to Materials List");
    });

    //Show Create Material popup
    $("a.btn.btn-default.createMaterial").click(function () {
        $('#createMaterialModal').modal('show');
        return false; //prevent browser default behavior
    });

    //Create new material after clicking create button in the Popup
    $("#createNewMaterialBtn").click(function () {
        var materialName = $("#materialName").val();
        var materialCode = $("#materialCode").val();
        var requiresSetup = document.getElementById('requiresSetupCheckBox').checked;;
        var isDisabled = document.getElementById('isDisabledCheckBox').checked;
        var materialDetails = $("#materialDetails").val();
        var CategoryId = $("#CategoryId").val();
        if (isEmptyOrNull(materialName)) {
            showError("", "Material must have a name!");
        }
        else {
            if (isEmptyOrNull(materialCode)) { showError("", "Material must have a Code!"); }
            else {
                infoNotification("Creating Material, please wait !");
                $.ajax({
                    url: $(this).data('url'),
                    type: 'POST',
                    contentType: 'application/json; charset=utf-8',
                    traditional: true,
                    data: JSON.stringify({ materialName: materialName, materialCode: materialCode, requiresSetup: requiresSetup, isDisabled: isDisabled, materialDetails: materialDetails, CategoryId: CategoryId }),
                    success: function (data) {
                        showInfo("Material created successfuly!");
                        $(".modal").addClass('hide');
                        location.reload();
                    },
                    error: function (jqXHR, textStatus, errorThrown) {
                        showError(jqXHR.status, errorThrown);
                    }
                });
            }

        }
    });
});