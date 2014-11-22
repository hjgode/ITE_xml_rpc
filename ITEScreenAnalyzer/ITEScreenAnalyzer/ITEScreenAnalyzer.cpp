// HelloClient.cpp: define el punto de entrada de la aplicación de consola.
//

#include "stdafx.h"
#include <windows.h>
#include <commctrl.h>
// Link against xmlrpc lib and whatever socket libs your system needs (ws2_32.lib 
// on windows)
#include "XmlRpc.h"
#include <iostream>
#include "Afxmt.h"
#include <afx.h>
#include "FastINI.h"

#pragma comment (lib, "itc50.lib")  //C:\Program Files (x86)\Intermec\Developer Library\Lib\WCE600\WM6\Armv4i
#include <itc50.h>	//C:\Program Files (x86)\Intermec\Developer Library\Include

using namespace XmlRpc;

#define MAX_BUFFER_SIZE 1024

XmlRpcServer s;
#define DEBUG_BUFLEN 1024
WCHAR Screen[20*30+1];
bool FirstScreen=true;
int Rows=0;
int Cols=0;
DWORD MaxLogFileSize=1024L*1024L*2L; // 2 Mb

//
#define MAX_CONDITION_PER_RULE	10
#define MAX_ACTION_PER_RULE	10
#define MAX_CONDITION_LENGTH	30
#define MAX_ACTION_LENGTH		128
#define MAX_RULES_COUNT			20
#define MAX_ACTION_PARAM_COUNT	10
#define ACTION_MSGBOX			1
#define ACTION_BEEP				2
#define ACTION_INVALID_VALUE	-1

typedef struct {
	int Row,Col;
	TCHAR MatchStr[MAX_CONDITION_LENGTH];
} tCondition;

typedef struct {
	int ActionType;
	TCHAR ActionParam[MAX_ACTION_PARAM_COUNT][MAX_ACTION_LENGTH];
} tAction;

typedef struct {
	tCondition lCondition[MAX_CONDITION_PER_RULE];
	tAction	   lAction[MAX_ACTION_PER_RULE];
	bool	   Matched;
	int		   CountCond;
	int		   CountActions;
} tRule;

tRule lRules[MAX_RULES_COUNT];
int CountRules=0;
//


#define CurRows 25
#define CurCols 80
WCHAR ScreenContent[CurRows][CurCols+1];


HWND RecurseFindWindow(WCHAR *strChildClassName, HWND hWndParent);
//bool CheckLineChange(int Row,int Col,TCHAR *MatchStr);
bool CheckLineChange(int Row);
void ClearRulesMatched();
void ProcessRulesMatched();
void DumpScreenContent();

bool bFound;

HWND hWndITEWindow;

XmlRpcClient RpcClient("localhost", 50023);
//XmlRpcClient RpcClient("192.168.0.44", 50023);

int                iAcceptSocket;  /* Actual socket descriptor           */

WCHAR LogFile[]=L"\\Flash File Store\\ITEScreenLOG.TXT";
WCHAR BackLogFile[]=L"\\Flash File Store\\ITEScreenLOG.BAK.TXT";

// Nivel 0 (NO LOG) por defecto
int LogLevel;
void Log(int MsgLevel,LPCTSTR lpszFormat, ...);

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

	if (LogLevel==0) 
		return;
	if (MsgLevel>LogLevel) 
		return;
	va_list args;
	va_start(args, lpszFormat);

	_vsntprintf(szBuf, DEBUG_BUFLEN, lpszFormat, args);

	va_end(args);

	DEBUGMSG(1, (L"%s", szBuf));

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

