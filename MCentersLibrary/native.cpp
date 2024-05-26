#include "pch.h"
#include "Zydis.h"

#include <fstream>
#include <vector>
#include <iostream>


namespace MCentersNative {
    const  int SizeOfLeaOpcode = 7;
    const int SizeOfLeaOpcodeWithoutMemoryOffset = 3;

    const char x64CrackOpcodeForServiceStore[] = { 0xb1, 0x00, 0x90 };


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
    void FindLeaReference() {

        std::fstream file("D:\\Windows.ApplicationModel.Store.dll", std::ios::in | std::ios::out | std::ios::binary);
        if (!file) {

            return ;
        }
        file.seekg(0, std::ios::end); // moving to end to help determine file size
        std::streamsize size = file.tellg();
        file.seekg(0, std::ios::beg);

        std::vector<char> buffer(size);

        if (file.read(buffer.data(), size))
        {
            int num = 1;
            if (*(char*)&num != 1) goto exit;  // this line ensures code only runs when running in little endian mode, we dont support big endian for now
            std::string fileStringMode(buffer.begin(), buffer.end());
            std::string search = "Windows::Services::Store::StoreAppLicense::get_IsTrial";
            auto offset = fileStringMode.find(search);
            
            size_t leaAddress = 0;
            std::string val = "\u004c\u008d\u0005";


            int offsetInLea = 0;
            while (leaAddress != -1) {
                leaAddress = fileStringMode.find(val, leaAddress + 1);
                
                fileStringMode.copy(reinterpret_cast<char*>(&offsetInLea), sizeof(offsetInLea), leaAddress + SizeOfLeaOpcodeWithoutMemoryOffset);
                if (leaAddress + offsetInLea + SizeOfLeaOpcode == offset) {
                    // Example size_t value
                    char hex_buffer[20];  // Buffer to hold the hex string, ensure it is large enough

                    // Convert size_t value to hex string
                    sprintf_s(hex_buffer, sizeof(hex_buffer), "0x%zx", leaAddress);;
                    auto str = std::string(hex_buffer);

                    ZyanU8 data[300];
                        
                    std::copy(buffer.begin() + leaAddress, buffer.begin() + leaAddress + 300, data);

                    // The runtime address (instruction pointer) was chosen arbitrarily here in order to better 
                    // visualize relative addressing. In your actual program, set this to e.g. the memory address 
                    // that the code being disassembled was read from. 
                    ZyanU64 runtime_address = leaAddress;

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
                        if (areStringsEqual(opcode,"ret"))
                            break;
                        if (stringStartsWith(opcode, "mov cl, [rcx")) {
                            auto address = runtime_address;
                            writeBytesAtAddress(file, address, x64CrackOpcodeForServiceStore, 3);
                            goto exit;
                        }
                        
                        offset += instruction.info.length;
                        runtime_address += instruction.info.length;
                    }
                    int k = 0;
                }
            }

        }
    exit:;
        file.close();
        return;
    }

}