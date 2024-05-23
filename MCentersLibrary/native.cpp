#include "native.h"

#include "pch.h"

#include <winrt/Windows.Data.Xml.Dom.h>
#include <winrt/Windows.UI.Notifications.h>
#include <winrt/base.h>
#include <string>
#include <combaseapi.h>
#include <wrl.h>
#include <Psapi.h>
#include <shobjidl_core.h>
#include <propvarutil.h>
#include <propkey.h>
using namespace Microsoft::WRL;
using namespace winrt::Windows::Data::Xml::Dom;
using namespace winrt::Windows::UI::Notifications;


bool com_initiated = false;


auto AppId = L"Tinedpakgamer.MCenters.MCenters8Main";
HRESULT InstallShortcut(_In_z_ wchar_t* shortcutPath)
{
    wchar_t exePath[MAX_PATH];

    DWORD charWritten = GetModuleFileNameEx(GetCurrentProcess(), nullptr, exePath, ARRAYSIZE(exePath));

    HRESULT hr = charWritten > 0 ? S_OK : E_FAIL;

    if (SUCCEEDED(hr))
    {
        ComPtr<IShellLink> shellLink;
        hr = CoCreateInstance(CLSID_ShellLink, nullptr, CLSCTX_INPROC_SERVER, IID_PPV_ARGS(&shellLink));

        if (SUCCEEDED(hr))
        {
            hr = shellLink->SetPath(exePath);
            if (SUCCEEDED(hr))
            {
                hr = shellLink->SetArguments(L"");
                if (SUCCEEDED(hr))
                {
                    ComPtr<IPropertyStore> propertyStore;

                    hr = shellLink.As(&propertyStore);
                    if (SUCCEEDED(hr))
                    {
                        PROPVARIANT appIdPropVar;
                        hr = InitPropVariantFromString(AppId, &appIdPropVar);
                        if (SUCCEEDED(hr))
                        {
                            hr = propertyStore->SetValue(PKEY_AppUserModel_ID, appIdPropVar);
                            if (SUCCEEDED(hr))
                            {
                                hr = propertyStore->Commit();
                                if (SUCCEEDED(hr))
                                {
                                    ComPtr<IPersistFile> persistFile;
                                    hr = shellLink.As(&persistFile);
                                    if (SUCCEEDED(hr))
                                    {
                                        hr = persistFile->Save(shortcutPath, TRUE);
                                    }
                                }
                            }
                            PropVariantClear(&appIdPropVar);
                        }
                    }
                }
            }
        }
    }
    return hr;
}


HRESULT TryCreateShortcut()
{
    wchar_t shortcutPath[MAX_PATH];
    DWORD charWritten = GetEnvironmentVariable(L"APPDATA", shortcutPath, MAX_PATH);
    HRESULT hr = charWritten > 0 ? S_OK : E_INVALIDARG;

    if (SUCCEEDED(hr))
    {
        errno_t concatError = wcscat_s(shortcutPath, ARRAYSIZE(shortcutPath), L"\\Microsoft\\Windows\\Start Menu\\Programs\\M Centers 8th Edition.lnk");

        hr = concatError == 0 ? S_OK : E_INVALIDARG;
        if (SUCCEEDED(hr))
        {
            DWORD attributes = GetFileAttributes(shortcutPath);
            bool fileExists = attributes < 0xFFFFFFF;

            if (!fileExists)
            {
                hr = InstallShortcut(shortcutPath);  // See step 2.
            }
            else
            {
                hr = S_FALSE;
            }
        }
    }
    return hr;
}
void ShowNotification(const std::wstring& title, const std::wstring& message)
{
    if (!com_initiated) {
        CoInitialize(NULL);
        TryCreateShortcut();
        com_initiated = true;
    }
    // Create the toast XML content
    XmlDocument toastXml = ToastNotificationManager::GetTemplateContent(ToastTemplateType::ToastText02);

    // Set the title and message
    auto toastNodeList = toastXml.GetElementsByTagName(L"text");
    toastNodeList.Item(0).AppendChild(toastXml.CreateTextNode(winrt::hstring(title)));
    toastNodeList.Item(1).AppendChild(toastXml.CreateTextNode(winrt::hstring(message)));

    // Create the toast notification
    ToastNotification toast(toastXml);

    // Show the toast notification
    auto def = ToastNotificationManager::GetDefault();
    auto notifier = def.CreateToastNotifier(AppId);
    notifier.Show(toast);
}



int test2(int a, int b) {
    return a + b;
}