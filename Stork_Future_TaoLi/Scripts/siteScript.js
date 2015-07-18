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


$('#category_panel').delegate('button.allow_strategy', 'click', function (e) {
    if ($(this).hasClass('btn-default')) {
        $(this).removeClass('btn-default');
        $(this).addClass('btn-success');
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
        new_category.find("[name='IndexValue']").text(Index);
        new_category.find("[name='OPValue']").text(op);
        new_category.find("[name='HDValue']").text(hd);

        new_category.attr('name', _name);


        $('#category_panel').append(new_category);
    }

        //需要添加小类
    var _li = $('.strategy_template').clone();
    _li.removeClass('sr-only');
    _li.removeClass('strategy_template');

    _li.find('i.OPValue').text(op);
    _li.find('i.HDValue').text(hd);

    var _ul = $('div.strategycategory[name=' + _name +']');
    var tt = _ul.children('li.list-group-item[OP=' + op + '][HD=' + hd + ']');
    if (tt.length != 0)
    {
        return;
    }

    var t = $('div.strategycategory[name=' + _name + '] ul.list-group');
    $('div.strategycategory[name=' + _name + '] ul.list-group').append(_li);

})