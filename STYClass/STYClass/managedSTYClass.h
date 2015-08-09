#include "Stdafx.h"
#include "CDataStruct.h"
#include "STYClass.h"


using namespace System;
using namespace STYClass;

namespace managedSTY
{
	

	public ref struct managedsecurityindex{
		char cSecuritytype;
		String^ cSecurity_code;

		securityindex GetInstance(){
			char* code = (char*)(void*)System::Runtime::InteropServices::Marshal::StringToHGlobalAnsi(cSecurity_code);
			securityindex m;
			m.cSecuritytype = cSecuritytype;
			strcpy_s(m.cSecurity_code, 31, code);

			return m;
		}
	};

	public ref struct managedIndexWeights{
		managedsecurityindex^ sSecurity; //证券信息
		double dweight;  //权重

		indexweightstruct GetInstance(){
			indexweightstruct* m = new indexweightstruct();
			m->sSecurity = sSecurity->GetInstance();
			m->dweight = dweight;

			return *m;
		}
	};

	public ref struct managedstockposition {
		managedsecurityindex^ sSecurity; //证券信息
		int tradevolume; //数量
		bool bstoped;    //当前状态(是否停盘)
		double dlastprice;  //当前价格

	     stockpotionstruct  GetInstance(){
			stockpotionstruct* m = new stockpotionstruct();
			m->sSecurity = sSecurity->GetInstance();
			m->ntradervolume = tradevolume;
			m->bstoped = bstoped;
			m->dlastprice = dlastprice;

			return *m;
		}
	};

	public ref struct open_args{
		array<managedIndexWeights^>^ weightlist; //权重文件
		int weightlistnum; //权重文件数量

		array<managedstockposition^>^ positionlist; //显示持仓
		int positionlistNUM;

		int nHands; //手数
		String^ indexCode; //指数
		String^ contractCode; //期货合约
		double dPositiveOpenDelta; //开仓点位

		bool bTradingAllowed;  //是否允许交易
	};

	public ref struct close_args{
		array<managedIndexWeights^>^ weightlist;//权重文件
		int weightlistnum; //权重文件数量

		array<managedstockposition^>^ positionlist; //显示持仓
		int positionlistNUM;

		int nHands; //手数
		String^ indexCode; //指数
		String^ contractCode; //期货合约

		double dStockBonus = 0;		//分红
		double dGiftValue = 0;		//送股
		double dStockOpenCost = 0;  //开仓成本
		double dFutureSellPoint = 0; //期货开仓点

		double dOpenedPoint = 0;   //开仓基差点位
		double dExpectedGain = 0;  //预计收益
		double dShortCharge = 0;  //预估费率

		bool  bTradingAllowed;//是否允许交易,勾"允许"时置为true	
	};

	public ref struct managedMarketInforStruct{
		managedsecurityindex^ msecurity;
		String^    security_name;		//名称
		String^    exchangeID;			//交易所
		int		nTime;					//时间(HHMMSSmmm)
		int		nStatus;				//状态
		double  nPreClose;				//前收盘价
		double  dLastPrice;				//最新价
		array<double>^  dAskPrice;		//申卖价
		array<double>^  dAskVol;		//申卖量
		array<double>^  dBidPrice;		//申买价
		array<double>^  dBidVol;		//申买量
		double  dHighLimited;			//涨停价
		double  dLowLimited;			//跌停价

		bool bstoped;					//状态位   停牌  正常交易  熔断
		int nInfotLag;					//行情更新间隔
		int LastUpdateTime;				//行情最新接受更新时间(HHMMSSmmm)

		MarketInforStruct CreateInstance(){
			MarketInforStruct m;
			strcpy_s(m.security_name, 18, (char*)(void*)System::Runtime::InteropServices::Marshal::StringToHGlobalAnsi(security_name));
			m.nTime = nTime;
			m.nStatus = nStatus;
			m.nPreClose = nPreClose;
			m.dLastPrice = dLastPrice;
			
			for (int i = 0; i < 10; i++)
			{
				m.dAskPrice[i] = dAskPrice[i];
				m.dAskVol[i] = dAskVol[i];
				m.dBidPrice[i] = dBidPrice[i];
				m.dBidVol[i] = dBidVol[i];
			}

			m.dHighLimited = dHighLimited;
			m.dLowLimited = dLowLimited;

			m.bStoped = bstoped;
			m.nInfotLag = nInfotLag;
			m.LastUpdateTime = LastUpdateTime;
		

			m.msecurity = msecurity->GetInstance();

			return m;
		};
	};

