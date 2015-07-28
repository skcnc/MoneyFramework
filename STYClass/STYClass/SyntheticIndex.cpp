#include "StdAfx.h"
#include "SyntheticIndex.h"

bool operator < (const securityindex & infor1,const securityindex &infor2) 
	{
		if (infor1.cSecuritytype == infor2.cSecuritytype)
		 {
			 if(strcmp(infor1.cSecurity_code,infor2.cSecurity_code)<0)
				 return true;
			 else
				 return false;
		 }

       if (infor1.cSecuritytype < infor2.cSecuritytype)
          return true;
      
        else
          return false;
    };

CSyntheticIndex::CSyntheticIndex(void)
{
}
CSyntheticIndex::~CSyntheticIndex(void)
{
}


double  CSyntheticIndex::getSimIndex()
{
	list<indexweightstruct>::iterator itor;
	 itor=stockweight.begin();
	 dSimIndex=0;
	 while(itor!=stockweight.end())
       {    
		   dSimIndex+=itor->dweight*stockDb[itor->sSecurity]->getlastprice();
		  itor++;
       } 
	 return dSimIndex;
}

double  CSyntheticIndex::getrealmarketvalue()
{
	 list<stockpotionstruct>::iterator itor;
	 itor=m_positionlist.begin();
	 marketvalue=0;
	 while(itor!=m_positionlist.end())
       { 
		   marketvalue+=stockDb[itor->sSecurity]->getrealmarketvalue(itor->ntradervolume);
		   itor++;
       } 
	
	 return  marketvalue;
}

double  CSyntheticIndex::getrealbuycost()
{
	 list<stockpotionstruct>::iterator itor;
	 itor=m_positionlist.begin();
	 double tempvalue=0;
	 while(itor!=m_positionlist.end())
       { 
		   tempvalue+=stockDb[itor->sSecurity]->getrealbuycost(itor->ntradervolume);
		   itor++;
       } 
	 return tempvalue;
}

double  CSyntheticIndex::getrealsellgain()
{
	 list<stockpotionstruct>::iterator itor;
	 itor=m_positionlist.begin();
	 double tempvalue=0;
	 while(itor!=m_positionlist.end())
       { 
		  tempvalue+=stockDb[itor->sSecurity]->getrealsellgain(itor->ntradervolume);
		   itor++;
       } 
	 return tempvalue;
}

bool    CSyntheticIndex::isupdated()
{

	 map<securityindex ,CSecurity * >::iterator itor;
	 itor=stockDb.begin();
	 while(itor!=stockDb.end())
       { 
		   if(!itor->second->isupdated())
		       return  false;
       } 
 
    return true;
}

bool   CSyntheticIndex::init(indexweightstruct * indexweightlist,int weightnum, stockpotionstruct * stockpotionlist,int positionnum,char *indexCode )
{

	 m_index.setcode(indexCode);   //初始指数


	 stockweight.clear();  //如果更新权重文件  释放内存
	 stockDb.clear();      //行情
	 m_positionlist.clear();

	for(int i=0;i<weightnum;i++)  
	{
		stockweight.push_back(indexweightlist[i]);//获取权重
     
	  if(isSecurityFocused(indexweightlist[i].sSecurity))
			continue;

	  if( indexweightlist[i].sSecurity.cSecuritytype=='s') //初始化股票数据
		   {

		      stockDb[indexweightlist[i].sSecurity]=new CStock;
			  stockDb[indexweightlist[i].sSecurity]->setcode(indexweightlist[i].sSecurity.cSecurity_code);

		   }
	   else

		return false;

	}

	for(int i=0;i<positionnum;i++)//获取交易列表
	 {
		  m_positionlist.push_back(*(stockpotionlist+i));
	 }

	 return true;
}



bool   CSyntheticIndex::updatepositioninfor()
{
	 list<stockpotionstruct>::iterator itor;
	 itor=m_positionlist.begin();
	 while(itor!=m_positionlist.end())
       { 
		   itor->bstoped=stockDb[itor->sSecurity]->isstoped();
		   itor->dlastprice=stockDb[itor->sSecurity]->getlastprice(); 
		   itor++;
       } 

	 return true;
}

bool   CSyntheticIndex::isSecurityFocused(securityindex  SecurityCode)
	{
		if(this->stockDb.find(SecurityCode) != stockDb.end())
		{ 
		  return true;
	    }
	  return false;
	}

void CSyntheticIndex::updateInfo(MarketInforStruct * MarketInfor )
{
	//if(isSecurityFocused(MarketInfor->msecurity)) //每次行情过来都要判断股票是否在hash表里
	{
	   this->stockDb[MarketInfor->msecurity]->updateInfo(MarketInfor);
	}
}
