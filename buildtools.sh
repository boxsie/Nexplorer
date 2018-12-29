#!/bin/bash

cd ~/Nexplorer/
#git pull
#cd Nexplorer.Web/wwwroot
#sudo rm -rf css
#sudo rm -rf js
#sudo rm -rf font
#cd ..
#sudo npm install
#sudo npm run build
sudo dotnet publish -c Release
sudo rm -rf /var/www/nexplorerTools/
sudo cp -r ~/Nexplorer/Nexplorer.Tools/bin/Release/netcoreapp2.2/publish/ /var/www/nexplorerTools
sudo cp ~/.Nexplorer/connectionStrings.json /var/www/nexplorerTools
sudo cp ~/.Nexplorer/emailConfig.json /var/www/nexplorerTools
sudo supervisorctl restart nexplorerTools
echo "Nexplorer web is running!"
