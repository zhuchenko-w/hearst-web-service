$(function () {
    loading(true);
    action();
});

function createReport() {
    getAjax(
        url,
        function (fileId) {
            window.open(downloadFileUrl + fileId, "_blank");
        }
    );
}

function commonRequest() {
    getAjax(url);
}

function unknownRequest() {
    loading(false);
    $(".unknown-request-message").slideDown();
}

function getAjax(url, successFunc) {
    $.ajax({
        url: url,
        type: "GET",
        contentType: "application/json",
        success: function (result) {
            if (successFunc) {
                successFunc(result);
            }
            $(".success-message").slideDown();
        },
        error: function (xhr, textStatus, error) {
            switch (xhr.status) {
                case 400:
                    $(".invalid-parameter-message").slideDown();
                    break;
                default:
                    $(".error-message").slideDown();
                    break;
            }
        },
        complete: function () {
            loading(false);
        }
    });
}

function loading(isLoading) {
    if (isLoading) {
        $(".loader").slideDown();
    } else {
        $(".loader").slideUp();
    }
}