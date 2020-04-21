$.extend({
    "id_toast": function (option) {
        let position;
        let showclose;
        if (option.positionClass) {
            position = option.positionClass;
        } else {
            position = typeof (option.allwaysopen) != "undefined" ? "toast-top-right" : "toast-bottom-right";
        }
        if (option.hasOwnProperty("showclose")) {
            showclose = option.showclose;
        } else {
            showclose = true;
        }
        toastr.options = {
            "closeButton": showclose, //是否显示关闭按钮
            "debug": false, //是否使用debug模式
            "positionClass": position,//弹出窗的位置
            "showDuration": "300",//显示的动画时间
            "hideDuration": "1000",//消失的动画时间
            "timeOut": typeof (option.allwaysopen) != "undefined" ? "-1" : "4000", //展现时间
            "extendedTimeOut": "1000",//加长展示时间
            "showEasing": "swing",//显示时的动画缓冲方式
            "hideEasing": "linear",//消失时的动画缓冲方式
            "showMethod": "fadeIn",//显示时的动画方式
            "hideMethod": "fadeOut", //消失时的动画方式
            "onclick": function () {
                var data = $("#toast-container").find(".toast-message").html();
                alert(data);
            }
        };
        if (option.type == "success" || option.code == 200 || option.code == 2000) {
            toastr.success(option.msg);
        } else if (option.type == "info") {
            toastr.info(option.msg);
        } else if (option.type == "warning") {
            toastr.warning(option.msg);
        } else if (option.type == "error" || option.code == 500 || option.code == 5000) {
            toastr.error(option.msg);
        }
    }
})