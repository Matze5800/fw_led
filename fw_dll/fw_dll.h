#pragma once
#ifdef FWDLL_EXPORTS
#define FWDLL_API __declspec(dllexport)
#else
#define FWDLL_API __declspec(dllimport)
#endif

extern "C" FWDLL_API void cmd_led(int state);
