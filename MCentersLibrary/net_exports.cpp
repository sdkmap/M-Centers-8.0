#include "net_pch.h"
#include "net_exports.h"
#include "native.h"
namespace MCentersLibrary {
	public  ref class Functions
	{
	
	public: static bool FindLeaReference() {
	 return	MCentersNative::Patchx64Dll();
	}
	};

}