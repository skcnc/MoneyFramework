#include "StdAfx.h"
#include "TimeUtil.h"


CTimeUtil::CTimeUtil(void)
{
}

int CTimeUtil::getIntDate(){
	SYSTEMTIME now;
	GetLocalTime(&now);
	int nRtn = now.wDay+ 100* now.wMonth+ 1000000* now.wYear;
	return nRtn;
}
int CTimeUtil::getAccurateIntTime(){
	SYSTEMTIME now;
	GetLocalTime(&now);
	int nRtn = now.wMilliseconds+ 1000* now.wSecond+ 100000* now.wMinute+10000000*now.wHour;
	return nRtn;
}
int CTimeUtil::getIntTime(){
	SYSTEMTIME now;
	GetLocalTime(&now);
	int nRtn =  now.wSecond+ 100* now.wMinute+10000*now.wHour;
	return nRtn;
}
int CTimeUtil::parseFutureTime(char* szTime){
	int nHour,nMin,nSecond;
	nHour = (szTime[0]-'0')*10+(szTime[1]-'0');
	nMin = (szTime[3]-'0')*10+(szTime[4]-'0');
	nSecond = (szTime[6]-'0')*10+(szTime[7]-'0');
	return nHour*10000+nMin*100+nSecond;
}
int CTimeUtil::getCurrentMiniute(){
	SYSTEMTIME now;
	GetLocalTime(&now);
	int nRtn = now.wMinute;
	return nRtn;
}
bool CTimeUtil::isIfTradingTime(){
	int nCurrentTime = CTimeUtil::getAccurateIntTime();
	if(nCurrentTime>=91500000&&nCurrentTime<113100000){
		return true;
	}
	if(nCurrentTime>=130000000&&nCurrentTime<151600000){
		return true;
	}	
	return false;
}
bool CTimeUtil::isCommodityTradingTime(){
	int nCurrentTime = CTimeUtil::getAccurateIntTime();
	if(nCurrentTime>=90000000&&nCurrentTime<101600000){
		return true;
	}
	if(nCurrentTime>=103000000&&nCurrentTime<113100000){
		return true;
	}
	if(nCurrentTime>=133000000&&nCurrentTime<150100000){
		return true;
	}	
	return false;
}
int CTimeUtil::getDeltaSecond(int nTime1,int nTime2)
{
	int nHour1,nMin1,nSecond1, nHour2,nMin2,nSecond2;

	nHour1 = (nTime1/10000);
	nMin1 =  (nTime1/100)%100;
	nSecond1 = (nTime1)%100;

	nHour2 = (nTime2/10000);
	nMin2 =  (nTime2/100)%100;
	nSecond2 = (nTime2)%100;

	int nDelta = (nHour1-nHour2)*3600+(nMin1-nMin2)*60+(nSecond1-nSecond2);
	if(nDelta>2)
		nDelta=nDelta;
	return nDelta;
}
bool CTimeUtil::isAutoTradingTime(){
	//return true;
	int now = CTimeUtil::getAccurateIntTime();
	if(now>=93030000&&now<=112930000)
		return true;
	if(now>=130020000&&now<=145600000)
		return true;
	return false;
	
}
CTimeUtil::~CTimeUtil(void)
{
}