	public ref struct managedTraderorderstruct
	{
		//交易部分
		String^    cExchangeID;            //交易所
		String^    cSecurity_code;     // 证券代码
		String^    security_name;      //证券名称
		long    nSecurity_amount;      // 委托数量
		double  dOrderprice;           // 委托价格
		char    cTraderdirection;      // 买卖类别（见数据字典说明）
		char    cOffsetFlag;           //开平标志
		char    cOrderPriceType;       //报单条件(限价  市价)

		//控制部分
		char    cSecuritytype;          //证券类型	
		char    cOrderlevel;             //报单优先级 执行顺序
		char    cOrderexecutedetail;     //报单执行细节

		managedTraderorderstruct(String^ mcExchangeID, String^ mcSecurity_code, String^ msecurity_name
			, long mnSecurity_amount, double mdOrderprice, char mcTraderdirection, char mcOffsetFlag
			, char mcOrderPriceType, char mcSecuritytype, char mcOrderlevel, char mcOrderexecutedetail)
		{
			//cExchangeID = (char*)(void*)System::Runtime::InteropServices::Marshal::StringToHGlobalAnsi(mcExchangeID);
			//cSecurity_code = (char*)(void*)System::Runtime::InteropServices::Marshal::StringToHGlobalAnsi(mcSecurity_code);
			//security_name = (char*)(void*)System::Runtime::InteropServices::Marshal::StringToHGlobalAnsi(msecurity_name);
			cExchangeID = mcExchangeID;
			cSecurity_code = mcSecurity_code;
			security_name = msecurity_name;
			nSecurity_amount = mnSecurity_amount;
			dOrderprice = mdOrderprice;
			cTraderdirection = mcTraderdirection;
			cOffsetFlag = mcOffsetFlag;
			cOrderPriceType = mcOrderPriceType;

			cSecuritytype = mcSecuritytype;
			cOrderlevel = mcOrderlevel;
			cOrderexecutedetail = mcOrderexecutedetail;

		};

		managedTraderorderstruct(){}

		void SetInstance(Traderorderstruct m){
			cExchangeID = gcnew String(m.cExchangeID);
			cSecurity_code = gcnew String(m.cSecurity_code);
			security_name = gcnew String(m.security_name);
			nSecurity_amount = m.nSecurity_amount;
			dOrderprice = m.dOrderprice;
			cTraderdirection = m.cTraderdirection;
			cOffsetFlag = m.cOffsetFlag;
			cOrderPriceType = m.cOrderPriceType;

			cSecuritytype = m.cSecuritytype;
			cOrderlevel = m.cOrderlevel;
			cOrderexecutedetail = m.cOrderexecutedetail;
		}

		Traderorderstruct createInstance()
		{
			Traderorderstruct unmanagedTraderorderstruct;
			char* exchangeid = (char*)(void*)System::Runtime::InteropServices::Marshal::StringToHGlobalAnsi(cExchangeID);
			strcpy_s(unmanagedTraderorderstruct.cExchangeID, 21, exchangeid);
			char* securitycode = (char*)(void*)System::Runtime::InteropServices::Marshal::StringToHGlobalAnsi(cSecurity_code);
			strcpy_s(unmanagedTraderorderstruct.cSecurity_code, 31, securitycode);
			char* securityname = (char*)(void*)System::Runtime::InteropServices::Marshal::StringToHGlobalAnsi(security_name);
			strcpy_s(unmanagedTraderorderstruct.security_name, 55, securityname);

			unmanagedTraderorderstruct.nSecurity_amount = nSecurity_amount;
			unmanagedTraderorderstruct.dOrderprice = dOrderprice;
			unmanagedTraderorderstruct.cTraderdirection = cTraderdirection;
			unmanagedTraderorderstruct.cOffsetFlag = cOffsetFlag;
			unmanagedTraderorderstruct.cOrderPriceType = cOrderPriceType;

			unmanagedTraderorderstruct.cSecuritytype = cSecuritytype;
			unmanagedTraderorderstruct.cOrderlevel = cOrderlevel;
			unmanagedTraderorderstruct.cOrderexecutedetail = cOrderexecutedetail;

			return unmanagedTraderorderstruct;
		}
	};


	//界面显示变量
	public ref struct managedIndexFutureArbitragecloseshowargs
	{
		double dTotalStockMarketValue = 0;  //股票市值
		double 	dStopedStockValue = 0;   //停牌市值
		double 	dDownlimitStockValue = 0;   //跌停市值
		double dTotalStockSellStrike = 0;  //股票冲击
		double drealStockIncome = 0; //真实股票卖出收益
		double dActualStockGain = 0; //真实股票收益（考虑费用 冲击）

		double dFutureBuyStrike = 0; //期货买入冲击
		double dActualFutureGain = 0;  //真实期货收益
		double dtotalgain = 0;		 //全部收益
		double  dzerobpgain = 0;	 //到0基差收益
		String^ statusmsg;			//错误原因 或状态
	};

	public ref class Strategy_OPEN
	{
	public:
		Strategy_OPEN();
		virtual ~Strategy_OPEN();

	public:
		/************行情部分********/
		void updateSecurityInfo(array<managedMarketInforStruct^>^ marketinfo, int num); //获得行情信息
		array<managedsecurityindex^>^ getsubscribelist();
		//bool getsubscribelist(array<managedsecurityindex^>^ securityIndex, int num);//获得订阅的股票，必须在初始化后调用

		/**********策略执行*******/
		void init(open_args^ m); //初始化设置，导入权重数据  更新股票列表  
		void calculateSimTradeStrikeAndDelta(); //计算模拟指数，交易指数，调整基差
		void isOpenPointReached(bool^ open); //是否达到开仓点，行情，资金

		/*****显示参数****/
		//bool   gettaderargs(open_args^ realargs);    //获得实际运行中的参数 包含samp文件
		void getshowstatus(String^ status); 

		/**********获取交易*******/
		array<managedTraderorderstruct^>^ getTradeList();

	private:
		CIndexFutureArbitrage_open* m_open_strategy;
	};

	public ref class Strategy_CLOSE
	{
	public :
		Strategy_CLOSE();
		virtual ~Strategy_CLOSE();

	public:
		/************行情部分********/
		void updateSecurityInfo(array<managedMarketInforStruct^>^ marketinfo, int num);//获得行情信息
		array<managedsecurityindex^>^ getsubscribelist();

		/**********策略执行*******/
		void init(close_args^ m);
		void calculateSimTradeStrikeAndDelta(); //计算模拟指数，交易指数，调整基差
		void isOpenPointReached(bool^ open); //是否达到开仓点，行情，资金

		/*****显示参数****/
		//close_args^ gettradeargs();
		managedIndexFutureArbitragecloseshowargs^ getshowstatus();

		/**********获取交易*******/
		array<managedTraderorderstruct^>^ getTradeList();

	private:
		CIndexFutureArbitrage_close* m_close_strategy;
	};
}