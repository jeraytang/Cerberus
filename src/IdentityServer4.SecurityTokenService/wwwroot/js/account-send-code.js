$(function(){
    var timetick = 60;
    var interval;
    function time() {
        if(timetick === 0) {
            reset();
            return;
        }
        --timetick;
        $("#btn-send-code").text(timetick + ' s');
    }
    function reset() {
        clearInterval(interval)
        timetick = 60;
        $("#btn-send-code").removeAttr("disabled");
        $("#btn-send-code").text('发送验证码');
    }
    $("#btn-send-code").click(function(){
        let phoneNumber = $("#PhoneNumber").val();
        if (!phoneNumber){
            $("#phoneval").text("请输入手机号码！");
            //$.id_toast({msg:"请输入手机号码！",type: "error"});
            return;
        }
        $("#btn-send-code").attr("disabled","disabled");
        interval = setInterval(time, 1000)
        $.ajax({
            url:"/Account/SendMobilePhoneCode",
            type:"post",
            data:{"phoneNumber":phoneNumber},
            success:function(data){
                if (data.code === 500){
                    $("#phoneval").text(data.msg);
                    reset();
                }
            },
            error:function(data){
                $.id_toast({msg:"service error！",type: "error"});
                reset();
            }
        });
    });
});