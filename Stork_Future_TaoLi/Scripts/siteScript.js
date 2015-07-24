//展开添加面板
function AddPanelOper()
{
    if($('#StrategyAddPanel').hasClass('show'))
    {
        $('#_bAddStrategy').removeClass('glyphicon-menu-up');
        $('#_bAddStrategy').addClass('glyphicon-menu-down');

        $('#StrategyAddPanel').removeClass('show');
        $('#StrategyAddPanel').addClass('hidden');
    }
    else
    {
        $('#_bAddStrategy').removeClass('glyphicon-menu-down');
        $('#_bAddStrategy').addClass('glyphicon-menu-up');

        $('#StrategyAddPanel').removeClass('hidden');
        $('#StrategyAddPanel').addClass('show');
    }
}

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

//创建新的策略实例
$('#addStrategy').click(function (e) {

    var ct =$.trim($('#CT_input').val());
    var op = $.trim($('#OP_input').val());
    var hd = $.trim($('#HD_input').val());
    var Index = $.trim($('#Index_input').val());

    if(ct == "" || op == "" || hd == "" || Index == "")
    {
        return;
    }
    var _name = ct + '-' + Index;

    var search = 'div.strategycategory[name=' + _name + ']';
    var cates = $.find(search);

    if(cates.length == 0)
    {
        //需要添加大类

        var new_category = $('.category_template').clone();
        new_category.removeClass('sr-only');
        new_category.removeClass('category_template');

        new_category.find("[name='CTValue']").text(ct);

        var IndexFullName;
        if (Index == 300)
        {
            IndexFullName = "沪深300";
        }
        else if (Index == 500)
        {
            IndexFullName = "中证500";
        }
        else if (Index == 50)
        {
            IndexFullName = "上证50";
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

    var _ul = $('div.strategycategory[name=' + _name +']');
    var tt = _ul.find('li.list-group-item[op_value=' + op + '][hd_value=' + hd + ']');
    if (tt.length != 0)
    {
        return;
    }
    var length = _ul.find('li.list-group-item').length
    _ul.find('span.badge_count').text(length + 1);

    var user = $('#userName')[0].innerText;
    var currentDate = new Date();
    var id = user + ":" + currentDate.getVarDate();
   
    if(Modernizr.localstorage)
    {
        //子页面中已经添加 类型 交易列表 权重信息

        //实例ID （用户名：）+（年年年年月月日日）
        localStorage.setItem()
        //期货合约
        localStorage.setItem()

    } else {
        // no native support for HTML5 storage :(
        // maybe try dojox.storage or a third-party solution
        alert("您当前使用的浏览器版本过低，网站功能将被限制！");
    }


    var _basic = {
        USER: "TESTER",
        ACTIVITY: "OPENCREATE",
        ORIENTATION: "0"
    }


    var _weight = 

    var strategy = {
        WEIGHTING: _weight,
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


    var t = $('div.strategycategory[name=' + _name + '] ul.list-group');
    $('div.strategycategory[name=' + _name + '] ul.list-group').append(_li);

})

$('#ajaxTest').click(function (e) {
    var hd = $.trim($('#HD_input').val());

    if (hd == "")
    {
        alert("手数不能为空！");
        return;
    }

    var _strategyID = $('#strategyid').text();
    window.open(encodeURI("/home/EditWeightAndTradeList?" + "StrategyID=" + _strategyID + "&HD=" + hd));
})



//预览交易列表
$('#btnViewList').click(function (e) {

    var hd = $.trim($('#hd_value')[0].innerText);
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

//确认交易列表
//用户确认后，会回到主页面，进行下一步添加操作
//权重信息和交易列表会保存在本地
$('#btnSubmit').click(function (e) {

    var buylist = $('#tradeOrder')[0].value;

    var weight = $('#weightList').text();

    var id = $('#strategyID').text();

    if (id == "") {
        id = "newer";
    }


    if (Modernizr.localstorage) {
        //window.localStorage is available!
        //0 : 开仓  1： 平仓 
        localStorage.setItem(id + ":TYPE", 0);
        //权重文件
        localStorage.setItem(id + ":weight", weight);
        //交易列表
        localStorage.setItem(id + ":buylist", buylist);

        
    } else {
    // no native support for HTML5 storage :(
    // maybe try dojox.storage or a third-party solution
        alert("您当前使用的浏览器版本过低，网站功能将被限制！");
}
})

//权重交易生成页面加载判断
$('#danger-alert_1').ready(function () {
    var err = $.trim($('#danger-alert_2')[0].innerText);
    if (err == "")
    {
        $('#danger-alert_1').addClass('sr-only');
        $('#danger-alert_2').addClass('sr-only');
    }
    else
    {
        $('#content').addClass('sr-only');
    }
})



