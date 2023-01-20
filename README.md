# AutoDeskDownloader

## Purpose of this tool
This tool allows to download all Autodesk screen casts you own and a pdf of the screen casts detail page so that you do not loose any additional information you stored there.

In case this tool worked out for you and saved some time manually downloading everything, please consider a donation via PayPal philipp.butz92@googlemail.com

## Disclaimer:
This tool was written fast and only to allow this special use case and not any generic. The page of Autodesk is sadly not written in a way which makes it easy to download files/information. Therefore the only way I figured out to work was using Selenium and actually simulate clicks in the browser. If you have any issues with this tool for your usecase, you might want to fork this repository, I will probably not put additional work into this.

## How to use this tool
You need to:
- if you do not have chrome, you need to download it as well
- download a version of this tool in the release folder in this repository.
- provide a chrome driver for your system (https://chromedriver.chromium.org/downloads) which should be next to the exe of this application (**the version has to be the same as your chrome installation**).
- provide your login information, it should be in a simple text file on your Desktop named "AutodeskLogin.txt" and should contain 2 lines; the first line should be your login email, the second one your password.

## Process
1) Open a powershell in the folder you placed this applications executable.
2) Run the executable (e.g. '.\AutoDeskDownloader_0.0.1.exe')
3) A new chrome window will open and you can see what this tool is doing (Do not close this window as the tool wont work, however, you can minimize it **but not resize**)
4) Additionally you can see in the powershell what this tool is printing out and what it is currently doing
5) In case you see any errors or exceptions you can add issues here (but I might not answer to them)
6) The chrome window will be closed automatically after finishing

## Resume Process
In case of an aborted process because the website broke etc. you are able to resume on the page you see last in the logs. The entry will look like this 'Extracting screen casts of page 58' .

To resume the process, just start over at page 58 by calling this tool like this '.\AutoDeskDownloader_0.0.1.exe --page 58'.

It will start downloading all screencasts on page 58 and will continue normally.

## Result of running this tool
You will have all your screencasts (of all pages) as MP4 in your Downloads folder on your computer additionally you will have a pdf per screencast named like the title of the screencast with the content of the screen casts detail page in it.

## Known issues
You might see log entries like '[24204:3772:0117/092357.726:ERROR:device_event_log_impl.cc(215)] [09:23:57.726] USB: usb_device_handle_win' . I did not figure out how to omit them, you can ignore those.

You might see a log entry like 'Current browser version is 108.xxxx'. Make sure that you download the correct version of the chrome driver (same as the chrome you have currently installed).

It might be the case that the window chrome does not maximize, please maximize it yourself as soon as the browser opens.