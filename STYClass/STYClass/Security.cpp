#include "StdAfx.h"
#include "Security.h"
#include "TimeUtil.h"

CSecurity::CSecurity(void)
{
}

CSecurity::~CSecurity(void)
{
}

bool  CSecurity::setcode(char * temp)
{
	strcpy(this->m_DepthMarketData.msecurity.cSecurity_code,temp);
	 return true;
}

double   CSecurity::getlastprice()
{
	return this->m_DepthMarketData.marketinfor.dLastPrice;
}

bool   CSecurity::isstoped()
{
	return this->m_DepthMarketData.marketinfor.bStoped ;
 }
bool   CSecurity::isupdated()
{
	//WaitForSingleObject( hlocalDataMutex, INFINITE ); 	
	int nCurrentTime =  CTimeUtil::getIntTime();
	if (CTimeUtil::getDeltaSecond(nCurrentTime, this->m_DepthMarketData.marketinfor.LastUpdateTime / 1000)>5 || this->m_DepthMarketData.marketinfor.nInfotLag>5)
	    this->m_DepthMarketData.bupdated=false;
	else
		this->m_DepthMarketData.bupdated=true;
	//ReleaseMutex( hlocalDataMutex );
	return this->m_DepthMarketData.bupdated;
}
double  CSecurity::getrealmarketvalue(int namount)
{
	return   namount*this->m_DepthMarketData.marketinfor.dLastPrice;;
}
double  CSecurity::getrealbuycost(int namount)
{
		
	if(this->m_DepthMarketData.marketinfor.bStoped)
		return getrealmarketvalue(namount);
	if(!this->m_DepthMarketData.bupdated)//没有更新
		return getrealmarketvalue(namount)*1.02;
	if(this->m_DepthMarketData.marketinfor.dLastPrice==this->m_DepthMarketData.marketinfor.dHighLimited) //涨停冲击为0
		return getrealmarketvalue(namount);
	int nQtyLeft = namount;
	if(namount==0)
		return 0;
	double cost = 0;
	int i =0;
	double dAskVol;
	double dAskPrice;
	while (nQtyLeft>0)
	{
		dAskPrice = this->m_DepthMarketData.marketinfor.dAskPrice[i];
		dAskVol = this->m_DepthMarketData.marketinfor.dAskVol[i];
		if(dAskPrice ==0){ 
			cost +=nQtyLeft * this->m_DepthMarketData.marketinfor.dLastPrice;
			return cost;
		}
		if(dAskVol >= nQtyLeft){
			cost +=nQtyLeft * dAskPrice;
			return cost;
		}else{
			cost +=dAskVol * dAskPrice;
			nQtyLeft -=dAskVol;
			i++;
		}
		
		if(i==this->m_DepthMarketData.nMarketdepth)
		{
			cost +=nQtyLeft * (2*this->m_DepthMarketData.marketinfor.dAskPrice[m_DepthMarketData.nMarketdepth-1]-this->m_DepthMarketData.marketinfor.dAskPrice[m_DepthMarketData.nMarketdepth-2]);
			
		}
		
	}

	return cost;
}
double  CSecurity::getrealsellgain(int namount)
{
	
	if(this->m_DepthMarketData.marketinfor.bStoped)
		return getrealmarketvalue(namount);
	if(!this->m_DepthMarketData.bupdated)//没有更新
		return getrealmarketvalue(namount)*0.98;
	if(this->m_DepthMarketData.marketinfor.dLastPrice==this->m_DepthMarketData.marketinfor.dLowLimited) //跌停冲击为0
		return getrealmarketvalue(namount);
	int nQtyLeft = namount;
	if(namount==0)
		return 0;
	double dGain = 0;
	int i =0;
	double dBidPrice,dBidVol;
	while (nQtyLeft>0)
	{
		dBidPrice = this->m_DepthMarketData.marketinfor.dBidPrice[i];
		dBidVol = this->m_DepthMarketData.marketinfor.dBidVol[i];
		if(dBidPrice ==0){
			dGain +=nQtyLeft * this->m_DepthMarketData.marketinfor.dLastPrice;
			return dGain;
		}
		if(dBidVol >= nQtyLeft){
			dGain +=nQtyLeft * dBidPrice;
			return dGain;
		}else{
			dGain +=dBidVol * dBidPrice;
			nQtyLeft -=dBidVol;
			i++;
		}

		if(i==this->m_DepthMarketData.nMarketdepth) 
		{
			dGain +=nQtyLeft * (2*this->m_DepthMarketData.marketinfor.dBidPrice[this->m_DepthMarketData.nMarketdepth-1]-this->m_DepthMarketData.marketinfor.dBidPrice[this->m_DepthMarketData.nMarketdepth-2]);
			
		}
	}

	return dGain;
}
void CSecurity::updateInfo(MarketInforStruct * tempmarketinfor)
{	
	this->m_DepthMarketData.marketinfor.update(tempmarketinfor);

}