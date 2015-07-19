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

//展开实例详情
//$('button.displaystrategy').click(function (e) {
//    var _list = $(this).parents("div.strategycategory").children('ul.list-group:eq(0)');
//    var _value = _list.css('display');

//    if (_value == 'none') {
//        _list.css('display', 'block');
//    }
//    else {
//        _list.css('display', 'none');
//    }
//})
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

    var t = $('div.strategycategory[name=' + _name + '] ul.list-group');
    $('div.strategycategory[name=' + _name + '] ul.list-group').append(_li);

})

$('#ajaxTest').click(function (e) {
    $.post("/Home/PostAjax", {
        ko: "Hello World!"
    },
    function (data, status) {
        alert("数据：" + data + "\n状态：" + status);
    })

})