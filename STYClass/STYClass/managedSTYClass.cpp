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

void Strategy_OPEN::updateSecurityInfo(array<managedMarketInforStruct^>^ marketinfo, int num){
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
     m_open_strategy->updateSecurityInfo(MarketInfo, num);
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

void Strategy_OPEN::init(open_args^ m){
	/*IndexFutureArbitrageopeninputargs* m_args = new IndexFutureArbitrageopeninputargs[1];
	m_args->weightlist = new indexweightstruct[1];
	m_args->positionlist = new stockpotionstruct[1];


	m_args->weightlistnum = m->weightlistnum;


	for (int i = 0; i < m_args->weightlistnum; i++){
		m_args->weightlist[i].dweight = m->weightlist[i]->dweight;
		strcpy_s(m_args->weightlist[i].sSecurity.cSecurity_code, 31, (char*)(void*)System::Runtime::InteropServices::Marshal::StringToHGlobalAnsi(m->weightlist[i]->sSecurity->cSecurity_code));
		m_args->weightlist[i].sSecurity.cSecuritytype = m->weightlist[i]->sSecurity->cSecuritytype;
	}

	m_args->positionlistnum = m->positionlistNUM;

	for (int i = 0; i < m_args->positionlistnum; i++){
		m_args->positionlist[i].bstoped = m->positionlist[i]->bstoped;
		m_args->positionlist[i].dlastprice = m->positionlist[i]->dlastprice;
		m_args->positionlist[i].ntradervolume = m->positionlist[i]->tradevolume;
		m_args->positionlist[i].sSecurity.cSecuritytype = m->positionlist[i]->sSecurity->cSecuritytype;
		strcpy_s(m_args->positionlist[i].sSecurity.cSecurity_code, 31, (char*)(void*)System::Runtime::InteropServices::Marshal::StringToHGlobalAnsi(m->positionlist[i]->sSecurity->cSecurity_code));
	}

	m_args->nHands = m->nHands;
	strcpy_s(m_args->indexCode, 32, (char*)(void*)System::Runtime::InteropServices::Marshal::StringToHGlobalAnsi(m->indexCode));
	strcpy_s(m_args->contractCode, 32, (char*)(void*)System::Runtime::InteropServices::Marshal::StringToHGlobalAnsi(m->contractCode));

	m_args->dPositiveOpenDelta = m->dPositiveOpenDelta;
	m_args->bTradingAllowed = m->bTradingAllowed;*/

	m_open_strategy->m_args = new IndexFutureArbitrageopeninputargs();
	m_open_strategy->m_args->weightlist = new indexweightstruct[1];
	m_open_strategy->m_args->positionlist = new stockpotionstruct[1];

	m_open_strategy->m_args->weightlistnum = m->weightlistnum;


	for (int i = 0; i < m_open_strategy->m_args->weightlistnum; i++){
		m_open_strategy->m_args->weightlist[i].dweight = m->weightlist[i]->dweight;
		strcpy_s(m_open_strategy->m_args->weightlist[i].sSecurity.cSecurity_code, 31, (char*)(void*)System::Runtime::InteropServices::Marshal::StringToHGlobalAnsi(m->weightlist[i]->sSecurity->cSecurity_code));
		m_open_strategy->m_args->weightlist[i].sSecurity.cSecuritytype = m->weightlist[i]->sSecurity->cSecuritytype;
	}

	m_open_strategy->m_args->positionlistnum = m->positionlistNUM;

	for (int i = 0; i < m_open_strategy->m_args->positionlistnum; i++){
		m_open_strategy->m_args->positionlist[i].bstoped = m->positionlist[i]->bstoped;
		m_open_strategy->m_args->positionlist[i].dlastprice = m->positionlist[i]->dlastprice;
		m_open_strategy->m_args->positionlist[i].ntradervolume = m->positionlist[i]->tradevolume;
		m_open_strategy->m_args->positionlist[i].sSecurity.cSecuritytype = m->positionlist[i]->sSecurity->cSecuritytype;
		strcpy_s(m_open_strategy->m_args->positionlist[i].sSecurity.cSecurity_code, 31, (char*)(void*)System::Runtime::InteropServices::Marshal::StringToHGlobalAnsi(m->positionlist[i]->sSecurity->cSecurity_code));
	}

	m_open_strategy->m_args->nHands = m->nHands;
	strcpy_s(m_open_strategy->m_args->indexCode, 32, (char*)(void*)System::Runtime::InteropServices::Marshal::StringToHGlobalAnsi(m->indexCode));
	strcpy_s(m_open_strategy->m_args->contractCode, 32, (char*)(void*)System::Runtime::InteropServices::Marshal::StringToHGlobalAnsi(m->contractCode));

	m_open_strategy->m_args->dPositiveOpenDelta = m->dPositiveOpenDelta;
	m_open_strategy->m_args->bTradingAllowed = m->bTradingAllowed;

	m_open_strategy->init();

}

void Strategy_OPEN::calculateSimTradeStrikeAndDelta(){
	 m_open_strategy->calculateSimTradeStrikeAndDelta();
}

void Strategy_OPEN::isOpenPointReached(bool^ open){
	bool b = m_open_strategy->isOpenPointReached();
	open = b;
}

void Strategy_OPEN::getshowstatus(String^ status){

	char* str = (char*)(void*)System::Runtime::InteropServices::Marshal::StringToHGlobalAnsi(status);
	//return true;// m_open_strategy->getshowstatus(str);
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