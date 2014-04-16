/*!
 * GridViewEX v1.4.0 (https://github.com/vcliment89/GridViewEX)
 * Copyright 2013-2014 Vicent Climent
 * Licensed under MIT (https://github.com/vcliment89/GridViewEX/blob/master/LICENSE)
 */

function GVEXColumnCheckboxesDisable(columnName, mode) {
    $('.' + columnName).each(function () {
        if (mode == 'checked'
            && $(this).is(':checked')) {
            $(this).attr('disabled', 'disabled');
        } else if (mode == 'unchecked'
            && $(this).is(':not(:checked)')
            && !$(this)[0].indeterminate) {
            $(this).attr('disabled', 'disabled');
        } else if (mode == 'null'
            && $(this)[0].indeterminate) {
            $(this).attr('disabled', 'disabled');
        } else if (mode == 'checkedOrNull'
            && ($(this).is(':checked')
                || $(this)[0].indeterminate)) {
            $(this).attr('disabled', 'disabled');
        } else if (mode == 'all') {
            $(this).attr('disabled', 'disabled');
        }
    });
}

function GVEXCheckboxIndeterminate(cbID) {
    var cb = document.getElementById(cbID);
    if (cb != null) {
        cb.indeterminate = true;
    }
}

function GVEXColumnSelectionShowAll(gvId, link, isShowAll, hfColumnsSelectedID, lbApplyID) {
    var arr = JSON.parse($(hfColumnsSelectedID).val());

    $(link).parent().children('ol').children().each(function () {
        $(this).children('input:checkbox').prop('checked', isShowAll);
    });

    $.map(arr, function (elementOfArray) {
        elementOfArray.V = Number(isShowAll);
    });

    $(hfColumnsSelectedID).val(JSON.stringify(arr));
    document.cookie = gvId + '_ColumnsSelected=' + escape(JSON.stringify(arr)) + '; ';
    $(lbApplyID).show();
}

function GVEXColumnSelectionChanged(gvId, cb, hfColumnsSelectedID, lbApplyID) {
    var arr = JSON.parse($(hfColumnsSelectedID).val());

    $.map(arr, function (elementOfArray) {
        if (elementOfArray.ID == $(cb).attr('data-field')) {
            elementOfArray.V = Number($(cb).is(':checked'));
        }
    });

    $(hfColumnsSelectedID).val(JSON.stringify(arr));
    document.cookie = gvId + '_ColumnsSelected=' + escape(JSON.stringify(arr)) + '; ';
    $(lbApplyID).show();
}

function GVEXColumnIndexChanged(gvId, link, index, hfColumnsSelectedID, lbApplyID) {
    var arr = JSON.parse($(hfColumnsSelectedID).val());
    var li = $(link).parent();
    var liPrev = li.prev();
    var liNext = li.next();
    var liChildren = li.children();
    var oldIndex = parseInt(liChildren.closest('input:checkbox').attr('data-index'));

    arr.move(oldIndex, index);
    $.map(arr, function (elementOfArray, indexInArray) {
        elementOfArray.I = indexInArray;
    });

    // Move the item on the list & handle the sort actions depending on the position
    if (index == 0) { // Second moved up to first place
        li.insertBefore(liPrev);
        liPrev.children().closest('input:checkbox').attr('data-index', index + 1);

        liChildren.closest('[data-action=""up""]').css('visibility', 'hidden');
        liPrev.children().closest('[data-action=""up""], [data-action=""down""]').css('visibility', 'visible');
    } else if (index == 1
        && index > oldIndex) { // First moved down to second place
        li.insertAfter(liNext);
        liNext.children().closest('input:checkbox').attr('data-index', index - 1);

        liNext.children().closest('[data-action=""up""]').css('visibility', 'hidden');
        liChildren.closest('[data-action=""up""], [data-action=""down""]').css('visibility', 'visible');
    } else if (index == li.parent().children().length - 2
        && oldIndex > index) { // Last moved up to penultimate place
        li.insertBefore(liPrev);
        liPrev.children().closest('input:checkbox').attr('data-index', index + 1);

        liPrev.children().closest('[data-action=""down""]').css('visibility', 'hidden');
        liChildren.closest('[data-action=""up""], [data-action=""down""]').css('visibility', 'visible');
    } else if (index == li.parent().children().length - 1) { // Penultimate moved down to last place
        li.insertAfter(liNext);
        liNext.children().closest('input:checkbox').attr('data-index', index - 1);

        liChildren.closest('[data-action=""down""]').css('visibility', 'hidden');
        liNext.children().closest('[data-action=""up""], [data-action=""down""]').css('visibility', 'visible');
    } else {
        if (oldIndex < index) { // Moved Down
            li.insertAfter(liNext);
            liNext.children().closest('input:checkbox').attr('data-index', index - 1);
        } else { // Moved Up
            li.insertBefore(liPrev);
            liPrev.children().closest('input:checkbox').attr('data-index', index + 1);
        }
    }
    liChildren.closest('input:checkbox').attr('data-index', index);

    $(hfColumnsSelectedID).val(JSON.stringify(arr));
    document.cookie = gvId + '_ColumnsSelected=' + escape(JSON.stringify(arr)) + '; ';
    $(lbApplyID).show();
}

function GVEXPopover(hoverElementID, divElementID) {
    $(hoverElementID).popover({
        html: true,
        trigger: 'click',
        placement: 'bottom',
        content: function () {
            return $(divElementID).html();
        }
    });
}

