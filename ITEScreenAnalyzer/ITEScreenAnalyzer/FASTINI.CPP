/*

    This is a reduced version of the INI file interpreter.
    Has the writing capabilities added but file must exist before writting
    Writting removes comments added to the existing keyword
    Instead of accesing the file in the GetProfile??? command it reads all the file in the
    constructor in order to speed up the config access. This will limit the length
    of the ini files to the maximum ram avaiable. Any how the king of applications
    this class is defined for uses small INI files less than 10 Kb. Also take into account
    that I am using heap space, so once the object is deleted, the memory is released

    It allows the reading of the profiles with duplicated keywords per section.
    ie:
    [Labels]
    Label=EAN,0,0,0,0,0
    Label=C39,0,0,0,0x11,0
    
*/

#include "stdafx.h"
#include "Fastini.h"
#include <string.h>
#include <ctype.h>



// internal variables
TCHAR *LastLineRead;
int  LastFileIndex;

void Log(int MsgLevel,LPCTSTR lpszFormat, ...);

xcIniFile::~xcIniFile(void)
{
    if (Line) delete Line;
    if (FileContents) delete FileContents;
}



xcIniFile::xcIniFile(TCHAR *IniFN)
{
    short   AllOK = TRUE;
	DWORD dwSizeHi;
	DWORD NumberOfBytesRead;
	TCHAR *Buffer;
	FileExists = TRUE;
	TCHAR MsgError[128];

    IniFileName = IniFN;
	hFile = CreateFile(IniFileName,GENERIC_READ,0,0,OPEN_EXISTING,0,0);
	if (hFile==INVALID_HANDLE_VALUE)
	{
		AllOK = FALSE;
		FileExists = FALSE;
	}
        
    if(AllOK)
    {
		FileLen = GetFileSize(hFile,&dwSizeHi);
		if (FileLen == 0xFFFFFFFF && GetLastError() != NO_ERROR) 
		{
			swprintf(MsgError,_T("Error FileSize %s"),IniFN);
			Log (1,MsgError);
			return;
		}
		else
		{
			if (NULL == (FileContents = (TCHAR *) new TCHAR[FileLen+1]))
			{
				swprintf(MsgError,_T("Error new FileContents %s"),IniFN);
				Log (1,MsgError);
				return;
			}
			if (NULL == (Buffer = (TCHAR *) new TCHAR[FileLen+1]))
			{
				swprintf(MsgError,_T("Error new Buffer %s"),IniFN);
				Log (1,MsgError);
				return;
			}
        
			if (FileExists) 
			{
				AllOK = ReadFile(hFile,(LPVOID) Buffer,FileLen,&NumberOfBytesRead,0);

				//OJO: Al salvar en el PC como UNICODE, los 2 primeros bytes
				// supongo que indican el tipo. Es necesario eliminarlos.
				wcsncpy (FileContents,Buffer+1,FileLen-1);
				delete Buffer;

			}
			else
			{
				*FileContents = (TCHAR) ' ';
			}
            
			if (AllOK)
			{
				FileContents[FileLen] = (TCHAR) '\0'; 
				// lets convert it into an string
				if (NULL == (Line    = (TCHAR *) new TCHAR[MAX_LINE_SIZE]))
				{
					swprintf(MsgError,_T("Error new Line %s"),IniFN);
					Log (1,MsgError);
					return;
				}
			}
			else
			{
				swprintf(MsgError,_T("Error ReadFile %s"),IniFN);
				Log (1,MsgError);
				delete FileContents;
			}
        
			if (FileExists)
				CloseHandle(hFile);
		}
	}
    
    if(!AllOK)
    {
        FileLen = 0;
        FileContents = Line = NULL;
    }
}



// Next three member modified to access the file in ram instead of directly
// from disk

short xcIniFile::OpenRead()
{
    FileIndex = 0;
    return (FileLen > 0);
}




int xcIniFile::GetC(TCHAR &C)
{
    if (FileIndex == FileLen)
        return 0;
    else
    {
        C = FileContents[FileIndex++];
        return 1;
    }
}

