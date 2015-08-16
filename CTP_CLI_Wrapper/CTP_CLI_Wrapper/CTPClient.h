#pragma once

#include "struct_m.h"
#include "CTPSpi.h"
#include "ThostFtdcTraderApi.h"

using namespace System;

namespace CTP_CLI
{

	public ref class CCTPClient
	{
	private:
		CCTPSpi* spi;
		CThostFtdcTraderApi* tradeApi;
		
		

	public:
		System::Int32 FrontID;
		System::Int32 SessionID;
		System::String^ investor;
		String^ password;
		String^ broker;
		String^ address;
		System::Int32 MaxOrderRef;

	public:
		CCTPClient(System::String^ _investor, System::String^ _pwd, System::String^ _broker, System::String^ _addr);
		CCTPClient(System::String^ _investor, System::String^ _pwd);

	public:
		void Connect();
		void Disconnect();
		int ReqUserLogin(); 
		int ReqUserLogOut();
		int OrderInsert(CThostFtdcInputOrderField_M^ Order);
		int SettlementInfoConfirm();
		/// <summary>
		/// 开平仓:限价单
		/// </summary>
		/// <param name="InstrumentID">合约代码</param>
		/// <param name="OffsetFlag">平仓:仅上期所平今时使用CloseToday/其它情况均使用Close</param>
		/// <param name="Direction">买卖</param>
		/// <param name="Price">价格</param>
		/// <param name="Volume">手数</param>
		int OrderInsert(System::String^ InstrumentID, EnumOffsetFlagType OffsetFlag, EnumDirectionType Direction, System::Double Price, System::Int32 Volume);
		//市价单
		/// <summary>
		/// 开平仓:市价单
		/// </summary>
		/// <param name="InstrumentID"></param>
		/// <param name="OffsetFlag">平仓:仅上期所平今时使用CloseToday/其它情况均使用Close</param>
		/// <param name="Direction"></param>
		/// <param name="Volume"></param>
		int OrderInsert(System::String^ InstrumentID, EnumOffsetFlagType OffsetFlag, EnumDirectionType Direction, System::Int32 Volume);

		/// <summary>
		/// 开平仓:触发单
		/// </summary>
		/// <param name="InstrumentID"></param>
		/// <param name="ConditionType">触发单类型</param>
		/// <param name="ConditionPrice">触发价格</param>
		/// <param name="OffsetFlag">平仓:仅上期所平今时使用CloseToday/其它情况均使用Close</param>
		/// <param name="Direction"></param>
		/// <param name="PriceType">下单类型</param>
		/// <param name="Price">下单价格:仅当下单类型为LimitPrice时有效</param>
		/// <param name="Volume"></param>
		int OrderInsert(System::String^ InstrumentID, EnumContingentConditionType ConditionType, System::Double ConditionPrice, EnumOffsetFlagType OffsetFlag, EnumDirectionType Direction, EnumOrderPriceTypeType PriceType, System::Double Price, System::Int32 Volume);
		int ReqOrderAction(CThostFtdcInputOrderActionField_M^ Order);
		
		/// <summary>
		/// 请求查询报单:不填-查所有
		/// </summary>
		/// <param name="_exchangeID"></param>
		/// <param name="_timeStart"></param>
		/// <param name="_timeEnd"></param>
		/// <param name="_instrumentID"></param>
		/// <param name="_orderSysID"></param>
		int QryOrder(System::String^ _exchangeID, System::String^ _timeStart,System::String^ _timeEnd,System::String^ _instrumentID, System::String^ _orderSysID);

		int QryOrder();
		/// <summary>
		///  请求查询成交:不填-查所有
		/// </summary>
		/// <param name="_exchangeID"></param>
		/// <param name="_timeStart"></param>
		/// <param name="_timeEnd"></param>
		/// <param name="_instrumentID"></param>
		/// <param name="_tradeID"></param>
		int QryTrade(System::DateTime _timeStart, System::DateTime _timeEnd
			, System::String^ _instrumentID, System::String^ _exchangeID, System::String^ _tradeID);

		int QryTrade();

		int QryInvestorPosition(System::String^ instrument);
		/// <summary>
		/// 查询帐户资金请求
		/// </summary>
		int QryTradingAccount();

		/// <summary>
		/// 请求查询投资者
		/// </summary>
		int QryInvestor();



	private:
		void SetFunction2Callback();
		void RaiseFrontConnected();
		void RaiseFrontDisconnected(int nReason);
		void RaiseResUserLogin(CThostFtdcRspUserLoginField* pRspUserLogin, CThostFtdcRspInfoField* pRspInfo, int nRequestID, bool bIsLast);
		void RaiseRspUserLogout(CThostFtdcUserLogoutField* pUserLogout, CThostFtdcRspInfoField* pRspInfo, int nRequestID, bool bIsLast);
		void RaiseHeartBeatWarning(int nTimeLapse);
		void RaiseRspError(CThostFtdcRspInfoField *pRspInfo, int nRequestID, bool bIsLast) ;
		void RaiseRtnOrder(CThostFtdcOrderField *pOrder) ;
		void RaiseRtnTrade(CThostFtdcTradeField *pTrade) ;
		void RaiseRspOrderAction(CThostFtdcInputOrderActionField *pInputOrderAction, CThostFtdcRspInfoField *pRspInfo, int nRequestID, bool bIsLast) ;
		void RaiseRspOrderInsert(CThostFtdcInputOrderField *pInputOrder, CThostFtdcRspInfoField *pRspInfo, int nRequestID, bool bIsLast) ;
		void RaiseErrRtnOrderAction(CThostFtdcOrderActionField *pOrderAction, CThostFtdcRspInfoField *pRspInfo) ;
		void RaiseErrRtnOrderInsert(CThostFtdcInputOrderField *pInputOrder, CThostFtdcRspInfoField *pRspInfo) ;
		void RaiseRspQryTrade(CThostFtdcTradeField *pTrade, CThostFtdcRspInfoField *pRspInfo, int nRequestID, bool bIsLast) ;
		void RaiseRspQryTradingCode(CThostFtdcTradingCodeField *pTradingCode, CThostFtdcRspInfoField *pRspInfo, int nRequestID, bool bIsLast) ;
		void RaiseRspQryTradingNotice(CThostFtdcTradingNoticeField *pTradingNotice, CThostFtdcRspInfoField *pRspInfo, int nRequestID, bool bIsLast) ;
		void RaiseRspQryOrder(CThostFtdcOrderField *pOrder, CThostFtdcRspInfoField *pRspInfo, int nRequestID, bool bIsLast) ;
		void RaiseRspQryTradingAccount(CThostFtdcTradingAccountField *pTradingAccount, CThostFtdcRspInfoField *pRspInfo, int nRequestID, bool bIsLast) ;
		void RaiseRspQryInvestor(CThostFtdcInvestorField *pInvestor, CThostFtdcRspInfoField *pRspInfo, int nRequestID, bool bIsLast) ;
		void RaiseRspQryInvestorPosition(CThostFtdcInvestorPositionField *pInvestorPosition, CThostFtdcRspInfoField *pRspInfo, int nRequestID, bool bIsLast) ;
		void RaiseRspSettlementInfoConfirm(CThostFtdcSettlementInfoConfirmField *pSettlementInfoConfirm, CThostFtdcRspInfoField *pRspInfo, int nRequestID, bool bIsLast);

#pragma region events
#pragma region OnFrontConnected
	protected:
		OnFrontConnected^ myOnFrontConnected;
	public:
		event OnFrontConnected^ FrontConnected
		{
		public:
			void add(OnFrontConnected^ _d){this->myOnFrontConnected+=_d;}
			void remove(OnFrontConnected^ _d){this->myOnFrontConnected-=_d;}
		public:
			void raise()
			{
				this->myOnFrontConnected();
			}
		}
		#pragma endregion OnFrontConnected

#pragma region OnFrontDisonnected
	protected:
		OnFrontDisconnected^ myOnFrontDisconnected;
	public:
		event OnFrontDisconnected^ FrontDisconnected
		{
		public:
			void add(OnFrontDisconnected^ _d){this->myOnFrontDisconnected+=_d;}
			void remove(OnFrontDisconnected^ _d){this->myOnFrontDisconnected-=_d;}
		public:
			void raise(System::Int32 nReason)
			{
				this->myOnFrontDisconnected(nReason);
			}
		}
#pragma endregion OnFrontDisonnected
	
#pragma region OnHeartBeatWarning
	protected:
		OnHeartBeatWarning^ myOnHeartBeatWarning;
	public:
		event OnHeartBeatWarning^ HeartBeatWarning
		{
		public:
			void add(OnHeartBeatWarning^ _d){this->myOnHeartBeatWarning+=_d;}
			void remove(OnHeartBeatWarning^ _d){this->myOnHeartBeatWarning-=_d;}
		public:
			void raise(int nTimeLapes)
			{
				myOnHeartBeatWarning(nTimeLapes);
			}
		}
#pragma endregion OnHeartBeatWarning

#pragma region OnRspUserLogin
	protected:
		OnRspUserLogin^ myOnRspUserLogin;
	public:
		event OnRspUserLogin^ RspUserLogin
		{
		public:
			void add(OnRspUserLogin^ _d){this->myOnRspUserLogin+=_d;}
			void remove(OnRspUserLogin^ _d){this->myOnRspUserLogin-=_d;}
		public:
			void raise(CThostFtdcRspUserLoginField_M^ pRspUserLogin, CThostFtdcRspInfoField_M^ pRspInfo, int nRequestID, bool bIsLast)
			{
				if(myOnRspUserLogin)
					myOnRspUserLogin(pRspUserLogin,pRspInfo,nRequestID,bIsLast);
			}
		}
#pragma endregion OnRspUserLogin

#pragma region OnRspUserLogout
	protected:
		OnRspUserLogout^ myOnRspUserLogout;
	public:
		event OnRspUserLogout^ RspUserLogout
		{
		public:
			void add(OnRspUserLogout^ _d){this->myOnRspUserLogout+=_d;}
			void remove(OnRspUserLogout^ _d){this->myOnRspUserLogout-=_d;}
		public:
			void raise(CThostFtdcUserLogoutField_M^ pUserLogout, CThostFtdcRspInfoField_M^ pRspInfo, int nRequestID, bool bIsLast)
			{
				if(myOnRspUserLogout)
					myOnRspUserLogout(pUserLogout,pRspInfo,nRequestID,bIsLast);
			}
		}
#pragma endregion OnRspUserLogout

		

#pragma region OnRspError
	protected:
		OnRspError^ myOnRspError;
	public:
		event OnRspError^ RspError
		{
		public:
			void add(OnRspError^ _d){this->myOnRspError+=_d;}
			void remove(OnRspError^ _d){this->myOnRspError-=_d;}
		public:
			void raise(CThostFtdcRspInfoField_M^ pRspInfo, int nRequestID, bool bIsLast)
			{
				myOnRspError(pRspInfo,nRequestID,bIsLast);
			}
		}
#pragma endregion OnRspError

#pragma region OnRtnOrder
	protected:
		OnRtnOrder^ myOnRtnOrder;
	public:
		event OnRtnOrder^ RtnOrder
		{
		public:
			void add(OnRtnOrder^ _d){this->myOnRtnOrder+=_d;}
			void remove(OnRtnOrder^ _d){this->myOnRtnOrder-=_d;}
		public:
			void raise(CThostFtdcOrderField_M^ pOrder)
			{
				myOnRtnOrder(pOrder);
			}
		}
#pragma endregion OnRtnOrder

#pragma region OnRtnTrade
	protected:
		OnRtnTrade^ myOnRtnTrade;
	public:
		event OnRtnTrade^ RtnTrade
		{
		public:
			void add(OnRtnTrade^ _d){this->myOnRtnTrade+=_d;}
			void remove(OnRtnTrade^ _d){this->myOnRtnTrade-=_d;}
		public:
			void raise(CThostFtdcTradeField_M^ pTrade)
			{
				myOnRtnTrade(pTrade);
			}
		}
#pragma endregion OnRtnTrade

#pragma region OnRspOrderAction
	protected:
		OnRspOrderAction^ myOnRspOrderAction;
	public:
		event OnRspOrderAction^ RspOrderAction
		{
		public:
			void add(OnRspOrderAction^ _d){this->myOnRspOrderAction+=_d;}
			void remove(OnRspOrderAction^ _d){this->myOnRspOrderAction-=_d;}
		public:
			void raise(CThostFtdcInputOrderActionField_M^ pInputOrderAction, CThostFtdcRspInfoField_M^ pRspInfo, int nRequestID, bool bIsLast) 
			{
				myOnRspOrderAction(pInputOrderAction,pRspInfo,nRequestID,bIsLast);
			}
		}
#pragma endregion OnRspOrderAction
		
#pragma region OnRspOrderInsert
	protected:
		OnRspOrderInsert^ myOnRspOrderInsert;
	public:
		event OnRspOrderInsert^ RspOrderInsert
		{
		public:
			void add(OnRspOrderInsert^ _d){this->myOnRspOrderInsert+=_d;}
			void remove(OnRspOrderInsert^ _d){this->myOnRspOrderInsert-=_d;}
		public:
			void raise(CThostFtdcInputOrderField_M^ pInputOrder, CThostFtdcRspInfoField_M^ pRspInfo, int nRequestID, bool bIsLast) 
			{
				myOnRspOrderInsert(pInputOrder,pRspInfo,nRequestID,bIsLast);
			}
		}
#pragma endregion OnRspOrderInsert
		
#pragma region OnErrRtnOrderAction
	protected:
		OnErrRtnOrderAction^ myOnErrRtnOrderAction;
	public:
		event OnErrRtnOrderAction^ ErrRtnOrderAction
		{
		public:
			void add(OnErrRtnOrderAction^ _d){this->myOnErrRtnOrderAction+=_d;}
			void remove(OnErrRtnOrderAction^ _d){this->myOnErrRtnOrderAction-=_d;}
		public:
			void raise(CThostFtdcOrderActionField_M^ pOrderAction, CThostFtdcRspInfoField_M^ pRspInfo)
			{
				myOnErrRtnOrderAction(pOrderAction,pRspInfo);
			}
		}
#pragma endregion OnErrRtnOrderAction

#pragma region OnErrRtnOrderInsert
	protected:
		OnErrRtnOrderInsert^ myOnErrRtnOrderInsert;
	public:
		event OnErrRtnOrderInsert^ ErrRtnOrderInsert
		{
		public:
			void add(OnErrRtnOrderInsert^ _d){this->myOnErrRtnOrderInsert+=_d;}
			void remove(OnErrRtnOrderInsert^ _d){this->myOnErrRtnOrderInsert-=_d;}
		public:
			void raise(CThostFtdcInputOrderField_M^ pInputOrder, CThostFtdcRspInfoField_M^ pRspInfo)
			{
				myOnErrRtnOrderInsert(pInputOrder,pRspInfo);
			}
		}
#pragma endregion OnErrRtnOrderInsert
		
#pragma region OnRspQryTrade
	protected:
		OnRspQryTrade^ myOnRspQryTrade;
	public:
		event OnRspQryTrade^ RspQryTrade
		{
		public:
			void add(OnRspQryTrade^ _d){this->myOnRspQryTrade+=_d;}
			void remove(OnRspQryTrade^ _d){this->myOnRspQryTrade-=_d;}
		public:
			void raise(CThostFtdcTradeField_M^ pTrade, CThostFtdcRspInfoField_M^ pRspInfo, int nRequestID, bool bIsLast)
			{
				myOnRspQryTrade(pTrade,pRspInfo,nRequestID,bIsLast);
			}
		}
#pragma endregion OnRspQryTrade

#pragma region OnRspQryTradingCode
	protected:
		OnRspQryTradingCode^ myOnRspQryTradingCode;
	public:
		event OnRspQryTradingCode^ RspQryTradingCode
		{
		public:
			void add(OnRspQryTradingCode^ _d){this->myOnRspQryTradingCode+=_d;}
			void remove(OnRspQryTradingCode^ _d){this->myOnRspQryTradingCode-=_d;}
		public:
			void raise(CThostFtdcTradingCodeField_M^ pTradingCode, CThostFtdcRspInfoField_M^ pRspInfo, int nRequestID, bool bIsLast)
			{
				myOnRspQryTradingCode(pTradingCode,pRspInfo,nRequestID,bIsLast);
			}
		}
#pragma endregion OnRspQryTradingCode

#pragma region OnRspQryTradingNotice
	protected:
		OnRspQryTradingNotice^ myOnRspQryTradingNotice;
	public:
		event OnRspQryTradingNotice^ RspQryTradingNotice
		{
		public:
			void add(OnRspQryTradingNotice^ _d){this->myOnRspQryTradingNotice+=_d;}
			void remove(OnRspQryTradingNotice^ _d){this->myOnRspQryTradingNotice-=_d;}
		public:
			void raise(CThostFtdcTradingNoticeField_M^ pTradingNotice, CThostFtdcRspInfoField_M^ pRspInfo, int nRequestID, bool bIsLast)
			{
				myOnRspQryTradingNotice(pTradingNotice,pRspInfo,nRequestID,bIsLast);
			}
		}
#pragma endregion OnRspQryTradingNotice
		
#pragma region OnRspQryOrder
	protected:
		OnRspQryOrder^ myOnRspQryOrder;
	public:
		event OnRspQryOrder^ RspQryOrder
		{
		public:
			void add(OnRspQryOrder^ _d){this->myOnRspQryOrder+=_d;}
			void remove(OnRspQryOrder^ _d){this->myOnRspQryOrder-=_d;}
		public:
			void raise(CThostFtdcOrderField_M^ pOrder, CThostFtdcRspInfoField_M^ pRspInfo, int nRequestID, bool bIsLast) 
			{
				myOnRspQryOrder(pOrder,pRspInfo,nRequestID,bIsLast);
			}
		}
#pragma endregion myOnRspQryOrder
		
#pragma region OnRspQryTradingAccount
	protected:
		OnRspQryTradingAccount^ myOnRspQryTradingAccount;
	public:
		event OnRspQryTradingAccount^ RspQryTradingAccount
		{
		public:
			void add(OnRspQryTradingAccount^ _d){this->myOnRspQryTradingAccount+=_d;}
			void remove(OnRspQryTradingAccount^ _d){this->myOnRspQryTradingAccount-=_d;}
		public:
			void raise(CThostFtdcTradingAccountField_M^ pTradingAccount, CThostFtdcRspInfoField_M^ pRspInfo, int nRequestID, bool bIsLast) 
			{
				myOnRspQryTradingAccount(pTradingAccount,pRspInfo,nRequestID,bIsLast);
			}
		}
#pragma endregion OnRspQryTradingAccount
		
#pragma region OnRspQryInvestor
	protected:
		OnRspQryInvestor^ myOnRspQryInvestor;
	public:
		event OnRspQryInvestor^ RspQryInvestor
		{
		public:
			void add(OnRspQryInvestor^ _d){this->myOnRspQryInvestor+=_d;}
			void remove(OnRspQryInvestor^ _d){this->myOnRspQryInvestor-=_d;}
		public:
			void raise(CThostFtdcInvestorField_M^ pInvestor, CThostFtdcRspInfoField_M^ pRspInfo, int nRequestID, bool bIsLast) 
			{
				myOnRspQryInvestor(pInvestor,pRspInfo,nRequestID,bIsLast);
			}
		}
#pragma endregion OnRspQryInvestor

#pragma region OnRspQryInvestorPosition
	protected:
		OnRspQryInvestorPosition^ myOnRspQryInvestorPosition;
	public:
		event OnRspQryInvestorPosition^ RspQryInvestorPosition
		{
		public:
			void add(OnRspQryInvestorPosition^ _d){this->myOnRspQryInvestorPosition+=_d;}
			void remove(OnRspQryInvestorPosition^ _d){this->myOnRspQryInvestorPosition-=_d;}
		public:
			void raise(CThostFtdcInvestorPositionField_M^ pInvestorPosition, CThostFtdcRspInfoField_M^ pRspInfo, int nRequestID, bool bIsLast)
			{
				myOnRspQryInvestorPosition(pInvestorPosition,pRspInfo,nRequestID,bIsLast);
			}
		}
#pragma endregion OnRspQryInvestorPosition

#pragma region OnRspSettlementInfoConfirm
	protected:
		OnRspSettlementInfoConfirm^ myOnRspSettlementInfoConfirm;
	public:
		event OnRspSettlementInfoConfirm^ RspSettlementInfoConfirm
		{
		public:
			void add(OnRspSettlementInfoConfirm^ _d){this->myOnRspSettlementInfoConfirm+=_d;}
			void remove(OnRspSettlementInfoConfirm^ _d){this->myOnRspSettlementInfoConfirm-=_d;}
		public:
			void raise(CThostFtdcSettlementInfoConfirmField_M^ pSettlementInfoConfirm, CThostFtdcRspInfoField_M^ pRspInfo, int nRequestID, bool bIsLast)
			{
				myOnRspSettlementInfoConfirm(pSettlementInfoConfirm,pRspInfo,nRequestID,bIsLast);
			}
		}
#pragma endregion OnRspSettlementInfoConfirm
		
		#pragma endregion events
	};

}
