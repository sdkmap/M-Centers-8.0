#include "pch.h"
#include "Zydis.h"

#include <fstream>
#include <vector>
#include <iostream>


namespace MCentersNative {
	const  int SizeOfx64LeaOpcode = 7;
	const int SizeOfx64LeaOpcodeWithoutMemoryOffset = 3;

	const char x64CrackOpcode[] = { 0xb1, 0x00, 0x90 };
	std::string x64LeaReferenceOpcodePrefix = "\u004c\u008d\u0005"; //means lea r8,
	std::string Windows_Services_Store_StoreApplicense_getTrial_keyword = "Windows::Services::Store::StoreAppLicense::get_IsTrial";
	std::string Windows_ApplicationModel_Store_LicenseInformationServer_getTrial_keyword = "Windows::ApplicationModel::Store::LicenseInformationServer::get_IsTrial";

	bool writeBytesAtAddress(std::fstream& file, ZyanU64 address, const char* data, std::size_t dataSize) {
		// Open the file for both reading and writing in binary mode
		if (!file) {

			return false;
		}

		// Seek to the specified address
		file.seekp(static_cast<std::streampos>(address));
		if (!file) {

			return false;
		}

		// Write the data to the file
		file.write(data, dataSize);
		if (!file) {

			return false;
		}
		return true;
	}


