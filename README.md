# Supervisor
Simple process supervisor for Windows. Download built app here: [https://github.com/chebum/Supervisor/releases/tag/1.0](https://github.com/chebum/Supervisor/releases/tag/1.0)

## Usage examples
Start and monitor one app:

	supervisor myapp.exe
                
Start and monitor two apps:

	supervisor app1.exe "Path With Spaces\app2.exe"

Start two apps with arguments:

	supervisor "app1.exe arg1" """Path With Spaces\app1.exe"" ""argument with spaces"""
