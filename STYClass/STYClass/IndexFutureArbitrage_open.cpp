#include "StdAfx.h"
#include "STYClass.h"
#include "TimeUtil.h"

namespace STYClass 
{
CIndexFutureArbitrage_open::CIndexFutureArbitrage_open(void)
{

}
CIndexFutureArbitrage_open::~CIndexFutureArbitrage_open(void)
{

}

bool    CIndexFutureArbitrage_open::updateSecurityInfo(MarketInforStruct * MarketInfor,int &num)      //获得行情信息  
{
	for(int i=0;i<num;i++)
	{
		if(MarketInfor[num].msecurity.cSecuritytype=='s')
		{
			this->m_SyntheticIndex.updateInfo(&MarketInfor[num]);  //复制行情
		}

		if(MarketInfor[num].msecurity==this->m_future.m_DepthMarketData.msecurity) //期货
		{
			this->m_future.updateInfo(&MarketInfor[num]);  //复制行情
		}

		if(MarketInfor[num].msecurity==this->m_index.m_DepthMarketData.msecurity) //期货
		{
			this->m_index.updateInfo(&MarketInfor[num]);  //复制行情
		}
	}

	return true;
}
bool   CIndexFutureArbitrage_open::getsubscribelist(securityindex* subscribelist,int& num)            //获得订阅的股票 
 {
	int totalnum = this->m_SyntheticIndex.stockDb.size() + 2;
	 //securityindex * subscribelist = new securityindex[totalnum];


	map<securityindex ,CSecurity * >::iterator itor;
	
	 itor=this->m_SyntheticIndex.stockDb.begin();
	 
	 num = 0;
	 while(itor!=m_SyntheticIndex.stockDb.end())
       {    
		   subscribelist[num].cSecuritytype=itor->first.cSecuritytype;
		   strcpy( subscribelist[num].cSecurity_code,itor->first.cSecurity_code);
		   itor++;
		   num++;
       } 
	 //指数
	 subscribelist[num].cSecuritytype='i';
	 strcpy(subscribelist[num].cSecurity_code,this->m_indexfuturearbitrageopenargs.indexCode);
	 itor++;
     num++;

	 subscribelist[num].cSecuritytype='f';
	 strcpy(subscribelist[num].cSecurity_code,this->m_indexfuturearbitrageopenargs.contractCode);
	 
     num++;
	 return true;
 }

bool   CIndexFutureArbitrage_open::init(IndexFutureArbitrageopeninputargs      m_indexfuturearbitrageopenargs)
{

	m_future.setcode(m_indexfuturearbitrageopenargs.contractCode); //初始期货

	//初始化position文件 
	if(m_indexfuturearbitrageopenargs.weightlistnum==0)
		return false;   //如果权重文件为空 

	m_SyntheticIndex.init( m_indexfuturearbitrageopenargs.weightlist,m_indexfuturearbitrageopenargs.weightlistnum,m_indexfuturearbitrageopenargs.positionlist,m_indexfuturearbitrageopenargs.positionlistnum,m_indexfuturearbitrageopenargs.indexCode);					 // 初始化模拟指数

	return   true;
}


bool CIndexFutureArbitrage_open::calculateSimTradeStrikeAndDelta() //计算模拟指数，交易指数，调整基差
{
	m_SyntheticIndex.updatepositioninfor();   //更新position文件

	dTotalStocksMarketValue=m_SyntheticIndex.getrealmarketvalue();
	this->dSimIndex=m_SyntheticIndex.getSimIndex();
	this->dSimerror=this->dSimIndex-m_index.getlastprice();
	this->dTradeIndex=dTotalStocksMarketValue/m_indexfuturearbitrageopenargs.nHands;
	dOrgDelta=m_future.getlastprice()-m_index.getlastprice();

	dTotalStockBuyStrike=m_SyntheticIndex.getrealbuycost()-dTotalStocksMarketValue;
	dFutureSellStrike=m_future.getrealmarketvalue(m_indexfuturearbitrageopenargs.nHands)-m_future.getrealsellgain(m_indexfuturearbitrageopenargs.nHands);

	this->dPositiveDelta = m_future.getlastprice() 
							- this->dSimIndex
							- (this->dTotalStockBuyStrike+this->dFutureSellStrike)/(m_indexfuturearbitrageopenargs.nHands*m_future.getfuturetime());
	return  true;
}

bool CIndexFutureArbitrage_open::isOpenPointReached()
{
	if(!this->m_SyntheticIndex.isupdated()||!this->m_future.isupdated())
	   return false;
	if(abs(this->dSimerror)>6)
		return false;
	if(!CTimeUtil::isAutoTradingTime())
		return false;
	 return true;
}

/*bool CIndexFutureArbitrage_open::doOpenAction()					//开仓,生成批量委托
{

	  Traderorderstruct temptraderorder;
	  list<stockpotionstruct>::iterator itor;
	  itor=this->m_SyntheticIndex.m_positionlist.begin();
	 while(itor!=this->m_SyntheticIndex.m_positionlist.end())
       { 
		   if(itor->bstoped)
			   continue;
		   /*******生成交易报单********/
		  /* strcpy_s(temptraderorder.cSecurity_code,itor->cSecurity_code);
		
		   itor++;
       }
	return true;
	
}
*/


bool   CIndexFutureArbitrage_open::gettaderargs(IndexFutureArbitrageopeninputargs &realargs)   //获得实际运行中的参数 包含samp文件
{
	
	return true;
}
bool   CIndexFutureArbitrage_open::getshowstatus(char * showstr)
	{
		strcpy(showstr,"test");
		return true;
	}
	 /**********获取交易*******/
bool   CIndexFutureArbitrage_open::gettaderlist(Traderorderstruct *, int &num)
	{
		return true;

	}
}