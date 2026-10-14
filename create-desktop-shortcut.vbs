' Creates a desktop shortcut "ChatGPT HAAVK" that points to HAAVK-ChatGPT.exe in this folder.
Set sh = CreateObject("WScript.Shell")
Set fso = CreateObject("Scripting.FileSystemObject")
dir = fso.GetParentFolderName(WScript.ScriptFullName)

If Not fso.FileExists(dir & "\HAAVK-ChatGPT.exe") Then
    MsgBox "HAAVK-ChatGPT.exe was not found next to this script.", 16, "HAAVK"
    WScript.Quit 1
End If

Set lnk = sh.CreateShortcut(sh.SpecialFolders("Desktop") & "\ChatGPT HAAVK.lnk")
lnk.TargetPath = dir & "\HAAVK-ChatGPT.exe"
lnk.WorkingDirectory = dir
lnk.IconLocation = dir & "\haavk.ico,0"
lnk.Description = "ChatGPT with the HAAVK loading animation"
lnk.Save

MsgBox "Desktop shortcut created: ChatGPT HAAVK", 64, "HAAVK"
