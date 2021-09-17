# # PostInstallSetup.ps1 #
$SharePath = "C:\inetpub\wwwroot\assets\attachments"
$Acl = Get-ACL $SharePath
$AccessRule = New-Object System.Security.AccessControl.FileSystemAccessRule("DefaultAppPool","full","ContainerInherit,Objectinherit","none","Allow")
$Acl.AddAccessRule($AccessRule)
Set-Acl $SharePath $Acl