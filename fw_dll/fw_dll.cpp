#include <windows.h>
#include <ioapiset.h>
#include <wil/resource.h>
#include <wil/result_macros.h>
#include <iostream>
#include "../CrosEC/Public.h"
#include "fw_dll.h"

static wil::unique_hfile device;

extern void cmd_led(int state) {
	device.reset(CreateFileW(LR"(\\.\GLOBALROOT\Device\CrosEC)", GENERIC_READ | GENERIC_WRITE,
	                         FILE_SHARE_READ | FILE_SHARE_WRITE, nullptr, OPEN_EXISTING, 0, nullptr));
	int64_t nv = 0;
	switch(state) {
		case 0:
			nv = 0x1; //off
			break;
		case 1:
			nv = 0x640001;  //red
			break;
		case 2:
			nv = 0x64000001;  //green
			break;
		case 3:
			nv = 0x640000000001;  //yellow
			break;
		case 4:
			nv = 0x0201; //auto
			break;
	}
	size_t cmdsz = sizeof(CROSEC_COMMAND) + CROSEC_CMD_MAX_REQUEST;
	if(PCROSEC_COMMAND cmd = (PCROSEC_COMMAND)calloc(1, cmdsz)) {
		if(char* bptr = CROSEC_COMMAND_DATA(cmd)) {
			memcpy(bptr, &nv, sizeof(nv));
			bptr += sizeof(nv);
		} else {
			printf("Out of memory!");
		}
		cmd->command = 0x29;
		cmd->insize = 8;
		cmd->outsize = 8;
		cmd->version = 1;
		DeviceIoControl(device.get(), IOCTL_CROSEC_XCMD, cmd, (DWORD)cmdsz, cmd, (DWORD)cmdsz, 0, nullptr);
		free(cmd);
	}
}


BOOL APIENTRY DllMain(HMODULE hModule, DWORD ul_reason_for_call, LPVOID lpReserved) {
	return TRUE;
}