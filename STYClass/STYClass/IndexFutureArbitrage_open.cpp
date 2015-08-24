#include "StdAfx.h"
#include "STYClass.h"
#include "TimeUtil.h"

namespace STYClass
{
	CIndexFutureArbitrage_open::CIndexFutureArbitrage_open(void)
	{
		bTradingAllowed = false;//是否允许交易,勾"允许"时置为true	 用于返回 
		dSimIndex = 0;			   //模拟指数大小
		dSimerrorPre = 0;           //模拟误差，百分比 
		dSimtraderPre = 0;		 	   //交易指数大小

		dTotalStockBuyStrike = 0;	//买入股票的冲击	
		dTotalStocksMarketValue = 0;//要买入的股票的市值(不包含停牌)
		stopmarketvalue = 0;			 //停牌市值
		dFutureSellStrike = 0;		//卖出期货的冲击

		dOrgDeltaPre = 0;				//原始基差 = 期货 -指数
		dPositiveDelta = 0;			//调整后的基差 = 	期货 - 模拟指数 -冲击（换算成点）

	}
	CIndexFutureArbitrage_open::~CIndexFutureArbitrage_open(void)
	{

	}

	bool    CIndexFutureArbitrage_open::updateSecurityInfo(MarketInforStruct * MarketInfor, int &num)      //获得行情信息  
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
	bool   CIndexFutureArbitrage_open::getsubscribelist(securityindex* subscribelist, int& num)            //获得订阅的股票 
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

	//bool   CIndexFutureArbitrage_open::init(IndexFutureArbitrageopeninputargs* m)
	bool  CIndexFutureArbitrage_open::init()
	{
		//IndexFutureArbitrageopeninputargs      indexfuturearbitrageopenargs = *m;
		//this->nHands = indexfuturearbitrageopenargs.nHands;  //手数
		//this->dExpectOpenDelta = indexfuturearbitrageopenargs.dPositiveOpenDelta; //开仓点位
		//this->bTradingAllowed = indexfuturearbitrageopenargs.bTradingAllowed;
		//m_future.setcode(indexfuturearbitrageopenargs.contractCode); //初始期货
		//m_index.setcode(indexfuturearbitrageopenargs.indexCode);     //初始化指数
		////初始化position文件 
		//if(indexfuturearbitrageopenargs.weightlistnum==0)  //对于期现套利，必须有权重文件
		//	return false;   //如果权重文件为空 

		//if (!m_SyntheticIndex.init(indexfuturearbitrageopenargs.weightlist, indexfuturearbitrageopenargs.weightlistnum, indexfuturearbitrageopenargs.positionlist, indexfuturearbitrageopenargs.positionlistnum, indexfuturearbitrageopenargs.indexCode))
		//	return  false;
		//	// 初始化模拟指数类型
		//return   true;

		this->nHands = m_args->nHands;  //手数
		this->dExpectOpenDelta = m_args->dPositiveOpenDelta; //开仓点位
		this->bTradingAllowed = m_args->bTradingAllowed;
		m_future.setcode(m_args->contractCode); //初始期货
		m_index.setcode(m_args->indexCode);     //初始化指数
		//初始化position文件 
		if (m_args->weightlistnum == 0)  //对于期现套利，必须有权重文件
			return false;
		if (!stringtoweightlist(m_args->weightliststr, m_args->weightlist, m_args->weightlistnum))
			return false;
		if (!stringtopositionlist(m_args->positionliststr, m_args->positionlist, m_args->positionlistnum))
			return false;

		if (!m_SyntheticIndex.init(m_args->weightlist, m_args->weightlistnum, m_args->positionlist, m_args->positionlistnum, m_args->indexCode))
			return false;
		return true;

	}

	bool CIndexFutureArbitrage_open::calculateSimTradeStrikeAndDelta() //计算模拟指数，交易指数，调整基差
	{
		m_SyntheticIndex.updatepositioninfor();   //更新position文件，便与生产traderlist文件

		dTotalStocksMarketValue = m_SyntheticIndex.getrealmarketvalue(stopmarketvalue);  //获取position的市值
		this->dSimIndex = m_SyntheticIndex.getSimIndex();
		this->dSimerrorPre = (this->dSimIndex - m_index.getlastprice()) / m_index.getlastprice();
		this->dSimtraderPre = dTotalStocksMarketValue / (nHands *m_index.getlastprice()*m_future.getfuturetime());
		dOrgDeltaPre = (m_future.getlastprice() - m_index.getlastprice()) / m_index.getlastprice();  //原始基差 

		dTotalStockBuyStrike = m_SyntheticIndex.getrealbuycost() - dTotalStocksMarketValue;  //冲击
		dFutureSellStrike = (m_future.getrealmarketvalue(nHands) - m_future.getrealsellgain(nHands))*m_future.getfuturetime();

		this->dPositiveDelta = m_future.getlastprice()   //调整后的基差
			- this->dSimIndex
			- (this->dTotalStockBuyStrike + this->dFutureSellStrike) / (nHands*m_future.getfuturetime());
		return  true;
	}

	bool CIndexFutureArbitrage_open::isOpenPointReached()
	{
		if (!this->m_SyntheticIndex.isupdated() || !this->m_future.isupdated()) //行情
		{
			return false;
		}
		if (abs(this->dSimerrorPre) > 0.002)  //模拟误差大于千分之2  
		{
			return false;
		}
		if (!CTimeUtil::isAutoTradingTime())  //交易时间
		{
			return false;
		}
		if (this->dPositiveDelta > this->dExpectOpenDelta) // 大于预设值，则允许开仓
		{
			return true;
		}
		else
			return false;

	}

	bool   CIndexFutureArbitrage_open::gettaderargs(IndexFutureArbitrageopeninputargs &realargs)   //获得实际运行中的参数 包含samp文件
	{
		realargs.nHands = this->nHands;  //手数
		realargs.dPositiveOpenDelta = this->dExpectOpenDelta; //开仓点位
		realargs.bTradingAllowed = this->bTradingAllowed;
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
	bool   CIndexFutureArbitrage_open::getshowstatus(IndexFutureArbitrageopenshowargs & msg)
	{
		msg.futureprice = this->m_future.getlastprice();
		msg.indexprice = this->m_index.getlastprice();
		msg.SimIndex = this->dSimIndex;
		msg.OrgDeltaPre = this->dOrgDeltaPre;
		msg.SimerrorPre = this->dSimerrorPre;

		msg.TotalStocksMarketValue = this->dTotalStocksMarketValue;
		msg.stopmarketvalue = this->stopmarketvalue;
		msg.uplimitmarketvalue = 0;
		msg.TotalStockBuyStrike = this->dTotalStockBuyStrike;
		msg.dFutureSellStrike = this->dFutureSellStrike;
		msg.dPositiveDelta = this->dPositiveDelta;
		msg.SimtraderPre = this->dSimtraderPre;
		strcpy(msg.statusmsg, this->statusmsg);
		return true;
	}
	/**********获取交易*******/
	bool   CIndexFutureArbitrage_open::gettaderlist(Traderorderstruct *m_stockorders, int &num)
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
			m_stockorders[stockordernum].dOrderprice = itor->dlastprice*1.02;   //以2%的溢价限价买入
			if (m_stockorders[stockordernum].dOrderprice > itor->duplimitprice)
				m_stockorders[stockordernum].dOrderprice = itor->duplimitprice;  //涨停价

			/**********************/
			itor++;
			stockordernum++;
		}

		num = stockordernum;

		return true;

	}

}//namespace STYClass 
