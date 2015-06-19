#ifndef __SWAGX_H
#define __SWAGX_H

#include <winsock.h>

typedef unsigned short WORD;
typedef unsigned char  BYTE;

#pragma pack(1)
typedef struct SWI_BlockHead
{
	WORD    block_size; 	// 整个消息块字节数，包含自身
	WORD    crc;       		// 从下个字段block_type起 (不包括block_size和
                   			// crc字段本身) 的CRC校验码
	BYTE    block_type;  	// 块类型  	1 - 请求         （SWI_???Request）
					 		//         	2 - 返回状态     （SWI_???Return）
					 		//          3 - 查询结果行   （SWI_???Result）
					 		//          5 - 取消查询
					 		//          4 - 警报信息
					 		//          5 - 网络对话
					 		//          6 - 连接请求(交换密钥)
					 		//          7 - 连接应答(交换密钥)
					 		//          8 - 扩展请求消息
					 		//          9 - 扩展应答消息
	BYTE    reserved1;    	// 保留，必须置0
	WORD    function_no;  	// 功能号
	long    cn_id;      	// 网关连接号
	WORD    reserved2;  	// 保留，内部使用（原营业部）
	WORD    dest_dpt;    	// 目的营业部编号
} SWI_BlockHead;

typedef struct SWI_ConnectRequest
{
	SWI_BlockHead  head;    	// 消息头 block_type == 6
	WORD   method;     	   		// 客户端的加密方式
								//0表示不加密
	char   entrust_method;   	// 委托方式（见数据字典说明）
	BYTE   return_cp;        	// 客户端希望的返回结构中中文字段的代码页
					     		//   	0 -- MBCS  （CP936简体中文）
					     		//   	1 -- Unicode(Java客户需要)
	BYTE   network[4];      	// 客户端Novell网段号
	BYTE   address[6];      	// 客户端网卡地址（Novell节点地址）
	WORD   client_major_ver;  // 客户端协议接口主版本号（用于版本协商）
	WORD   client_minor_ver;   // 客户端协议接口次版本号（用于版本协商）
	WORD   key_length;			// RSA密钥长度（对method 1, key_length=0）
} SWI_ConnectRequest;

typedef struct SWI_ConnectResponse
{
	SWI_BlockHead  head;    	// 消息头 block_type == 7
    WORD return_value;			// 返回状态
	WORD method;				// 网关同意的加密方式
	char department_name[60];	// 营业部名称（MBCS/Unicode）
	WORD key_length;			// DES密钥长度（key_length=8）
	BYTE des_key[8];			// DES密钥(请用"ExpressT"作为密钥des解密)
} SWI_ConnectResponse;

typedef	struct SWI_ReturnHead
{
	SWI_BlockHead head;
	long          return_status;
} SWI_ReturnHead;

typedef struct SWI_ResultHead
{
	SWI_BlockHead head;
	WORD          row_no;
} SWI_ResultHead;

typedef struct SWI_OpenAccountRequest
{
	SWI_BlockHead  head;    	// 消息头 block_type == 1
	char   account_type;     	// 客户帐号类型
	char   account[16];      	// 客户帐号
	char   pwd[8];           	// 交易密码（如为操作员登陆，则为操作员密码）
	short  op_code;             // 操作员工号
	unsigned  long  flag;      //"特殊条件"判断标志位组合,每一个二进制位对应一个"特殊条件",缺省值为0表示不判断任何"特殊条件"
	char   productno[7];		//产品信息编号：5位电脑网络中心产品编码+2位子
							    //  产品编码（共7位字符）；缺省值为空。
	char   note[30];			//备注：目前用于在客户委托时存放外围客户登陆的MAC地址 或IP 地址或电话号码等信息。
	char   note2[30];           // 备注，和note字段共同使用，具体使用方法见注意事项5
	char   login_flag;          // 记录本此登录信息的标志，'0'--不记录（默认）；'1'--记录，客户界面主动发起的登录应填'1'
} SWI_OpenAccountRequest;

typedef struct SWI_ErrorMsgRequest
{
	struct SWI_BlockHead head;  // function_no==0x901, block_type==1
	long    error_no;           // 出错代码
} SWI_ErrorMsgRequest;

typedef struct SWI_ErrorMsgReturn
{
	struct SWI_BlockHead head;	//function_no==0x901,block_type==2
	long    return_status;      // 返回状态
	char    error_msg[40];      // 错误信息（MBCS/Unicode）
}SWI_ErrorMsgReturn;

