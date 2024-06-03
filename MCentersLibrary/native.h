#pragma once
#include <string>

namespace MCentersNative {
	namespace DllMethod {
		bool Patchx64Dll();
		bool Patchx86Dll();
		int GetPlatform(std::string dllFile);
		bool IsPresent();
	}
}