short xcIniFile::ReadLine(void)
{
    TCHAR		c = 0;
    int    Leidos;
    TCHAR    *P = Line;
    int     MaxLineLen = MAX_LINE_SIZE;

    LastFileIndex = FileIndex;
    LastLineRead = FileContents+FileIndex;
    
    while ((--MaxLineLen > 0)  &&  (Leidos = GetC(c)))
        if(c == '\r' || c == '\n')
            break;
        else
            *(P++) = c;

    if (!Leidos && P == Line)  return FALSE;
    *P = '\0';
    
    // Skip the rest of CR FL sequence or empty lines
    
    while (Leidos = GetC(c))
        if((c == '\r') || (c == '\n'))
            continue;
        else
        {
            if (FileIndex) FileIndex--;
            break;
        }
        
    return TRUE;
}



short xcIniFile::ScanForSection (TCHAR *Section)
{
    int SecLen = wcslen ( Section );

    // find section, if present
    do {
        if ( !ReadLine()) return FALSE; // didn't find the section.  Return not found
//    } while ( *Line != '['  || strnicmp ( Section, Line+1, SecLen ) || *(Line+1+SecLen) != ']' );
    } while ( *Line != '['  || _wcsnicmp ( Section, Line+1, SecLen ) || *(Line+1+SecLen) != ']' );
    return TRUE;
}



TCHAR *xcIniFile::ScanForKeyName (TCHAR *KeyName )
{
    register int KeyLen = wcslen ( KeyName );

    do {
        if ( !ReadLine() ) return NULL; // didn't find the keyword
        
        // at the end of an application.  Obviously we didn't find the keyword
        if ( *Line == '[' ) return NULL; // didn't find the keyword.  Return not found
    } while ( _wcsnicmp ( KeyName, Line, KeyLen )  || *(Line+KeyLen) != '=' );

    // OK, we found it, and it's in the current buffer.  Return success.
    // Success is the nonzero index into the return string of where
    // the result (after the equal sign) is located
    return (Line+KeyLen+1);
}


TCHAR *xcIniFile::ScanFor (TCHAR *Section, TCHAR *KeyName )
{
    if (!ScanForSection(Section))
        return NULL;
        
    return ScanForKeyName (KeyName );
}

long   xcIniFile::GetProfileLong(TCHAR *SecName, TCHAR *KeyName, long Default)
{
    TCHAR *Ptr;

    if (!OpenRead()) return Default;
    if (!(Ptr = ScanFor ( SecName, KeyName))) return Default;
//    return strtol( Ptr, NULL, 10);
    return wcstol( Ptr, NULL, 10);
}


int   xcIniFile::GetProfileInt(TCHAR *SecName, TCHAR *KeyName, int Default)
{
    TCHAR *Ptr;

    if (!OpenRead()) return Default;
    if (!(Ptr = ScanFor ( SecName, KeyName))) return Default;
    return (int) wcstol( Ptr, NULL, 10);
}

unsigned int   xcIniFile::GetProfileWord(TCHAR *SecName, TCHAR *KeyName, unsigned int Default)
{
    TCHAR *Ptr;

    if (!OpenRead()) return Default;
    if (!(Ptr = ScanFor ( SecName, KeyName))) return Default;
    return (unsigned int) wcstol( Ptr, NULL, 10);
}


// This function now allows you to look into a file with duplicated section names
// It shouldn't be necessary but if you haven't looked to a PTC.ASC file, do it now
// you will find a duplicated RF section
TCHAR *xcIniFile::GetProfileString(TCHAR *SecName, TCHAR *KeyName, TCHAR *Default)
{
    TCHAR *Ptr;
/*M90*/ static TCHAR Nulo[10];
/*M90*/ TCHAR        *Retorno;

/*M90*/ memset(Nulo, 0, sizeof(Nulo));
/*M90*/ if(Default == NULL)
/*M90*/     Retorno = Nulo;
/*M90*/ else
/*M90*/     Retorno = Default;

    if ( !OpenRead() ) 
        return Default;

    while (TRUE)
    {    
        if ( !ScanForSection( SecName))
            return Default;

        if (Default != (Ptr = GetNextString(KeyName,Default)))
        {
            return Ptr;
        }
    }
}

TCHAR *xcIniFile::GetNextString(TCHAR *KeyName, TCHAR *Default)
{
    register TCHAR *Ptr;
    register int i;
    register TCHAR *p;


    
    if ( !(Ptr = ScanForKeyName ( KeyName)) ) 
        return Default;

    if ( NULL != ( p = wcschr ( Ptr, ';' ) ) ) *p = '\0'; // lets get ride of comments

    // trim off trailing whitespace
    for ( i = wcslen (Ptr)-1; i > 0; i-- ) 
        if ( isspace(Ptr[i]) ) Ptr[i] = '\0';   // trim it
        else break;
        
    return Ptr;
}


