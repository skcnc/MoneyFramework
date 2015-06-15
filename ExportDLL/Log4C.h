#pragma once


#include <string>
#include <list>
using namespace std;
using namespace System;
namespace ExportDLL {
 public ref class   CLog4C
{
public:
	CLog4C(void);
	bool flag;
	//void logRealMessage(string msg,list<string>,bool isWrite,const char* filePath);
	void logRealMessage(string msg, list<string> buffer ,bool isWrite,const char* filePath);
	int cal(int i,int j);
	char* returnstring(char *s);
	bool isDone();
	~CLog4C(void);
};
}



//#pragma once
//using namespace System;
//
//#include <string>
//#include <list>
//
//using namespace std;
//
//namespace Test_CSharp_To_CPP {
//	public ref class  CLog4C
//	{
//		public:
//			CLog4C(void);
//			bool flag;
//			void logRealMessage(string msg,list<string>,bool isWrite,const char* filePath);
//			bool isDone();
//			~CLog4C(void);
//	};
//}