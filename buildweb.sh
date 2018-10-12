#!/bin/bash

cd ~/Nexplorer/
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
sudo cp -r ~/Nexplorer/Nexplorer.Web/bin/Release/netcoreapp2.1/publish/ /var/nexplorer
sudo supervisorctl restart nexplorer
echo "Nexplorer web is running!"