$("#form-perfil").submit(function (e) {
    e.preventDefault();
    var formData = new FormData(this);

    $.ajax({
        url: "/Perfil/Editar",
        type: "POST",
        data: formData,
        success: function (mensagem) {
            $('#mensagem-perfil').text('').removeClass();
            if (mensagem.trim() === "Editado com Sucesso") {
                $('#btn-fechar-perfil').click();
                location.reload();
            } else {
                $('#mensagem-perfil').addClass('text-danger').text(mensagem);
            }
        },
        cache: false,
        contentType: false,
        processData: false
    });
});
