// The following ifdef block is the standard way of creating macros which make exporting 
// from a DLL simpler. All files within this DLL are compiled with the ITESCREENANALYZERDLL_EXPORTS
// symbol defined on the command line. this symbol should not be defined on any project
// that uses this DLL. This way any other project whose source files include this file see 
// ITESCREENANALYZERDLL_API functions as being imported from a DLL, whereas this DLL sees symbols
// defined with this macro as being exported.
#ifdef ITESCREENANALYZERDLL_EXPORTS
#define ITESCREENANALYZERDLL_API __declspec(dllexport)
#else
#define ITESCREENANALYZERDLL_API __declspec(dllimport)
#endif

// This class is exported from the ITEScreenAnalyzerDLL.dll
class ITESCREENANALYZERDLL_API CITEScreenAnalyzerDLL {
public:
	CITEScreenAnalyzerDLL(void);
	// TODO: add your methods here.
};

extern ITESCREENANALYZERDLL_API int nITEScreenAnalyzerDLL;

ITESCREENANALYZERDLL_API int fnITEScreenAnalyzerDLL(void);
