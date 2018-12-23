#!/bin/bash

cd ~/Nexplorer/
git pull
cd Nexplorer.Web/wwwroot
sudo rm -rf css
sudo rm -rf js
sudo rm -rf font
cd ..
sudo npm install
sudo npm run build
sudo dotnet publish -c Release
sudo rm -rf /var/www/nexplorer/
sudo cp -r ~/Nexplorer/Nexplorer.Web/bin/Release/netcoreapp2.2/publish/ /var/www/nexplorer
sudo cp ~/.Nexplorer/connectionStrings.json /var/www/nexplorer
sudo cp ~/.Nexplorer/emailConfig.json /var/www/nexplorer
sudo supervisorctl restart nexplorer
echo "Nexplorer web is running!"