struct SWI_OpenAccountReturn
{
	struct SWI_BlockHead head; 	// function_no==0x101
	long    return_status;  		// 返回状态
	BYTE    flag;				// (新扩充字段)大集中网关标志，0—否（默认）；1—是	当客户号登录返回1时,适用于【大集中约定】
	char    last_login_date[9];  // 上次登录的日期，格式为：YYYYMMDD
	char    last_login_time[9];  // 上次登录的时间，格式为：HH:MM:SS
	char    last_full_note[60];  // 上次的登陆信息，参见注意事项5的说明
};

struct SWI_AccountLoginRequest
{
	struct SWI_BlockHead head;	// function_no==0x111, block_type == 1
	char   account_type;     	// 客户帐号类型（见数据字典说明）
	char   account[16];      	// 客户帐号
	char   pwd[8];           	// 交易密码
	unsigned  long   flag;         //"特殊条件"判断标志位组合,每一个二进制位对应一个"特殊条件",缺省值为0表示不判断任何"特殊条件"
	char   productno [7];		    //产品信息编号：5位电脑网络中心产品编码+2位子	产品编码（共7位字符）；缺省值为空。
	char   note[30];				//备注：目前用于在客户委托时存放外围客户登陆的MAC地址 或IP 地址或电话号码等信息。
	char   note2[30];           // 备注，和note字段共同使用，具体使用方法见0x101注意事项5
	char   login_flag;           // 记录本此登录信息的标志，‘0’——不记录（默认）；	‘1’——记录，客户界面主动发起的登录应填‘1’


};

struct SWI_AccountLoginResult
{
	struct SWI_BlockHead head;     	// function==0x111,block_type==3
	WORD   row_no;          			// 记录号，0xFFFF表示记录集结束
	long   bankbook_number;         // 资金帐号
	char   account_type;  			// 帐号类型（见数据字典, 不含资金帐号‘0’）
	char   security_account[16];	 	// 证券帐号
};
struct SWI_AccountLoginReturn
{
	struct SWI_BlockHead head;		// function_no = 0x111,block_type==2
	long   return_status;     		// 返回状态
	long    bankbook_number;     	// 资金帐号
	BYTE    account_status;      	// 帐号状态（见数据字典说明）
	char    name[20];            	// 客户姓名（MBCS/Unicode）
	char    id[19];      			// 身份证号
	BYTE    card_version;          	//磁卡版本号
	char		customer_flag[16];      //客户个性化信息标志
	char		Cust_flag[32];		//客户权限标志（字段中如出现R——允许融资融券）
	char		Cust_risk_type;		//客户风险评级类别（见数据字典说明）
	short   depart_number;			//营业部编码（4位）
	char    last_login_date[9];     // 上次登录的日期，格式为：YYYYMMDD
	char    last_login_time[9];     // 上次登录的时间，格式为：HH:MM:SS
	char    last_full_note[60];     // 上次的登陆信息，参见0x101注意事项5的说明
};
typedef struct _SWIMoney    // Same as the type CURRENCY in VB
{
	DWORD lo_value;
	long  hi_value;
} SWIMoney;
typedef long	 SWISmallMoney;
struct SWI_AddEntrustRequest
{
	struct SWI_BlockHead head;// function_no == 0x201, block_type==1//16
	char    account_type;     // 客户帐号类型（见数据字典说明）//17
	char    account[16];      // 客户帐号//33
	BYTE    exchange_no;      // 交易所编号（见数据字典说明）//34
	char    bs_type;          // 买卖类别（见数据字典说明）//35
	char    security_code[7]; // 证券代码//42
	long    stock_amount;     // 委托数量//46
	SWISmallMoney price;      // 委托价格（SWISmallMoney见数据字典说明，下同）//50
	short   effect_days;      // 有效天数 //52
	SWIMoney   apply_amount;  //申请金额 //60
	BYTE    mark;             //是否连续赎回 1为连续赎回,0为非连续 确省为0//61
	int		Frozen_no ;        //证券端资金冻结流水号(银证通专用，缺省为0)//65
	SWIMoney		fund_amount;  //委托数量(支持开放式基金小数份额)//73
	char    Entrust_sign[10]; //交易签名(缺省为’’)//83
	SWIMoney    Cash_limit ;  //委托资金额度（缺省为0表示不控制委托资金额度;否	则委托冻结资金超过委托资金额度时本笔委托失败,B		股暂不控制）//91
	char 	MarketOrder_type[2]; //价格类型（见数据字典说明）（缺省‘00’表示限价委托）//93
	char	Trade_type;		//交易性质（见数据字典说明）（缺省‘’表示现金交易）//94
	char    CompNo[3];		//海外公司编号//97
	long	contract_no;	//约定号（价格类型为‘11’时需要输入）//101
	char 	PBU_no[8];		//对方交易单元（价格类型为‘11’时需要输入）//109
};
struct SWI_AddEntrustReturn
{
	struct SWI_BlockHead head;
	long return_status;   	      	//  返回状态
	long entrust_sn;      			//  委托编号
	SWIMoney  frozen_money;    	//冻结资金金额
	long     frozen_amount;     	//冻结证券数量
};

