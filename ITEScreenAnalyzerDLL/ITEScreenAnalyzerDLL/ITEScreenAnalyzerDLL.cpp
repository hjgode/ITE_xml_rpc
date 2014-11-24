// ITEScreenAnalyzerDLL.cpp : Defines the entry point for the DLL application.
//

#include "stdafx.h"
#include "ITEScreenAnalyzerDLL.h"

#include "XmlRpc.h"
//#include <iostream>
//#include "Afxmt.h"
//#include <afx.h>

using namespace XmlRpc;
#define MAX_BUFFER_SIZE 1024
#define CurRows 25
#define CurCols 80
WCHAR ScreenContent[CurRows][CurCols+1];
XmlRpcClient RpcClient("localhost", 50023);
int                iAcceptSocket;  /* Actual socket descriptor           */

BOOL APIENTRY DllMain( HANDLE hModule, 
                       DWORD  ul_reason_for_call, 
                       LPVOID lpReserved
					 )
{
	switch (ul_reason_for_call)
	{
	case DLL_PROCESS_ATTACH:
	case DLL_THREAD_ATTACH:
	case DLL_THREAD_DETACH:
	case DLL_PROCESS_DETACH:
		break;
	}
    return TRUE;
}

// This is an example of an exported variable
ITESCREENANALYZERDLL_API int nITEScreenAnalyzerDLL=0;

// This is an example of an exported function.
ITESCREENANALYZERDLL_API int fnITEScreenAnalyzerDLL(void)
{
	return 42;
}

// This is the constructor of a class that has been exported.
// see ITEScreenAnalyzerDLL.h for the class definition
CITEScreenAnalyzerDLL::CITEScreenAnalyzerDLL()
{ 
	return; 
}
