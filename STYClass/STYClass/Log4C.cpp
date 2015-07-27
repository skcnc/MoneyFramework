#include "StdAfx.h"
#include "Log4C.h"

CLog4C::CLog4C(void)
{
}


CLog4C::~CLog4C(void)
{
}


void CLog4C::logRealMessage(string msg,list<string> msglist,bool isWrite,const char* filePath)
{
	this->flag=false;
	if(!isWrite)
	{
		return;
	}
	FILE * fp;
	fp=fopen(filePath,"a");	
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

bool CLog4C::isDone()
{
	return this->flag;
}