#include "StdAfx.h"
#include "STYClass.h"
#include "TimeUtil.h"

namespace STYClass
{
	CIndexFutureArbitrage_close::CIndexFutureArbitrage_close(void)
	{

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

			if (MarketInfor[i].msecurity == this->m_index.m_DepthMarketData.msecurity) //期货
			{
				this->m_index.updateInfo(&MarketInfor[i]);  //复制行情
			}
		}

		return true;

	}
	bool    CIndexFutureArbitrage_close::getsubscribelist(securityindex* subscribelist, int& num)          //获得订阅的股票 必须在初始化结束后调用
	{
		int totalnum = this->m_SyntheticIndex.stockDb.size() + 2;
		//securityindex * subscribelist = new securityindex[totalnum];


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

	bool     CIndexFutureArbitrage_close::init(IndexFutureArbitragecloseinputargs      indexfuturearbitragecloseargs)	//初始化设置，导入权重数据  更新股票列表  
	{

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
			return false;
		}

		if (!CTimeUtil::isAutoTradingTime())  //交易时间
		{
			return false;
		}
		if (dActualFutureGain + dActualStockGain < this->dExpectedGain)
		{
			return false;
		}
		else
			return true;
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
		return true;
	}

	/**********获取交易*******/
	bool    CIndexFutureArbitrage_close::gettaderlist(Traderorderstruct * m_stockorders, int &num)
	{
		int stockordernum = 0;						   //委托数量

		list<stockpotionstruct>::iterator itor;
		itor = this->m_SyntheticIndex.m_positionlist.begin();
		while (itor != this->m_SyntheticIndex.m_positionlist.end())
		{
			if (itor->bstoped)
				continue;
			/*******生成交易报单********/
			strcpy(m_stockorders[stockordernum].cSecurity_code, itor->sSecurity.cSecurity_code);
			m_stockorders[stockordernum].cSecuritytype = itor->sSecurity.cSecuritytype;
			m_stockorders[stockordernum].nSecurity_amount = itor->ntradervolume;
			m_stockorders[stockordernum].dOrderprice = itor->dlastprice*0.98;   //以2%的溢价限价卖出
			if (m_stockorders[stockordernum].dOrderprice< itor->ddownlimitprice)
				m_stockorders[stockordernum].dOrderprice = itor->ddownlimitprice;  //跌停价

			/**********************/
			itor++;
			stockordernum++;
		}

		num = stockordernum;

		return true;
	}
}