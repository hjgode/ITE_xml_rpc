// ITEScreenWatch.cpp
//

#include "stdafx.h"
#include <windows.h>
#include <commctrl.h>
// Link against xmlrpc lib and whatever socket libs your system needs (ws2_32.lib 
// on windows)

#include "winsock2.h"
#pragma comment(lib, "ws2.lib")

#include "..\xmlrpc++0.7\src\XmlRpc.h"
#if defined WIN32_PLATFORM_PSPC
	#pragma comment (lib, "../xmlrpc++0.7/Windows Mobile 5.0 Pocket PC SDK (ARMV4I)/Debug/xmlrpc++0.7.wm5.lib")
#endif

#include <iostream>
#include "Afxmt.h"
#include <afx.h>

using namespace XmlRpc;

#define MAX_BUFFER_SIZE 1024

XmlRpcServer s;
#define DEBUG_BUFLEN 1024
WCHAR Screen[20*30+1];
bool FirstScreen=true;
int Rows=0;
int Cols=0;
DWORD MaxLogFileSize=1024L*1024L*2L; // 2 Mb

#define CurRows 25
#define CurCols 80
WCHAR ScreenContent[CurRows][CurCols+1];

HWND RecurseFindWindow(WCHAR *strChildClassName, HWND hWndParent);
void DumpScreenContent();

bool bFound;

HWND hWndITEWindow;

TCHAR g_pLocalIP[16];
char* g_pLocalIPA =(char*)malloc(16);

//moved to local inside functions
//XmlRpcClient RpcClient("localhost", 50023);	//localhost may not work
//XmlRpcClient RpcClient("169.254.2.1", 50023);	//localhost may not work
//XmlRpcClient RpcClient("192.168.0.44", 50023);

int                iAcceptSocket;  /* Actual socket descriptor           */

WCHAR LogFile[]=L"\\Flash File Store\\ITEScreenLOG.TXT";
WCHAR BackLogFile[]=L"\\Flash File Store\\ITEScreenLOG.BAK.TXT";

// Nivel 0 (NO LOG) por defecto
int LogLevel;
void Log(int MsgLevel,LPCTSTR lpszFormat, ...);

HANDLE g_hMsgQueue=NULL;

void closeMsgQueue(HANDLE hMsgQueue){
	if(hMsgQueue==NULL)
		DEBUGMSG(1, (L"closeMsgQueue called with NULL handle\n"));

	if(CloseMsgQueue(hMsgQueue))
		g_hMsgQueue=NULL;
	else
		DEBUGMSG(1, (L"closeMsgQueue failed: %i\n", GetLastError()));

}
HANDLE createMsgQueue(){
	if(g_hMsgQueue!=NULL)
		closeMsgQueue(g_hMsgQueue);

	MSGQUEUEOPTIONS lpOptions;
	lpOptions.bReadAccess=FALSE;	//need only write access
	lpOptions.cbMaxMessage=(CurCols + 1)*sizeof(TCHAR);
	lpOptions.dwFlags=MSGQUEUE_ALLOW_BROKEN;// MSGQUEUE_NOPRECOMMIT; //or MSGQUEUE_ALLOW_BROKEN
	lpOptions.dwMaxMessages=0;			//allow unlimitted number of messages, implicitly sets MSGQUEUE_NOPRECOMMIT
	lpOptions.dwSize=sizeof(MSGQUEUEOPTIONS);

	g_hMsgQueue=CreateMsgQueue(L"ITESCREENS", &lpOptions);
	if(g_hMsgQueue==NULL)
		DEBUGMSG(1, (L"CreateMsgQueue failed with: %i\n", GetLastError()));
	return g_hMsgQueue;
}

void writeToMsgQueue(TCHAR* sMsg){
	if(g_hMsgQueue==NULL)
		g_hMsgQueue = createMsgQueue();
	if(g_hMsgQueue==NULL){
		DEBUGMSG(1, (L"writeToMsgQueue failed: no msqQueue handle\n"));
		return;
	}
	DWORD dwTimeOut=100; //100ms
	DWORD dwFlags=0x00;
	if(!WriteMsgQueue(g_hMsgQueue, (VOID*)sMsg, wcslen(sMsg)*sizeof(TCHAR), dwTimeOut, dwFlags)){
		DWORD dwErr=GetLastError();
		DEBUGMSG(1, (L"WriteMsgQueue failed: %i, ", dwErr));
		switch(dwErr){
			case ERROR_INSUFFICIENT_BUFFER:
				DEBUGMSG(1, (L"ERROR_INSUFFICIENT_BUFFER\n"));
				break;
			case ERROR_PIPE_NOT_CONNECTED:
				DEBUGMSG(1, (L"ERROR_PIPE_NOT_CONNECTED\n"));
				break;
			case ERROR_TIMEOUT:
				DEBUGMSG(1, (L"ERROR_TIMEOUT\n"));
				break;
			case ERROR_OUTOFMEMORY:
				DEBUGMSG(1, (L"ERROR_OUTOFMEMORY\n"));
				break;
			default:
				DEBUGMSG(1, (L"UNKNOWN error code\n"));
				break;
		}
	}
	else
		DEBUGMSG(1, (L"msgQueue write done\n"));
}

