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
	bool   CIndexFutureArbitrage_open::getsubscribelist(securityindex* * psubscribelist, int& num)            //获得订阅的股票 
	{
		int totalnum = this->m_SyntheticIndex.stockDb.size() + 2;
		if (*psubscribelist != 0)
			delete *psubscribelist;
		*psubscribelist = new securityindex[totalnum];
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
		if (m_args->weightlistnum <= 0 || m_args->positionlistnum<=0)  //对于期现套利，必须有权重文件
			return false;
		if (m_args->weightlist != 0)
			delete m_args->weightlist;

		if (m_args->positionlist != 0)
			delete m_args->positionlist;

		m_args->weightlist = new  indexweightstruct[m_args->weightlistnum];
		m_args->positionlist = new  stockpotionstruct[m_args->positionlistnum];

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
		isOpenPointReached();
		return  true;
	}

	bool CIndexFutureArbitrage_open::isOpenPointReached()
	{
		if (!this->m_SyntheticIndex.isupdated() || !this->m_future.isupdated()) //行情
		{
			strcpy(this->statusmsg, "行情有问题");
			return false;
		}
		//if (abs(this->dSimerrorPre) > 0.002)  //模拟误差大于千分之2  
		//{
			//return false;
		//}
		if (!CTimeUtil::isAutoTradingTime())  //交易时间
		{
			strcpy(this->statusmsg, "非交易时间");
			return false;
		}
		if (this->dPositiveDelta > this->dExpectOpenDelta) // 大于预设值，则允许开仓
		{
			strcpy(this->statusmsg, "等待交易");
			return true;

		}
		else
		{
			strcpy(this->statusmsg, "正常运行");
			return false;
		}

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
		msg.futureprice = this->m_future.getlastprice(); //期货价格
		msg.indexprice = this->m_index.getlastprice(); //指数价格
		msg.SimIndex = this->dSimIndex; //模拟指数
		msg.OrgDeltaPre = this->dOrgDeltaPre;//原始基差
		msg.SimerrorPre = this->dSimerrorPre;//模拟误差

		msg.TotalStocksMarketValue = this->dTotalStocksMarketValue; //模拟市值
		msg.stopmarketvalue = this->stopmarketvalue;//停盘市值
		msg.uplimitmarketvalue = 0;//
		msg.TotalStockBuyStrike = this->dTotalStockBuyStrike;//买入冲击
		msg.dFutureSellStrike = this->dFutureSellStrike;//期货卖出冲击
		msg.dPositiveDelta = this->dPositiveDelta;//调整基差
		msg.SimtraderPre = this->dSimtraderPre;//交易误差
		strcpy(msg.statusmsg, this->statusmsg);//显示状态 
		return true;
	}
	/**********获取交易*******/
	bool   CIndexFutureArbitrage_open::gettaderlist(Traderorderstruct **mp_stockorders, int &num)
	{
		int stockordernum = 0;						   //委托数量
		if (*mp_stockorders != 0)
			delete (*mp_stockorders);
		*mp_stockorders = new Traderorderstruct[this->m_SyntheticIndex.m_positionlist.size() + 1];   //shen qing  nei cun 
		Traderorderstruct * m_stockorders = *mp_stockorders;
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
			m_stockorders[stockordernum].nSecurity_amount = 100;// itor->ntradervolume; //测试
			m_stockorders[stockordernum].dOrderprice = itor->dlastprice*1.02;   //以2%的溢价限价买入
			if (m_stockorders[stockordernum].dOrderprice > itor->duplimitprice)
				m_stockorders[stockordernum].dOrderprice = itor->duplimitprice;  //涨停价

			strcpy(m_stockorders[stockordernum].cExchangeID, getExchangeNumByStockCode(itor->sSecurity.cSecurity_code));

			m_stockorders[stockordernum].cOffsetFlag = 0;  //股票不需要
			m_stockorders[stockordernum].cOrderexecutedetail = 0; //保留 暂不使用
			m_stockorders[stockordernum].cOrderlevel = 1;  //优先级
			m_stockorders[stockordernum].cOrderPriceType = 0; //股票只有限价单  不需要
			m_stockorders[stockordernum].cTraderdirection = '1'; //买入
			/**********************/


			itor++;
			stockordernum++;
		}

		/**************期货********/
		strcpy(m_stockorders[stockordernum].cSecurity_code, this->m_future.m_DepthMarketData.msecurity.cSecurity_code);
		m_stockorders[stockordernum].cSecuritytype = this->m_future.m_DepthMarketData.msecurity.cSecuritytype;
		strcpy(m_stockorders[stockordernum].cExchangeID, "cf");
		m_stockorders[stockordernum].cOffsetFlag = '0'; //0  开 1 平
		m_stockorders[stockordernum].cOrderPriceType = '2'; //限价
		m_stockorders[stockordernum].dOrderprice = this->m_future.getlastprice() -10; //已低于实时价5报单
		m_stockorders[stockordernum].cTraderdirection = '1'; //0 买入 1 卖出
		m_stockorders[stockordernum].cOrderlevel = 1;  //优先级
		m_stockorders[stockordernum].cOrderexecutedetail = 0; //保留 暂不使用
		m_stockorders[stockordernum].nSecurity_amount = nHands;
		stockordernum++;
		num = stockordernum;

		return true;

	}

}//namespace STYClass 
