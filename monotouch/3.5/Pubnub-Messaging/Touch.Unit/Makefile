MDTOOL="/Applications/Xamarin Studio.app/Contents/MacOS/mdtool"
MTOUCH=/Developer/MonoTouch/usr/bin/mtouch
TOUCH_SERVER=/Users/rajat-ml/Projects/Xamarin.iOS/3.5/Pubnub-Messaging-modified-tests/Touch.Server/bin/Debug/Touch.Server.exe

all: build-simulator

run run-test: run-simulator

$(TOUCH_SERVER):
	cd Touch.Server && xbuild

build-simulator:
	$(MDTOOL) -v build -t:Build "-c:Debug|iPhoneSimulator" /Users/rajat-ml/Projects/Xamarin.iOS/3.5/Pubnub-Messaging-modified-tests/Pubnub-Messaging.sln
run-simulator: build-simulator 
	rm -f sim-results.log
	mono --debug $(TOUCH_SERVER) --launchsim bin/iPhoneSimulator/Debug/TouchUnit.app -autoexit -logfile=sim-results.log
	cat sim-results.log
