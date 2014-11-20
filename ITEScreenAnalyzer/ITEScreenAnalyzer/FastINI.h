
#ifndef _FAST_INI_FILE_HPP_
#define _FAST_INI_FILE_HPP_

//#include "xxtypes.hpp"
// general definitions
#ifndef FALSE
#define FALSE 0
#define TRUE (!FALSE)
#endif


#define MAX_LINE_SIZE   512

class xcIniFile {

    public:
                    xcIniFile(TCHAR *IniFile);
                    ~xcIniFile();
            TCHAR *  GetProfileString(TCHAR *SecName, TCHAR *KeyName, TCHAR *Default);
            long    GetProfileLong(TCHAR *SecName, TCHAR *KeyName, long Default);
            int     GetProfileInt(TCHAR *SecName, TCHAR *KeyName, int Default);
            unsigned int   GetProfileWord(TCHAR *SecName, TCHAR *KeyName, unsigned int Default);
            TCHAR *  GetNextString(TCHAR *KeyName, TCHAR *Default);
//            short   WriteProfileString(TCHAR *SecName, TCHAR *KeyName, TCHAR *Value);
            short   IniFileFound() { return FileExists;}
//            void    DelCurrentKeyword();            
    private:
            short   OpenRead(void);
            short   ReadLine(void);
            int     GetC(TCHAR &C);
            short   ScanForSection (TCHAR *Section);
            TCHAR *  ScanForKeyName (TCHAR *KeyName );
            TCHAR *  ScanFor (TCHAR *Section, TCHAR *KeyName );
//            short   FlushBuffer();
            
            TCHAR                *Line; // Here I read a line
            TCHAR                 *FileContents;  // here I read the complete file
            DWORD		         FileLen; //Self explanatory
            unsigned int         FileIndex; // Index to current pos in FileContents
            TCHAR                *IniFileName;
            short               FileExists;
			HANDLE				hFile;
};

#endif

