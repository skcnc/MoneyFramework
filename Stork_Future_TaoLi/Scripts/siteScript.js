

$('#category_panel_open').delegate('button.displaystrategy', 'click', function (e) {
    var _list = $(this).parents("div.strategycategory_open").children('ul.list-group:eq(0)');
    var _value = _list.css('display');

    if (_value == 'none') {
        _list.css('display', 'block');
    }
    else {
        _list.css('display', 'none');
    }
})

//允许运行
$('#category_panel_open').delegate('button.runopenstrategy', 'click', function (e) {


    var _li = $(this).parents('li.list-group-item');
    var _userName = $('#userName').text();
    var _id = _li.find('span.liid').text();
    var _bRUN = localStorage[_id + ":RUN"];
    var _bALLOW = localStorage[_id + ":ALLOW"];

    if (_bRUN == 0) {
        //当前为不可以运行，将转换成可以运行
        $(this).removeClass('btn-default');
        $(this).addClass('btn-success');

        _bRUN = 1; 
    }
    else if (_bRUN == 1 && _bALLOW == 0) {
        //当前状态为运行，下一步转成禁止运行
        $(this).removeClass('btn-success');
        $(this).addClass('btn-default');

        _bRUN = 0;
    }
    else if (_bRUN == 1 && _bALLOW == 1) {
        alert("交易允许时无法停止策略！");
        return;
    }

    localStorage.setItem(_id + ":RUN", _bRUN);

    //发送至服务器
    var _basic = {
        USER: _userName,
        ACTIVITY: "OPENRUN",
        ORIENTATION: "0",
        ID: _id
    }

    var strategy = {
        basic: _basic,
        RUN: _bRUN
    }

    var JSONSTRING = JSON.stringify(strategy);

    $.post("/Home/ImportHarbor", {
        InputJson: JSONSTRING
    }, function (data, status) {
        alert("数据：" + data + "\n状态：" + status);
    })

})

//允许交易
$('#category_panel_open').delegate('button.allow_strategy', 'click', function (e) {


    var _li = $(this).parents('li.list-group-item');
    var _userName = $('#userName').text();
    var _id = _li.find('span.liid').text();
    var _bRUN = localStorage[_id + ":RUN"];
    var _bALLOW = localStorage[_id + ":ALLOW"];

    if (_bALLOW == 0 && _bRUN == 1) {
        //下一步允许交易
        $(this).removeClass('btn-default');
        $(this).addClass('btn-success');

        _bALLOW = 1;
    }
    else if(_bALLOW == 1 && _bRUN == 1){
        $(this).removeClass('btn-success');
        $(this).addClass('btn-default');

        _bALLOW = 0;
    }
    else{
        alert("只有运行中的策略才能允许交易！");
        return ;
    }


    localStorage.setItem(_id + ":ALLOW", _bALLOW);

    //发送至服务器
    var _basic = {
        USER: _userName,
        ACTIVITY: "OPENALLOW",
        ORIENTATION: "0",
        ID: _id
    }

    var strategy = {
        basic: _basic,
        ALLOW: _bALLOW
    }

    var JSONSTRING = JSON.stringify(strategy);

    $.post("/Home/ImportHarbor", {
        InputJson: JSONSTRING
    }, function (data, status) {
        alert("数据：" + data + "\n状态：" + status);
    })
})

//删除开仓策略
$('#category_panel_open').delegate('button.delete_strategy', 'click', function (e) {
    var _li = $(this).parents('li.list-group-item');
    var _ul = $(this).parents('ul.list-group');
    var _div = $(this).parents('div.strategycategory_open');

    var _userName = $('#userName').text();
    var _id = _li.find('span.liid').text();

    var ct = _div.find('[name=CTValue]').text();
    var index_fullname = _div.find('[name=IndexValue]').text();

    var op = _li.find('a.OPValue').text();
    var hd = _li.find('a.HDValue').text();

    var _length = _ul.find('li.list-group-item').length;


    var _allow =  localStorage[_id + ":ALLOW"];
    var _run = localStorage[_id + ":RUN"];

    if(_allow + _run > 0)
    {
        alert("交易运行时无法删除！");
        return;
    }
    
    if (_length == 1)
    {
        $(_div).remove();
    }
    else
    {
        _div.find('span.badge_count').text(_length - 1);
        $(_li).remove();
    }

    //发送至服务器
    var _basic = {
        USER: _userName,
        ACTIVITY: "OPENDELETE",
        ORIENTATION: "0",
        ID: _id
    }

    var strategy = {
        basic: _basic
    }


    var JSONSTRING = JSON.stringify(strategy);

    $.post("/Home/ImportHarbor", {
        InputJson: JSONSTRING
    }, function (data, status) {
        alert("数据：" + data + "\n状态：" + status);
    })

    //删除对应的键值
    localStorage.removeItem(_id + ":TYPE");
    localStorage.removeItem(_id + ":DT");
    localStorage.removeItem(_id + ":BUYLIST");
    localStorage.removeItem(_id + ":CT");
    localStorage.removeItem(_id + ":HD");
    localStorage.removeItem(_id + ":CHANGE");
    localStorage.removeItem(_id + ":WEIGHT");
    localStorage.removeItem(_id + ":OP");
    localStorage.removeItem(_id + ":INDEX");
    localStorage.removeItem(_id + ":RUN");
    localStorage.removeItem(_id + ":ALLOW");

})

