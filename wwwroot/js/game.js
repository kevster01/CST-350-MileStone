
function updateCell(row, cell) {
    $.ajax({
        type: "POST",
        url: "/Game/UpdateCell",
        data: {
            row: row,
            cell: cell
        },
        success: function (data) {
            $("#game-board").html(data);
        },
        error: function (xhr, status, error) {
            console.error("Error:", error);
        }
    });
}