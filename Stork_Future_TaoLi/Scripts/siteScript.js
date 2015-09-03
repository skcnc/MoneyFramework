
//展开开仓策略细节
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

//展开平仓策略细节
$('#category_panel_close').delegate('button.displaystrategy', 'click', function (e) {
    var _list = $(this).parents('div.strategycategory_close').children('ul.list-group:eq(0)');
    var _value = _list.css('display');

    if (_value == 'none') {
        _list.css('display', 'block');
    }
    else {
        _list.css('display', 'none');
    }
})

//开仓允许运行
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

    var JSONSTRING = GetHeader(_basic.ACTIVITY) + JSON.stringify(strategy);

    $.post("/Home/ImportHarbor", {
        InputJson: JSONSTRING
    }, function (data, status) {
        alert("数据：" + data + "\n状态：" + status);
    })

})

//平仓允许运行
$('#category_panel_close').delegate('button.runopenstrategy', 'click', function (e) {
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
        ACTIVITY: "CLOSERUN",
        ORIENTATION: "1",
        ID: _id
    }

    var strategy = {
        basic: _basic,
        RUN: _bRUN
    }

    var JSONSTRING = GetHeader(_basic.ACTIVITY) + JSON.stringify(strategy);

    $.post("/Home/ImportHarbor", {
        InputJson: JSONSTRING
    }, function (data, status) {
        alert("数据：" + data + "\n状态：" + status);
    })
})

//开仓允许交易
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

    var JSONSTRING = GetHeader(_basic.ACTIVITY) + JSON.stringify(strategy);

    $.post("/Home/ImportHarbor", {
        InputJson: JSONSTRING
    }, function (data, status) {
        alert("数据：" + data + "\n状态：" + status);
    })
})

//平仓允许交易
$('#category_panel_close').delegate('button.allow_strategy', 'click', function (e) {


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
    else if (_bALLOW == 1 && _bRUN == 1) {
        $(this).removeClass('btn-success');
        $(this).addClass('btn-default');

        _bALLOW = 0;
    }
    else {
        alert("只有运行中的策略才能允许交易！");
        return;
    }


    localStorage.setItem(_id + ":ALLOW", _bALLOW);

    //发送至服务器
    var _basic = {
        USER: _userName,
        ACTIVITY: "CLOSEALLOW",
        ORIENTATION: "1",
        ID: _id
    }

    var strategy = {
        basic: _basic,
        ALLOW: _bALLOW
    }

    var JSONSTRING = GetHeader(_basic.ACTIVITY) + JSON.stringify(strategy);

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


    var JSONSTRING = GetHeader(_basic.ACTIVITY) + JSON.stringify(strategy);

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

    var chat = $.connection.proxyHub;
    chat.delete();
})

//删除平仓策略
$('#category_panel_close').delegate('button.delete_strategy', 'click', function (e) {
    var _li = $(this).parents('li.list-group-item');
    var _ul = $(this).parents('ul.list-group');
    var _div = $(this).parents('div.strategycategory_close');

    var _userName = $('#userName').text();
    var _id = _li.find('span.liid').text();

    var _length = _ul.find('li.list-group-item').length;


    var _allow = localStorage[_id + ":ALLOW"];
    var _run = localStorage[_id + ":RUN"];

    if (_allow + _run > 0) {
        alert("交易运行时无法删除！");
        return;
    }

    if (_length == 1) {
        $(_div).remove();
    }
    else {
        _div.find('span.badge_count').text(_length - 1);
        $(_li).remove();
    }

    //发送至服务器
    var _basic = {
        USER: _userName,
        ACTIVITY: "CLOSEDELETE",
        ORIENTATION: "1",
        ID: _id
    }

    var strategy = {
        basic: _basic
    }


    var JSONSTRING = GetHeader(_basic.ACTIVITY) + JSON.stringify(strategy);

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
    localStorage.removeItem(_id + ":SP");
    localStorage.removeItem(_id + ":COE");
    localStorage.removeItem(_id + ":SD");
    localStorage.removeItem(_id + ":SA");
    localStorage.removeItem(_id + ":PE");
    localStorage.removeItem(_id + ":BASIS");
    localStorage.removeItem(_id + ":RUN");
    localStorage.removeItem(_id + ":ALLOW");

    var chat = $.connection.proxyHub;
    chat.delete();
})

