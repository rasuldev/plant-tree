function initEdit() {
    $('h2.delete').on('click', function () {
        if (confirm('Вы уверены, что хотите удалить данное изображение?')) {
            deleteImage($(this).attr('data-id'));
        }
    });
}

function deleteImage(id) {
    $.ajax({
        url: '/api/images/' + id,
        type: 'DELETE',
        success: function (image) {
            var h2 = $('h2.delete[data-id="' + id + '"]');
            h2.closest('div.project-image').remove();
            if (h2.hasClass('main-image')) {
                $('input#ImageId').val("");
            }
        },
        error: handleException
    });
}

function handleException(request, message, error) {
    console.log(message, error);
    alert('При удалении изображения произошла ошибка. Попробуйте еще раз.');
}