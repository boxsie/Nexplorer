#!/bin/bash

cd ~/nexplorer/
git pull
cd Nexplorer.Web/wwwroot
sudo rm -rf css
sudo rm -rf js
sudo rm -rf font 
cd ..
npm install
npm run build
sudo dotnet publish -c Release
cd /var/
sudo rm -rf nexplorer
sudo cp -r ~/nexplorer/Nexplorer.Web/bin/Release/netcoreapp2.2/publish/ /var/nexplorer
sudo cp ~/.nexplorer/connectionStrings.json /var/nexplorer
sudo cp ~/.nexplorer/emailConfig.json /var/nexplorer
sudo supervisorctl restart nexplorer
echo "Nexplorer web is running!"