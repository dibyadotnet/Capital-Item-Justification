let equipments = [];
$(document).ready(function () {
    debugger;
    var equipmentJson = $("#EquipmentJson").val();
    if (equipmentJson && equipmentJson !== "") {
        equipments = JSON.parse(equipmentJson);
    }
});
$("#ddlItemType").on("change", function () {

    if ($("#ddlItemType option:selected").text() === "Medical") {
        $("#divCommiteeComments").show();
    } else {
        $("#divCommiteeComments").hide();
        $("#txtCommiteeComments").val("");
    }

});
$("#btnEquipmentSave").on("click", function () {
    addEquipment();
});
function loadEquipmentTable() {

    $("#tblEquipment tbody").empty();
    $.each(equipments, function (i, item) {

        $("#tblEquipment tbody").append(`
            <tr>
                <td>${i + 1}</td>
                <td>${item.EquipmentName}</td>
                <td>${item.EquipmentQty}</td>
                <td>${item.Make}</td>
                <td>${item.Model}</td>
                <td>${item.EquipmentCost}</td>
                 <td>${item.PreferenceOrder}</td>
                <td>
                    <button type="button" onclick="EditEquipment(${i})" class="btn btn-warning btn-sm me-1"> <i class="bi bi-pencil-square"></i></button>
                    <button type="button" onclick="DeleteEquipment(${i})" class="btn btn-danger btn-sm"><i class="bi bi-trash"></i></button>
                </td>
            </tr>
        `);

    });
    // Store the latest equipment list in the hidden field
    $("#EquipmentJson").val(JSON.stringify(equipments));
}
function addEquipment() {
    debugger;
    var equipment = {
        EquipmentId: parseInt($("#EquipmentId").val())||0,
        EquipmentName: $("#EquipmentName").val(),
        EquipmentQty: parseInt($("#EquipmentQty").val())||0,
        Make: $("#Make").val(),
        Model: $("#Model").val(),
        EquipmentCost: parseFloat($("#EquipmentCost").val())||0,
        PreferenceOrder: parseInt($("#PreferenceOrder").val())||0
    };
    var index = parseInt($("#EquipmentIndex").val())||0;
    if (index == -1) {
        equipments.push(equipment);
    }
    else {
        equipments[index] = equipment;
    }

    $("#EquipmentJson").val(JSON.stringify(equipments));

    $("#EquipmentIndex").val("-1");
    $("#btnEquipmentSave").text("Save");

  
    loadEquipmentTable();

    calculateTotalEquipmentCost();

    clearEquipment();

    bootstrap.Modal.getInstance(
        document.getElementById("equipmentModal")
    ).hide();

}
function EditEquipment(index) {
    debugger;
    var item = equipments[index];

    $("#EquipmentIndex").val(index);
    $("#EquipmentId").val(item.EquipmentId);

    $("#EquipmentName").val(item.EquipmentName);

    $("#EquipmentQty").val(item.EquipmentQty);

    $("#Make").val(item.Make);

    $("#Model").val(item.Model);

    $("#EquipmentCost").val(item.EquipmentCost);
    $("#PreferenceOrder").val(item.PreferenceOrder) 

    $("#btnEquipmentSave").text("Update");

    var modal = new bootstrap.Modal(
        document.getElementById("equipmentModal")
    );

    modal.show();

}
function DeleteEquipment(index) {
    debugger;
    if (!confirm("Are you sure you want to delete this equipment?"))
        return;

    equipments.splice(index, 1);

    loadEquipmentTable();
    calculateTotalEquipmentCost();

}

function clearEquipment() {

    $("#EquipmentName").val("");

    $("#EquipmentQty").val("");

    $("#Make").val("");

    $("#Model").val("");

    $("#EquipmentCost").val("");
    $("#PreferenceOrder").val("");

}
function calculateTotalEquipmentCost() {
    let total = 0;
    $.each(equipments, function (i, item) {
        total += parseFloat(item.EquipmentCost || 0);
    });
    var eqpCost="₹ " + total.toLocaleString('en-IN', {
        minimumFractionDigits: 2,
        maximumFractionDigits: 2
    })
    $("#TotalEstEquipmentCost").val(eqpCost);
    $("#CIJRequest_TotalEquipmentCostDisplay").val(eqpCost);
    $("#CIJRequest_TotalEquipmentCost").val(total);
}
$("#btnModalClose").on("click", function () {
    clearEquipment();
    $("#EquipmentIndex").val("-1");
});
$("#btnModalCancel").on("click", function () {
    clearEquipment();
    $("#EquipmentIndex").val("-1");
});
function deleteAttachment(attachmentId) {
    if (!confirm("Are you sure you want to delete this attachment?")) {
        return;
    }
    $.ajax({
        url: '/CIJ/DeleteAttachment',
        type: 'POST',
        data: {
            attachmentId: attachmentId
        },
        success: function (response) {
            debugger;
            if (response.success) {
                location.reload();
            } else {
                alert(response.message);
            }
        },
        error: function () {
            alert("Error while deleting attachment.");
        }
    });
}