//修改开仓策略
$('#category_panel_open').delegate('button.modify-strategy', 'click', function (e) {
    var _li = $(this).parents('li.list-group-item');
    var _userName = $('#userName').text();
    var _id = _li.find('span.liid').text();

    var _href = "/home/OPEN_EDIT?StrategyID=" + _id + "&USER=" + _userName;
    window.open(encodeURI(_href), "_blank");
})

//修改平仓策略
$('#category_panel_close').delegate('button.modify-strategy', 'click', function (e) {
    var _li = $(this).parents('li.list-group-item');
    var _userName = $('#userName').text();
    var _id = _li.find('span.liid').text();

    var _href = "/home/CLOSE_EDIT?StrategyID=" + _id + "&USER=" + _userName;
    window.open(encodeURI(_href), "_blank");
})

//页面重新进入
window.onload = function (e) {

    if (e.currentTarget.location.pathname != "/" && e.currentTarget.location.pathname != "/home/Syslogin")
    {
        if (Modernizr.localstorage) {
            var userName = localStorage["USERNAME"];
            var loginTime = localStorage["TIMESTAMP"];
            var dNow = new Date();

            if (userName == "" || userName == undefined) {
                window.location.href = "/";
                return;
            }
            if (loginTime == "" || loginTime == undefined) {
                window.location.href = "/";
                return;
            }
            var days = Math.floor((dNow.getTime() - loginTime) / (24 * 3600 * 1000));

            if (days > 0) {
                window.location.href = "/";
                return;
            }
        }
        else
        {
            alert("您当前使用的浏览器版本过低，网站功能将被限制！");
            window.location.href = "/";
        }
    }

    if (e.currentTarget.location.pathname == "/home/MonitorConsole") {
        if (Modernizr.localstorage) {
            localStorage.setItem("IDCollection", "");
            UpdateOPENStrategies(false);
        }
        else {
            alert("您当前使用的浏览器版本过低，网站功能将被限制！");
            return
        }
    }
    else if (e.currentTarget.location.pathname == "/home/OPEN_EDIT") {
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
    else if (e.currentTarget.location.pathname == "/home/CLOSE_EDIT") {
        if (Modernizr.localstorage) {
            var _queryString = e.currentTarget.location.href.split('?')[1];
            var _id = _queryString.split('&')[0].split('=')[1];

            if(_id != "")
            {
                var ct = localStorage[_id + ":CT"];
                var sp = localStorage[_id + ":SP"];
                var hd = localStorage[_id + ":HD"];
                var coe = localStorage[_id + ":COE"];
                var sd = localStorage[_id + ":SD"];
                var sa = localStorage[_id + ":SA"];
                var pe = localStorage[_id + ":PE"];
                var basis = localStorage[_id + ":BASIS"];
                var order = localStorage[_id + ":BUYLIST"];

                $('#ct_value').val(ct);
                $('#sp_value').val(sp);
                $('#hd_value').val(hd);
                $('#coe_value').val(coe);
                $('#sd_value').val(sd);
                $('#sa_value').val(sa);
                $('#pe_value').val(pe);
                $('#basis_value').val(basis);
                $('#buylist').val(order);
            }
        }
        else {
            alert("您当前使用的浏览器版本过低，网站功能将被限制！");
            return;
        }
    }
}

//刷新策略实例
function UpdateOPENStrategies(changeFlag)
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
                    //默认实例已经存在
                    var isExist = true;
                    if (idCollection[id] == undefined)
                    {
                        //新建实例
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

                var new_category = $('.category_open_template').clone();
                new_category.removeClass('sr-only');
                new_category.removeClass('category_open_template');

                new_category.find("[name='CTValue']").text(ct);

                var IndexFullName = GetIndexFullName(Index);

                new_category.find("[name='IndexValue']").text(IndexFullName);
                new_category.find("[name='OPValue']").text(op);
                new_category.find("[name='HDValue']").text(hd);

                new_category.attr('name', _name);


                $('#category_panel_open').append(new_category);
            }

            //需要添加小类
            var _li = $('.strategy_open_template').clone();

            $('#category_panel_open').find('span.liid').each(function (index, element) {
                if(element.innerText == id)
                {
                    _li = $(this).parents("li.list-group-item");
                }
            })
            

            _li.removeClass('sr-only');
            _li.removeClass('strategy_open_template');
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

            var user = localStorage["USERNAME"];

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
                var chat = $.connection.proxyHub;
                chat.join(id);
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
                orderli: buylist,
                weightli:weight,
                basic: _basic,
                OP: op,
                HD: hd,
                CT: ct,
                INDEX: Index
            }

            var JSONSTRING = GetHeader(_basic.ACTIVITY) + JSON.stringify(strategy);

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

            var _name = ct + '-' + Index;
            var search = 'div.strategycategory_close[name=' + _name + ']';
            var cates = $.find(search);

            if (cates.length == 0) {
                //需要添加大类

                var new_category = $('.category_close_template').clone();
                new_category.removeClass('sr-only');
                new_category.removeClass('category_close_template');

                new_category.find("[name='CTValue']").text(ct);

                new_category.attr('name', _name);
                $('#category_panel_close').append(new_category);
            }

            //需要添加小类
            var _li = $('.strategy_close_template').clone();

            $('#category_panel_close').find('span.liid').each(function (index, element) {
                if (element.innerText == id) {
                    _li = $(this).parents("li.list-group-item");
                }
            })


            _li.removeClass('sr-only');
            _li.removeClass('strategy_close_template');

            _li.attr('sp_value', sp);
            _li.attr('coe_value', coe);
            _li.attr('sd_value', sd);
            _li.attr('sa_value', sa);
            _li.attr('pe_value', pe);

            _li.find('a.SPValue').text(sp);
            _li.find('a.COEValue').text(coe);
            _li.find('a.SDValue').text(sd);
            _li.find('a.SAValue').text(sa);
            _li.find('a.PEValue').text(pe);
            _li.find('a.BASISValue').text(basis);

            _li.find('span.liid').text(id);

            if (run == 1) {
                _li.find('button.runopenstrategy').removeClass('btn-default');
                _li.find('button.runopenstrategy').addClass('btn-success');
            }
            else {
                _li.find('button.runopenstrategy').removeClass('btn-success');
                _li.find('button.runopenstrategy').addClass('btn-default');
            }

            if (allow == 1) {
                _li.find('button.allow_strategy').addClass('btn-success');
                _li.find('button.allow_strategy').removeClass('btn-default');
            }
            else {
                _li.find('button.allow_strategy').addClass('btn-default');
                _li.find('button.allow_strategy').removeClass('btn-success');
            }

            var _ul = $('div.strategycategory_close[name=' + _name + ']');
            var tt = _ul.find('li.list-group-item[sp_value=' + op + '][coe_value=' + hd + '][sd_value=' + sd + '][sa_value=' + sa + '][pe_value=' + pe + ']');
            if (tt.length != 0) {
                continue;
            }
            var length = _ul.find('li.list-group-item').length
            _ul.find('span.badge_count').text(length + 1);

            var user = localStorage["USERNAME"];

            var activity = undefined;

            $('div.strategycategory_close[name=' + _name + '] ul.list-group').append(_li);

            //向服务器发送数据

            if (isExist == "false") {
                activity = "CLOSECREATE";
                var exist = localStorage.getItem("IDCollection");
                exist += id + ";"
                localStorage.setItem("IDCollection", exist);
                localStorage.setItem(id + ":CHANGE", 0);

                var chat = $.connection.proxyHub;
                chat.join(id);
            }
            else if (isExist == "true") {
                activity = "CLOSEMODIFY";
            }
            else {
                var exist = localStorage.getItem("IDCollection");
                exist += id + ";"
                localStorage.setItem("IDCollection", exist);
                continue;
            }


            var _basic = {
                USER: user,
                ACTIVITY: activity,
                ORIENTATION: "1",
                ID: id
            }



            var strategy = {
                POSITION: buylist,
                basic: _basic,
                SP: sp,
                HD: hd,
                CT: ct,
                STOCKALLOTMENT: sa,
                COSTOFEQUITY: coe,
                STOCKDIVIDENDS: sd,
                PROSPECTIVEARNINGS: pe,
                OB: basis
            }
            

            var JSONSTRING = GetHeader(_basic.ACTIVITY) + JSON.stringify(strategy);

            $.post("/Home/ImportHarbor", {
                InputJson: JSONSTRING
            }, function (data, status) {
                alert("数据：" + data + "\n状态：" + status);
            })

        }
    }

    
}


