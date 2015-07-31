#include "Stdafx.h"
#include "managedSTYClass.h"

using namespace System;
using namespace System::Collections::Generic;
using namespace managedSTY;


Strategy_OPEN::Strategy_OPEN()
{
	m_open_strategy = new CIndexFutureArbitrage_open();
};

Strategy_OPEN::~Strategy_OPEN()
{
	delete m_open_strategy;
};

bool Strategy_OPEN::updateSecurityInfo(array<managedMarketInforStruct^>^ marketinfo, int num){
	MarketInforStruct * MarketInfo = new MarketInforStruct();
	
	for (int i = 0; i < 10; i++)
	{
		MarketInfo->dAskPrice[i] = 0;
		MarketInfo->dAskVol[i] = 0;
		MarketInfo->dBidPrice[i] = 0;
		MarketInfo->dBidVol[i] = 0;
	}

	MarketInfo = new MarketInforStruct();

	for (int i = 0; i < num; i++)
	{
		MarketInfo[i] = marketinfo[i]->CreateInstance();
	}
	return m_open_strategy->updateSecurityInfo(MarketInfo, num);
};

array<managedsecurityindex^>^ Strategy_OPEN::getsubscribelist(){

	securityindex*  subscribelist = new securityindex[1];
	array<managedsecurityindex^>^ securityIndexs;
	int num;
	if (m_open_strategy->getsubscribelist(subscribelist, num))
	{
		securityIndexs = gcnew array<managedsecurityindex^>(num);
		for (int i = 0; i < num; i++){
			managedsecurityindex^ index = gcnew managedsecurityindex();
			securityIndexs[i] = gcnew managedsecurityindex();

			index->cSecuritytype = subscribelist[i].cSecuritytype;
			index->cSecurity_code = gcnew String(subscribelist[i].cSecurity_code);
			securityIndexs[i]->cSecurity_code = gcnew String(index->cSecurity_code);
			securityIndexs[i]->cSecuritytype = index->cSecuritytype;
		
		}
		return securityIndexs;
	}

	return securityIndexs;
}

bool Strategy_OPEN::init(open_args^ m){
	IndexFutureArbitrageopeninputargs m_args = m->GetInstance();
	return m_open_strategy->init(m_args);
}

bool Strategy_OPEN::calculateSimTradeStrikeAndDelta(){
	return m_open_strategy->calculateSimTradeStrikeAndDelta();
}

bool Strategy_OPEN::isOpenPointReached(){
	return m_open_strategy->isOpenPointReached();
}

bool Strategy_OPEN::gettaderargs(open_args^ realargs){
	IndexFutureArbitrageopeninputargs m = realargs->GetInstance();

	bool b = m_open_strategy->gettaderargs(m);

	return b;
}

bool Strategy_OPEN::getshowstatus(String^ status){

	char* str = (char*)(void*)System::Runtime::InteropServices::Marshal::StringToHGlobalAnsi(status);
	return true;// m_open_strategy->getshowstatus(str);
}

array<managedTraderorderstruct^>^ Strategy_OPEN::getTradeList(){
	array<managedTraderorderstruct^>^ orderlist;
	Traderorderstruct *m_list;

	memset(m_list->cExchangeID, 0, 21);
	memset(m_list->cSecurity_code, 0, 31);
	memset(m_list->security_name, 0, 55);

	int m_num;

	bool b = m_open_strategy->gettaderlist(m_list, m_num);

	if (b == true){
		for (int i = 0; i < m_num; i++){
			orderlist[i] = gcnew managedTraderorderstruct();
			orderlist[i]->SetInstance(m_list[i]);
		}
	}

	return orderlist;
}