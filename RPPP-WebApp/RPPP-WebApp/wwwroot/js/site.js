tableLangSettings = {
  search: "Pretraga",
  info: "_START_ - _END_ od ukupno _TOTAL_ zapisa",
  lengthMenu: "Prikaži _MENU_ zapisa",
  pagebox: {
    first: "Prva",
    previous: "Prethodna",
    next: "Sljedeća",
    last: "Zadnja"
  },
  emptyTable: "Nema podataka za prikaz",
  infoEmpty: "Nema podataka za prikaz",
  infoFiltered: "(filtrirano od ukupno _MAX_ zapisa)",
  zeroRecords: "Ne postoje traženi podaci"
};

function clearOldMessage() {
  $("#tempmessage").siblings().remove();
  $("#tempmessage").removeClass("alert-success");
  $("#tempmessage").removeClass("alert-danger");
  $("#tempmessage").html('');
}

$(function () {
  $(document).on('click', '.delete', function (event) {
    if (!confirm("Obrisati zapis?")) {
      event.preventDefault();
    }
  });
});