function GVEXSaveSearchExp(hfID, focusID, value) {
    $('#' + hfID).val(value);
    $('#' + focusID).focus();
}

function GVEXDeleteAlert(gvId) {
    var d = new Date();
    document.cookie = gvId + '_AlertMessage=; expires=' + d.toUTCString() + '; path=';
}

function GVEXCreateTopScrollbar(gvId) {
    var element = $('#' + gvId + 'GridViewTable');
    var scrollbar = $('<div></div>')
        .attr('id', gvId + 'Scrollbar')
        .css('overflow-x', 'auto')
        .css('overflow-y', 'hidden')
        .append($('<div></div>')
            .width(element
                .children('div')
                .children('table')[0]
                .scrollWidth)
            .css('padding-top', '1px')
            .append('\xA0'));
    scrollbar.scroll(function () {
        element.scrollLeft(scrollbar.scrollLeft());
    });
    element.scroll(function () {
        scrollbar.scrollLeft(element.scrollLeft());
    });
    element.before(scrollbar);
}

function GVEXToggleInlineFilter(gvId, btnElement, isVisible) {
    if (typeof isVisible != 'undefined') {
        window[gvId + 'IsFilterShown'] = isVisible;
        if (isVisible) {
            $('.' + gvId + 'Filters').show();
            $(btnElement).addClass('active');
        } else {
            $('.' + gvId + 'Filters').hide();
            $(btnElement).removeClass('active');
        }
    }
    else {
        window[gvId + 'IsFilterShown'] = !window[gvId + 'IsFilterShown'];
        $('.' + gvId + 'Filters').toggle();
        if (window[gvId + 'IsFilterShown']) {
            $(btnElement).addClass('active');
        } else {
            $(btnElement).removeClass('active');
        }
    }
    $('#' + gvId + 'Scrollbar')
        .children('div')
        .width($('#' + gvId + 'GridViewTable')
            .children('div')
            .children('table')[0]
            .scrollWidth);
}

function GVEXCompactTable(gvId, isCompact, btnExpand, btnCompact) {
    window[gvId + 'SizeCompact'] = isCompact;
    if (isCompact) {
        $('#' + gvId).addClass('table-condensed');
        $('#' + btnExpand).show();
        $('#' + btnCompact).hide();
    } else {
        $('#' + gvId).removeClass('table-condensed');
        $('#' + btnExpand).hide();
        $('#' + btnCompact).show();
    }

    $('#' + gvId + 'Scrollbar')
        .children('div')
        .width($('#' + gvId + 'GridViewTable')
            .children('div')
            .children('table')[0]
            .scrollWidth);
}

function GVEXSaveView(gvId, modalID, txtViewNameID, divViewCheckBoxesID, divAlertID) {
    var isViewValid = true;
    var viewCheckBoxes = $(divViewCheckBoxesID).find('input[type=\'checkbox\']');

    // Will be true if bootstrap 3 is loaded, false if bootstrap 2 or no bootstrap
    var bootstrap3_enabled = (typeof $().emulateTransitionEnd == 'function');
    var alertErrorClass = bootstrap3_enabled
        ? 'alert-danger'
        : 'alert-error';

    isViewValid = ($(txtViewNameID).val().length > 0);

    var count = 0;
    $.map(viewCheckBoxes, function (elementOfArray) {
        if ($(elementOfArray).is(':checked')) {
            count++;
        }
    });

    if (viewCheckBoxes.length >= 1
        && count <= 0) {
        isViewValid = false;
    }

    if (isViewValid) {
        var d = new Date();
        var d2 = new Date(d.getTime() + 5 * 60000); // 5 minutes
        document.cookie = gvId + '_AlertMessage=' + escape('View Saved!') + '; expires=' + d2.toUTCString() + '; path=';

        $(modalID).modal('hide');
        return true;
    }
    else {
        if ($(txtViewNameID).val().length <= 0) {
            if ($(divAlertID).length <= 0) {
                $(txtViewNameID).parent().parent().next().children('div:first-child').append('<div id=\'' + divAlertID.substring(1, divAlertID.length) + '\' class=\'alert ' + alertErrorClass + ' fade in\' style=\'font-size: 14px;line-height: 20px;\'><button class=\'close\' data-dismiss=\'alert\'>x</button><div>Must provide a name</div></div>');
            } else {
                $(divAlertID).children('div').html('Must provide a name');
                $(divAlertID).show();
            }

            $(txtViewNameID).css('border-color', '#B94A48')
                .css('color', '#B94A48')
                .focus();
        }
        else if (viewCheckBoxes.length >= 1
            && count <= 0) {
            if ($(divAlertID).length <= 0) {
                $(txtViewNameID).parent().parent().next().children('div:first-child').append('<div id=\'' + divAlertID.substring(1, divAlertID.length) + '\' class=\'alert ' + alertErrorClass + ' fade in\' style=\'font-size: 14px;line-height: 20px;\'><button class=\'close\' data-dismiss=\'alert\'>x</button><div>Must select at least one option</div></div>');
            } else {
                $(divAlertID).children('div').html('Must select at least one option');
                $(divAlertID).show();
            }

            $.map(viewCheckBoxes, function (elementOfArray) {
                $(elementOfArray).parent().css('color', '#B94A48');
            });
        }

        return false;
    }
}