//修改开仓策略
$('#category_panel_open').delegate('button.modify-strategy', 'click', function (e) {
    var _li = $(this).parents('li.list-group-item');
    var _userName = $('#userName').text();
    var _id = _li.find('span.liid').text();

    var _href = "/home/EditWeightAndTradeList?StrategyID=" + _id + "&USER=" + _userName;
    window.open(encodeURI(_href), "_blank");
})

//页面重新进入
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
    else if (e.currentTarget.location.pathname == "/home/EditWeightAndTradeList") {
        if (Modernizr.localstorage) {
            var _queryString = e.currentTarget.location.href.split('?')[1];
            var _id = _queryString.split('&')[0].split('=')[1];
            
            if (_id != "") {
                var ct = localStorage[_id + ":CT"];
                var op = localStorage[_id + ":OP"];
                var hd = localStorage[_id + ":HD"];
                var Index = localStorage[_id + ":INDEX"];
                var weight = localStorage[_id + ":WEIGHT"];
                var order = localStorage[_id + ":BUYLIST"];

                var fullName = GetIndexFullName(Index);
                
                $('#ct_value').val(ct);
                $('#op_value').val(op);
                $('#hd_value').val(hd);
                $('#index_input').val(fullName);
                $('#weightList').text(weight);
                $('#tradeOrder').text(order);
            }
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

        var run = localStorage[id + ":RUN"];
        var allow = localStorage[id + ":ALLOW"];
        
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
            var search = 'div.strategycategory_open[name=' + _name + ']';
            var cates = $.find(search);

            if (cates.length == 0) {
                //需要添加大类

                var new_category = $('.category_template').clone();
                new_category.removeClass('sr-only');
                new_category.removeClass('category_template');

                new_category.find("[name='CTValue']").text(ct);

                var IndexFullName = GetIndexFullName(Index);

                new_category.find("[name='IndexValue']").text(IndexFullName);
                new_category.find("[name='OPValue']").text(op);
                new_category.find("[name='HDValue']").text(hd);

                new_category.attr('name', _name);


                $('#category_panel_open').append(new_category);
            }

            //需要添加小类
            var _li = $('.strategy_template').clone();

            $('#category_panel_open').find('span.liid').each(function (index, element) {
                if(element.innerText == id)
                {
                    _li = $(this).parents("li.list-group-item");
                }
            })
            

            _li.removeClass('sr-only');
            _li.removeClass('strategy_template');
            _li.attr('op_value', op);
            _li.attr('hd_value', hd);

            _li.find('a.OPValue').text(op);
            _li.find('a.HDValue').text(hd);

            _li.find('span.liid').text(id);

            if (run == 1)
            {
                _li.find('button.runopenstrategy').removeClass('btn-default');
                _li.find('button.runopenstrategy').addClass('btn-success');
            }
            else {
                _li.find('button.runopenstrategy').removeClass('btn-success');
                _li.find('button.runopenstrategy').addClass('btn-default');
            }

            if (allow == 1)
            {
                _li.find('button.allow_strategy').addClass('btn-success');
                _li.find('button.allow_strategy').removeClass('btn-default');
            }
            else {
                _li.find('button.allow_strategy').addClass('btn-default');
                _li.find('button.allow_strategy').removeClass('btn-success');
            }

            var _ul = $('div.strategycategory_open[name=' + _name + ']');
            var tt = _ul.find('li.list-group-item[op_value=' + op + '][hd_value=' + hd + ']');
            if (tt.length != 0) {
                continue;
            }
            var length = _ul.find('li.list-group-item').length
            _ul.find('span.badge_count').text(length + 1);

            var user = $('#userName')[0].innerText;

            var activity = undefined;

            $('div.strategycategory_open[name=' + _name + '] ul.list-group').append(_li);

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

//刷新界面
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

        if(localStorage[id+":RUN"] == undefined)
        {
            localStorage.setItem(id + ":RUN", 0);
        }

        if (localStorage[id + ":ALLOW"] == undefined) localStorage.setItem(id + ":ALLOW", 0);

        
    } else {
    // no native support for HTML5 storage :(
    // maybe try dojox.storage or a third-party solution
        alert("您当前使用的浏览器版本过低，网站功能将被限制！");
        return;
    }
    alert('参数已写入，请刷新控制页面')
})

//辅助函数
function GetIndexFullName(briefName)
{
    if(briefName == 500)
    { return "中证500" }
    else if(briefName == 300){ return "沪深300";}
    else if (briefName == 50) { return "上证50" }
    else { return "未知";}
}




