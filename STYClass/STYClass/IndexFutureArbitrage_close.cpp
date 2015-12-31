#include "StdAfx.h"
#include "STYClass.h"
#include "TimeUtil.h"

namespace STYClass
{
	CIndexFutureArbitrage_close::CIndexFutureArbitrage_close(void)
	{
		dTotalStockMarketValue=0;  //股票市值
		dStopedStockValue=0;   //停牌市值
		dDownlimitStockValue=0;   //跌停市值
		dTotalStockSellStrike=0;  //股票冲击
		drealStockIncome=0; //真实股票卖出收益
		dActualStockGain=0; //真实股票收益（考虑费用 冲击）

		dFutureBuyStrike=0; //期货买入冲击
		dActualFutureGain=0;  //真实期货收益

		 dzerobpgain=0;  //到0基差收益
	

	}

	CIndexFutureArbitrage_close::~CIndexFutureArbitrage_close(void)
	{

	}


	bool    CIndexFutureArbitrage_close::updateSecurityInfo(MarketInforStruct *MarketInfor, int &num)     //获得行情信息  
	{

		for (int i = 0; i<num; i++)
		{
			char m = MarketInfor[i].msecurity.cSecuritytype;
			if (MarketInfor[i].msecurity.cSecuritytype == 's')
			{
				this->m_SyntheticIndex.updateInfo(&MarketInfor[i]);  //复制行情
			}

			if (MarketInfor[i].msecurity == this->m_future.m_DepthMarketData.msecurity) //期货
			{
				this->m_future.updateInfo(&MarketInfor[i]);  //复制行情
			}

			/*if (MarketInfor[i].msecurity == this->m_index.m_DepthMarketData.msecurity) //期货
			{
				this->m_index.updateInfo(&MarketInfor[i]);  //复制行情
			}*/
		}

		return true;

	}
	bool    CIndexFutureArbitrage_close::getsubscribelist(securityindex* *psubscribelist, int& num)          //获得订阅的股票 必须在初始化结束后调用
	{
		int totalnum = this->m_SyntheticIndex.stockDb.size() + 2;
		if (*psubscribelist != 0)
			delete *psubscribelist;
		* psubscribelist = new securityindex[totalnum];
		securityindex *subscribelist = *psubscribelist;

		map<securityindex, CSecurity * >::iterator itor;

		itor = this->m_SyntheticIndex.stockDb.begin();

		num = 0;
		while (itor != m_SyntheticIndex.stockDb.end())
		{
			subscribelist[num].cSecuritytype = itor->first.cSecuritytype;
			strcpy(subscribelist[num].cSecurity_code, itor->first.cSecurity_code);
			itor++;
			num++;
		}
		//指数
		subscribelist[num].cSecuritytype = 'i';
		strcpy(subscribelist[num].cSecurity_code, this->m_index.m_DepthMarketData.msecurity.cSecurity_code);
		num++;
		//期货
		subscribelist[num].cSecuritytype = 'f';
		strcpy(subscribelist[num].cSecurity_code, this->m_future.m_DepthMarketData.msecurity.cSecurity_code);
		num++;
		return true;
	}

