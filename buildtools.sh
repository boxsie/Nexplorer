#!/bin/bash

cd ~/Nexplorer/
git pull
sudo dotnet publish -c Release
sudo rm -rf /var/www/nexplorerTools/
sudo cp -r ~/Nexplorer/Nexplorer.Tools/bin/Release/netcoreapp2.2/publish/ /var/www/nexplorerTools
sudo cp ~/.Nexplorer/connectionStrings.json /var/www/nexplorerTools
sudo cp ~/.Nexplorer/emailConfig.json /var/www/nexplorerTools
sudo chgrp -R www-data /var/www/nexplorerTools/
sudo chmod -R g+w /var/www/nexplorerTools/
sudo supervisorctl restart nexplorerTools
echo "Nexplorer web is running!"