//void Log(LPCTSTR lpszFormat, ...)
//{
//	TCHAR szBuf[DEBUG_BUFLEN+1];
//	CTime time;
//	time= CTime::GetCurrentTime();
//
//	memset(szBuf, 0, sizeof(szBuf));
//	va_list args;
//	va_start(args, lpszFormat);
//
//	_vsntprintf(szBuf, DEBUG_BUFLEN, lpszFormat, args);
//
//	va_end(args);
//
//	FILE* fp = _tfopen(_T("\\ITEScreenAnalyzer.LOG.TXT"), _T("a"));
//	if (fp)
//	{
//		_ftprintf(fp,_T("%02d-%02d-%04d,%02d:%02d:%02d,%s\n"),
//		time.GetDay(),time.GetMonth(),time.GetYear(),
//		time.GetHour(),time.GetMinute(),time.GetSecond(),
//			szBuf);
//		fclose(fp);
//	}
//
//}

 void StartGetScreenContents()
{
		XmlRpcValue Args;
		XmlRpcValue registerResult;
		Log(1,L"StartGetScreenContents");
		Args[0] = "ITC.GetScreenContents";
		Args[1] = "localhost";
		//Args[1] = "192.168.1.145";
		Args[2] = 12345;
		//Args[2] = 50023;
		try
		{
			Log(1,L"Calling ITC.registerScreenContentsCallback");
			if (!RpcClient.execute("ITC.registerScreenContentsCallback", Args, registerResult))
				Log(1,L"Error calling registerScreenContentsCallback");
			else
				Log(1,L"StartGetScreenContents OK");
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
			if (!RpcClient.execute("ITC.stopScreenContentsResponse", Arg3, result3))
				Log(1,L"Error calling 'stopScreenContentsResponse'");
			else
				Log(1,L"StopGetScreenContents OK");
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
	//void DumpScreen()
	//{
	//	for (int row=0; row<Rows; row++)
	//	{
	//		Log(L"<%*.*s>",Cols,Cols,Screen+row*Cols);
	//	}
	//}
	
	void execute(XmlRpcValue& params, XmlRpcValue& result)
  {
	int nArgs = params.size();
	int requiredSize;
//    wchar_t *wFieldText;
    wchar_t wFieldText[129];
//	WCHAR *PtrIni,*PtrFin;
	bool ShowMsg=false;

	Log(2,L"RX: %d",nArgs);
	ClearRulesMatched();

	for (int i=0; i<nArgs; i++)
	{
		int y = (int) (params[i]["Row"]);
		int x = (int) (params[i]["Column"]);
		std::string& fieldText = params[i]["Field"];
		int attribute = params[i]["Attribute"];
			
	    requiredSize = mbstowcs(NULL, fieldText.c_str(), 0); // C4996
		if (requiredSize>127) 
			requiredSize=127;
		//wFieldText = (wchar_t *)malloc( (requiredSize + 1) * sizeof( wchar_t ));
		//if (! wFieldText)
		//{
		//	Log(L"Memory allocation failure");
		//	return;
		//}
		mbstowcs(wFieldText,fieldText.c_str(),requiredSize+1);


		if (0!=wcsncmp(ScreenContent[y]+x,wFieldText,wcslen(wFieldText)))
		{
			wcsncpy(ScreenContent[y]+x,wFieldText,wcslen(wFieldText));
			Log(2,L"R:%d C:%d <%s>",y,x,wFieldText);
			if (CheckLineChange(y))
			{
				Log(1,L"MATCH!!!");
				ProcessRulesMatched();
			}
		}
		//free(wFieldText);
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


//HWND MyFindWindow(WCHAR *strChildClassName, HWND hWndTopLevel)
//{
//   HWND hwndCur = (HWND)0;
//   hwndCur = GetWindow(hWndTopLevel, GW_CHILD);
//   return RecurseFindWindow(strChildClassName, hwndCur);
//}
//
//HWND RecurseFindWindow(WCHAR *strChildClassName, HWND hWndParent)
//{
//   HWND hwndCur = (HWND)0;
//   WCHAR chArWindowClass[32];
//
//   if (hWndParent == (HWND)0)
//       return (HWND)0;
//   else
//   {
//       //check if we get the searched class name
//       GetClassName(hWndParent, chArWindowClass, 256);
//       bFound = (wcscmp(chArWindowClass,strChildClassName)==0);
//       if (bFound)
//           return hWndParent;
//       else
//       {
//           //recurse into first child
//           HWND hwndChild = GetWindow(hWndParent, GW_CHILD);
//           if (hwndChild != (HWND)0)
//               hwndCur = RecurseFindWindow(strChildClassName, hwndChild);
//
//           if(!bFound)
//           {
//               HWND hwndBrother = (HWND)0;
//               //enumerate each brother windows and recurse into
//               do
//               {
//                   hwndBrother = GetWindow(hWndParent, GW_HWNDNEXT);
//                   hWndParent = hwndBrother;
//                   if (hwndBrother != (HWND)0)
//                   {
//                       hwndCur = RecurseFindWindow(strChildClassName, hwndBrother);
//                       if (bFound)
//                           break;
//                   }
//               }
//               while (hwndBrother != (HWND)0);
//           }
//       }
//       return hwndCur;
//   }
//}

DWORD WINAPI XMLRpcClientThread(PVOID ThreadParm)
{
	StartGetScreenContents();
	RPCListener(NULL);
	return 1;
}

BOOL GetModulePath(TCHAR* pBuf, DWORD dwBufSize) 
{
  if (GetModuleFileName(NULL,pBuf,dwBufSize)) {
 
    //PathRemoveFileSpec(pBuf); // remove executable name
	// _splitPath
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

void ProcessINIFile()
{
TCHAR *Value;
TCHAR CurDir[MAX_PATH+1];
WCHAR fileName[MAX_PATH];
TCHAR Section[20],SecParam[20];
WCHAR *Token;
WCHAR StrAux[128];
int i,j,k;
int CountCond;
int CountActions;

	GetModulePath(CurDir,MAX_PATH);
	wsprintf(fileName,L"%s\\ITEScript.INI",CurDir);


	CountRules=0;
	for (i=0; i<MAX_RULES_COUNT; i++)
	{
		for (j=0; j<MAX_CONDITION_PER_RULE; j++)
		{
				lRules[i].lCondition[j].MatchStr[0] = (TCHAR) 0;
				lRules[i].lCondition[j].Row = -1;
				lRules[i].lCondition[j].Col = -1;
				lRules[i].Matched = false;
				lRules[i].CountCond = 0;
		}
	}

	xcIniFile IniFile(fileName);

	Value = IniFile.GetProfileString(L"Debug",L"Level",NULL);
	if (Value!=NULL) {
		LogLevel = _wtoi(Value);
	}

	for (i=0; i<MAX_RULES_COUNT; i++)
	{
		wsprintf(Section,L"Rule_%d",i+1);
		CountCond=0;
		for (j=0; j<MAX_CONDITION_PER_RULE; j++)
		{
			wsprintf(SecParam,L"Match_%d",j+1);
			Value = IniFile.GetProfileString(Section,SecParam,NULL);
			if (Value==NULL) {
				continue;
			}
			// Formato:
			// Match_x=<Row>,<Col>,"<MatchString>"
			k=0;
			Token = wcstok(Value,L",");
			while ( Token!=NULL)
			{
				if (k==0)
				{
					lRules[CountRules].lCondition[CountCond].Row=_wtoi(Token)-1;
					// <Row>
				}
				else
				if (k==1)
				{
					lRules[CountRules].lCondition[CountCond].Col=_wtoi(Token)-1;
					// <Row>
				}
				else
				if (k==2)
				{
					// "<MatchString>"
						// Eliminar comillas
						wcscpy(StrAux,Token+1);
						StrAux[wcslen(StrAux)-1]=(WCHAR) 0;
						wsprintf(lRules[CountRules].lCondition[CountCond].MatchStr,L"%*.*s",wcslen(Token)-2,wcslen(Token)-2,Token+1);
				}
				k++;
				Token = wcstok(NULL,L",");
			}

			lRules[CountRules].CountCond++;
			CountCond++;
			CountRules++;
		}
		CountActions=0;
		for (j=0; j<MAX_ACTION_PER_RULE; j++)
		{
			// Posibles formatos:
			// Action_x=MsgBox,"<Mensaje>","<Título ventana>"
			// Action_x=Beep,<Frecuencia>,<Duracion>,<Volumen>
			wsprintf(SecParam,L"Action_%d",j+1);
			Value = IniFile.GetProfileString(Section,SecParam,NULL);
			if (Value==NULL) continue;
			for (k=0; k<MAX_ACTION_PARAM_COUNT; k++)
			{
				lRules[i].lAction[CountActions].ActionParam[k][0] = (WCHAR) 0;
			}
			k=0;
			Token = wcstok(Value,L",");
			while ( Token!=NULL)
			{
				if (k==0)
				{
					// Primer parámetro (MsgBox ó Beep)
					if (_wcsicmp(Token,L"MsgBox")==0) lRules[i].lAction[CountActions].ActionType = ACTION_MSGBOX;
					else
					if (_wcsicmp(Token,L"Beep")==0) lRules[i].lAction[CountActions].ActionType = ACTION_BEEP;
					else 
					{
						lRules[i].lAction[CountActions].ActionType = ACTION_INVALID_VALUE;
						continue;
					}
				}
				else
				{
					if (lRules[i].lAction[CountActions].ActionType == ACTION_MSGBOX)
					{
						// Eliminar comillas
						wcscpy(StrAux,Token+1);
						StrAux[wcslen(StrAux)-1]=(WCHAR) 0;
						wcscpy(lRules[i].lAction[CountActions].ActionParam[k-1],StrAux);
					}
					else
					if (lRules[i].lAction[CountActions].ActionType == ACTION_BEEP)
					{
						// PENDIENTE: eliminar blancos delante y detrás del parámetro
						wcscpy(lRules[i].lAction[CountActions].ActionParam[k-1],Token);
					}
				}
				k++;
				Token = wcstok(NULL,L",");
			}
			lRules[i].CountActions++;
			CountActions++;
		}
	}
}

void ClearRulesMatched()
{
	int i,j;
	for (i=0; i<CountRules; i++)
	{
		for (j=0; j<lRules[i].CountCond; j++)
		{
			lRules[i].Matched = false;
		}
	}
}

//bool CheckLineChange(int Row,int Col,TCHAR *MatchStr)
bool CheckLineChange(int Row)
{
	int i,j;
	int CountMatched=0;
	WCHAR *Ptr;
	for (i=0; i<CountRules; i++)
	{
		for (j=0; j<lRules[i].CountCond; j++)
		{
			//if ( (lRules[i].lCondition[j].Row == Row) && (lRules[i].lCondition[j].Col == Col) 
			//	&& (wcscmp(lRules[i].lCondition[j].MatchStr,MatchStr)==0))
			Ptr = ScreenContent[Row]+lRules[i].lCondition[j].Col;
			if ((lRules[i].lCondition[j].Row == Row) && 
				(wcsncmp(Ptr,lRules[i].lCondition[j].MatchStr,wcslen(lRules[i].lCondition[j].MatchStr))==0))
			{
				lRules[i].Matched = true;
			}
			else
			{
				lRules[i].Matched = false;
				break;
			}
		}
		if (lRules[i].Matched) {
			CountMatched++;
		}
	}
	return (CountMatched!=0);
}

void ProcessRulesMatched()
{
	int i,j;
	DWORD Frec,Vol,Duration;
	for (i=0; i<CountRules; i++)
	{
		if (lRules[i].Matched)
		{
			//Log(L"Processing Rule %d CountActions %d",i,lRules[i].CountActions);
			for (j=0; j<lRules[i].CountActions; j++)
			{
				if (lRules[i].lAction[j].ActionType == ACTION_MSGBOX)
				{
				// El párámetro de ACTION_MSGBOX es: "<Mensaje>","<Título ventana>"
					MessageBox(hWndITEWindow,lRules[i].lAction[j].ActionParam[0],lRules[i].lAction[j].ActionParam[1],MB_OK || MB_SETFOREGROUND || MB_TOPMOST);
				}
				else
				if (lRules[i].lAction[j].ActionType == ACTION_BEEP)
				{
				// El párámetro de ACTION_BEEP es: <Frecuency>,<Volume>,<Duration>
					Frec = _wtoi(lRules[i].lAction[j].ActionParam[0]);
					Vol = _wtoi(lRules[i].lAction[j].ActionParam[1]);
					Duration = _wtoi(lRules[i].lAction[j].ActionParam[2]);
					ITCAudioPlayTone(Frec,Vol,Duration);
				}

			}
		}
	}

}

//void DumpIni()
//{
//	int i,j;
//	WCHAR Msg[128];
//	for (i=0; i<CountRules; i++)
//	{
//		for (j=0; j<lRules[i].CountCond; j++)
//		{
//			wsprintf(Msg,L"Rule %d Cond %d R:%d C:%d Str:<%s> ActionParam[0]=<%s> ActionParam[1]=<%s>",i,j,
//				lRules[i].lCondition[j].Row,lRules[i].lCondition[j].Col,lRules[i].lCondition[j].MatchStr,
//				lRules[i].lAction[j].ActionParam[0],
//				lRules[i].lAction[j].ActionParam[1]);
//			Log(Msg);
//		}
//	}
//}

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
	}
}


int _tmain(int argc, _TCHAR* argv[])
{
	DWORD dwXmlRpcThreadID;
	PROCESS_INFORMATION pi;
	HANDLE hXMLRpcClientThread;
	bool ThreadCreated=false;

	LogLevel=1;
	Log(1,L"ITEScreenAnalyzer V 1.0: Program Entry");
	ProcessINIFile();
	//DumpIni();
	//return 0;
	InitScreenContent();

	CreateProcess(L"\\Program Files\\Intermec\\ITE\\intermte.exe", L"", NULL, NULL, FALSE, 0, NULL, NULL, NULL, &pi); 
	Sleep(5000);
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
			Log(1,L"CreateTheread failed");
		}
		else
		{
			ThreadCreated=true;
		}
	}
	
	do{
		WaitForSingleObject(pi.hProcess,INFINITE);
	}while((FindWindow(_T("IntermTE"), _T("ITE")))!=NULL);

	Log(1,L"ITE Exit");
// OJO: NO funciona el salir de la función s.work, es necesario finalizar con TerminateThread
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
	Log(1,L"Program Exit");
	return 0;
}