struct SWI_QueryEntrustRequest
{
	struct SWI_BlockHead head; 	// function_no == 0x402 ,block_type==1
	char    account_type;    	// 客户帐号类型（见数据字典说明）
	char    account[16];     	// 客户帐号	
	BYTE    exchange_no;     	// 交易所编号（见数据字典说明）
	long    begin_date;      	// 起始日期（形如：yyyymmdd）
	long    end_date;        	// 结束日期（形如：yyyymmdd）
	long    entrust_sn;      	// 委托编号（默认为0，0表示全部）
	char    security_code[7];	// 股票代码（默认为空，空表示全部）
	WORD    max_results;     	// 最大查询记录数（默认为0）
	char 	stock_type;			// 证券类别, 缺省为空，查询全部证券类别. 	（见数据字典说明）
};

struct SWI_QueryEntrustResult
{
	struct SWI_BlockHead head; // function_no==0x402,block_type==3
	WORD row_no;               // 记录号,  0xFFFF结果集结束
	long entrust_date;         //  委托日期（形如：yyyymmdd）
	long entrust_sn;           //  委托编号
	char entrust_time[9];      //  委托时间（形如：hh:mm:ss）
	BYTE exchange_no;          //  交易所编号（见数据字典说明）
	char security_account[11]; //  股东代码
	char security_code[7];     //  证券代码
	char security_name[18];    //  证券名称（MBCS/Unicode）
	char bs_type;              //  买卖类别（见数据字典说明）
	long stock_ammount;        //  委托数量
	SWISmallMoney price;       //  委托价格
	SWIMoney frozen_money;     //  冻结金额/基金申请金额
	char  entrust_method;      //  委托方式（见数据字典说明）
	char  entrust_status;      //  委托状态（见数据字典说明）
	short operator_no;         //  操作员工号
	long  bargain_ammount;     //  已成交数量
	char  error_msg[32];		  //委托失败原因/备注；当买卖类别是场外开放式基金分红方式修改时表示所设定的分红方式（红利转投/现金分红）；当买卖类别是场外开放式基金转换时，表示目的证券代码。 
	BYTE    mark;             //表示是否连续赎回 1为连续赎回,0为非连续； 
	char stock_account[20];   //开放式基金帐号
	SWIMoney		fund_amount;  //委托数量（支持开放式基金小数份额）
	char    Entrust_sign[10]; //交易签名(缺省为’’)
	long     frozen_amount;     //冻结证券数量
	char 	MarketOrder_type[2];		//市价委托方式（缺省‘00’表示限价委托）
	char		Trade_type;		//交易性质（见数据字典说明）（缺省为空，表示现金交易）
	long  withdraw_ammount;     //  撤单数量
	long		contract_no;			//约定号
	char 	PBU_no[8];			//对方交易单元
	SWIMoney  done_money;        // 已成交金额
};
struct SWI_QueryEntrustReturn
{
	struct SWI_BlockHead head; 		// function_no==0x402,block_type==2
	long   return_status;      		// 返回状态
};

struct SWI_BatchDetail
{
	char bs_type;          //买卖类别（仅支持‘1‘-买入,’2‘-卖出）
						//implement_mode=’1’时要求每笔委托方向一致以第一笔为准）
	char security_code[7]; // 证券代码
	long stock_amount;     // 委托数量
	SWISmallMoney price;   // 委托价格
	char entrust_sign[10]; // 交易签名(缺省为’’)（仅implement_mode=’0’时有效）
};

