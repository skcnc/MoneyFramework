//展开添加面板
//function AddPanelOper()
//{
//    if($('#StrategyAddPanel').hasClass('show'))
//    {
//        $('#_bAddStrategy').removeClass('glyphicon-menu-up');
//        $('#_bAddStrategy').addClass('glyphicon-menu-down');

//        $('#StrategyAddPanel').removeClass('show');
//        $('#StrategyAddPanel').addClass('hidden');
//    }
//    else
//    {
//        $('#_bAddStrategy').removeClass('glyphicon-menu-down');
//        $('#_bAddStrategy').addClass('glyphicon-menu-up');

//        $('#StrategyAddPanel').removeClass('hidden');
//        $('#StrategyAddPanel').addClass('show');
//    }
//}

$('#category_panel').delegate('button.displaystrategy', 'click', function (e) {
    var _list = $(this).parents("div.strategycategory").children('ul.list-group:eq(0)');
    var _value = _list.css('display');

    if (_value == 'none') {
        _list.css('display', 'block');
    }
    else {
        _list.css('display', 'none');
    }
})

//允许运行
$('#category_panel').delegate('button.runopenstrategy', 'click', function (e) {
    if ($(this).hasClass('btn-default')) {
        $(this).removeClass('btn-default');
        $(this).addClass('btn-success');
    }
})

//允许交易
$('#category_panel').delegate('button.allow_strategy', 'click', function (e) {
    if ($(this).hasClass('btn-default')) {
        $(this).removeClass('btn-default');
        $(this).addClass('btn-success');
    }
})

//删除交易
$('#category_panel').delegate('button.delete_strategy', 'click', function (e) {
    var _li = $(this).parents('li.list-group-item');
    var _ul = $(this).parents('ul.list-group');
    var _div = $(this).parents('div.strategycategory');

    var ct = _div.find('[name=CTValue]').text();
    var index_fullname = _div.find('[name=IndexValue]').text();

    var op = _li.find('a.OPValue').text();
    var hd = _li.find('a.HDValue').text();

    var _length = _ul.find('li.list-group-item').length;
    
    if (_length == 1)
    {
        $(_div).remove();
    }
    else
    {
        _div.find('span.badge_count').text(_length - 1);
        $(_li).remove();
    }
})

//控制面板刷新时的处理
window.onload = function (e) {
    if (e.currentTarget.location.pathname == "/") {
        if (Modernizr.localstorage) {
            localStorage.setItem("IDCollection", "");

            UpdateStrategies(false);
        }
        else {
            alert("您当前使用的浏览器版本过低，网站功能将被限制！");
            return
        }
    }
}

//创建新的策略实例
function UpdateStrategies(changeFlag)
{
    var idCollection = null;
    var changeArray = new Array();

    //判断哪些实例发生改变
    if (Modernizr.localstorage) {
        if (localStorage.getItem("IDCollection") == null) {
            localStorage.setItem("IDCollection", "");
        }

        idCollection = localStorage.getItem("IDCollection").toString().split(';')

        var length = localStorage.length;

        for(var i =0;i<length;i++)
        {
            if (localStorage.key(i).split(':')[2] == "CHANGE") {
                var id = localStorage.key(i).split(':')[0] + ":" + localStorage.key(i).split(':')[1];

                if(localStorage[id + ":CHANGE"] == 1)
                {
                    //默认已经开仓
                    var isExist = true;
                    if (idCollection[id] == undefined)
                    {
                        //新建开仓
                        isExist = false;
                    }

                    //说明该实例发生改变了
                    changeArray[changeArray.length] = id + ":" + isExist;
                }
                else if(changeFlag == false)
                {
                    changeArray[changeArray.length] = id + ":repaint"
                }
            }
        }

    } else {
        // no native support for HTML5 storage :(
        // maybe try dojox.storage or a third-party solution
        alert("您当前使用的浏览器版本过低，网站功能将被限制！");
        return
    }

    for (var i = 0; i < changeArray.length; i++) {

        var id = changeArray[i].split(':')[0] + ":" + changeArray[i].split(':')[1];
        var type = localStorage[id + ":TYPE"];
        var buylist = localStorage[id + ":BUYLIST"];
        var ct = localStorage[id + ":CT"];
        var hd = localStorage[id + ":HD"];
        
        var isExist = changeArray[i].split(':')[2];

        if (type == "" || buylist == "" || ct == "" || hd == "") {
            continue;
        }
        
        if (type == 0) {
            //开仓类
            var weight = localStorage[id + ":WEIGHT"];
            var Index = localStorage[id + ":INDEX"];
            var op = localStorage[id + ":OP"];

            if (weight == "" || Index == "" || op == "") continue;

            var _name = ct + '-' + Index;
            var search = 'div.strategycategory[name=' + _name + ']';
            var cates = $.find(search);

            if (cates.length == 0) {
                //需要添加大类

                var new_category = $('.category_template').clone();
                new_category.removeClass('sr-only');
                new_category.removeClass('category_template');

                new_category.find("[name='CTValue']").text(ct);

                var IndexFullName;
                if (Index == 300) {
                    IndexFullName = "沪深300";
                }
                else if (Index == 500) {
                    IndexFullName = "中证500";
                }
                else if (Index == 50) {
                    IndexFullName = "上证50";
                }
                else
                {
                    IndexFullName = "未知";
                }


                new_category.find("[name='IndexValue']").text(IndexFullName);
                new_category.find("[name='OPValue']").text(op);
                new_category.find("[name='HDValue']").text(hd);

                new_category.attr('name', _name);


                $('#category_panel').append(new_category);
            }

            //需要添加小类
            var _li = $('.strategy_template').clone();
            _li.removeClass('sr-only');
            _li.removeClass('strategy_template');
            _li.attr('op_value', op);
            _li.attr('hd_value', hd);

            _li.find('a.OPValue').text(op);
            _li.find('a.HDValue').text(hd);

            var _ul = $('div.strategycategory[name=' + _name + ']');
            var tt = _ul.find('li.list-group-item[op_value=' + op + '][hd_value=' + hd + ']');
            if (tt.length != 0) {
                return;
            }
            var length = _ul.find('li.list-group-item').length
            _ul.find('span.badge_count').text(length + 1);

            var user = $('#userName')[0].innerText;

            var activity = undefined;

            $('div.strategycategory[name=' + _name + '] ul.list-group').append(_li);

            //向服务器发送数据

            if (isExist == "false")
            {
                activity = "OPENCREATE";
                var exist = localStorage.getItem("IDCollection");
                exist += id + ";"
                localStorage.setItem("IDCollection", exist);
                localStorage.setItem(id + ":CHANGE", 0);
            }
            else if(isExist == "true"){
                activity = "OPENMODIFY";
            }
            else
            {
                var exist = localStorage.getItem("IDCollection");
                exist += id + ";"
                localStorage.setItem("IDCollection", exist);
                continue;
            }


            var _basic = {
                USER: user,
                ACTIVITY: activity,
                ORIENTATION: "0",
                ID: id
            }



            var strategy = {
                orderli:buylist,
                basic: _basic,
                OP: op,
                HD: hd,
                CT: ct,
                INDEX: Index
            }

            var JSONSTRING = JSON.stringify(strategy);

            $.post("/Home/ImportHarbor", {
                InputJson: JSONSTRING
            }, function (data, status) {
                alert("数据：" + data + "\n状态：" + status);
            })

            
        }
        else {
            //平仓类
            var sp = localStorage[id + ":SP"];
            var coe = localStorage[id + ":COE"];
            var sd = localStorage[id + ":SD"];
            var sa = localStorage[id + ":SA"];
            var pe = localStorage[id + ":PE"];
            var basis = localStorage[id + ":BASIS"];

            if (sp == "" || coe == "" || sd == "" || sa == "" || pe == "" || basis == "") continue;
        }
    }
}


