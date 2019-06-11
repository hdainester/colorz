WIN := platforms/desktop/bin/Release/netcoreapp2.2/win-x64/publish
OSX := platforms/desktop/bin/Release/netcoreapp2.2/osx-x64/publish
ICO := platforms/desktop/set/colorz.ico
ISS := platforms/desktop/set/colorz.iss
EXE := $(WIN)/Colorz.exe
ZIP := Colorz.zip

setup: icon
	iscc $(ISS)

zipos: osx-exe
	cd $(OSX); zip -r ../../../../$(ZIP) .; cd .

icon: win-exe
	rh -open $(EXE) -save $(EXE) -action addoverwrite -res $(ICO) -mask ICONGROUP,MAINICON

win-exe: game
	cd platforms/desktop; dotnet publish -c Release -r win-x64 --self-contained false

osx-exe: game
	cd platforms/desktop; dotnet publish -c Release -r osx-x64 --self-contained false

game: restore
	cd game; dotnet build;

restore-game: clean
	cd game; dotnet restore

restore-desktop:
	cd platforms/desktop; dotnet restore

clean-bin:
	find . -type d -name "bin" -exec rm -rfv {} +

clean-obj:
	find . -type d -name "obj" -exec rm -rfv {} +

clean-tmp:
	find . -type f -name "*.template" -exec rm -rfv {} +

restore: restore-game restore-desktop
clean: clean-bin clean-tmp clean-obj
all: setup zipos