struct SWI_BatchEntrustRequest
{
	struct SWI_BlockHead head;// function_no == 0x205, block_type==1
	char    account_type;     // 客户帐号类型（见数据字典说明）
	char    account[16];      // 客户帐号
	BYTE    exchange_no;      // 交易所编号（见数据字典说明）
	long   entrust_length;    // 委托笔数(implement_mode=’0’时最大支持50，
	//          implement_mode=‘1’时最大支持15)
	WORD    detail_block_size; // 委托明细结构体字节大小，
							   //注释By谭大礼：我觉得是没有包含自身的，不然detail_block_size应该= 笔数x sizeof(SWI_BatchDetail) +2
	SWI_BatchDetail	detail_buf[50]; // 委托明细内容，见后说明；
	SWIMoney    Cash_limit ;  //委托资金额度（缺省为0表示不控制委托资金额度;
	//超过委托资金总额度后的委托返回失败
	//(仅implement_mode=’0’时有效）
	long      trade_interval; //每笔委托之间停留的间隔，单位为毫秒,默认200ms
	//(仅implement_mode=’0’时有效)
	char      implement_mode; // 实现模式（新增参数默认‘0’：AGC实现，比较慢，一次最多支持50笔
	//                       ‘1’：后台实现.比较快，一次最多支持15笔）
};
struct SWI_BatchEntrustResult
{
	struct SWI_BlockHead head; // function_no==0x205,block_type==3
	WORD row_no;               // 记录号,  0xFFFF结果集结束
	long return_status;        // 返回状态: >  0  成功 <= 0  失败
	long entrust_sn;           // 委托编号: 返回状态<0为失败
	SWIMoney frozen_money;     // 冻结金额
	long     frozen_amount;     // 冻结证券数量
	char    Entrust_sign[10];  // 交易签名(缺省为’’)
	char    err_msg[60];       // 错误原因，当委托成功时，返回为空；委托失败时，返回失败原因
};
struct SWI_BatchEntrustReturn
{
	struct SWI_BlockHead head;
	long return_status;              // 返回状态；>0 调用成功 〈=0调用失败
};
struct SWI_QueryBalanceSheetRequest
{
	struct SWI_BlockHead head;	//function_no == 0x303,block_type==1
	char    account_type;       // 客户帐号类型（见数据字典说明）
	char    account[16];        // 客户帐号
	char    currency_type;      // 币种（见数据字典说明，默认为人民币，不能为空）
};
struct SWI_QueryBalanceSheetReturn
{
	struct SWI_BlockHead head; 	// function_no == 0x303 block_type==2
	long      return_status; 	// 返回状态
	char      name[20];      	// 股东姓名（MBCS/Unicode）
	SWIMoney		net_assets;    //净资产
	SWIMoney		total_liabilities;	//负债总额
	SWIMoney		fund_liabilities; 	//资金负债
	SWIMoney		stock_liabilities;	//股票负债
	SWIMoney		profit_loss_in_shortsale;	//融券盈亏
	SWIMoney		assure_rate;	//担保率
	SWIMoney		deposit_usable;	//保证金可用额度
	SWIMoney		credit_range;	//授信额度
	SWIMoney	    financeable;		//可融资买入额度
	SWIMoney	    shortsale_usable;	//可融券卖出额度
	SWIMoney		usable;		//可用资金
	SWIMoney		market_value	;	//市值
	SWIMoney		total_assets	;	//总资产
	SWIMoney		total_profit_loss;	//总浮动盈亏
	SWIMoney		exceptional_frozen;	//异常冻结资金
	SWIMoney		fund_balance;	//资金余额
	SWIMoney		today_bought_money;	//当日买入成交金额
	SWIMoney		today_sell_money;	//当日卖出成交金额
	SWIMoney		today_bought_nobargain;	//当日买入未成交金额
	SWIMoney		bought_nobargain;	//买入未交收金额
	SWIMoney		sell_nobargain;	//卖出未交收金额
};


SOCKET AGCConnect(char *ip,WORD port,char entrust_way,WORD *method,char *des_key,int *key_len);
int AGCSend(SOCKET sock,char *pSendData,WORD method,const char *des_key);

int endes_data(const char *des_key,char *buf2,int len);
int dedes_data(const char *des_key,char *buf2,int len);
WORD CalCRC(void *pData, int nDataLen);

#endif
