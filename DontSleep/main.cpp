#include <Windows.h>

int CALLBACK wWinMain(HINSTANCE hInstance, HINSTANCE hPrevInstance, LPWSTR lpCmdLine, int nCmdShow)
{
    REASON_CONTEXT reasonContext = { 0 };
    reasonContext.Version = POWER_REQUEST_CONTEXT_VERSION;
    reasonContext.Flags = POWER_REQUEST_CONTEXT_SIMPLE_STRING;
    reasonContext.Reason.SimpleReasonString = L"알람 기능을 위해서 전원 설정을 조정합니다";

    HANDLE powerRequest = PowerCreateRequest(&reasonContext);
    if (powerRequest == INVALID_HANDLE_VALUE)
    {
        MessageBoxW(nullptr, L"PowerCreateRequest 호출 실패", L"오류", MB_OK);
        return 1;
    }

    PowerSetRequest(powerRequest, PowerRequestExecutionRequired);

    // CreateProcess는 첫번째 인자로 NULL을 받는다면 두번째 인자가 거의 커맨드라인 역할을 한다고 한다
    DWORD creationFlags = CREATE_SUSPENDED;

    STARTUPINFOW startupInfo = { 0 };
    startupInfo.cb = sizeof(STARTUPINFOW);

    PROCESS_INFORMATION processInformation;

    if (!CreateProcessW(nullptr, lpCmdLine, nullptr, nullptr, FALSE, creationFlags, nullptr, nullptr, &startupInfo, &processInformation))
    {
        MessageBoxW(nullptr, L"인자로 들어온 프로그램 호출에 실패했습니다.", L"오류", MB_OK);
        return 1;
    }

    HANDLE job = CreateJobObjectW(nullptr, nullptr);
    HANDLE ioPort = CreateIoCompletionPort(INVALID_HANDLE_VALUE, nullptr, 0, 1);

    JOBOBJECT_ASSOCIATE_COMPLETION_PORT port;
    port.CompletionKey = job;
    port.CompletionPort = ioPort;

    SetInformationJobObject(job, JobObjectAssociateCompletionPortInformation, &port, sizeof(port));


    AssignProcessToJobObject(job, processInformation.hProcess);



    ResumeThread(processInformation.hThread);

    DWORD completionCode;
    ULONG_PTR completionKey;
    LPOVERLAPPED overlapped;
    while (GetQueuedCompletionStatus(ioPort, &completionCode, &completionKey, &overlapped, INFINITE))
    {
        if ((HANDLE)completionKey == job && completionCode == JOB_OBJECT_MSG_ACTIVE_PROCESS_ZERO)
            break;
    }

    CloseHandle(processInformation.hProcess);
    CloseHandle(processInformation.hThread);
    CloseHandle(job);

    PowerClearRequest(powerRequest, PowerRequestExecutionRequired);
    CloseHandle(powerRequest);
    return 0;
}