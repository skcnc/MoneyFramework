#pragma once

using namespace System;
using namespace System::Data;

namespace CTP_CLI
{
	public ref class DataTypeConvertor
	{
	public:
		//ÍÐ¹Ü×Ö·û´®µ½×Ö·û´®Êý×é
		static char* GetStr(System::String^ string)
		{			
			IntPtr ip = Marshal::StringToHGlobalAnsi(string);
			char* result = static_cast<char*>(ip.ToPointer());
			return result;
		}

		static const char* GetConstStr(System::String^ string)
		{			
			IntPtr ip = Marshal::StringToHGlobalAnsi(string);
			const char* result = static_cast<const char*>(ip.ToPointer());
			return result;
		}
	};

}