int MyGetIpAdress()
{
    WSADATA wsa_Data;
    WCHAR pWideCharStr[80];
    int wsa_ReturnCode = WSAStartup(0x202,&wsa_Data);
    char szHostName[255];
    gethostname(szHostName, 255);
    struct hostent *host_entry;
    host_entry=gethostbyname(szHostName);
    char * szLocalIP;
    szLocalIP = inet_ntoa (*(struct in_addr *)*host_entry->h_addr_list);
    MultiByteToWideChar(CP_ACP, 0,szLocalIP , -1,pWideCharStr , sizeof(pWideCharStr) / sizeof(wchar_t));
    
    //MessageBox(NULL,pWideCharStr, TEXT("IP address"), MB_OK|MB_ICONERROR);
	DEBUGMSG(1, (L"IP address: %s\n", pWideCharStr));

	strncpy(g_pLocalIPA, szLocalIP, strlen(szLocalIP));
	wcsncpy(g_pLocalIP, pWideCharStr, wcslen(g_pLocalIP));

    WSACleanup();
    return wcslen(g_pLocalIP);
}

LPTSTR FormatCurrentTime(LPTSTR szCurrentTime)
{

	SYSTEMTIME st;
    memset(&st, 0, sizeof(SYSTEMTIME));
    GetLocalTime(&st);

    wsprintf(szCurrentTime, TEXT("%02d/%02d/%04d, %02d:%02d:%02d"),
                    st.wDay,
                    st.wMonth,
                    st.wYear,
                    st.wHour,
                    st.wMinute,
                    st.wSecond);
    
	return szCurrentTime;
}

void Log(int MsgLevel,LPCTSTR lpszFormat, ...)
{
	bool CreateNew = false;
	HANDLE hFile;
	DWORD dwSize;
	TCHAR szBuf[DEBUG_BUFLEN + 1];
	memset(szBuf, 0, sizeof(szBuf));

	va_list args;
	va_start(args, lpszFormat);

	_vsntprintf(szBuf, DEBUG_BUFLEN, lpszFormat, args);

	va_end(args);

	DEBUGMSG(1, (L"%s\n", szBuf));

	if (LogLevel==0) 
		return;
	if (MsgLevel>LogLevel) 
		return;

	hFile = CreateFile(LogFile,GENERIC_WRITE,0,0,OPEN_EXISTING,0,0);
	if (hFile==INVALID_HANDLE_VALUE)
	{
		CreateNew = true;
	}

	// Try to obtain hFile's size 
	if (!CreateNew) 
	{
		dwSize = GetFileSize (hFile, NULL) ; 
		CloseHandle(hFile);
 
		// Result on failure. 
		if (dwSize == 0xFFFFFFFF && GetLastError() != NO_ERROR) { 
 
			// Obtain the error code. 
	//		dwError = GetLastError() ; 
 
			// Resolve the failure. 
			return;
		} // End of error handler

		if (dwSize>MaxLogFileSize) {
			if (BackLogFile!=NULL) {
				DeleteFile(BackLogFile);
				MoveFile(LogFile,BackLogFile);
			}
		}
	}


	FILE* fp = _tfopen(LogFile, _T("a"));
	TCHAR szTimeBuf[256];
	if (fp)
	{
		_ftprintf(fp,_T("%s: %s\n"),FormatCurrentTime(szTimeBuf),szBuf);
		fclose(fp);
	}
}

 void StartGetScreenContents()
{
		XmlRpcValue Args;
		XmlRpcValue registerResult;
		Log(1,L"StartGetScreenContents");
		Args[0] = "ITC.GetScreenContents";
		//Args[1] = "localhost";	//localhost may not work
		Args[1] = g_pLocalIPA;// "169.254.2.1";
		Args[2] = 12345;
		//Args[2] = 50023;
		try
		{
			Log(1,L"Calling ITC.registerScreenContentsCallback");
			XmlRpcClient RpcClient(g_pLocalIPA, 50023);	//localhost may not work
			if (!RpcClient.execute("ITC.registerScreenContentsCallback", Args, registerResult))
				Log(1,L"Error calling registerScreenContentsCallback");
			else
				Log(1,L"StartGetScreenContents OK");
			RpcClient.close();
		}
		catch (const XmlRpcException& fault)
		{
			Log(1,L"Exception in registerScreenContentsCallback %s", fault.getMessage());
		}
}

void StopGetScreenContents()
{
		XmlRpcValue result3,Arg3;
		Arg3[0] = "";
		try
		{
			XmlRpcClient RpcClient(g_pLocalIPA, 50023);	//localhost may not work
			if (!RpcClient.execute("ITC.stopScreenContentsResponse", Arg3, result3))
				Log(1,L"Error calling 'stopScreenContentsResponse'");
			else
				Log(1,L"StopGetScreenContents OK");
			RpcClient.close();
		}
		catch (const XmlRpcException& fault)
		{
			Log(1,L"Exception calling stopScreenContentsResponse %s", fault.getMessage());
		}
}



