#pragma once

#include <windows.h>¡¡
#include  "winbase.h"

class CTimeUtil
{
public:
	CTimeUtil(void);
	static int getAccurateIntTime();
	static int getIntDate();
	static int getIntTime();
	static int getCurrentMiniute();
	static bool isCommodityTradingTime();
	static bool isIfTradingTime();
	static bool isAutoTradingTime();
	static int parseFutureTime(char * szTime);
	static int getDeltaSecond(int nTime1,int nTime2);
	~CTimeUtil(void);
};

