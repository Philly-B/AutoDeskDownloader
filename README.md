# AutoDeskDownloader

## Purpose of this tool
This tool allows to download all Autodesk screen casts you own and a pdf of the screen casts detail page so that you do not loose any additional information you stored there. 


## Disclaimer:
This tool was written fast and only to allow this special use case and not any generic. The page of Autodesk is sadly not written in a way which makes it easy to download files/information. Therefore the only way I figured out to work was using Selenium and actually simulate clicks in the browser. If you have any issues with this tool for your usecase, you might want to fork this repository, I will probably not put additional work into this.

## How to use this tool
You need to:
- download a version of this tool in the release folder in this repository.
- provide a chrome driver for your system (https://chromedriver.chromium.org/downloads) which should be next to the exe of this application.
- provide your login information, it should be in a simple text file on your Desktop named "AutodeskLogin.txt" and should contain 2 lines; the first line should be your login email, the second one your password.

## Process
1) Open a powershell in the folder you placed this applications executable.
2) Run the executable (e.g. '.\AutoDeskDownloader_0.0.1.exe')
3) A new chrome window will open and you can see what this tool is doing (Do not close this window as the tool wont work, however, you can minimize it)
4) Additionally you can see in the powershell what this tool is printing out and what it is currently doing
5) In case you see any errors or exceptions you can add issues here (but I might not answer to them)
6) The chrome window will be closed automatically after finishing

## Result of running this tool
You will have all your screencasts (of all pages) as MP4 in your Downloads folder on your computer additionally you will have a pdf per screencast named like the title of the screencast with the content of the screen casts detail page in it. 