$('#refresh').click(function (e) {
    UpdateStrategies(true);
})

//预览交易列表
$('#btnViewList').click(function (e) {

    var hd = $.trim($('#hd_value')[0].value);
    var weight = $.trim($('#weightList').val());
    var id = $.trim($('#strategyID').val());

    if (hd == "") {
        alert("手数值未设置，请重新打开本页面");
        return;
    }

    if(weight == "")
    {
        alert("权重列表不能为空！")
        return;
    }

    var weight_items = weight.split('\n');

    var buylist = "";

    weight_items.forEach(function (value, index) {
        var item = value.split(';');
        var code = item[0];
        var type = item[1];
        var weightValue = item[2];

        var buy = (weightValue * hd) - (weightValue * hd) % 100;

        buylist += (code + ";" + type + ";" + buy + "\n");
    });

    $('#tradeOrder').text(buylist);

})

//创建新开仓实例对路径的修改
$('#OpenStrategyCreate').click(function (e) {
    var user = $.trim($('#userName')[0].innerText);
    var href = $(this).attr('href') + user;

    $(this).attr('href', href);

})

//确认交易列表
//用户确认后，会回到主页面，进行下一步添加操作
//权重信息和交易列表会保存在本地
$('#btnSubmit').click(function (e) {



    var ct = $.trim($('#ct_value')[0].value);
    var hd = $.trim($('#hd_value')[0].value);
    var op = $.trim($('#op_value')[0].value);
    var index = $('#index_input')[0].value;

    var buylist = $('#tradeOrder')[0].value;
    var weight = $('#weightList')[0].value;
    var id = $('#strategyID')[0].innerText;

    var userName = $('#userName')[0].innerText;
    var currentDate = new Date();

    if (id == "NEW")
    {
        id = userName + ":" + currentDate.getTime();
    }

    
    if (Modernizr.localstorage) {
        //window.localStorage is available!
        //0 : 开仓  1： 平仓 
        localStorage.setItem(id + ":TYPE", 0);
        //日期，只取天
        localStorage.setItem(id + ":DT", currentDate.getDate());
        //交易列表
        localStorage.setItem(id + ":BUYLIST", buylist);
        //期货合约
        localStorage.setItem(id + ":CT", ct);
        //手数
        localStorage.setItem(id + ":HD", hd);
       
        //权重文件
        localStorage.setItem(id + ":WEIGHT", weight);
        //开仓点位
        localStorage.setItem(id + ":OP", op);
        //指数类型
        localStorage.setItem(id + ":INDEX", index);

        localStorage.setItem(id + ":CHANGE", 1);
        
    } else {
    // no native support for HTML5 storage :(
    // maybe try dojox.storage or a third-party solution
        alert("您当前使用的浏览器版本过低，网站功能将被限制！");
        return;
    }
    alert('参数已写入，请刷新控制页面')
})