	bool     CIndexFutureArbitrage_close::init()	//初始化设置，导入权重数据  更新股票列表  
	{
		IndexFutureArbitragecloseinputargs  indexfuturearbitragecloseargs = *m_args;
		this->nHands = indexfuturearbitragecloseargs.nHands;  //手数
		this->dStockBonus = indexfuturearbitragecloseargs.dStockBonus;
		this->dGiftValue = indexfuturearbitragecloseargs.dGiftValue;
		this->dStockOpenCost = indexfuturearbitragecloseargs.dStockOpenCost;
		this->dFutureSellPoint = indexfuturearbitragecloseargs.dFutureSellPoint;
		this->dOpenedPoint = indexfuturearbitragecloseargs.dOpenedPoint; //开仓基差
		this->dExpectedGain = indexfuturearbitragecloseargs.dExpectedGain; //预期收益
		this->dShortCharge = indexfuturearbitragecloseargs.dShortCharge;
		//this->bTradingAllowed = indexfuturearbitrageopenargs.bTradingAllowed;
		m_future.setcode(indexfuturearbitragecloseargs.contractCode); //初始期货
		m_index.setcode(indexfuturearbitragecloseargs.indexCode);     //初始化指数
		//初始化position文件 
		indexfuturearbitragecloseargs.weightlistnum = 0;//对于期现平仓套利，无须权重文件

		if (indexfuturearbitragecloseargs.positionlist != 0)
			delete indexfuturearbitragecloseargs.positionlist;

		//if (indexfuturearbitragecloseargs.positionlistnum <= 0)
			//return false;

		indexfuturearbitragecloseargs.positionlist= new  stockpotionstruct[indexfuturearbitragecloseargs.positionlistnum];
		//stringtoweightlist(indexfuturearbitragecloseargs.weightliststr, indexfuturearbitragecloseargs.weightlist, indexfuturearbitragecloseargs.weightlistnum);
		if (!stringtopositionlist(indexfuturearbitragecloseargs.positionliststr, indexfuturearbitragecloseargs.positionlist, indexfuturearbitragecloseargs.positionlistnum))
			return false;
		if (!m_SyntheticIndex.init(indexfuturearbitragecloseargs.weightlist, indexfuturearbitragecloseargs.weightlistnum, indexfuturearbitragecloseargs.positionlist, indexfuturearbitragecloseargs.positionlistnum, indexfuturearbitragecloseargs.indexCode))
			return  false;
		// 初始化模拟指数类型
		return   true;

	}
	bool	 CIndexFutureArbitrage_close::calculateSimTradeStrikeAndDelta() //计算模拟指数，交易指数，调整基差
	{

		m_SyntheticIndex.updatepositioninfor();   //更新position文件，便与生产traderlist文件

		dTotalStockMarketValue = m_SyntheticIndex.getrealmarketvalue(dStopedStockValue);  //获取position的市值
		drealStockIncome = m_SyntheticIndex.getrealsellgain();
		dTotalStockSellStrike = dTotalStockMarketValue - drealStockIncome;  //冲击

		double drealFutureBuyCost = m_future.getrealbuycost(nHands);
		dFutureBuyStrike = m_future.getrealmarketvalue(nHands) -drealFutureBuyCost;

		this->dActualStockGain = this->drealStockIncome + this->dGiftValue + this->dStockBonus - this->dStockOpenCost - (this->dShortCharge*this->dTotalStockMarketValue + this->nHands * 100);
		dActualFutureGain = (this->dFutureSellPoint *nHands - drealFutureBuyCost)*m_future.getfuturetime();
		return  true;
	}
	bool	 CIndexFutureArbitrage_close::isOpenPointReached()				//是否达到开仓点，行情，资金
	{
		if (!this->m_SyntheticIndex.isupdated() || !this->m_future.isupdated()) //行情
		{
			strcpy(this->statusmsg, "行情有问题");
			return false;
		}

		if (!CTimeUtil::isAutoTradingTime())  //交易时间
		{
			strcpy(this->statusmsg, "非交易时间");
			return false;
		}
		if (dActualFutureGain + dActualStockGain < this->dExpectedGain)
		{
			strcpy(this->statusmsg, "等待交易");
			return false;
		}
		else
		{
			strcpy(this->statusmsg, "正常运行");
			return true;
		}
	}

