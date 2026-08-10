let equipments = [];
$(document).ready(function () {

    $('#BeneDeptdropdown').select2({
        placeholder: '-- Select --',
        allowClear: true,
        width: '100%'
    });
    $('#BeneLocdropdown').select2({
        placeholder: '-- Select --',
        allowClear: true,
        width: '100%'
    });

    var equipmentJson = $("#EquipmentJson").val();
    if (equipmentJson && equipmentJson !== "") {
        equipments = JSON.parse(equipmentJson);
    }

    //Committee Comments
    if ($("#ddlItemType option:selected").text() === "Medical") {
        $("#divCommiteeComments").show();
    } else {
        $("#divCommiteeComments").hide();
    }


    togglePreviousPurchase();

    changeCostCenter();

    changeBudgetProvision();

    enableBudgetType();

    changePurchasePurpose();

    changeItemType();
});

$("#CIJRequest_CostCenterId").on("change", function () {
    changeCostCenter();
});
$("#CIJRequest_BudgetTypeId").on("change", function () {
    enableBudgetType();
});
$("#CIJRequest_BudgetProvision").on("change", function () {
    changeBudgetProvision();
});
$('input[name="Justification.IsPurchasedEarlier"]').on('change', function () {
    togglePreviousPurchase();
});
$("#CIJRequest_PurchasePurposeId").on("change", function () {
    changePurchasePurpose();
});
$("#ddlItemType").on("change", function () {
    changeItemType();
});
function changeCostCenter() {
    var costCenterId = $("#CIJRequest_CostCenterId option:selected").val();
    if (costCenterId === "1" || !costCenterId) {
        //Project
        $('#CIJRequest_BudgetTypeId').prop('disabled', false);
        $('#CIJRequest_ProjectCost').prop('disabled', false);
        $('#CIJRequest_Scehcost').prop('disabled', true);
        $('#CIJRequest_Scehcost').val('');
    } else {
        $('#CIJRequest_BudgetTypeId').prop('disabled', true);
        $("#CIJRequest_BudgetTypeId").val("");
        $('#CIJRequest_ProjectCost').prop('disabled', true);
        $("#CIJRequest_ProjectCost").val("");
        $('#CIJRequest_Scehcost').prop('disabled', false);
    }
}
function togglePreviousPurchase() {
    if ($('#rbYes').is(':checked')) {
        $('#divRemarkSection').hide();
        $('#Justification_Remarks').val('');

    } else {
        $('#divRemarkSection').show();
    }
}
//function enableSCEHCost() {
//    var costCenterId = $("#CIJRequest_CostCenterId option:selected").val();
//    if (costCenterId === "2" || !costCenterId) {
//        //SCEH
//        $('#CIJRequest_Scehcost').prop('disabled', false);
//        $('#CIJRequest_ProjectCost').prop('disabled', true);
//        $("#CIJRequest_ProjectCost").val("");

//    } else {
//        $('#CIJRequest_Scehcost').prop('disabled', true);
//        $('#CIJRequest_ProjectCost').prop('disabled', false);
//        $("#CIJRequest_Scehcost").val("");

//    }
//}
function enableBudgetType() {
    var budgetTypeId = $("#CIJRequest_BudgetTypeId option:selected").val();
    if (budgetTypeId === "2" || !budgetTypeId) {
        //Partially Funded
        $('#CIJRequest_Scehcost').prop('disabled', false);
    } else {
        $('#CIJRequest_Scehcost').prop('disabled', true);
        $("#CIJRequest_Scehcost").val("");
    }
}
function changeBudgetProvision() {
    var budgetProvision = $("#CIJRequest_BudgetProvision option:selected").text();
    if (budgetProvision === "Yes" || budgetProvision === '-- Select --') {
        $('#CIJRequest_BudgetAmount').prop('disabled', false);
    } else {
        $('#CIJRequest_BudgetAmount').prop('disabled', true);
        $("#CIJRequest_BudgetAmount").val("");
    }
}
function changePurchasePurpose() {
    var purchasePurposeId = $("#CIJRequest_PurchasePurposeId option:selected").val();
    if (purchasePurposeId === "2" || !purchasePurposeId) {
        //Replacement
        $('#CIJRequest_OldEquipmentTreatmentId').prop('disabled', false);
        $('#CIJRequest_OldEquipmentCost').prop('disabled', false);
    } else {
        $('#CIJRequest_OldEquipmentTreatmentId').prop('disabled', true);
        $('#CIJRequest_OldEquipmentCost').prop('disabled', true);
        $("#CIJRequest_OldEquipmentTreatmentId").val("");
        $("#CIJRequest_OldEquipmentCost").val("");
    }
}

