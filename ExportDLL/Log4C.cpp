#include "StdAfx.h"
#include "Log4C.h"

namespace ExportDLL {
CLog4C::CLog4C(void)
{
}


CLog4C::~CLog4C(void)
{
}

char* CLog4C::returnstring(char *s)
{
	char ss[] = "Hello Cheng";
	return ss;
}

void CLog4C::logRealMessage(string msg,list<string> msglist,bool isWrite,const char* filePath)
{
	this->flag=false;
	if(!isWrite)
	{
		return;
	}
	FILE * fp;
	fopen_s(&fp, filePath,"a");	
	if(fp!=NULL)
	{
		fwrite(msg.c_str(),1,strlen(msg.data()),fp);

		list<string>::iterator itor=msglist.begin();
		 while(itor!=msglist.end())
       { 
		   fwrite(itor->c_str(),1,strlen(itor->data()),fp);
		   itor++;
       } 
		fclose(fp);
		this->flag=true;
	}
}

int CLog4C::cal(int i,int j)
{
	return i+j;
}

bool CLog4C::isDone()
{
	return this->flag;
}
}