//刷新界面
$('#refresh').click(function (e) {
    UpdateOPENStrategies(true);
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

$('#CloseStrategyCreate').click(function (e) {
    var user = $.trim($('#userName')[0].innerText);
    var href = $(this).attr('href') + user;

    $(this).attr('href', href);
})

//确认交易列表
//用户确认后，会回到主页面，进行下一步添加操作
//权重信息和交易列表会保存在本地
$('#btnSubmit_open').click(function (e) {

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

//平仓提交
$('#btnSubmit_close').click(function (e) {

    var ct = $.trim($('#ct_value')[0].value);
    var hd = $.trim($('#hd_value')[0].value);
    var sp = $.trim($('#sp_value')[0].value);
    var coe = $('#coe_value')[0].value;
    var sd = $('#sd_value')[0].value;
    var sa = $('#sa_value')[0].value;
    var pe = $('#pe_value')[0].value;
    var basis = $('#basis_value')[0].value;

    var buylist = $('#buylist')[0].value;
    var id = $('#strategyID')[0].innerText;

    var userName = $('#userName')[0].innerText;
    var currentDate = new Date();

    if (id == "NEW") {
        id = userName + ":" + currentDate.getTime();
    }


    if (Modernizr.localstorage) {
        //window.localStorage is available!
        //0 : 开仓  1： 平仓 
        localStorage.setItem(id + ":TYPE", 1);
        //日期，只取天
        localStorage.setItem(id + ":DT", currentDate.getDate());
        //持仓列表
        localStorage.setItem(id + ":BUYLIST", buylist);
        //期货合约
        localStorage.setItem(id + ":CT", ct);
        //手数
        localStorage.setItem(id + ":HD", hd);

        //空头点位
        localStorage.setItem(id + ":SP", sp);
        //股票成本
        localStorage.setItem(id + ":COE", coe);
        //股票分红
        localStorage.setItem(id + ":SD", sd);
        //股票配股
        localStorage.setItem(id + ":SA", sa);
        //预期收益
        localStorage.setItem(id + ":PE", pe);
        //开仓基差
        localStorage.setItem(id + ":BASIS", basis);

        localStorage.setItem(id + ":CHANGE", 1);

        if (localStorage[id + ":RUN"] == undefined) {
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

$('#tm_btnMakeOrder').click(function (e) {
    var type = $('input[name="RadioType"]:checked').val();
    var code = $('#tm_input_code')[0].value.trim();
    var direction = $('input[name="RadioDirection"]:checked').val();
    var mark = $('input[name="RadioMark"]:checked').val();
    var volume = $('#tm_input_volume')[0].value.trim();
    var price = $('#tm_input_price')[0].value.trim();

    var user = localStorage["USERNAME"];

    var trade = {
        User:user,
        cSecurityCode: code,
        nSecurityAmount: volume,
        dOrderPrice: price,
        cTradeDirection: direction,
        cOffsetFlag: mark,
        cSecurityType: type,
        belongStrategy: "00",
        OrderRef:"0"
    }

    var JSONSTRING = "C1" + JSON.stringify(trade);

    $.post("/Home/ImportTrade", {
        InputJson:JSONSTRING    
    },function(data,status){
        alert("数据：" + data + "\n状态：" + status);
    })

})

$('#login_btnLogin').click(function (e) {
    var username = $('#login_user')[0].value.trim();
    var password = $('#login_password')[0].value.trim();

    if (username == "" || password == "" || username == undefined || password == undefined) return;

    var dt = new Date();
    if (Modernizr.localstorage) {
        localStorage.setItem("USERNAME", username);
        localStorage.setItem("TIMESTAMP", dt.getTime());
    }
    else {
        alert("您当前使用的浏览器版本过低，网站功能将被限制！");
        return;
    }

    window.location.href = '/Home/MonitorConsole'
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

//添加报文头
function GetHeader(headMark) {
    if (headMark == "OPENCREATE") { return "A1"; }
    else if (headMark == "OPENMODIFY") { return "A2"; }
    else if (headMark == "OPENRUN") { return "A3"; }
    else if (headMark == "OPENALLOW") { return "A4"; }
    else if (headMark == "OPENDELETE") { return "A5"; }
    else if (headMark == "CLOSECREATE") { return "B1"; }
    else if (headMark == "CLOSEMODIFY") { return "B2"; }
    else if (headMark == "CLOSERUN") { return "B3"; }
    else if (headMark == "CLOSEALLOW") { return "B4"; }
    else if (headMark == "CLOSEDELETE") { return "B5";}
}