class GetScreenContents : public XmlRpcServerMethod
{
private:
	XmlRpcValue paramsBuffer[30];
public:
	GetScreenContents(XmlRpcServer* s) : XmlRpcServerMethod("ITC.GetScreenContents", s) {}

	void execute(XmlRpcValue& params, XmlRpcValue& result)
	{
		int nArgs = params.size();
		int requiredSize;
		wchar_t wFieldText[129];
		bool ShowMsg=false;

		Log(2,L"RX: %d",nArgs);

		for (int i=0; i<nArgs; i++)
		{
			int y = (int) (params[i]["Row"]);
			int x = (int) (params[i]["Column"]);
			std::string& fieldText = params[i]["Field"];
			int attribute = params[i]["Attribute"];

			requiredSize = mbstowcs(NULL, fieldText.c_str(), 0); // C4996
			if (requiredSize>127) 
				requiredSize=127;

			mbstowcs(wFieldText,fieldText.c_str(),requiredSize+1);

			//row updated?
			if (0 != wcsncmp(ScreenContent[y]+x, wFieldText, wcslen(wFieldText)))
			{
				wcsncpy(ScreenContent[y]+x, wFieldText, wcslen(wFieldText));
				Log(2,L"R:%d C:%d <%s>",y,x,wFieldText);
			}
		}
		DumpScreenContent();

		result = "OK";
	}
} GetScreenContents(&s);

static DWORD RPCListener(LPVOID lpData) 
{
	int port=12345;

	//  XmlRpc::setVerbosity(5);

	// Create the server socket on the specified port
	s.bindAndListen(port);

	// Enable introspection
	s.enableIntrospection(true);

	// Wait for requests indefinitely
	s.work(-1.0);
	Log(1,L"XMLRPC Server exit");
	return 1;

}

DWORD WINAPI XMLRpcClientThread(PVOID ThreadParm)
{
	StartGetScreenContents();
	RPCListener(NULL);
	return 1;
}

BOOL GetModulePath(TCHAR* pBuf, DWORD dwBufSize) 
{
  if (GetModuleFileName(NULL,pBuf,dwBufSize)) {
 
      for(int i=wcslen(pBuf)-2; i>-1; i--)
      {
            if(pBuf[i]==(WCHAR)'\\'){
                  pBuf[i] = (WCHAR)'\0';
                  break;
            }
      }
 
    return TRUE;
  }
  return FALSE;
}

void InitScreenContent()
{
	int i,j;
	for (i=0; i<CurRows;i++)
	{
		for (j=0; j<CurCols; j++) ScreenContent[i][j]=(WCHAR) ' ';
		ScreenContent[i][j]=(WCHAR)0;
	}
}

void DumpScreenContent()
{
	int i;
	for (i=0; i<CurRows;i++)
	{
		Log(2,L"<%*.*s>",25,25,ScreenContent[i]);
		//DEBUGMSG(1, (L"<%*.*s>\n",25,25,ScreenContent[i]));
		writeToMsgQueue(ScreenContent[i]);
	}
}


int _tmain(int argc, _TCHAR* argv[])
{
	DWORD dwXmlRpcThreadID;
	PROCESS_INFORMATION pi;
	HANDLE hXMLRpcClientThread;
	bool ThreadCreated=false;

	LogLevel=1;
	Log(1,L"ITEScreenWatch V 0.1");

	MyGetIpAdress();	// read IP into g_pLocalIP

	InitScreenContent();

	CreateProcess(L"\\Program Files\\Intermec\\ITE\\intermte.exe", L"", NULL, NULL, FALSE, 0, NULL, NULL, NULL, &pi); 
	Sleep(10000);
	hWndITEWindow = FindWindow(_T("IntermTE"), _T("ITE"));
	if (hWndITEWindow==NULL)
	{
		Log(1,L"FindWindows ITE failes");
	}
	else
	{
		hXMLRpcClientThread=CreateThread(NULL, 0, XMLRpcClientThread, (VOID *)0, 0,&dwXmlRpcThreadID);
		if (hXMLRpcClientThread == NULL)
		{
			Log(1,L"CreateThread failed");
		}
		else
		{
			Log(1,L"CreateThread OK");
			ThreadCreated=true;
		}
	}
	
	Log(1,L"Waiting for TE exit");
	do{
		WaitForSingleObject(pi.hProcess,INFINITE);
	}while((FindWindow(_T("IntermTE"), _T("ITE")))!=NULL);

	Log(1,L"ITE Exit");

	if (ThreadCreated)
	{
		s.exit();
		s.shutdown();
	}
	//Sleep(2000);
	TerminateThread(hXMLRpcClientThread,1);
	//Log(L"Wait Thread end");
	//WaitForSingleObject(hXMLRpcClientThread,INFINITE);
	Sleep(1000);
	
	closeMsgQueue(g_hMsgQueue);

	Log(1,L"Program Exit");
	return 0;
}