	/*****显示参数****/
	bool    CIndexFutureArbitrage_close::gettaderargs(IndexFutureArbitragecloseinputargs &realargs)    //获得实际运行中的参数 包含samp文件
	{
		realargs.nHands = this->nHands;  //手数
		realargs.dExpectedGain = this->dExpectedGain; //预期收益
		realargs.dFutureSellPoint = this->dFutureSellPoint;
		realargs.dGiftValue = this->dGiftValue;
		realargs.dShortCharge = this->dShortCharge;
		realargs.dStockBonus = this->dStockBonus;
		realargs.dStockOpenCost = this->dStockOpenCost;
		realargs.dOpenedPoint = this->dOpenedPoint;
		//realargs.bTradingAllowed=this->bTradingAllowed;
		strcpy(realargs.contractCode, m_future.m_DepthMarketData.msecurity.cSecurity_code); //初始期货
		strcpy(realargs.indexCode, m_index.m_DepthMarketData.msecurity.cSecurity_code);    //初始化指数
		//复制position文件  weight文件不返回 
		int num = 0;
		list<stockpotionstruct>::iterator itor;
		itor = this->m_SyntheticIndex.m_positionlist.begin();
		while (itor != this->m_SyntheticIndex.m_positionlist.end())
		{
			realargs.positionlist[num].bstoped = itor->bstoped;
			realargs.positionlist[num].dlastprice = itor->dlastprice;
			realargs.positionlist[num].ntradervolume = itor->ntradervolume;
			realargs.positionlist[num].sSecurity = itor->sSecurity;
			num++;
			itor++;
		}
		realargs.positionlistnum = num;
		return true;
	}
	bool    CIndexFutureArbitrage_close::getshowstatus(IndexFutureArbitragecloseshowargs & msg)
	{
		msg.dTotalStockMarketValue = dTotalStockMarketValue;  //股票市值
		msg.dStopedStockValue = dStopedStockValue;   //停牌市值
		msg.dDownlimitStockValue = 0;   //跌停市值
		msg.dTotalStockSellStrike = dTotalStockSellStrike;  //股票冲击
		msg.drealStockIncome = drealStockIncome; //真实股票卖出收益
		msg.dActualStockGain = dActualStockGain; //真实股票收益（考虑费用 冲击）

		msg.dFutureBuyStrike = dFutureBuyStrike; //期货买入冲击
		msg.dActualFutureGain = dActualFutureGain;  //真实期货收益
		msg.dtotalgain = dActualFutureGain + this->dActualStockGain;		 //全部收益
		msg.dzerobpgain = 0;	 //到0基差收益
		strcpy(statusmsg, this->statusmsg);			//错误原因 或状态

		return true;
	}
	/**********获取交易*******/
	bool    CIndexFutureArbitrage_close::gettaderlist(Traderorderstruct ** mp_stockorders, int &num)
	{
		if (*mp_stockorders != 0)
			delete (*mp_stockorders);
		* mp_stockorders = new Traderorderstruct[this->m_SyntheticIndex.m_positionlist.size() + 1];   //shen qing  nei cun 
		Traderorderstruct * m_stockorders = *mp_stockorders;
		int stockordernum = 0;						   //委托数量
		list<stockpotionstruct>::iterator itor;
		itor = this->m_SyntheticIndex.m_positionlist.begin();
		while (itor != this->m_SyntheticIndex.m_positionlist.end())
		{
			if (itor->bstoped)
			{
				itor++;
				continue;
			}
			/*******生成交易报单********/
			strcpy(m_stockorders[stockordernum].cSecurity_code, itor->sSecurity.cSecurity_code);
			m_stockorders[stockordernum].cSecuritytype = itor->sSecurity.cSecuritytype;
			m_stockorders[stockordernum].nSecurity_amount = itor->ntradervolume;
			m_stockorders[stockordernum].dOrderprice = itor->dlastprice*0.98;   //以2%的溢价限价卖出
			if (m_stockorders[stockordernum].dOrderprice< itor->ddownlimitprice)
				m_stockorders[stockordernum].dOrderprice = itor->ddownlimitprice;  //跌停价
			strcpy(m_stockorders[stockordernum].cExchangeID, getExchangeNumByStockCode(itor->sSecurity.cSecurity_code));

			m_stockorders[stockordernum].cOffsetFlag = 0;  //股票不需要
			m_stockorders[stockordernum].cOrderexecutedetail = 0; //保留 暂不使用
			m_stockorders[stockordernum].cOrderlevel = 1;  //优先级
			m_stockorders[stockordernum].cOrderPriceType = 0; //股票只有限价单  不需要
			m_stockorders[stockordernum].cTraderdirection = '2';
			//‘1’---- - 买入（所有市场，特殊使用参见功能号0x201录入委托功能号的说明）
			//‘2’---- - 卖出（所有市场，特殊使用参见功能号0x20

			itor++;
			stockordernum++;
		}

		/**************期货********/
		strcpy(m_stockorders[stockordernum].cSecurity_code, this->m_future.m_DepthMarketData.msecurity.cSecurity_code);
		m_stockorders[stockordernum].cSecuritytype = this->m_future.m_DepthMarketData.msecurity.cSecuritytype;
		strcpy(m_stockorders[stockordernum].cExchangeID, "cf");
		m_stockorders[stockordernum].cOffsetFlag = '1'; //0  开 1 平
		m_stockorders[stockordernum].cOrderPriceType = '2'; //限价
		m_stockorders[stockordernum].dOrderprice = this->m_future.getlastprice()+5; //已高于实时价5报单
		m_stockorders[stockordernum].cTraderdirection = '0'; //0 买入 1 卖出
		m_stockorders[stockordernum].cOrderlevel = 1;  //优先级
		m_stockorders[stockordernum].cOrderexecutedetail = 0; //保留 暂不使用
		m_stockorders[stockordernum].nSecurity_amount = nHands;
		stockordernum++;

		num = stockordernum;

		return true;
	}
}