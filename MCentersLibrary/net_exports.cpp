#include "net_pch.h"
#include "native.h"
#include "net_exports.h"

namespace MCentersLibrary {
	public  ref class Functions
	{
	public: static void SendToastNotification() {
		ShowNotification(L"Example", L"example2");
	}
	public: static int Add(int a, int b) {
		return test2(a, b);
	}
	};

}