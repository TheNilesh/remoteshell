using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace RemoteShell
{
    class CmdPrompt
    {
        private Process process;
        private string currDir;
        private bool started;

        public CmdPrompt()
        {
            started = false;
        }

        public string startCmd()
        {
            if (started == false)
            {
                process = new Process
                {
                    StartInfo = new ProcessStartInfo("cmd")
                    {
                        UseShellExecute = false,
                        RedirectStandardInput = true,
                        RedirectStandardOutput = true,
                        CreateNoWindow = true,
                        Arguments = "/k",
                    }
                };

                process.Start();
                started = true;
                Console.WriteLine("cmd started");
            }

            currDir = RunCommand("echo %cd%").Trim();
            //Console.Write(currDir + "> ");
            return currDir + "> ";
        }

        public string execute(string c)
        {
            if (started == false)
            {
                return "Not Initiated";
            }

            var result = new StringBuilder();

            //Console.Write(RunCommand(c));
            result.Append(RunCommand(c));
            if (c.StartsWith("cd"))     //only if cd command issued
            {
                currDir = RunCommand("echo %cd%").Trim();
            }
            //Console.Write(currDir + "> ");
            result.Append(currDir + "> ");

            return result.ToString();
        }

        //ref: http://stackoverflow.com/questions/15870516/redirect-input-and-output-for-cmd-exe
        private string RunCommand(string command)
        {
            const string prompt = "--foobar";

            // replacing standard prompt in order to determine end of command output
            process.StandardInput.WriteLine("prompt " + prompt);
            process.StandardInput.Flush();
            process.StandardOutput.ReadLine();
            process.StandardOutput.ReadLine();

            var commandResult = new StringBuilder();

            try
            {
                process.StandardInput.WriteLine(command);
                process.StandardInput.WriteLine();
                process.StandardInput.Flush();
                if(command.Equals("exit"))
                {
                    started=false;
                    return "EXITED";
                }
                process.StandardOutput.ReadLine();

                while (true)
                {
                    var line = process.StandardOutput.ReadLine();

                    if (line == prompt) // end of command output
                        break;

                    commandResult.AppendLine(line);
                }
            }
            catch { }
            return commandResult.ToString();
        }//RunCommand
    }
}
