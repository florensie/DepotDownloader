using System;
using System.Diagnostics;
using System.Threading.Tasks;
using SteamKit2.Authentication;

namespace DepotDownloader;

public class CmdAuthenticator : IAuthenticator
{
    private readonly string _cmd;

    public CmdAuthenticator(string cmd)
    {
        _cmd = cmd;
    }

    public Task<string> GetDeviceCodeAsync(bool previousCodeWasIncorrect)
    {
        Console.WriteLine("Waiting for totp code from subprocess.");
        var process = StartProcess("totp");
        var output = process.StandardOutput.ReadToEnd().Trim();
        process.WaitForExit();
        if (process.ExitCode != 0)
        {
            throw new SystemException($"Non-zero exit code from subprocess: {process.StandardError.ReadToEnd()}");
        }
        return Task.FromResult(output);
    }

    public Task<string> GetEmailCodeAsync(string email, bool previousCodeWasIncorrect)
    {
        Console.WriteLine("Waiting for email verification code from subprocess");
        var process = StartProcess("email");
        var output = process.StandardOutput.ReadToEnd().Trim();
        process.WaitForExit();
        if (process.ExitCode != 0)
        {
            throw new SystemException($"Non-zero exit code from subprocess: {process.StandardError.ReadToEnd()}");
        }
        return Task.FromResult(output);
    }

    public Task<bool> AcceptDeviceConfirmationAsync()
    {
        Console.WriteLine("Waiting for device confirmation from subprocess.");
        var process = StartProcess("deviceconfirmation");
        process.WaitForExit();
        return Task.FromResult(process.ExitCode == 0);
    }

    private Process StartProcess(string arguments)
    {
        var process = new Process();
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.CreateNoWindow = true;
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.RedirectStandardError = true;
        process.StartInfo.FileName = _cmd;
        process.StartInfo.Arguments = arguments;
        process.Start();
        return process;
    }
}
