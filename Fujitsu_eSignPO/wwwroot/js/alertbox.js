
function alertPopup(Message) {

    
    return Swal.fire({
        position: 'middle',
        icon: 'success',
        title: 'Success',
        text: Message,
        showConfirmButton: true,
        allowOutsideClick: false
    });

}

function alertErrorPopup(Message) {
    return Swal.fire({
        icon: 'error',
        title: 'Something Wrong',
        text: Message,
        footer: 'Please Contact Admin'
    });


}

function alertValidate(Message) {
    return Swal.fire({
        icon: 'warning',
        title: 'Validation Item',
        text: Message,
        
    });


}