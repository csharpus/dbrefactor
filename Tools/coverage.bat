"C:\Program Files (x86)\Gubka Bob\PartCover .NET 2.3\PartCover.exe" ^
	--target "D:\Tools\NUnit-2.5.2.9222\NUnit-2.5.2.9222\bin\net-2.0\nunit-console-x86.exe" ^
	--target-work-dir "D:\Projects\DbRefactor\Source\Tests.Integration\bin\Debug" ^
	--target-args "DbRefactor.Tests.Integration.dll" ^
	--include [DbRefactor]* ^
	--exclude [DbRefactor.Tests.Integration]* ^
	--output "D:\Projects\DbRefactor\Source\Tests.Integration\bin\Debug\coverage.xml"
pause