# LNKMod

## Output File Names
- Will update the .LNK you specify if using ```-modify```
- Will create ```payload.lnk``` if using ```-create```

## Usage / Test Cases
- Modify (will modify .LNK in current directory, only place one!)
    - Modify path to executable, no arguments
        -  ```.\LNKMod.exe -modify -path "C:\Windows\system32\calc.exe"```
    - Modify path to executable and arguments
        - ```.\LNKMod.exe -modify -path "C:\Windows\system32\cmd.exe" -args "/c notepad.exe"```
- Create (will create ```payload.lnk``` in current directory, do not have any other .LNK in current directory!)
    - Create .LNK with path to executable, no arguments, and path to icon
        - ```.\LNKMod.exe -create -path "C:\Windows\system32\calc.exe" -icopath "C:\Users\John\AppData\Local\Microsoft\OneDrive\OneDrive.exe"```
    - Create .LNK with path to executable, arguments, and path to icon 
        - ```.\LNKMod.exe -create -path "C:\Windows\system32\cmd.exe" -args "/c calc.exe" -icopath "C:\Users\John\AppData\Local\Microsoft\OneDrive\OneDrive.exe"```
    - Sets access/write time to current time and creation time to a (reasonable) random time
- Dump metadata for a specified .LNK
    - ```.\LNKMod.exe -dump "C:\Users\John\source\repos\LNKMod\LNKMod\bin\Release\payload.lnk"```
- Dump metadata for a specified .LNK in JSON format
    - ```.\LNKMod.exe -dumpjson "C:\Users\John\source\repos\LNKMod\LNKMod\bin\Release\payload.lnk"```

## Potentially Dangerous Metadata
- CreationTime
- AccessTime
- WriteTime
- DriveSerialNumber
- KnownFolderID (GUID can be converted to a string can leak usernames)
- MachineID (NetBIOS name)

## References
- https://github.com/securifybv/ShellLink
- https://github.com/JamesNK/Newtonsoft.Json