function changeItemType() {
    var itemTypeId = $("#ddlItemType option:selected").val();
    if (itemTypeId === "1" || !itemTypeId) {
        //Medical
        $("#divCommiteeComments").show();
    } else {
        $("#divCommiteeComments").hide();
        $("#txtCommiteeComments").val("");
    }
}

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
                <td>${item.Make}</td>
                <td>${item.Model}</td>
                <td>${item.EquipmentQty}</td>
                <td>${item.EquipmentCost}</td>
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
    var equipment = {
        EquipmentId: parseInt($("#EquipmentId").val()) || 0,
        EquipmentName: $("#EquipmentName").val(),
        EquipmentQty: parseInt($("#EquipmentQty").val()) || 0,
        Make: $("#Make").val(),
        Model: $("#Model").val(),
        EquipmentCost: parseFloat($("#EquipmentCost").val()) || 0,
        /*PreferenceOrder: parseInt($("#PreferenceOrder").val())||0*/
    };
    var index = parseInt($("#EquipmentIndex").val()) || 0;
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
    var item = equipments[index];

    $("#EquipmentIndex").val(index);
    $("#EquipmentId").val(item.EquipmentId);

    $("#EquipmentName").val(item.EquipmentName);

    $("#EquipmentQty").val(item.EquipmentQty);

    $("#Make").val(item.Make);

    $("#Model").val(item.Model);

    $("#EquipmentCost").val(item.EquipmentCost);
    /*$("#PreferenceOrder").val(item.PreferenceOrder) */

    $("#btnEquipmentSave").text("Update");

    var modal = new bootstrap.Modal(
        document.getElementById("equipmentModal")
    );

    modal.show();

}
function DeleteEquipment(index) {
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
    /*$("#PreferenceOrder").val("");*/

}
function calculateTotalEquipmentCost() {
    let total = 0;
    $.each(equipments, function (i, item) {
        total += parseFloat(item.EquipmentCost || 0);
    });
    var eqpCost = "₹ " + total.toLocaleString('en-IN', {
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
function validateField(selector, message) {
    const field = $(selector);

    if (!field.val()) {
        field.addClass("is-invalid");

        if (field.next(".custom-error").length === 0) {
            field.after(
                `<span class="invalid-feedback custom-error">${message}</span>`
            );
        }

        return false;
    }

    field.removeClass("is-invalid");
    field.next(".custom-error").remove();

    return true;
}
$("#cijForm").on("submit", function (e) {

    let isValid = true;
    if (!validateField("#CIJRequest_LocationId", "Location is required.")) {
        isValid = false;
    }
    if (!validateField("#CIJRequest_CostCenterId","Cost Center is required.")) {
        isValid = false;
    }
    if (!validateField("#CIJRequest_BudgetProvision", "Budget Provision is required.")) {
        isValid = false;
    }
    if ($("CIJRequest_BudgetProvision").val() ==="Yes") {
        if (!validateField("#CIJRequest_BudgetAmount", "Budget Amount is required.")) {
            isValid = false;
        }
    }
    if (!validateField("#ddlItemType", "Item type is required.")) {
        isValid = false;
    }
    if (!validateField("#CIJRequest_TotalEquipmentCostDisplay", "Equipment Cost is required.")) {
        isValid = false;
    }
    if (!validateField("#CIJRequest_PurchasePurposeId", "Purpose of purchase is required.")) {
        isValid = false;
    }

    if (!isValid) {
        e.preventDefault();
        $(".is-invalid").first().focus();
    }
});
$(document).on("input", "input, textarea", function () {
    if ($(this).val().trim() !== "") {
        $(this).removeClass("is-invalid");
        $(this).next(".custom-error").remove();
    }
});
$(document).on("change", "select", function () {
    if ($(this).val()) {
        $(this).removeClass("is-invalid");
        $(this).next(".custom-error").remove();
    }
});