	bool areStringsEqual(const char* str1, const char* str2) {
		while (*str1 && *str2) {
			if (*str1 != *str2) {
				return false;
			}
			++str1;
			++str2;
		}
		// Both strings must end at the same time to be equal
		return *str1 == '\0' && *str2 == '\0';
	}
	bool stringStartsWith(const char* str, const char* prefix) {
		while (*prefix) {
			if (*str != *prefix) {
				return false;
			}
			++str;
			++prefix;
		}
		return true;
	}
	bool Patchx64Dll() {
		bool result = false;
		int num = 1;
		if (*(char*)&num != 1) return false;  // this line ensures code only runs when running in little endian mode, we dont support big endian for now
		std::fstream file("D:\\Windows.ApplicationModel.Store.dll", std::ios::in | std::ios::out | std::ios::binary);
		if (!file) {

			return false;
		}
		file.seekg(0, std::ios::end); // moving to end to help determine file size
		std::streamsize size = file.tellg();
		file.seekg(0, std::ios::beg);

		std::vector<char> fileBuffer(size);

		if (!file.read(fileBuffer.data(), size)) goto exit;
		else {

			// windows service store part
			std::string fileStringMode(fileBuffer.begin(), fileBuffer.end());
			auto storeApplicenseGetTrialKeywordPosition = fileStringMode.find(Windows_Services_Store_StoreApplicense_getTrial_keyword);
			if (storeApplicenseGetTrialKeywordPosition != -1) {
				size_t storeApplicenseGetTrialKeywordReferenceOffset = 0;



				int offsetInLea = 0;
				while (storeApplicenseGetTrialKeywordReferenceOffset != -1) {
					storeApplicenseGetTrialKeywordReferenceOffset = fileStringMode.find(x64LeaReferenceOpcodePrefix, storeApplicenseGetTrialKeywordReferenceOffset + 1);
					if (storeApplicenseGetTrialKeywordReferenceOffset == -1) goto exit;
					fileStringMode.copy(reinterpret_cast<char*>(&offsetInLea), sizeof(offsetInLea), storeApplicenseGetTrialKeywordReferenceOffset + SizeOfx64LeaOpcodeWithoutMemoryOffset);
					if (storeApplicenseGetTrialKeywordReferenceOffset + offsetInLea + SizeOfx64LeaOpcode == storeApplicenseGetTrialKeywordPosition) {

						ZyanU8 data[100];

						std::copy(fileBuffer.begin() + storeApplicenseGetTrialKeywordReferenceOffset, fileBuffer.begin() + storeApplicenseGetTrialKeywordReferenceOffset + 100, data);

						// The runtime address (instruction pointer) was chosen arbitrarily here in order to better 
						// visualize relative addressing. In your actual program, set this to e.g. the memory address 
						// that the code being disassembled was read from. 
						ZyanU64 runtime_address = storeApplicenseGetTrialKeywordReferenceOffset;

						// Loop over the instructions in our buffer. 
						ZyanUSize offset = 0;
						ZydisDisassembledInstruction instruction;
						while (ZYAN_SUCCESS(ZydisDisassembleIntel(
							/* machine_mode:    */ ZYDIS_MACHINE_MODE_LONG_64,
							/* runtime_address: */ runtime_address,
							/* buffer:          */ data + offset,
							/* length:          */ sizeof(data) - offset,
							/* instruction:     */ &instruction
						))) {

							auto opcode = instruction.text;
							if (stringStartsWith(opcode, "mov cl, [rcx")) {
								if (!writeBytesAtAddress(file, runtime_address, x64CrackOpcode, 3)) goto exit;
								break;
							}
							if (areStringsEqual(opcode, "ret"))
								goto exit;

							offset += instruction.info.length;
							runtime_address += instruction.info.length;
						}
						break;
					}
				}
			}


			// windows application model store part
			auto licenseInformationServerGetTrialKeywordPosition = fileStringMode.find(Windows_ApplicationModel_Store_LicenseInformationServer_getTrial_keyword);
			if (licenseInformationServerGetTrialKeywordPosition == -1) goto exit; // no dll will be missing this value so we have an error in dll and we must exit
			size_t licenseInformationServerGetTrialKeywordReferenceOffset = 0;



			int offsetInLea = 0;
			while (licenseInformationServerGetTrialKeywordReferenceOffset != -1) {
				licenseInformationServerGetTrialKeywordReferenceOffset = fileStringMode.find(x64LeaReferenceOpcodePrefix, licenseInformationServerGetTrialKeywordReferenceOffset + 1);
				if (licenseInformationServerGetTrialKeywordReferenceOffset == -1) goto exit;
				fileStringMode.copy(reinterpret_cast<char*>(&offsetInLea), sizeof(offsetInLea), licenseInformationServerGetTrialKeywordReferenceOffset + SizeOfx64LeaOpcodeWithoutMemoryOffset);
				if (licenseInformationServerGetTrialKeywordReferenceOffset + offsetInLea + SizeOfx64LeaOpcode == licenseInformationServerGetTrialKeywordPosition) {

					ZyanU8 data[100];

					std::copy(fileBuffer.begin() + licenseInformationServerGetTrialKeywordReferenceOffset, fileBuffer.begin() + licenseInformationServerGetTrialKeywordReferenceOffset + 100, data);

					// The runtime address (instruction pointer) was chosen arbitrarily here in order to better 
					// visualize relative addressing. In your actual program, set this to e.g. the memory address 
					// that the code being disassembled was read from. 
					ZyanU64 runtime_address = licenseInformationServerGetTrialKeywordReferenceOffset;

					// Loop over the instructions in our buffer. 
					ZyanUSize offset = 0;
					ZydisDisassembledInstruction instruction;
					while (ZYAN_SUCCESS(ZydisDisassembleIntel(
						/* machine_mode:    */ ZYDIS_MACHINE_MODE_LONG_64,
						/* runtime_address: */ runtime_address,
						/* buffer:          */ data + offset,
						/* length:          */ sizeof(data) - offset,
						/* instruction:     */ &instruction
					))) {

						auto opcode = instruction.text;

						if (stringStartsWith(opcode, "setnz cl")) {

							if (!writeBytesAtAddress(file, runtime_address, x64CrackOpcode, 3))goto exit;
							break;
						}
						if (areStringsEqual(opcode, "ret"))
							goto exit;

						offset += instruction.info.length;
						runtime_address += instruction.info.length;
					}
					break;
				}
			}


		}
		result = true;
	exit:;
		file.close();
		return result;
	}

}