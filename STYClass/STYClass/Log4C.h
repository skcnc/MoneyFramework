#pragma once
#include <string>
#include <list>
using namespace std;
class CLog4C
{
public:
	CLog4C(void);
	bool flag;
	void logRealMessage(string msg,list<string>,bool isWrite,const char* filePath);
	bool isDone();
	~CLog4C(void);
};

